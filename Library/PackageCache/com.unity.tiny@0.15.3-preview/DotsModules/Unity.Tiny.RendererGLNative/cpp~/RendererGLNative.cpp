#include "RendererGLNative.h"
#include "bind-Unity_Tiny_GLFW.h"
#include "bind-Unity_Tiny_Image2DIOSTB.h"
//#include "bind-Unity_Tiny_Renderer.h"
//#include "bind-Unity_Tiny_RendererGL.h"
#include "bind-Unity_Tiny_RendererGLNative.h"
#include "bind-Unity_Tiny_Text.h"
#include "bind-Unity_Tiny_Shape2D.h"
#include "zeroplayer.h"
#include "EntityWrappers.h"
#include "GeminiMath.h"

#include <glew.h>
#include <string>
#include <vector>
#include <cstring>
#include <stdlib.h>

using namespace ut;
//using namespace ut::arch;
using namespace ut::Core2D;
using namespace ut::GLFW;
using namespace Unity::Tiny::Core2D;
using namespace Unity::Tiny::Text;
using namespace Unity::Tiny::Rendering;

#ifdef _DEBUG

#ifdef __GNUC__
#define __LOCATION__ __PRETTY_FUNCTION__
#elif defined(_MSC_VER)
#define __LOCATION__ __FUNCSIG__
#else
#define __LOCATION__ "Unknown location"
#endif

#define LOGE(fmt, ...) printf(fmt "\n", __VA_ARGS__)

struct GLErrorChecker
{
    const char* const where_;

    GLErrorChecker(const char* where)
        : where_(where)
    {
        GLint err = glGetError();
        if (err != GL_NO_ERROR) {
            LOGE("%s: pending error %x", where_, err);
        }
    }

    ~GLErrorChecker()
    {
        GLint err = glGetError();
        if (err  != GL_NO_ERROR) {
            LOGE("%s: while executing, error %x", where_, err);
        }
    }
};
#else

struct GLErrorChecker
{
#define __LOCATION__ 0
    GLErrorChecker(const char*) {}
};

#endif

struct Image2DRenderToTextureGLFW : ISystemStateComponentData
{
    struct
    {
        unsigned int glTexId;
        int w, h;
    } rendering, displaying;
};

GLuint
compileShader(GLenum type, const char* const src)
{
    GLErrorChecker checker(__LOCATION__);
    const char* start;
    if ((start = strstr(src, "//start")) == nullptr)
        start = src;
    GLuint shader = glCreateShader(type);
    GLsizei srcLen = (GLsizei)strlen(start);
    GLchar const* version = "#version 100\n";
    GLsizei verLen = (GLsizei)strlen(version);
    GLchar const* fpPrec = "precision highp float;\n";
    GLsizei fpPrecLen = (GLsizei)strlen(fpPrec);
    GLchar const* files[] = { version, fpPrec,    start };
    GLsizei lengths[]     = { verLen,  fpPrecLen, srcLen };
    glShaderSource(shader, 3, files, lengths);
    glCompileShader(shader);
    GLint status;
    glGetShaderiv(shader, GL_COMPILE_STATUS, &status);
    if (status == GL_FALSE) {
        GLint infoLength;
        glGetShaderiv(shader, GL_INFO_LOG_LENGTH, &infoLength);
        std::vector<char> logVec(infoLength + 1);
        glGetShaderInfoLog(shader, infoLength, nullptr, &logVec[0]);
        ut::log("GLERROR %s\n", &logVec[0]);
        glDeleteShader(shader);
        ut::log("Shader contents:\n--------------------\n%s\n====================\n", start);
        return 0;
    }
    return shader;
}

bool
linkProgram(GLuint pgm, GLuint vertShader, GLuint fragShader)
{
    GLErrorChecker checker(__LOCATION__);
    if ((pgm == 0) || (vertShader == 0) || (fragShader == 0))
        return false;
    glAttachShader(pgm, vertShader);
    glAttachShader(pgm, fragShader);
    glLinkProgram(pgm);
    GLint status;
    glGetProgramiv(pgm, GL_LINK_STATUS, &status);
    if (status == GL_FALSE) {
        GLint infoLength;
        glGetProgramiv(pgm, GL_INFO_LOG_LENGTH, &infoLength);
        std::vector<char> logVec(infoLength + 1);
        glGetProgramInfoLog(pgm, infoLength, nullptr, &logVec[0]);
        return false;
    }
    return true;
}

RendererPrivateGL::ShaderProgram
createProgram(const char* const vertShader, const char* const fragShader)
{
    GLErrorChecker checker(__LOCATION__);

    RendererPrivateGL::ShaderProgram r(glCreateProgram());

    GLuint vert = compileShader(GL_VERTEX_SHADER, vertShader);
    if (!vert)
        return RendererPrivateGL::ShaderProgram(0);

    GLuint frag = compileShader(GL_FRAGMENT_SHADER, fragShader);
    if (!frag)
        return RendererPrivateGL::ShaderProgram(0);

    glBindAttribLocation(r.id(), 0, "a_pos");
    glBindAttribLocation(r.id(), 1, "a_color");
    glBindAttribLocation(r.id(), 2, "a_matrow0");
    glBindAttribLocation(r.id(), 3, "a_matrow1");
    glBindAttribLocation(r.id(), 4, "a_texrect");

    // tile map case
    glBindAttribLocation(r.id(), 1, "a_uv");
    glBindAttribLocation(r.id(), 2, "a_tileidx");

    if (!linkProgram(r.id(), vert, frag)) {
        glDeleteProgram(r.id());
        r = RendererPrivateGL::ShaderProgram(0);
    }
    if (vert != 0)
        glDeleteShader(vert);
    if (frag != 0)
        glDeleteShader(frag);

    return r;
}

