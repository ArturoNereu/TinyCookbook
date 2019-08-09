#include <stdint.h>
#include <vector>

#include "bind-Unity_Tiny_Core2D.h"
#include "bind-Unity_Tiny_Image2D.h"
#include "bind-Unity_Tiny_Sprite2D.h"
#include "bind-Unity_Tiny_Shape2D.h"
#include "bind-Unity_Tiny_HitBox2D.h"

#include "zeroplayer.h"
#include "GeminiMath.h"
#include "EntityWrappers.h"
#include <cstring>

using namespace Unity::Entities;
using namespace Unity::Tiny::Core2D;
using namespace Unity::Tiny::HitBox2D;
using namespace Unity::Mathematics;
using namespace Unity::Tiny::Math;

class HitBox2DPrivate
{
public:

bool initialized;
std::vector<Entity> flatLocalResultsE;
std::vector<HitBoxOverlap> flatLocalResultsHBO; 

struct EntityHasher {
    size_t operator()(const Entity& e) const { return e.index; }
};

struct CheckEntry
{
    Entity e;
    Vector2f viewVerts[4]; // view bounds, not aligned
    Vector2f bbMin, bbMax; // view bounds, axis aligned
    Sprite2D* usePixels;
    Shape2DVertex* useShape; // DynamicBuffer<Shape2DVertex>
    int useShapeLength;
    DisplayListEntry de; // display list entry
    Rectf useBounds;     // object bounds
};

struct SweepEvent
{
    Vector2f p;
    int checkEntryIdx;
    bool addEvent;

    bool operator<(const SweepEvent& r) const
    {
        // sort "wrong way" so we can pop events from a vector in the right order
        if (p.x == r.p.x) {
            return p.y > r.p.y;
        } else {
            return p.x > r.p.x;
        }
    };
};

struct PixelMaskCheckEntry
{
    Rectf spriteRect;
    Vector2f imgSize;
    const uint8_t* maskData;
    float threshold;
};

struct SWTextureSource
{
    int w, h;
    int stride;
    const uint8_t* mem;
    uint8_t threshold;
};

struct SWViewport
{
    struct EdgeEntry
    {
        EdgeEntry()
            : uv(0.0f, 0.0f)
            , x(0.0f)
            , valid(false){};
        Vector2f uv;
        float x;
        bool valid;
    } * edge;
    uint8_t* dest;
    int w, h;
    int x, y;
};

void init();
void deInit();
void sweepLine(EntityManager& em, Entity entity, Camera2D cam);
int GetSweepResults(Entity **e, HitBoxOverlap** outPtr);
void hitTest(EntityManager& man, float3 hitPoint, Entity camera, HitTestResult* result);
void rayCast(EntityManager& man, const float3& startPoint, const float3& endPoint, Entity camera, RayCastResult* result);
int detailedOverlapInformation(EntityManager& man, Entity e, const HitBoxOverlap& overlap, float2* result);

void ClipPolygonWithLine(std::vector<Vector2f>& vertices, const Vector2f& p0, const Vector2f& p1);
bool IntersectLine(const CheckEntry& ce, const Vector2f& rayStart, const Vector2f& rayEnd, float& t);
float IntersectLinesT(const Vector2f& p1, const Vector2f& p2, const Vector2f& p3, const Vector2f& p4);
Vector2f IntersectLines(const Vector2f& p1, const Vector2f& p2, const Vector2f& p3, const Vector2f& p4);
bool CheckDoLinesIntersect(const Vector2f& p1, const Vector2f& p2, const Vector2f& p3, const Vector2f& p4);
bool CoarseCanRayIntersect(const CheckEntry& ce, const Vector2f& rayStart, const Vector2f& rayEnd);
bool CreateCheckEntry(EntityManager& man, CheckEntry& entry, DisplayListEntry& de);
inline bool Side(const Vector2f& v0, const Vector2f& v1, const Vector2f& p);
bool FineHit(EntityManager& man, CheckEntry& ce, const Vector2f& p, float& z, Vector2f& uv);
bool DoesSeparatingLineExist(const Vector2f* poly, int npoly, const Vector2f* points, int npoints);
inline bool BoundsContain(const CheckEntry& ce, const Vector2f& p);
bool DoEntiresOverlap(CheckEntry& a, CheckEntry& b);
bool ReadyAlphaMask(EntityManager& em, CheckEntry& e, PixelMaskCheckEntry& eMask);
void InitTextureSource(SWTextureSource& tex, const PixelMaskCheckEntry& mask);
inline bool SampleBinary(const Vector2f& uv, const SWTextureSource& texture);
inline uint8_t SampleValue(const Vector2f& uv, const SWTextureSource& texture);
inline int IFloor(float x);
bool ScanLine(int y, float x0, Vector2f uv0, float x1, Vector2f uv1, SWViewport& target, const SWTextureSource& texture,
         bool check);
bool StepEdge(Vector2f p0, Vector2f p1, Vector2f uv0, Vector2f uv1, SWViewport& target, const SWTextureSource& texture,
         bool check);
bool RasterizeQuad(const Vector2f* vertices, SWViewport& target, const SWTextureSource& texture, float camScale, bool check);
void IntersectBounds(const CheckEntry& ceA, const CheckEntry& ceB, Vector2f& bbMin, Vector2f& bbMax);

bool CheckMaskMask(const CheckEntry& ceA, const PixelMaskCheckEntry& maskA, const CheckEntry& ceB,
              const PixelMaskCheckEntry& maskB, float camScale);
void TransformShapeVerticesToWorld(EntityManager& em, CheckEntry& ce, std::vector<Vector2f>& dest);
bool OverlapCheckPolygons(const Vector2f* polyA, int nA, const Vector2f* polyB, int nB);
bool AccurateOverlapCheckBothShapes(EntityManager& em, CheckEntry& ceA, CheckEntry& ceB);
bool AccurateOverlapCheckOneShape(EntityManager& em, CheckEntry& ceShape, CheckEntry& ceSolid);
bool PixelAccurateOverlapCheckBothPixels(EntityManager& man, CheckEntry& ceA, CheckEntry& ceB, float camScale);
bool PixelAccurateOverlapCheckOnePixels(EntityManager& man, CheckEntry& cePixels, CheckEntry& ceSolid, float camScale);
bool PixelAccurateOverlapCheck(EntityManager& em, CheckEntry& cei, CheckEntry& cej, float camScale);
void CompareAndUpdatePair(EntityManager& em, CheckEntry& cei, CheckEntry& cej, Entity came, float camScale);
};

