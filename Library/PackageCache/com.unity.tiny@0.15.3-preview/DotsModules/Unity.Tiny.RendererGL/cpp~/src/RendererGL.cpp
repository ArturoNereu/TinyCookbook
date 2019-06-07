#include "../include/RendererGL.h"

using namespace ut;
using namespace ut::Core2D;

using namespace Unity::Tiny;
using namespace Unity::Tiny::Core2D;
using namespace Unity::Tiny::Text;

// start tags are required so shaders can be trimmed for when running in core profile

static float nextFullScale(float x, float bias=.5f)
{
    // bias is "Stretch Value" in bigU
    // helper used for computing scale factor for adaptive tiling
    if (x < 0.0f)
        return -nextFullScale(-x,bias);
    x += bias;
    if (x <= 1.0f)
        return 1.0f;
    return (float)((int)x);
}

DLLEXPORT void
RendererPrivateGL::computeTileConstants(const Image2D* image, const Sprite2D* sprite, const DisplayListEntry& de,
                                        const Sprite2DRendererOptions* til, Vector4f& out)
{
    float xscale =
        de.inBounds.width / (sprite->imageRegion.width * sprite->pixelsToWorldUnits * image->imagePixelSize.x);
    float yscale =
        de.inBounds.height / (sprite->imageRegion.height * sprite->pixelsToWorldUnits * image->imagePixelSize.y);
    if (til->drawMode == DrawMode::AdaptiveTiling) {
        xscale = nextFullScale(xscale);
        yscale = nextFullScale(yscale);
    }
    out.Set(0, 0, xscale, yscale);
}

DLLEXPORT void
RendererPrivateGL::computeSlice9Constants(const Image2D* image, const Sprite2DBorder* border, const Sprite2D* sprite,
                                          const DisplayListEntry& de, const Sprite2DRendererOptions* til,
                                          Slice9Constants& out)
{
    const auto &rbig = sprite->imageRegion;

    // compute tiling uniforms
    float xscale = de.inBounds.width / (sprite->pixelsToWorldUnits * image->imagePixelSize.x);
    float yscale = de.inBounds.height / (sprite->pixelsToWorldUnits * image->imagePixelSize.y);

    // values where border starts and ends, in normalized uv
    float bl_tx = border->bottomLeft.x * rbig.width / xscale;
    float bl_ty = border->bottomLeft.y * rbig.height / yscale;
    float tr_tx = 1.0f - ((1.0f - border->topRight.x) * rbig.width / xscale);
    float tr_ty = 1.0f - ((1.0f - border->topRight.y) * rbig.height / yscale);

    out.border.Set(bl_tx, bl_ty, tr_tx, tr_ty);

    // border bottom left 1:1 mapped
    out.uvmod_bl.Set(rbig.x, 1.0f-rbig.y, xscale, -yscale);

    // border top right 1:1 mapped
    float bdx = rbig.x + border->topRight.x*rbig.width;
    float bdy = rbig.y + border->topRight.y*rbig.height;
    out.uvmod_tr.Set(-tr_tx*xscale + bdx, 
                     -tr_ty*-yscale + 1.0f - bdy,
                     xscale, -yscale);

    // compute scale for inner tiling in [0..1] range so we can clip it
    float inW = (border->topRight.x - border->bottomLeft.x) * rbig.width;
    float inH = (border->topRight.y - border->bottomLeft.y) * rbig.height;

    float xscaleInner = xscale / inW;
    float yscaleInner = yscale / inH;
    float tW = tr_tx - bl_tx;
    float tH = tr_ty - bl_ty;
    switch (til->drawMode) {
        case DrawMode::AdaptiveTiling:
            xscaleInner *= tW;
            yscaleInner *= tH;
            xscaleInner = nextFullScale(xscaleInner);
            yscaleInner = nextFullScale(yscaleInner);
            xscaleInner /= tW;
            yscaleInner /= tH;
            break;
        case DrawMode::Stretch:
            xscaleInner = 1.0f/tW;
            yscaleInner = 1.0f/tH;
            break;
        case DrawMode::ContinuousTiling:
            break;
        default: 
            //AssertNotReached();
            return;
    }
    out.tilerect.Set(-bl_tx * xscaleInner, -bl_ty * yscaleInner, xscaleInner, yscaleInner);

    // compute inner texture rectangle
    Rectf rinner = { rbig.x + border->bottomLeft.x*rbig.width, rbig.y + border->bottomLeft.y*rbig.height, inW,inH };
    out.innertexrect.Set(rinner.x, 1.0f - rinner.y, rinner.width, -rinner.height);
}