RendererPrivateGL::BasicShader::BasicShader(ShaderProgram const& p)
    : ShaderProgram(p)
{
    GLErrorChecker checker(__LOCATION__);
    u_camera = glGetUniformLocation(pgm_, "u_camera");
    GLint mainTexture = glGetUniformLocation(pgm_, "mainTexture");
    if (mainTexture >= 0) {
        glUseProgram(pgm_);
        glUniform1i(mainTexture, 0);
        glUseProgram(0);
    }
}

RendererPrivateGL::SolidShapeShader::SolidShapeShader(ShaderProgram const& p)
    : BasicShader(p)
{
    GLErrorChecker checker(__LOCATION__);
    u_color = glGetUniformLocation(pgm_, "u_color");
    u_trMatrix = glGetUniformLocation(pgm_, "u_trMatrix");
}

RendererPrivateGL::TilingShader::TilingShader(ShaderProgram const& p)
    : BasicShader(p)
{
    GLErrorChecker checker(__LOCATION__);
    u_tilerect = glGetUniformLocation(pgm_, "u_tilerect");
}

RendererPrivateGL::SlicingShader::SlicingShader(ShaderProgram const& p)
    : BasicShader(p)
{
    GLErrorChecker checker(__LOCATION__);

    u_border = glGetUniformLocation(pgm_, "u_border");
    u_uvmod_bl = glGetUniformLocation(pgm_, "u_uvmod_bl");
    u_uvmod_tr = glGetUniformLocation(pgm_, "u_uvmod_tr");
    u_tilerect = glGetUniformLocation(pgm_, "u_tilerect");
    u_innertexrect = glGetUniformLocation(pgm_, "u_innertexrect");
}

void
GLFWLRendererPrivate::beginScene(EntityManager& w, const Vector2f& targetSize)
{
    updateRTT(w);
    inrtt = false;
    if (usePresentShader) {
        renderTarget.resize(static_cast<int>(targetSize.x), static_cast<int>(targetSize.y));
        renderTarget.bind();
    } else {
        renderTarget.bindZero();
    }
    presentX = 0;
    presentY = 0;
    presentW = (int)targetSize.x;
    presentH = (int)targetSize.y;
}

void
GLFWLRendererPrivate::endScene(EntityManager&)
{
    GLErrorChecker checker(__LOCATION__);

    // reset all gl state
    glDisable(GL_SCISSOR_TEST);
    glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);
    renderTarget.bindZero();
    glDisable(GL_BLEND);
    if (usePresentShader) {
        renderTarget.bindZero();
        glViewport(presentX+presentXd,presentY+presentYd,presentW+presentWd,presentH+presentHd);
        glUseProgram(presentShader.id());
        renderTarget.bindTexture();
        glBindVertexArray(vaos[VB_Quad]);
        glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_SHORT, nullptr);
    }
    glBindTexture(GL_TEXTURE_2D, 0);
    glBindVertexArray(0);
    glUseProgram(0);
}

bool
GLFWLRendererPrivate::beginRenderToTexture(EntityManager& man, Entity e, const Camera2DRenderToTexture* rtt)
{
#if 0
    if (!man.hasComponent<Image2DRenderToTextureGLFW>(rtt->target) || !man.hasComponent<TextureGL>(rtt->target)) {
        Assert(0);
        return false;
    }
    TextureGL* destGLFW = man.getComponentPtrUnsafe<TextureGL>(rtt->target);
    Image2DRenderToTextureGLFW* dest = man.getComponentPtrUnsafe<Image2DRenderToTextureGLFW>(rtt->target);
    Image2D* destImg = man.getComponentPtrUnsafe<Image2D>(rtt->target);
    // (re)allocate dest target if needed - this smoothly handles resolution changes and init
    if (dest->rendering.w != rtt->width || dest->rendering.h != rtt->height || !dest->rendering.glTexId) {
        if (!dest->rendering.glTexId)
            glGenTextures(1, &dest->rendering.glTexId);
        glBindTexture(GL_TEXTURE_2D, dest->rendering.glTexId);
        glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, rtt->width, rtt->height, 0, GL_RGBA, GL_UNSIGNED_BYTE, 0);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, destImg->disableSmoothing ? GL_NEAREST : GL_LINEAR);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, destImg->disableSmoothing ? GL_NEAREST : GL_LINEAR);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
        glBindTexture(GL_TEXTURE_2D, 0);
        dest->rendering.w = rtt->width;
        dest->rendering.h = rtt->height;
    }
    // bind
    if (!rttTarget.hasFBO())
        rttTarget.allocateFBO();
    rttTarget.bindAndAttach(dest->rendering.glTexId);
    GLErrorChecker checker(__LOCATION__);
    inrtt = true;
    return true;
#endif
    return false;
}

void
GLFWLRendererPrivate::endRenderToTexture(EntityManager& man, Entity e, const Camera2DRenderToTexture* rtt)
{
#if 0
    Image2D* desti = man.getComponentPtrUnsafe<Image2D>(rtt->target);
    TextureGL* destGLFW = man.getComponentPtrUnsafe<TextureGL>(rtt->target);
    Image2DRenderToTextureGLFW* dest = man.getComponentPtrUnsafe<Image2DRenderToTextureGLFW>(rtt->target);
    std::swap(dest->displaying, dest->rendering);
    // update image
    desti->status = ImageStatus::Loaded; // now ready to use
    desti->imagePixelSize.x = (float)dest->displaying.w;
    desti->imagePixelSize.y = (float)dest->displaying.h;
    desti->hasAlpha = true;
    // update native image to rendered target
    if (!destGLFW->externalOwner && destGLFW->glTexId)
        glDeleteTextures(1, &destGLFW->glTexId);
    destGLFW->externalOwner = true;
    destGLFW->glTexId = dest->displaying.glTexId;
    // bind back original target
    renderTarget.bind();
    GLErrorChecker checker(__LOCATION__);
    inrtt = false;
#endif
}

