#pragma once

#include "zeroplayer.h"
#include "GeminiMath.h"

#include "EntityWrappers.h"

#include "bind-Unity_Tiny_Core2D.h"
#include "bind-Unity_Tiny_Image2D.h"
#include "bind-Unity_Tiny_Sprite2D.h"
#include "bind-Unity_Tiny_Shape2D.h"
#include "bind-Unity_Tiny_Text.h"

#if defined(BUILD_UNITY_TINY_RENDERERGL)
#define RENDERER_GL_EXPORT DLLEXPORT
#else
#define RENDERER_GL_EXPORT DLLIMPORT
#endif

namespace ut {
namespace Core2D {

// base class that contains common code for opengl based renderers
class RENDERER_GL_EXPORT RendererPrivateGL {
public:
    class ShaderProgram {
    public:
        ShaderProgram()
            : ShaderProgram(0)
        {
        }
        explicit ShaderProgram(int pgm)
            : pgm_(pgm)
        {
        }
        int id() const { return pgm_; }

    protected:
        int pgm_;
    };

    // TODO FIXME is now defined in c# and c++ needs some :bee: magic to be passed into both sides 
    static const int sMaxBatchSize = 4096;
    static const int sMaxSolidVertices = 0xffff;
    static const int sMaxSolidIndices = sMaxSolidVertices*6;

protected:
    struct /*alignas(32)*/ Vertex
    {
        float x, y;
        float rgba[4]; // Core2D::Color
        float matrix[3 * 2];
        float texrect[4]; // Core2D::Rectf
    };

    struct SolidVertex
    {
        float x, y;
    };

    struct /*alignas(32)*/ TilemapVertex
    {
        float x, y;
        float u, v;
        float tileidx;
    };

    Vertex vertexBuffer[sMaxBatchSize * 4];
    SolidVertex solidVertexBuffer[sMaxSolidVertices];
    TilemapVertex tilemapVertexBuffer[sMaxBatchSize * 4];

    struct Slice9Constants
    {
        Vector4f border;
        Vector4f uvmod_bl;
        Vector4f uvmod_tr;
        Vector4f tilerect;
        Vector4f innertexrect;
    };

    static void computeSlice9Constants(const Unity::Tiny::Core2D::Image2D* image, const Unity::Tiny::Core2D::Sprite2DBorder* border,
                                       const Unity::Tiny::Core2D::Sprite2D* sprite, const Unity::Tiny::Core2D::DisplayListEntry& de,
                                       const Unity::Tiny::Core2D::Sprite2DRendererOptions* til, Slice9Constants& out);
    static void computeTileConstants(const Unity::Tiny::Core2D::Image2D* image, const Unity::Tiny::Core2D::Sprite2D* sprite,
                                     const Unity::Tiny::Core2D::DisplayListEntry& de, const Unity::Tiny::Core2D::Sprite2DRendererOptions* til,
                                     Vector4f& out);

    static const char shaderSrcVertex[];
    static const char shaderSrcSolidVertex[];
    static const char shaderSrcFragment[];
    static const char shaderSrcSolidFragment[];
    static const char shaderSrcFragmentTiling[];
    static const char shaderSrcFragmentSlicing[];
    static const char shaderSrcVertexSlicing[];
    static const char shaderSrcVertexTilemap[];

    // TODO: add back in text & tilemap helper functions once modules are ready

    //void addTileVertices(const Unity::Tiny::Tilemap2D::TilemapChunkTilePrivate& tile, TilemapVertex* dest);
    void addTextQuadVertices(const Unity::Tiny::Text::Text2DRenderer* textRenderer, const Matrix4x4f& de, const Rectf rect,
                             const Unity::Tiny::Text::GlyphPrivate* glyph, const Unity::Tiny::Core2D::Color& color, Vertex* dest);

    void addQuadVertices(const Unity::Tiny::Core2D::Color& color, const Unity::Tiny::Core2D::DisplayListEntry& de,
                         const Unity::Tiny::Core2D::Sprite2D* s, Vertex* dest);

    void addTriangleFanVertices(const Unity::Tiny::Core2D::Shape2DVertex* vertices, SolidVertex* dest, int count);

    // Shader support
    struct BasicShader : public ShaderProgram
    {
        int u_camera;
        BasicShader()
            : ShaderProgram()
        {
        }
        BasicShader(ShaderProgram const& p);
    };

    struct SolidShapeShader : public BasicShader
    {
        int u_color;
        int u_trMatrix;
        
        SolidShapeShader()
            : BasicShader()
        {
        }
        SolidShapeShader(ShaderProgram const& p);
    };

    struct TilingShader : public BasicShader
    {
        int u_tilerect;
        TilingShader()
            : BasicShader()
        {
        }
        TilingShader(ShaderProgram const& p);
    };

    struct SlicingShader : public BasicShader
    {
        int u_border, u_uvmod_bl, u_uvmod_tr, u_tilerect, u_innertexrect;
        SlicingShader()
            : BasicShader()
        {
        }
        SlicingShader(ShaderProgram const& p);
    };

    struct TilemapShader : public BasicShader
    {
        int u_spriterects;
        int u_spritecolors;
        int u_spritepivots;
        int u_mapcolor;

        int u_tilematrix;
        int u_objtoworld;

        int u_anchor;
        int u_cellspacing;
        int u_cellsize;

        TilemapShader()
            : BasicShader()
        {
        }
        TilemapShader(ShaderProgram const& p);
    };
};

} // namespace Core2D
} // namespace ut