RENDERER_GL_EXPORT const char RendererPrivateGL::shaderSrcSolidVertex[] = R"shader(
        #version 100
        precision highp float;
        //start

        attribute vec2 a_pos;

        uniform vec3 u_camera;
        uniform mat3 u_trMatrix;

        void main() {
            vec3 pos = u_trMatrix * vec3(a_pos, 1.0);
            pos = u_camera * pos;
            gl_Position = vec4(pos.x,pos.y,0.0,1.0);
        }
    )shader";

const char RendererPrivateGL::shaderSrcSolidFragment[] =  R"shader(
        #version 100
        precision mediump float;
        //start

        uniform vec4 u_color;

        void main() {
            gl_FragColor = u_color;
        }
    )shader";

const char RendererPrivateGL::shaderSrcVertex[] = R"shader(
        #version 100
        precision highp float;
        //start

        attribute vec2 a_pos;
        attribute vec4 a_color;
        attribute vec3 a_matrow0;
        attribute vec3 a_matrow1;
        attribute vec4 a_texrect;
        varying vec4 v_color;
        varying vec2 v_uv;
        varying vec2 v_uvraw;
        varying vec4 v_texrect;
        uniform vec3 u_camera;
        void main() {
            v_color = a_color;
            vec4 tr = vec4(a_texrect.x, 1.0-a_texrect.y, a_texrect.z, -a_texrect.w);
            vec3 pos;
            pos.x = dot(a_matrow0,vec3(a_pos.x,a_pos.y,1.0));
            pos.y = dot(a_matrow1,vec3(a_pos.x,a_pos.y,1.0));
            pos.z = 1.0;
            pos = u_camera * pos;
            v_uvraw = a_pos;
            v_uv = a_pos*tr.zw+tr.xy;
            v_texrect = tr;
            gl_Position = vec4(pos.x,pos.y,0.0,1.0);
        }
    )shader";

const char RendererPrivateGL::shaderSrcFragment[] =  R"shader(        
        #version 100
        precision mediump float;
        //start
        
        uniform sampler2D mainTexture;
        varying vec4 v_color;
        varying vec2 v_uv;
        void main() {
            vec4 c = v_color * texture2D(mainTexture, v_uv);
            c.rgb *= v_color.a;
            gl_FragColor = c;
        }
    )shader";

const char RendererPrivateGL::shaderSrcFragmentTiling[] = R"shader(
        #version 100
        precision highp float;
        //start

        uniform sampler2D mainTexture;
        varying vec4 v_color;
        varying vec2 v_uvraw;
        varying vec4 v_texrect;
        uniform vec4 u_tilerect;
        void main() {
            vec2 uv = v_uvraw*u_tilerect.zw + u_tilerect.xy;
            uv = fract(uv);
            uv = uv*v_texrect.zw + v_texrect.xy;
            vec4 c = texture2D(mainTexture, uv);
            c *= v_color;
            c.rgb *= v_color.a;
            gl_FragColor = c;
        }
    )shader";