void
GLFWLRendererPrivate::beginCamera(EntityManager& man, Entity e, const Vector2f& targetSize)
{
    GLErrorChecker checker(__LOCATION__);
    const Camera2D& cam = *man.getComponentPtrConstUnsafe<Camera2D>(e);
    Vector2f actualTargetSize;
    if (cam.rect.width <= 0.0f || cam.rect.height <= 0.0f) {
        glDisable(GL_SCISSOR_TEST);
        if (!usePresentShader && !inrtt) {
            glViewport(presentXd, presentYd, (GLsizei)targetSize.x + presentWd, (GLsizei)targetSize.y + presentHd);
            actualTargetSize.x = targetSize.x + (float)presentWd;
            actualTargetSize.x = targetSize.y + (float)presentHd;
        } else {
            glViewport(0, 0, (GLsizei)targetSize.x, (GLsizei)targetSize.y);
            actualTargetSize = targetSize;
        }
    } else {
        Rectf targetRect(0, 0, targetSize.x, targetSize.y);
        Rectf clipRect(cam.rect.x * targetSize.x, cam.rect.y * targetSize.y, cam.rect.width * targetSize.x,
                       cam.rect.height * targetSize.y);
        if (!usePresentShader && !inrtt) {
            targetRect.x += (float)presentXd; targetRect.y += (float)presentYd; targetRect.width += (float)presentWd; targetRect.height += (float)presentHd;
            clipRect.x += (float)presentXd; clipRect.y += (float)presentYd; clipRect.width += (float)presentWd; clipRect.height += (float)presentHd;
        }
        if (inrtt)
            clipRect.y = targetSize.y - clipRect.height - clipRect.y;
        // needs scissor and viewport, viewport alone does not clip
        clipRect.Clamp(targetRect);
        glScissor((GLint)clipRect.x, (GLint)clipRect.y, (GLsizei)clipRect.width, (GLsizei)clipRect.height);
        glViewport((GLint)clipRect.x, (GLint)clipRect.y, (GLsizei)clipRect.width, (GLsizei)clipRect.height);
        actualTargetSize = Vector2f(clipRect.width, clipRect.height);
        glEnable(GL_SCISSOR_TEST);
    }
    if (cam.clearFlags == CameraClearFlags::SolidColor) {
        glClearColor(cam.backgroundColor.r, cam.backgroundColor.g, cam.backgroundColor.b, cam.backgroundColor.a);
        glClear(GL_COLOR_BUFFER_BIT);
    }
    // force shader re-bind to update camera uniform
    currentCam[0] =
        actualTargetSize.y /
        (cam.halfVerticalSize * actualTargetSize.x); // 2/w -> pixels to normalized, then uniform scale by (h/2)/halfH
    currentCam[1] = 1.0f / cam.halfVerticalSize;
    currentCam[2] = 1.0f;
    if (inrtt)
        currentCam[1] = -currentCam[1];
}

void
GLFWLRendererPrivate::endCamera(EntityManager&, Entity)
{
}

void
GLFWLRendererPrivate::renderShape(EntityManager& man, DisplayListEntry& de)
{
    GLErrorChecker checker(__LOCATION__);

    const auto sr = man.getComponentPtrConstUnsafe<Shape2DRenderer>(de.e);
    const auto vertices = (Shape2DVertex*)man.getBufferElementDataPtrConstUnsafe<Shape2DVertex>(sr->shape);
    int verticesCount = man.getBufferElementDataLength<Shape2DVertex>(sr->shape);
    Assert((int)verticesCount <= sMaxSolidVertices);

    // setup drawing state
    setupBlending(sr->blending, sr->color.a != 1.0f);
    glUseProgram(solidShapeShader.id());
    float cpre[4];
    cpre[0] = sr->color.r * sr->color.a;
    cpre[1] = sr->color.g * sr->color.a;
    cpre[2] = sr->color.b * sr->color.a;
    cpre[3] = sr->color.a;
    glUniform4fv(solidShapeShader.u_color, 1, cpre);
    glUniform3fv(solidShapeShader.u_camera, 1, currentCam);

    // Convert matrix 4x4 to matrix 3x3.
    auto fm = de.finalMatrix.data;
    float mat[9] =
    {
        fm[00], fm[01], fm[02],
        fm[04], fm[05], fm[06],
        fm[12], fm[13], 1
    };

    glUniformMatrix3fv(solidShapeShader.u_trMatrix, 1, false, mat);

    addTriangleFanVertices(vertices, solidVertexBuffer, verticesCount);
    // upload
    glBindBuffer(GL_ARRAY_BUFFER, vbos[VB_Shape2D]);
    glBufferSubData(GL_ARRAY_BUFFER, 0, sizeof(SolidVertex) * verticesCount, solidVertexBuffer);

    glBindVertexArray(vaos[VB_Shape2D]);

    if (!man.hasComponent<Shape2DIndex>(sr->shape))
    {
        glDrawArrays(GL_TRIANGLE_FAN, 0, (GLsizei)verticesCount);
    }
    else
    {
#if 0  // Indexed polygons are not supported right now.
        const auto indices = (Shape2DIndex*)man.getBufferElementDataPtrConstUnsafe<Shape2DIndex>(sr->shape);
        int indicesCount = man.getBufferElementDataLength<Shape2DIndex>(sr->shape);
        Assert((int)indicesCount <= sMaxSolidIndices);
        glDrawElements(GL_TRIANGLES, (GLsizei)indicesCount, GL_UNSIGNED_SHORT, indices);
#endif
    }
}