/*
// long, readable, scalar version of below 9-slice shader
// it's split into vertex and fragment shader now 
// u
if (v_uvraw.x<=u_border.x) {
    uv.x = v_uvraw.x*u_uvmod_bl.z + u_uvmod_bl.x;
} else if (v_uvraw.x>=u_border.z) {
    uv.x = v_uvraw.x*u_uvmod_tr.z + u_uvmod_tr.x;
} else {
    // tile it 
    uv.x = v_uvraw.x*u_tilerect.z + u_tilerect.x;
    uv.x = fract(uv.x);
    uv.x = uv.x*u_innertexrect.z + u_innertexrect.x;
}

// v
if (v_uvraw.y<=u_border.y) {
    uv.y = v_uvraw.y*u_uvmod_bl.w + u_uvmod_bl.y;
} else if (v_uvraw.y>=u_border.w) {
    uv.y = v_uvraw.y*u_uvmod_tr.w + u_uvmod_tr.y;
} else {
    // tile it 
    uv.y = v_uvraw.y*u_tilerect.w + u_tilerect.y;
    uv.y = fract(uv.y);
    uv.y = uv.y*u_innertexrect.w + u_innertexrect.y;
}
*/

const char RendererPrivateGL::shaderSrcVertexSlicing[] = R"shader(
        #version 100
        precision highp float;
        //start

        attribute vec2 a_pos;
        attribute vec4 a_color;
        attribute vec3 a_matrow0;
        attribute vec3 a_matrow1;
        
        varying vec4 v_color;
        varying vec2 v_uvraw;
        varying vec4 v_uvraw_uv; 
        varying vec4 v_uv_bltr;

        uniform vec4 u_uvmod_bl;
        uniform vec4 u_uvmod_tr;
        uniform vec4 u_tilerect;

        uniform vec3 u_camera;
        void main() {
            v_color = a_color;
            vec3 pos;
            pos.x = dot(a_matrow0,vec3(a_pos.x,a_pos.y,1.0));
            pos.y = dot(a_matrow1,vec3(a_pos.x,a_pos.y,1.0));
            pos.z = 1.0;
            pos = u_camera * pos;

            vec2 uv = a_pos*u_tilerect.zw + u_tilerect.xy;
            vec2 uv_bl = a_pos*u_uvmod_bl.zw + u_uvmod_bl.xy;
            vec2 uv_tr = a_pos*u_uvmod_tr.zw + u_uvmod_tr.xy;

            v_uvraw_uv = vec4(a_pos, uv);
            v_uv_bltr = vec4(uv_bl, uv_tr);

            gl_Position = vec4(pos.x,pos.y,0.0,1.0);
        }
    )shader";

const char RendererPrivateGL::shaderSrcFragmentSlicing[] = R"shader(
        #version 100
        precision highp float;
        //start

        uniform sampler2D mainTexture;

        varying vec4 v_color;
        varying vec4 v_uvraw_uv;
        varying vec4 v_uv_bltr;

        uniform vec4 u_border;
        uniform vec4 u_innertexrect;

        void main() {
            // 9-slice uv
            bvec4 pred = lessThanEqual(v_uvraw_uv.xyxy,u_border);
            vec2 uv = fract(v_uvraw_uv.zw)*u_innertexrect.zw + u_innertexrect.xy;
            uv = mix(uv,v_uv_bltr.xy,vec2(pred.xy)); 
            uv = mix(v_uv_bltr.zw,uv,vec2(pred.zw));

            // fetch
            vec4 c = texture2D(mainTexture, uv);
            c *= v_color;
            c.rgb *= v_color.a;
            gl_FragColor = c;
        }
    )shader";