void
GLFWLRendererPrivate::renderSpriteSliced(EntityManager& man, DisplayListEntry& de)
{
    GLErrorChecker checker(__LOCATION__);

    const Sprite2DRendererOptions* til = man.getComponentPtrConstUnsafe<Sprite2DRendererOptions>(de.e);
    const Sprite2DRenderer* sr = man.getComponentPtrConstUnsafe<Sprite2DRenderer>(de.e);
    const Sprite2D* sprite = man.getComponentPtrConstUnsafe<Sprite2D>(sr->sprite);
    const Sprite2DBorder* border = man.getComponentPtrConstUnsafe<Sprite2DBorder>(sr->sprite);
    const Image2D* image = man.getComponentPtrConstUnsafe<Image2D>(sprite->image);
    const TextureGL* tex = man.getComponentPtrConstUnsafe<TextureGL>(sprite->image);

    // setup drawing state
    setupBlending(sr->blending, image->hasAlpha || sr->color.a != 1.0f);
    glBindTexture(GL_TEXTURE_2D, tex->glTexId);
    glUseProgram(slicingShader.id());
    glUniform3fv(slicingShader.u_camera, 1, currentCam);

    addQuadVertices(sr->color, de, sprite, vertexBuffer);

    Slice9Constants c;
    computeSlice9Constants(image, border, sprite, de, til, c);

    glUniform4fv(slicingShader.u_border, 1, c.border.GetPtr());
    glUniform4fv(slicingShader.u_uvmod_bl, 1, c.uvmod_bl.GetPtr());
    glUniform4fv(slicingShader.u_uvmod_tr, 1, c.uvmod_tr.GetPtr());
    glUniform4fv(slicingShader.u_tilerect, 1, c.tilerect.GetPtr());
    glUniform4fv(slicingShader.u_innertexrect, 1, c.innertexrect.GetPtr());

    // upload
    glBindBuffer(GL_ARRAY_BUFFER, vbos[VB_Default]);
    glBufferSubData(GL_ARRAY_BUFFER, 0, sizeof(Vertex)*4, vertexBuffer);

    glBindVertexArray(vaos[VB_Default]);

    // draw
    glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_SHORT, nullptr);
}

void
GLFWLRendererPrivate::renderSpriteTiled(EntityManager& man, DisplayListEntry& de)
{
    GLErrorChecker checker(__LOCATION__);

    // tiling is single draw,
    const Sprite2DRendererOptions* til = man.getComponentPtrConstUnsafe<Sprite2DRendererOptions>(de.e);
    const Sprite2DRenderer* sr = man.getComponentPtrConstUnsafe<Sprite2DRenderer>(de.e);
    const Sprite2D* sprite = man.getComponentPtrConstUnsafe<Sprite2D>(sr->sprite);
    const Image2D* image = man.getComponentPtrConstUnsafe<Image2D>(sprite->image);
    const TextureGL* tex = man.getComponentPtrConstUnsafe<TextureGL>(sprite->image);

    // setup drawing state
    setupBlending(sr->blending, image->hasAlpha || sr->color.a != 1.0f);
    glBindTexture(GL_TEXTURE_2D, tex->glTexId);
    glUseProgram(tilingShader.id());
    glUniform3fv(tilingShader.u_camera, 1, currentCam);

    addQuadVertices(sr->color, de, sprite, vertexBuffer);

    // setup tiling uniforms
    Vector4f tilerect;
    computeTileConstants(image, sprite, de, til, tilerect);
    glUniform4fv(tilingShader.u_tilerect, 1, tilerect.GetPtr());

    // upload
    glBindBuffer(GL_ARRAY_BUFFER, vbos[VB_Default]);
    glBufferSubData(GL_ARRAY_BUFFER, 0, sizeof(Vertex)*4, vertexBuffer);

    glBindVertexArray(vaos[VB_Default]);

    // draw
    glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_SHORT, nullptr);
}

void
GLFWLRendererPrivate::renderText(EntityManager& man, DisplayListEntry& de)
{
    GLErrorChecker checker(__LOCATION__);

    if (man.hasComponent<Text2DStyleNativeFont>(de.e)) {
        //TODO replace with a log warning
        ut::log("Native fonts are not supported yet in GL Native");
    }
    else if (man.hasComponent<Text2DStyleBitmapFont>(de.e))
    {
        const Text2DRenderer* textRenderer = man.getComponentPtrConstUnsafe<Text2DRenderer>(de.e);
        const Text2DStyle* style = man.getComponentPtrConstUnsafe<Text2DStyle>(de.e);

        const Text2DPrivateBitmap* textPrivate = man.getComponentPtrConstUnsafe<Text2DPrivateBitmap>(de.e);
        const GlyphPrivate* glyphBuffer = (GlyphPrivate*)man.getBufferElementDataPtrConstUnsafe<GlyphPrivateBuffer>(de.e);
        int glyphCount = man.getBufferElementDataLength<GlyphPrivateBuffer>(de.e);

        if (glyphCount == 0)
            return;

        const Text2DStyleBitmapFont* bitmapStyle = man.getComponentPtrConstUnsafe<Text2DStyleBitmapFont>(de.e);
        const BitmapFont* bitmapFont = man.getComponentPtrConstUnsafe<BitmapFont>(bitmapStyle->font);
        const TextureGL* tex = man.getComponentPtrConstUnsafe<TextureGL>(bitmapFont->textureAtlas);
        const Image2D* bitmapFontImage = man.getComponentPtrConstUnsafe<Image2D>(bitmapFont->textureAtlas);

        // setup drawing state
        setupBlending(textRenderer->blending, bitmapFontImage->hasAlpha || style->color.a != 1.0f);
        glBindTexture(GL_TEXTURE_2D, tex->glTexId);
        glUseProgram(basicShader.id());
        glUniform3fv(basicShader.u_camera, 1, currentCam);

        Matrix4x4f finalMatrix;
        std::memcpy(static_cast<void*>(&finalMatrix), static_cast<void*>(&de.finalMatrix), 16 * sizeof(float));

        for (int i = 0; i < glyphCount; i++)
        {
            GlyphPrivate glyph = *(glyphBuffer + i);
            //Adjust position and dim per glyph according to pivot position and text origin (originY is baseline)
            float transX = de.inBounds.width * textRenderer->pivot.x;
            float transY = de.inBounds.height * textRenderer->pivot.y - std::abs(bitmapFont->descent);
            Rectf rec;
            rec.x = (glyph.position.x - glyph.ci.width / 2 - transX) * textPrivate->fontScale.x + de.inBounds.x;
            rec.y = (glyph.position.y - glyph.ci.height / 2 - transY)* textPrivate->fontScale.y + de.inBounds.y;
            rec.width = glyph.ci.width * textPrivate->fontScale.x;
            rec.height = glyph.ci.height * textPrivate->fontScale.y;

            addTextQuadVertices(textRenderer, finalMatrix, rec, &glyph, style->color, vertexBuffer + i * 4);
        }

        // upload
        glBindBuffer(GL_ARRAY_BUFFER, vbos[VB_Default]);
        glBufferSubData(GL_ARRAY_BUFFER, 0, sizeof(Vertex)*glyphCount * 4, vertexBuffer);

        glBindVertexArray(vaos[VB_Default]);

        // draw
        glDrawElements(GL_TRIANGLES, glyphCount * 6, GL_UNSIGNED_SHORT, nullptr);
    }

#if 0
    Text2DRenderer* textRenderer = e.getComponentPtr<Text2DRenderer>();
    Text2DGLFW* tex = e.getComponentPtr<Text2DGLFW>();

    Assert(textRenderer && tex);

    //Use basic shader for now
    bindShader(basicShader);
    //Setup blending
    //setupBlending(BlendOp::Alpha, tex->style.color.a != 1.0f);
    //Bind sf::texture
    bindTexture(tex->rtex->getTexture());

    // Create vertex buffer
    addQuadVertices(textRenderer, de, vertexBuffer);

    // Draw elements
    glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_SHORT, indexBuffer);
#endif
}

void
GLFWLRendererPrivate::renderSprites(EntityManager& man, int n, DisplayListEntry* list)
{
    GLErrorChecker checker(__LOCATION__);

    const Sprite2DRenderer* sr = man.getComponentPtrConstUnsafe<Sprite2DRenderer>(list[0].e);
    const Sprite2D* sprite = man.getComponentPtrConstUnsafe<Sprite2D>(sr->sprite);
    const Image2D* image = man.getComponentPtrConstUnsafe<Image2D>(sprite->image);
    const TextureGL* tex = man.getComponentPtrConstUnsafe<TextureGL>(sprite->image);

    // setup drawing state
    setupBlending(sr->blending, image->hasAlpha || sr->color.a != 1.0f);
    glBindTexture(GL_TEXTURE_2D, tex->glTexId);
    glUseProgram(basicShader.id());
    glUniform3fv(basicShader.u_camera, 1, currentCam);

    // build vertex buffer
    for (int i = 0; i < n; i++) {
        const Sprite2DRenderer* sir = man.getComponentPtrConstUnsafe<Sprite2DRenderer>(list[i].e);
        const Sprite2D* s = man.getComponentPtrConstUnsafe<Sprite2D>(sir->sprite);
        addQuadVertices(sir->color, list[i], s, vertexBuffer + i * 4);
    }

    // upload
    glBindBuffer(GL_ARRAY_BUFFER, vbos[VB_Default]);
    glBufferSubData(GL_ARRAY_BUFFER, 0, sizeof(Vertex)*n*4, vertexBuffer);

    glBindVertexArray(vaos[VB_Default]);

    // draw
    glDrawElements(GL_TRIANGLES, n * 6, GL_UNSIGNED_SHORT, nullptr);
}

void
GLFWLRendererPrivate::renderSpriteBatch(int n, DisplayListEntry* list, EntityManager& man)
{
    GLErrorChecker checker(__LOCATION__);
    Assert(n > 0 && n <= sMaxBatchSize);

    switch (list->type) {
    case DisplayListEntryType::HitBoxOnly:
    case DisplayListEntryType::GroupOnly:
        return;
    case DisplayListEntryType::Shape:
        for (int i = 0; i < n; i++)
            renderShape(man, list[i]);
        return;
    case DisplayListEntryType::TiledSprite:
        for (int i = 0; i < n; i++)
            renderSpriteTiled(man, list[i]);
        return;
    case DisplayListEntryType::SlicedSprite:
        for (int i = 0; i < n; i++)
            renderSpriteSliced(man, list[i]);
        return;
    case DisplayListEntryType::Text:
        for (int i = 0; i < n; i++)
            renderText(man, list[i]);
        return;
    case DisplayListEntryType::Sprite:
        renderSprites(man, n, list);
        break;
    default:
        Assert(0);
        return;
    }
}

void
GLFWLRendererPrivate::setupBlending(BlendOp blending, bool hasAlpha)
{
    GLErrorChecker checker(__LOCATION__);
    switch (blending) {
    case BlendOp::Alpha:
        if (!hasAlpha) {
            glDisable(GL_BLEND);
        } else {
            glEnable(GL_BLEND);
            glBlendFunc(GL_ONE, GL_ONE_MINUS_SRC_ALPHA);
        }
        break;
    case BlendOp::Add:
        glEnable(GL_BLEND);
        glBlendFunc(GL_ONE, GL_ONE);
        break;
    case BlendOp::Multiply:
        glEnable(GL_BLEND);
        glBlendFunc(GL_DST_COLOR, GL_ONE_MINUS_SRC_ALPHA);
        break;
    case BlendOp::MultiplyAlpha:
        glEnable(GL_BLEND);
        glBlendFunc (GL_ZERO, GL_SRC_ALPHA);
        break;
    default:
        Assert(0);
        break;
    }
}