void
RendererPrivateGL::addQuadVertices(const Color& color, const DisplayListEntry& de, const Sprite2D* s,
                                   Vertex* dest)
{
    /*
    // long form:
    Matrix4x4f m, mSize;
    mSize.SetScaleAndPosition(Vector3f(de->inBounds.width, de->inBounds.height,1.0f),
                              Vector3f(de->inBounds.x, de->inBounds.y,0.0f) );
    MultiplyMatrices4x4(&de->finalMatrix, &mSize, &m); */

    // build vertices, source size transform is added in here so vertex coordinates can be normalized
    Vertex v;
    v.matrix[0] = de.finalMatrix.Get(0, 0) * de.inBounds.width;
    v.matrix[1] = de.finalMatrix.Get(0, 1) * de.inBounds.height;
    v.matrix[2] =
        de.finalMatrix.Get(0, 0) * de.inBounds.x + de.finalMatrix.Get(0, 1) * de.inBounds.y + de.finalMatrix.Get(0, 3);
    v.matrix[3] = de.finalMatrix.Get(1, 0) * de.inBounds.width;
    v.matrix[4] = de.finalMatrix.Get(1, 1) * de.inBounds.height;
    v.matrix[5] =
        de.finalMatrix.Get(1, 0) * de.inBounds.x + de.finalMatrix.Get(1, 1) * de.inBounds.y + de.finalMatrix.Get(1, 3);
    v.rgba[0] = color.r;
    v.rgba[1] = color.g;
    v.rgba[2] = color.b;
    v.rgba[3] = color.a;
    v.texrect[0] = s->imageRegion.x;
    v.texrect[1] = s->imageRegion.y;
    v.texrect[2] = s->imageRegion.width;
    v.texrect[3] = s->imageRegion.height;

    // duplicate 4x, can be done nicer if we have different stream frequencies
    v.x = 0.0f; v.y = 0.0f;
    dest[0] = v;
    v.x = 1.0f; v.y = 0.0f;
    dest[1] = v;
    v.x = 1.0f; v.y = 1.0f;
    dest[2] = v;
    v.x = 0.0f; v.y = 1.0f;
    dest[3] = v;
}

void
RendererPrivateGL::addTriangleFanVertices(const Unity::Tiny::Core2D::Shape2DVertex* vertices, SolidVertex* dest, int count)
{
    for (int i = 0; i < count; i++)
    {
        dest[i].x = vertices[i].position.x;
        dest[i].y = vertices[i].position.y;
    }
}

void
RendererPrivateGL::addTextQuadVertices(const Text2DRenderer* textRenderer, const Matrix4x4f& m, const Rectf rect, const GlyphPrivate* glyph, const Color& color, Vertex* dest)
{
    Vertex v;
    v.matrix[0] = m.Get(0, 0) * rect.width;
    v.matrix[1] = m.Get(0, 1) * rect.height;
    v.matrix[2] = m.Get(0, 0) * rect.x + m.Get(0, 1) * rect.y + m.Get(0, 3);
    v.matrix[3] = m.Get(1, 0) * rect.width;
    v.matrix[4] = m.Get(1, 1) * rect.height;
    v.matrix[5] = m.Get(1, 0) * rect.x + m.Get(1, 1) * rect.y + m.Get(1, 3);

    v.rgba[0] = color.r;
    v.rgba[1] = color.g;
    v.rgba[2] = color.b;
    v.rgba[3] = color.a;

    if (glyph != nullptr)
    {
        CharacterInfo ci = glyph->ci;
        //Glyph uvs defined in the font texture atlas
        v.texrect[0] = ci.characterRegion.x;
        v.texrect[1] = ci.characterRegion.y;
        v.texrect[2] = ci.characterRegion.width;
        v.texrect[3] = ci.characterRegion.height;
    }
    else
    {
        v.texrect[0] = 0;
        v.texrect[1] = 0;
        v.texrect[2] = 1;
        v.texrect[3] = 1;
    }

    v.x = 0.0f;
    v.y = 0.0f;
    dest[0] = v;
    v.x = 1.0f;
    v.y = 0.0f;
    dest[1] = v;
    v.x = 1.0f;
    v.y = 1.0f;
    dest[2] = v;
    v.x = 0.0f;
    v.y = 1.0f;
    dest[3] = v;
}