bool
GLFWLRendererPrivate::initShaders()
{
    // build shader
    if ((basicShader = createProgram(shaderSrcVertex, shaderSrcFragment)).id() == 0)
        return false;

    if ((tilingShader = createProgram(shaderSrcVertex, shaderSrcFragmentTiling)).id() == 0)
        return false;

    if ((solidShapeShader = createProgram(shaderSrcSolidVertex, shaderSrcSolidFragment)).id() == 0)
        return false;

    if ((slicingShader = createProgram(shaderSrcVertexSlicing, shaderSrcFragmentSlicing)).id() == 0)
        return false;

    if ((presentShader = createProgram(
               R"(attribute vec2 aPos;
                attribute vec2 aTexCoords;
                varying vec2 TexCoords;

                void main()
                {
                    gl_Position = vec4(aPos.x, aPos.y, 0.0, 1.0);
                    TexCoords = aTexCoords;
                })",

             R"(varying vec2 TexCoords;
                uniform sampler2D screenTexture;
                void main()
                {
                    gl_FragColor = texture2D(screenTexture, TexCoords);
                })")).id() == 0)
        return false;

    glBindAttribLocation(presentShader.id(), 0, "aPos");
    glBindAttribLocation(presentShader.id(), 1, "aTexCoords");
    glLinkProgram(presentShader.id());

    return true;
}

void
GLFWLRendererPrivate::initBuffers()
{
    for (int i = 0, j = 0; i < sMaxBatchSize * 4; i += 4, j += 6) {
        // create two triangles for every four vertices
        indexBuffer[j] = i;
        indexBuffer[j + 1] = i + 1;
        indexBuffer[j + 2] = i + 2;
        indexBuffer[j + 3] = i + 2;
        indexBuffer[j + 4] = i + 3;
        indexBuffer[j + 5] = i;
    }

    glGenBuffers(1, &ibo);
    glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ibo);
    glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(indexBuffer), indexBuffer, GL_STATIC_DRAW);

    glGenBuffers(VB_Count, vbos);
    glGenVertexArrays(VB_Count ,vaos);

    // Screen Quad
    glBindVertexArray(vaos[VB_Quad]);
    glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ibo);
    glBindBuffer(GL_ARRAY_BUFFER, vbos[VB_Quad]);

    struct QuadVertex
    {
        Vector2f xy;
        Vector2f uv;
    };

    QuadVertex quadVertices[] = {
        {{-1.0f, -1.0f}, {0.0f, 0.0f}},
        {{-1.0f, 1.0f}, {0.0f, 1.0f}},
        {{1.0f, 1.0f}, {1.0f, 1.0f}},
        {{1.0f, -1.0f}, {1.0f, 0.0f}},
    };

    glBufferData(GL_ARRAY_BUFFER, sizeof(quadVertices), quadVertices, GL_STATIC_DRAW);

    glVertexAttribPointer(0, 2, GL_FLOAT, GL_FALSE, 4 * sizeof(float), (void*)offsetof(QuadVertex, xy));
    glVertexAttribPointer(1, 2, GL_FLOAT, GL_FALSE, 4 * sizeof(float), (void*)offsetof(QuadVertex, uv));

    glEnableVertexAttribArray(0);
    glEnableVertexAttribArray(1);

    // Default
    glBindVertexArray(vaos[VB_Default]);
    glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ibo);
    glBindBuffer(GL_ARRAY_BUFFER, vbos[VB_Default]);
    glBufferData(GL_ARRAY_BUFFER, sizeof(vertexBuffer), vertexBuffer, GL_DYNAMIC_DRAW);

    glVertexAttribPointer(0, 2, GL_FLOAT, GL_FALSE, sizeof(Vertex), (void*)offsetof(Vertex, x));
    glVertexAttribPointer(1, 4, GL_FLOAT, GL_FALSE, sizeof(Vertex), (void*)offsetof(Vertex, rgba));
    glVertexAttribPointer(2, 3, GL_FLOAT, GL_FALSE, sizeof(Vertex), (void*)offsetof(Vertex, matrix[0]));
    glVertexAttribPointer(3, 3, GL_FLOAT, GL_FALSE, sizeof(Vertex), (void*)offsetof(Vertex, matrix[3]));
    glVertexAttribPointer(4, 4, GL_FLOAT, GL_FALSE, sizeof(Vertex), (void*)offsetof(Vertex, texrect));

    glEnableVertexAttribArray(0);
    glEnableVertexAttribArray(1);
    glEnableVertexAttribArray(2);
    glEnableVertexAttribArray(3);
    glEnableVertexAttribArray(4);

    // Shape2D
    glBindVertexArray(vaos[VB_Shape2D]);
    glBindBuffer(GL_ARRAY_BUFFER, vbos[VB_Shape2D]);
    glBufferData(GL_ARRAY_BUFFER, sizeof(solidVertexBuffer), solidVertexBuffer, GL_DYNAMIC_DRAW);
    glVertexAttribPointer(0, 2, GL_FLOAT, GL_FALSE, sizeof(SolidVertex), (void*)offsetof(SolidVertex, x));
    glEnableVertexAttribArray(0);

    // reset
    glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
    glBindBuffer(GL_ARRAY_BUFFER, 0);
    glBindVertexArray(0);
}

GLFWLRendererPrivate::GLFWLRendererPrivate()
    : initialized(false)
{
}

bool
GLFWLRendererPrivate::init(EntityManager& w)
{
    if (!initialized && (glewInit() == GL_NO_ERROR)) {
        if (!initShaders())
            return false;
        initBuffers();
        initialized = true;
        ut::log("GL Native renderer initialized.\n");
        InitComponentId<Image2D>();
        InitComponentId<TextureGL>();
        InitComponentId<Image2DRenderToTexture>();
        //InitComponentId<Image2DRenderToTextureGLFW>();
        InitComponentId<Camera2D>();
        InitComponentId<Shape2DRenderer>();
        InitComponentId<Shape2DVertex>();
        InitComponentId<Shape2DIndex>();
        InitComponentId<Sprite2DRendererOptions>();
        InitComponentId<Sprite2DRenderer>();
        InitComponentId<Sprite2DBorder>();
        InitComponentId<Sprite2D>();
        InitComponentId<Text2DRenderer>();
        InitComponentId<Text2DStyleNativeFont>();
        InitComponentId<Text2DStyleBitmapFont>();
        InitComponentId<Text2DStyle>();
        InitComponentId<TextString>();
        InitComponentId<Text2DPrivateNative>();
        InitComponentId<TextPrivateFontName>();
        InitComponentId<Text2DPrivateBitmap>();
        InitComponentId<GlyphPrivateBuffer>();
        InitComponentId<BitmapFont>();
        presentXd = 0;
        presentYd = 0;
        presentWd = 0;
        presentHd = 0;
        usePresentShader = true;
    }
    return initialized;
}

GLFWLRendererPrivate::~GLFWLRendererPrivate() { }

void
GLFWLRendererPrivate::setPresentBorder(int dx, int dy, int dw, int dh) {
    presentXd = dx;
    presentYd = dy;
    presentWd = dw;
    presentHd = dh;
}

void
GLFWLRendererPrivate::deInit(EntityManager& w)
{
    if (!initialized)
        return;
    // TODO: free gl resources properly
    renderTarget.reset();
    initialized = false;
}

uint32_t
GLFWLRendererPrivate::uploadNewTexture(int w, int h, const uint8_t *pixels, bool disableSmoothing)
{
    uint32_t r = 0;
    glGenTextures(1, &r);
    glBindTexture(GL_TEXTURE_2D, r);
    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, w, h, 0, GL_RGBA, GL_UNSIGNED_BYTE, pixels);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, disableSmoothing ? GL_NEAREST : GL_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, disableSmoothing ? GL_NEAREST : GL_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
    glBindTexture(GL_TEXTURE_2D, 0);
    if ( glGetError() != GL_NO_ERROR ) {
        ut::log( "GL texture upload error!");
        return 0;
    }
    return r;
}

bool
GLFWLRendererPrivate::isBadIntelDriver()
{
#ifdef _WIN32
    const char *v = (const char*)glGetString(GL_VENDOR);
    if ( v && strncmp(v,"Intel",5)==0) {
        char *ver = (char*)glGetString(GL_VERSION);
        // known bad:   3.3.0 - Build 21.20.16.4664
        // known good:  3.3.0 - Build 21.20.16.4550
        //              3.3.0 - Build 23.20.16.4973
        ver = strstr(ver,"Build ");
        if (!ver)
            return false;
        int ver0 = strtol(ver+6, &ver, 10);
        int ver1 = strtol(ver+1, &ver, 10);
        int ver2 = strtol(ver+1, &ver, 10);
        int ver3 = strtol(ver+1, &ver, 10);
        if ( ver0==32 && ver1==20 && ver2==16 && ver3==4664 )
            return true;
        return false;
    }
#endif

    return false;
}

/*
Image
GLFWLRendererPrivate::readResultingImage()
{
    if (initialized) {
        return renderTarget.readFromGPU();
    }
    return Image();
}
*/

void
GLFWLRendererPrivate::updateRTT(EntityManager& w)
{
#if 0
    // initialize empty images - this could go into it's own system
    EntityCommandBuffer ecb;

    // add the native image component for things that need it for rtt
    w.forEach<Entity>({ComponentSpec::subtractive<TextureGL>(), ComponentSpec::create<Image2D>(),
                       ComponentSpec::create<Image2DRenderToTexture>()},
                      [&ecb](Entity& entity) {
                          ecb.addComponent<TextureGL>(entity);
                      }, BufferingMode::Unbuffered);
    ecb.commit(w);

    // add the native rtt component that is need for rtt
    w.forEach<Entity, TextureGL>({ComponentSpec::subtractive<Image2DRenderToTextureGLFW>(),
                                    ComponentSpec::create<Image2D>(), ComponentSpec::create<Image2DRenderToTexture>()},
                                   [&ecb, &w](Entity& entity, TextureGL& ig) {
                                       ecb.addComponent<Image2DRenderToTextureGLFW>(entity);
                                   }, BufferingMode::Unbuffered);
    ecb.commit(w);

    // remove the native rtt component and image if the api component is gone
    w.forEach<Entity, TextureGL, Image2DRenderToTextureGLFW>(
        {ComponentSpec::subtractive<Image2DRenderToTexture>()},
        [&ecb, &w](Entity& entity, TextureGL& ig, Image2DRenderToTextureGLFW& igrtt) {
            if (!ig.externalOwner && ig.glTexId)
                glDeleteTextures(1, &ig.glTexId);
            if (w.hasComponent<Image2D>(entity)) {
                // we still have an image, promote the rtt texture to a normal texture
                ig.externalOwner = false;
                ig.glTexId = igrtt.displaying.glTexId;
                if (igrtt.rendering.glTexId)
                    glDeleteTextures(1, &igrtt.rendering.glTexId);
            } else {
                if (igrtt.displaying.glTexId)
                    glDeleteTextures(1, &igrtt.displaying.glTexId);
                ecb.removeComponent<TextureGL>(entity);
            }
            // always remove rendering
            if (igrtt.rendering.glTexId)
                glDeleteTextures(1, &igrtt.rendering.glTexId);
            // remove native rtt
            ecb.removeComponent<Image2DRenderToTextureGLFW>(entity);
        }, BufferingMode::Unbuffered);
    ecb.commit(w);
#endif
}

void
RenderTarget::allocateTexture(int w, int h)
{
    Assert(fbo_);
    glBindFramebuffer(GL_FRAMEBUFFER, fbo_);
    if (!renderTex_)
        glGenTextures(1, &renderTex_);
    glBindTexture(GL_TEXTURE_2D, renderTex_);

    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, w, h, 0, GL_RGBA, GL_UNSIGNED_BYTE, nullptr);

    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);

    glFramebufferTexture(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, renderTex_, 0);

    GLenum DrawBuffers[1] = {GL_COLOR_ATTACHMENT0};
    glDrawBuffers(1, DrawBuffers);

    Assert(glCheckFramebufferStatus(GL_FRAMEBUFFER) == GL_FRAMEBUFFER_COMPLETE);
    glBindTexture(GL_TEXTURE_2D, 0);
    glBindFramebuffer(GL_FRAMEBUFFER, 0);
    w_ = w;
    h_ = h;
}

void
RenderTarget::allocateFBO()
{
    Assert(fbo_ == 0);
    glGenFramebuffers(1, &fbo_);
}

RenderTarget::RenderTarget()
    : fbo_(0)
    , renderTex_(0)
    , w_(0)
    , h_(0)
{
}

void
RenderTarget::bindAndAttach(unsigned int tex)
{
    Assert(glIsTexture(tex));
    int w, h;
    glBindTexture(GL_TEXTURE_2D, tex);
    glGetTexLevelParameteriv(GL_TEXTURE_2D, 0, GL_TEXTURE_WIDTH, &w);
    glGetTexLevelParameteriv(GL_TEXTURE_2D, 0, GL_TEXTURE_WIDTH, &h);
    Assert(w > 0 && h > 0);
    Assert(fbo_);
    glBindFramebuffer(GL_FRAMEBUFFER, fbo_);
    glFramebufferTexture(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, tex, 0);
    GLenum DrawBuffers[1] = {GL_COLOR_ATTACHMENT0};
    glDrawBuffers(1, DrawBuffers);
    GLenum fbostat = glCheckFramebufferStatus(GL_FRAMEBUFFER);
    Assert(fbostat == GL_FRAMEBUFFER_COMPLETE);
}

void
RenderTarget::bind()
{
    Assert(fbo_);
    glBindFramebuffer(GL_FRAMEBUFFER, fbo_);
}

void
RenderTarget::bindZero()
{
    glBindFramebuffer(GL_FRAMEBUFFER, 0);
}

RenderTarget::~RenderTarget() { reset(); }

void
RenderTarget::reset()
{
    fbo_ = 0;
    renderTex_ = 0;
    w_ = h_ = 0;
}

void
RenderTarget::resize(int w, int h)
{
    if ((w_ != w) || (h_ != h)) {
        w_ = w;
        h_ = h;
        if (!fbo_)
            allocateFBO();
        allocateTexture(w, h);
    }
}

void
RenderTarget::bindTexture()
{
    glBindTexture(GL_TEXTURE_2D, renderTex_);
}

/*
Image
RenderTarget::readFromGPU()
{
    if (!renderTex_)
        return Image();
    glBindTexture(GL_TEXTURE_2D, renderTex_);
    Image r({static_cast<uint32_t>(w_), static_cast<uint32_t>(h_)});
    glGetTexImage(GL_TEXTURE_2D, 0, GL_RGBA, GL_UNSIGNED_BYTE, r.get());
    glBindTexture(GL_TEXTURE_2D, 0);
    return r;
}*/

// --------------------------------------------------------- BIND TO C# --------------------------------------------------

static GLFWLRendererPrivate sInst;


ZEROPLAYER_EXPORT
void ZEROPLAYER_CALL init_rendererglfw(void *emHandle) {
    EntityManager em(emHandle);
    sInst.init(em);
}

ZEROPLAYER_EXPORT
void ZEROPLAYER_CALL deinit_rendererglfw(void *emHandle) {
    EntityManager em(emHandle);
    sInst.deInit(em);
}

ZEROPLAYER_EXPORT
void ZEROPLAYER_CALL begincamera_rendererglfw(void *emHandle, Entity ecam, float w, float h) {
    EntityManager em(emHandle);
    Vector2f targetSize(w,h);
    sInst.beginCamera(em, ecam, targetSize);
}

ZEROPLAYER_EXPORT
void ZEROPLAYER_CALL endcamera_rendererglfw(void *emHandle, Entity ecam) {
    EntityManager em(emHandle);
    sInst.endCamera(em, ecam);
}

ZEROPLAYER_EXPORT
void ZEROPLAYER_CALL beginscene_rendererglfw(void *emHandle, float w, float h) {
    EntityManager em(emHandle);
    Vector2f targetSize(w,h);
    sInst.beginScene(em, targetSize);
}

ZEROPLAYER_EXPORT
void ZEROPLAYER_CALL endscene_rendererglfw(void *emHandle) {
    EntityManager em(emHandle);
    sInst.endScene(em);
}

ZEROPLAYER_EXPORT
void ZEROPLAYER_CALL beginrtt_rendererglfw(void *emHandle, Entity ecam, const Camera2DRenderToTexture *rtt) {
    EntityManager em(emHandle);
    sInst.beginRenderToTexture(em,ecam,rtt);
}

ZEROPLAYER_EXPORT
void ZEROPLAYER_CALL endrtt_rendererglfw(void *emHandle, Entity ecam, const Camera2DRenderToTexture *rtt) {
    EntityManager em(emHandle);
    sInst.endRenderToTexture(em,ecam,rtt);
}

ZEROPLAYER_EXPORT
void ZEROPLAYER_CALL drawbatch_rendererglfw(void *emHandle, int n, DisplayListEntry *batch) {
    EntityManager em(emHandle);
    sInst.renderSpriteBatch(n, batch, em);
}

ZEROPLAYER_EXPORT
void ZEROPLAYER_CALL setPresentBorder_rendererglfw(int dx, int dy, int dw, int dh) {
    sInst.setPresentBorder(dx,dy,dw,dh);
}

ZEROPLAYER_EXPORT
bool ZEROPLAYER_CALL isBadIntelDriver_rendererglfw() {
    return sInst.isBadIntelDriver();
}

ZEROPLAYER_EXPORT
uint32_t ZEROPLAYER_CALL uploadNewTexture_rendererglfw(int w, int h, uint8_t *pixels, int disableSmoothing) {
    return sInst.uploadNewTexture(w,h,pixels,disableSmoothing!=0);
}

