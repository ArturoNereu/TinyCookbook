#include <stdint.h>
#include <float.h>
#include <math.h>
#include <stdio.h>
#include <vector>

#include "HitBox2D.h"
#include "EntityWrappers.h"

using namespace Unity::Entities;

bool HitBox2DPrivate::CreateCheckEntry(EntityManager& man, CheckEntry& entry, DisplayListEntry& de) {

    entry.e = de.e;
    entry.usePixels = 0;
    entry.useShape = 0;
    entry.de = de;
   /* if (!man.exists(entry.e))
        return false;*/

    // grab bounds either from sprite or manually set
    if (man.hasComponent<Sprite2DRendererHitBox2D>(entry.e)) {
        const Sprite2DRendererHitBox2D* sbp = man.getComponentPtrConstUnsafe<Sprite2DRendererHitBox2D>(entry.e);
        Assert(man.hasComponent<Sprite2DRenderer>(entry.e));
        const Sprite2DRenderer* sp = man.getComponentPtrConstUnsafe<Sprite2DRenderer>(entry.e);
        // use sprite
#ifdef DEVELOPMENT
        if (man.hasComponent<RectHitBox2D>(entry.e))
            printf("%s has a Sprite2DRendererHitBox2D and a RectHitBox2D at the same time. Using the "
                "Sprite2DRendererHitBox2D.\n",
                man.formatEntity(entry.e).c_str());
#endif
        // take size from camera display list entry
        entry.useBounds = Rectf(de.inBounds.x, de.inBounds.y, de.inBounds.width, de.inBounds.height);

        // if pixel accurate, use sprite
        if (sbp->pixelAccurate == 1 && !sp->sprite.isNone()) {
            entry.usePixels = man.getComponentPtrUnsafe<Sprite2D>(sp->sprite);
        }
    }
    else {
        if (!man.hasComponent<RectHitBox2D>(entry.e))
            return false;
        RectHitBox2D* rhb = man.getComponentPtrUnsafe<RectHitBox2D>(entry.e);
        entry.useBounds = Rectf(rhb->box.x, rhb->box.y, rhb->box.width, rhb->box.height);
        if (man.hasComponent<Shape2DVertex>(entry.e)) {
            entry.useShape = (Shape2DVertex*)man.getBufferElementDataPtrUnsafe<Shape2DVertex>(entry.e);
            entry.useShapeLength = man.getBufferElementDataLength<Shape2DVertex>(entry.e);
        }
    }

    // transform bounds
    Matrix4x4f finalMatrix;
    std::memcpy(static_cast<void*>(&finalMatrix), static_cast<void*>(&de.finalMatrix), 16 * sizeof(float));

    entry.viewVerts[0] = finalMatrix.MultiplyPoint2(Vector2f(entry.useBounds.x, entry.useBounds.y));
    entry.viewVerts[1] = finalMatrix.MultiplyPoint2(Vector2f(entry.useBounds.x + entry.useBounds.width, entry.useBounds.y));
    entry.viewVerts[2] = finalMatrix.MultiplyPoint2(Vector2f(entry.useBounds.x + entry.useBounds.width, entry.useBounds.y + entry.useBounds.height));
    entry.viewVerts[3] = finalMatrix.MultiplyPoint2(Vector2f(entry.useBounds.x, entry.useBounds.y + entry.useBounds.height));

    if (finalMatrix.Get(0,0)*finalMatrix.Get(1,1) - finalMatrix.Get(0,1)*finalMatrix.Get(1,0) < 0.0f) {
        // flip order of vertices if the transform mirrors
        std::swap(entry.viewVerts[1], entry.viewVerts[2]);
        std::swap(entry.viewVerts[0], entry.viewVerts[3]);
    }

    // get axis aligned bounds
    entry.bbMin = entry.viewVerts[0];
    entry.bbMax = entry.viewVerts[0];
    for (int i = 1; i < 4; i++) {
        entry.bbMin.Min(entry.viewVerts[i]);
        entry.bbMax.Max(entry.viewVerts[i]);
    }
    if (!(entry.bbMax.x > entry.bbMin.x))
        return false;
    if (!(entry.bbMax.y > entry.bbMin.y))
        return false;

    return true;
}

inline bool HitBox2DPrivate::Side(const Vector2f& v0, const Vector2f& v1,
                        const Vector2f& p) // directed line between v0 and v1, check if p is on left or right
{
    return ((v1 - v0).Reflected()).Dot(p - v0) <= 0.0f;
}

bool HitBox2DPrivate::DoesSeparatingLineExist(const Vector2f* poly, int npoly, const Vector2f* points, int npoints)
{
    Assert(npoly >= 2);
    int iPrev = npoly - 1;
    int i, j;
    for (i = 0; i < npoly; i++) {
        for (j = 0; j < npoints; j++) {
            if (Side(poly[iPrev], poly[i], points[j]))
                goto notAllOut;
        }
        return true;
    notAllOut:
        iPrev = i;
    }
    return false;
}

inline bool
HitBox2DPrivate::BoundsContain(const CheckEntry& ce, const Vector2f& p)
{
    if (p.x < ce.bbMin.x || p.y < ce.bbMin.y)
        return false;
    if (p.x > ce.bbMax.x || p.y > ce.bbMax.y)
        return false;
    return true;
}

bool
HitBox2DPrivate::DoEntiresOverlap(CheckEntry& a, CheckEntry& b)
{
    // check bounds first, axis aligned separating planes
    if (a.bbMax.x <= b.bbMin.x)
        return false;
    if (a.bbMax.y <= b.bbMin.y)
        return false;
    if (a.bbMin.x >= b.bbMax.x)
        return false;
    if (a.bbMin.y >= b.bbMax.y)
        return false;
    // plane clip b vs a
    if (DoesSeparatingLineExist(a.viewVerts, 4, b.viewVerts, 4))
        return false;
    if (DoesSeparatingLineExist(b.viewVerts, 4, a.viewVerts, 4))
        return false;
    return true;
}

bool
HitBox2DPrivate::ReadyAlphaMask(EntityManager& em, CheckEntry& e, PixelMaskCheckEntry& eMask)
{
    if (!e.usePixels)
        return false;
    Assert(!e.useShape);
    Entity imagee = e.usePixels->image;
    if (imagee.isNone()) // todo: add warning
        return false;
    if (!em.hasComponent<Image2D>(imagee))
        return false;
    const Image2D* img = em.getComponentPtrConstUnsafe<Image2D>(imagee);
    if (img->status != ImageStatus::Loaded)
        return false;
    if (!em.hasComponent<Image2DAlphaMask>(imagee)) {
        printf("%s is set to pixel accurate hit testing, but the Image2D %s has no "
           "Image2DAlphaMask component.\n", em.formatEntity(e.e).c_str(), em.formatEntity(imagee).c_str() );
        return false;
    }
    if (!em.hasComponent<Image2DAlphaMaskData>(imagee)) {
        printf("%s is set to pixel accurate hit testing, but the Image2D %s has no "
           "Image2DAlphaMaskData component.\n", em.formatEntity(e.e).c_str(), em.formatEntity(imagee).c_str() );
        return false;
    }

    const Image2DAlphaMask* imgMask = em.getComponentPtrConstUnsafe<Image2DAlphaMask>(imagee);
    const uint8_t* bufferData = (uint8_t*)em.getBufferElementDataPtrConstUnsafe<Image2DAlphaMaskData>(imagee);
    int bufferLength = em.getBufferElementDataLength<Image2DAlphaMaskData>(imagee);
    if (bufferLength != 0) {
        eMask.imgSize = Vector2f(img->imagePixelSize.x, img->imagePixelSize.y);
        eMask.maskData = bufferData;
        Assert(bufferLength >= (int)(img->imagePixelSize.x * img->imagePixelSize.y));
        eMask.spriteRect = Rectf(e.usePixels->imageRegion.x, e.usePixels->imageRegion.y, e.usePixels->imageRegion.width, e.usePixels->imageRegion.height);
        eMask.threshold = imgMask->threshold;
        // could modulate by spriterenderer alpha here
        //TODO: Add formatEntity
        if (!(eMask.threshold > 0.0f && eMask.threshold < 1.0f)) {
            printf("%s Image2DAlphaMask component threshold out of range (%f, but should be "
                "]0..1[).\n",
                em.formatEntity(imagee).c_str(), eMask.threshold);
        }
       return true;
    } else {
        printf("%s is set to pixel accurate hit testing, but the Image2D entity %s has an empty "
            "Image2DAlphaMask component.\n"
            "Make sure the ImageBitMask is added next to the Image2D before loading.\n",
            em.formatEntity(e.e).c_str(), em.formatEntity(imagee).c_str());
    }
    return false;
}

inline uint8_t
HitBox2DPrivate::SampleValue(const Vector2f& uv, const SWTextureSource& texture)
{
    int ix = (int)uv.x;
    int iy = (int)uv.y;
    if ((unsigned int)ix >= (unsigned int)texture.w || // same as ix<0 || ix>=texture.w
        (unsigned int)iy >= (unsigned int)texture.h)   // same as iy<0 || iy>=texture.h
        return 0;
    return *(texture.mem + iy * texture.stride + ix);
}

inline bool
HitBox2DPrivate::SampleBinary(const Vector2f& uv, const SWTextureSource& texture)
{
    return SampleValue(uv, texture) >= texture.threshold;
}

inline int
HitBox2DPrivate::IFloor(float x)
{
    int i = int(x);
    if ((float)i > x)
        i--;
    //if ( i != (int)std::floor(x) )
    //    ut::log ( "bad floor: %f -> me = %i, std = %i\n", x, i, (int)std::floor(x));
    Assert(i == (int)std::floor(x));
    return i;
}

bool
HitBox2DPrivate::ScanLine(int y, float x0, Vector2f uv0, float x1, Vector2f uv1, SWViewport& target, const SWTextureSource& texture,
         bool check)
{
    if (x0 > x1) {
        std::swap(x0, x1);
        std::swap(uv0, uv1);
    }
    int ix0 = IFloor(x0 - target.x);
    int ix1 = IFloor(x1 - target.x);
    if (ix0 == ix1)
        return true;
    Vector2f duv = (uv1 - uv0) / (x1 - x0);
    Vector2f uv = uv0;
    uint8_t* dest = target.dest + y * target.w;
    if (ix1 > target.w)
        ix1 = target.w;
    if (ix0 < 0) {
        uv += duv * (float)(-ix0);
        ix0 = 0;
    }
    if (check) {
        for (int x = ix0; x < ix1; x++) {
            if (dest[x] && SampleBinary(uv, texture))
                return false;
            uv += duv;
        }
    } else {
        for (int x = ix0; x < ix1; x++) {
            dest[x] = SampleBinary(uv, texture) ? 1 : 0;
            uv += duv;
        }
    }
    return true;
}

bool
HitBox2DPrivate::StepEdge(Vector2f p0, Vector2f p1, Vector2f uv0, Vector2f uv1, SWViewport& target, const SWTextureSource& texture,
         bool check)
{
    // sort
    if (p0.y > p1.y) {
        std::swap(p0, p1);
        std::swap(uv0, uv1);
    }
    int iy0 = IFloor(p0.y - target.y);
    int iy1 = IFloor(p1.y - target.y);
    if (iy0 == iy1)
        return true;
    // step
    float invdy = 1.0f / (p1.y - p0.y);
    float x = p0.x;
    float dx = (p1.x - p0.x) * invdy;
    Vector2f uv = uv0;
    Vector2f duv = (uv1 - uv0) * invdy;

    if (iy1 > target.h)
        iy1 = target.h;
    if (iy0 < 0) {
        uv += duv * (float)(-iy0);
        x += dx * (float)(-iy0);
        iy0 = 0;
    }
    for (int y = iy0; y < iy1; y++) {
        if (target.edge[y].valid) {
            bool r = ScanLine(y, target.edge[y].x, target.edge[y].uv, x, uv, target, texture, check);
            if (check && !r)
                return false;
            target.edge[y].valid = false;
        } else {
            target.edge[y].valid = true;
            target.edge[y].x = x;
            target.edge[y].uv = uv;
        }
        uv += duv;
        x += dx;
    }
    return true;
}

bool
HitBox2DPrivate::RasterizeQuad(const Vector2f* vertices, SWViewport& target, const SWTextureSource& texture, float camScale, bool check)
{
    bool r = StepEdge(vertices[0] * camScale, vertices[1] * camScale, Vector2f(0, (float)texture.h),
                      Vector2f((float)texture.w, (float)texture.h), target, texture, check);
    if (check && !r)
        return false;
    r = StepEdge(vertices[1] * camScale, vertices[2] * camScale, Vector2f((float)texture.w, (float)texture.h),
                 Vector2f((float)texture.w, 0), target, texture, check);
    if (check && !r)
        return false;
    r = StepEdge(vertices[2] * camScale, vertices[3] * camScale, Vector2f((float)texture.w, 0), Vector2f(0, 0), target,
                 texture, check);
    if (check && !r)
        return false;
    r = StepEdge(vertices[3] * camScale, vertices[0] * camScale, Vector2f(0, 0), Vector2f(0, (float)texture.h), target,
                 texture, check);
    if (check && !r)
        return false;
    return true;
}

void
HitBox2DPrivate::IntersectBounds(const CheckEntry& ceA, const CheckEntry& ceB, Vector2f& bbMin, Vector2f& bbMax)
{
    bbMin = ceA.bbMin;
    bbMax = ceA.bbMax;
    bbMin.Max(ceB.bbMin);
    bbMax.Min(ceB.bbMax);
    //if (!(bbMin.x < bbMax.x) || !(bbMin.y < bbMax.y))
    //    ut::log("bad box: (%f,%f) - (%f,%f)\n", bbMin.x ,bbMin.y ,bbMax.x ,bbMax.y );
    Assert(bbMin.x < bbMax.x);
    Assert(bbMin.y < bbMax.y);
}

void
HitBox2DPrivate::InitTextureSource(SWTextureSource& tex, const PixelMaskCheckEntry& mask)
{
    tex.w = (int)(mask.imgSize.x * mask.spriteRect.width);
    tex.h = (int)(mask.imgSize.y * mask.spriteRect.height);
    tex.stride = (int)mask.imgSize.x;
    int y = (int)(mask.imgSize.y * mask.spriteRect.y);
    y = (int)(mask.imgSize.y) - (y + tex.h);
    tex.mem = mask.maskData + (int)(mask.imgSize.x * mask.spriteRect.x) + y * tex.stride;
    tex.threshold = (uint8_t)(mask.threshold * 255.0f);
}

bool
HitBox2DPrivate::CheckMaskMask(const CheckEntry& ceA, const PixelMaskCheckEntry& maskA, const CheckEntry& ceB,
              const PixelMaskCheckEntry& maskB, float camScale)
{
    Vector2f bbMin, bbMax;
    // UnionBounds (ceA,ceB,bbMin,bbMax);
    IntersectBounds(ceA, ceB, bbMin, bbMax);
    // rasterize both
    SWViewport vp;
    bbMin *= camScale;
    bbMax *= camScale;
    vp.w = (int)(bbMax.x - bbMin.x) + 2;
    vp.h = (int)(bbMax.y - bbMin.y) + 2;
    vp.x = (int)bbMin.x - 1;
    vp.y = (int)bbMin.y - 1;

    static std::vector<uint8_t> frameBuffer;
    static std::vector<SWViewport::EdgeEntry> edgeBuffer;
    // TODO: optimize me, this can be done much nicer without an edgebuffer by sorting input vertices
    //       and stepping left/right simultaneously

    frameBuffer.clear();
    frameBuffer.resize(vp.w * vp.h, 0);
    edgeBuffer.clear();
    edgeBuffer.resize(vp.h);

    vp.dest = frameBuffer.data();
    vp.edge = edgeBuffer.data();

    SWTextureSource texA;
    InitTextureSource(texA, maskA);
    SWTextureSource texB;
    InitTextureSource(texB, maskB);

    RasterizeQuad(ceA.viewVerts, vp, texA, camScale, false);
    // memset(vp.edge,0,sizeof(SWViewport::EdgeEntry)*vp.h); // should not be needed
    return !RasterizeQuad(ceB.viewVerts, vp, texB, camScale, true);
}

void
HitBox2DPrivate::TransformShapeVerticesToWorld(EntityManager& em, CheckEntry& ce, std::vector<Vector2f>& dest)
{
    Assert(ce.useShape);
    
    Shape2DVertex* v = ce.useShape;
    dest.resize(ce.useShapeLength);
    for (int i = 0; i < ce.useShapeLength; i++)
    {
        Vector2f res;
        res.x = ce.de.finalMatrix.Get(0,0) * (*(v + i)).position.x + ce.de.finalMatrix.Get(1,0) * (*(v + i)).position.y + ce.de.finalMatrix.Get(3,0);
        res.y = ce.de.finalMatrix.Get(0,1) * (*(v + i)).position.x + ce.de.finalMatrix.Get(1,1) * (*(v + i)).position.y + ce.de.finalMatrix.Get(3,1);
        dest[i] = res;
    }
}

bool
HitBox2DPrivate::OverlapCheckPolygons(const Vector2f* polyA, int nA, const Vector2f* polyB, int nB)
{
    if (DoesSeparatingLineExist(polyA, nA, polyB, nB))
        return false;
    if (DoesSeparatingLineExist(polyB, nB, polyA, nA))
        return false;
    return true;
}

bool
HitBox2DPrivate::AccurateOverlapCheckBothShapes(EntityManager& em, CheckEntry& ceA, CheckEntry& ceB)
{
    std::vector<Vector2f> vertsA;
    std::vector<Vector2f> vertsB;
    TransformShapeVerticesToWorld(em, ceA, vertsA);
    TransformShapeVerticesToWorld(em, ceB, vertsB);
    return OverlapCheckPolygons(vertsA.data(), (int)vertsA.size(), vertsB.data(), (int)vertsB.size());
}

bool
HitBox2DPrivate::AccurateOverlapCheckOneShape(EntityManager& em, CheckEntry& ceShape, CheckEntry& ceSolid)
{
    std::vector<Vector2f> verts;
    TransformShapeVerticesToWorld(em, ceShape, verts);
    return OverlapCheckPolygons(verts.data(), (int)verts.size(), ceSolid.viewVerts, 4);
}

bool
HitBox2DPrivate::PixelAccurateOverlapCheckBothPixels(EntityManager& man, CheckEntry& ceA, CheckEntry& ceB, float camScale)
{
    PixelMaskCheckEntry maskA;
    if (!ReadyAlphaMask(man, ceA, maskA))
        return false;
    PixelMaskCheckEntry maskB;
    if (!ReadyAlphaMask(man, ceB, maskB))
        return false;
    return CheckMaskMask(ceA, maskA, ceB, maskB, camScale);
}

bool
HitBox2DPrivate::PixelAccurateOverlapCheckOnePixels(EntityManager& man, CheckEntry& cePixels, CheckEntry& ceSolid, float camScale)
{
    PixelMaskCheckEntry maskPixels;
    if (!ReadyAlphaMask(man, cePixels, maskPixels))
        return false;
    PixelMaskCheckEntry maskSolid;
    maskSolid.imgSize = Vector2f(1, 1);
    uint8_t solidMask = 0xff;
    maskSolid.maskData = &solidMask;
    maskSolid.spriteRect = Rectf(0, 0, 1, 1);
    return CheckMaskMask(cePixels, maskPixels, ceSolid, maskSolid, camScale);
}

bool
HitBox2DPrivate::PixelAccurateOverlapCheck(EntityManager& em, CheckEntry& cei, CheckEntry& cej, float camScale)
{
    if (cei.useShape) {
        if (cej.useShape)
            return AccurateOverlapCheckBothShapes(em, cei, cej);
        else if (!AccurateOverlapCheckOneShape(em, cei, cej))
            return false;
    } else {
        if (cej.useShape)
            if (!AccurateOverlapCheckOneShape(em, cej, cei))
                return false;
    }
    if (cei.usePixels) {
        if (cej.usePixels)
            return PixelAccurateOverlapCheckBothPixels(em, cei, cej, camScale);
        else
            return PixelAccurateOverlapCheckOnePixels(em, cei, cej, camScale);
    } else {
        if (!cej.usePixels)
            return true; // none use pixels, so overlap is true
        else
            return PixelAccurateOverlapCheckOnePixels(em, cej, cei, camScale);
    }
}

void
HitBox2DPrivate::CompareAndUpdatePair(EntityManager& em, CheckEntry& cei, CheckEntry& cej, Entity came, float camScale)
{
    if (DoEntiresOverlap(cei, cej)) {
        if (PixelAccurateOverlapCheck(em, cei, cej, camScale)) {
            // add results to both entities
            HitBoxOverlap hboij;
            hboij.otherEntity = cej.e;
            hboij.camera = came;
            HitBoxOverlap hboji; 
            hboji.otherEntity = cei.e;
            hboji.camera = came;
            flatLocalResultsE.push_back(cei.e);
            flatLocalResultsHBO.push_back(hboij);
            flatLocalResultsE.push_back(cej.e);
            flatLocalResultsHBO.push_back(hboji);
        }
    }
}

bool
HitBox2DPrivate::FineHit(EntityManager& man, CheckEntry& ce, const Vector2f& p, float& z, Vector2f& uv)
{
    if (!Side(ce.viewVerts[0], ce.viewVerts[1], p))
        return false;
    if (!Side(ce.viewVerts[1], ce.viewVerts[2], p))
        return false;
    if (!Side(ce.viewVerts[2], ce.viewVerts[3], p))
        return false;
    if (!Side(ce.viewVerts[3], ce.viewVerts[0], p))
        return false;

    Matrix4x4f finalMatrix;
    std::memcpy(static_cast<void*>(&finalMatrix), static_cast<void*>(&ce.de.finalMatrix), 16 * sizeof(float));
    // inverse transform
    Matrix4x4f invMat;
    Matrix4x4f::Invert_General3D(finalMatrix, invMat);
    Vector2f pLocal = invMat.MultiplyPoint2(p);
    uv.x = (pLocal.x - ce.useBounds.x) / ce.useBounds.width;
    uv.y = (pLocal.y - ce.useBounds.y) / ce.useBounds.height;
    Assert(uv.x >= -0.5f && uv.x <= 1.5f);
    Assert(uv.y >= -0.5f && uv.y <= 1.5f);
    uv.x = clamp(uv.x, 0.0f, 1.0f);
    uv.y = clamp(uv.y, 0.0f, 1.0f);
    z = ce.de.finalMatrix.Get(2, 3);

    // Pixel accurate?
    if (ce.usePixels) {
        PixelMaskCheckEntry mask;
        ReadyAlphaMask(man, ce, mask);
        SWTextureSource swtex;
        InitTextureSource(swtex, mask);
        Vector2f uvtex;
        uvtex.x = uv.x * (float)swtex.w;
        uvtex.y = (1.0f - uv.y) * (float)swtex.h;
        return SampleBinary(uvtex, swtex);
    }
    // Shape?
    if (ce.useShape) {
        int iPrev = ce.useShapeLength - 1;

        for (int i = 0; i < ce.useShapeLength; i++) {
            Shape2DVertex v = *(ce.useShape + i);
            Shape2DVertex vPrev = *(ce.useShape + iPrev);
            if (!Side(Vector2f(vPrev.position.x, vPrev.position.y),Vector2f(v.position.x, v.position.y), p))
                return false;
        }
    }
    return true;
}

bool HitBox2DPrivate::CoarseCanRayIntersect(const CheckEntry& ce, const Vector2f& rayStart, const Vector2f& rayEnd)
{
    Vector2f rayp[2] = {rayStart, rayEnd};
    if (DoesSeparatingLineExist(rayp, 2, ce.viewVerts, 4)) // could be a better check, tis tests both sides of the line
        return false;
    if (DoesSeparatingLineExist(ce.viewVerts, 4, rayp, 2))
        return false;
    return true;
}

Vector2f HitBox2DPrivate::IntersectLines(const Vector2f& p1, const Vector2f& p2, const Vector2f& p3, const Vector2f& p4)
{
    Vector2f d12 = p1 - p2;
    Vector2f d34 = p3 - p4;
    float det = d12.Cross(d34);
    if (std::abs(det) <= 0.0000001f)
        return (p1 + p2) * .5f;
    det = 1.0f / det;
    return (d34 * p1.Cross(p2) - d12 * p3.Cross(p4)) * det;
}

bool HitBox2DPrivate::IntersectLine(const CheckEntry& ce, const Vector2f& rayStart, const Vector2f& rayEnd, float& t) // t in/out: max t in, result out if closer
{
    // intersect with bounds rect first
    // TODO can be much more efficient
    int iPrev = 3;
    bool found = false;
    for (int i = 0; i < 4; i++) {
        if (CheckDoLinesIntersect(rayStart, rayEnd, ce.viewVerts[iPrev], ce.viewVerts[i])) {
            float tInter = IntersectLinesT(rayStart, rayEnd, ce.viewVerts[iPrev], ce.viewVerts[i]);
            if (tInter >= 0.0f && tInter < t) {
                t = tInter;
                found = true;
            }
        }
        iPrev = i;
    }
    return found;
}

bool HitBox2DPrivate::CheckDoLinesIntersect(const Vector2f& p1, const Vector2f& p2, const Vector2f& p3, const Vector2f& p4)
{
    if (Side(p1, p2, p3) == Side(p1, p2, p4))
        return false;
    if (Side(p3, p4, p1) == Side(p3, p4, p2))
        return false;
    return true;
}

float HitBox2DPrivate::IntersectLinesT(const Vector2f& p1, const Vector2f& p2, const Vector2f& p3, const Vector2f& p4)
{
    // TODO: can do better (less) here
    Vector2f p = IntersectLines(p1, p2, p3, p4);
    Vector2f d21 = p2 - p1;
    if (std::abs(d21.x) > std::abs(d21.y))
        return (p.x - p1.x) / d21.x;
    else
        return (p.y - p1.y) / d21.y;
}

void
HitBox2DPrivate::ClipPolygonWithLine(std::vector<Vector2f>& vertices, const Vector2f& p0, const Vector2f& p1)
{
    if (vertices.empty())
        return;
    std::vector<Vector2f> outvertices;
    int nIn = (int)vertices.size();
    bool iOutside = Side(p0, p1, vertices[0]) == 0;
    Vector2f vect;

    for (int i = 0; i < nIn; i++) {
        int iNext = i + 1;
        if (iNext == nIn)
            iNext = 0;

        bool iNextOutside = Side(p0, p1, vertices[iNext]) == 0;
        if (iOutside && iNextOutside) // both outside
            goto nextPoint;
            
        if (!iOutside) {
            outvertices.push_back(vertices[i]);
            if (!iNextOutside) // both inside
                goto nextPoint;
        }
        // clip
        vect = IntersectLines(p0, p1, vertices[i], vertices[iNext]);
        outvertices.push_back(vect);
        
    nextPoint:
        iOutside = iNextOutside;
    }
    vertices = std::move(outvertices);
}

// HitBox services entry points
int HitBox2DPrivate::GetSweepResults(Entity **e, HitBoxOverlap** outPtr) {
    // flatten
    *e = flatLocalResultsE.data();
    *outPtr = flatLocalResultsHBO.data();
    return (int)flatLocalResultsE.size();
}

void HitBox2DPrivate::sweepLine(EntityManager& em, Entity entity, Camera2D cam) {
    flatLocalResultsE.clear();
    flatLocalResultsHBO.clear();
    const DisplayListEntry* dlBuffer = (DisplayListEntry*)em.getBufferElementDataPtrConstUnsafe<DisplayListEntry>(entity);
    int dlBufferLength = em.getBufferElementDataLength<DisplayListEntry>(entity);
    const float pixelCheckResolution = 100.0f;
    float camScale = pixelCheckResolution / cam.halfVerticalSize;
    // sweep line
    std::vector<CheckEntry> list;
    list.reserve(dlBufferLength);
    // grab the ones that have
    for (int i = 0; i < dlBufferLength; i++) {
        CheckEntry ce;
        DisplayListEntry l = *(dlBuffer + i);
        if (CreateCheckEntry(em, ce, l)) {
            list.push_back(ce);
        }
    }
    if (list.empty())
        return;
    std::vector<SweepEvent> sweepEvents;
    sweepEvents.reserve(list.size() * 2);
    for (int i = 0; i < (int)list.size(); i++) {
        SweepEvent eAdd = {list[i].bbMin, i, true};
        sweepEvents.push_back(eAdd);
        SweepEvent eRemove = {list[i].bbMax, i, false};
        sweepEvents.push_back(eRemove);
    }
    std::sort(sweepEvents.begin(), sweepEvents.end());
    std::vector<int> activeList;
    activeList.reserve(list.size());    
    while (!sweepEvents.empty()) {
        SweepEvent& e = sweepEvents.back();
        if (e.addEvent) {
            // compare new element to all the others in active list
            for (int i = 0; i < (int)activeList.size(); i++)
                CompareAndUpdatePair(em, list[e.checkEntryIdx], list[activeList[i]], entity, camScale);
            // add it to active list
            activeList.push_back(e.checkEntryIdx);
        } else {
            // remove it from active list (linear scan for now)
            for (int i = 0; i < (int)activeList.size(); i++) {
                if (activeList[i] == e.checkEntryIdx) {
                    activeList[i] = activeList.back();
                    activeList.pop_back();
                    break;
                }
            }
        }
        sweepEvents.pop_back();
    }
    Assert(activeList.empty());
}

void HitBox2DPrivate::hitTest(EntityManager& man, float3 hitPoint, Entity camera, HitTestResult* result) {
    const DisplayListCamera* dlc = man.getComponentPtrConstUnsafe<DisplayListCamera>(camera);
    //Convert Unity::Mathematics::float4x4 to Matric4x4f, TODO: if we keep Tiny.Math in c++ we need to factorize this code somewhere
    float data[16] = { dlc->inverseWorld.Get(0,0), dlc->inverseWorld.Get(1,0), dlc->inverseWorld.Get(2,0), dlc->inverseWorld.Get(3,0),
        dlc->inverseWorld.Get(0,1), dlc->inverseWorld.Get(1,1), dlc->inverseWorld.Get(2,1), dlc->inverseWorld.Get(3,1),
        dlc->inverseWorld.Get(0,2), dlc->inverseWorld.Get(1,2), dlc->inverseWorld.Get(2,2), dlc->inverseWorld.Get(3,2),
        dlc->inverseWorld.Get(0,3), dlc->inverseWorld.Get(1,3), dlc->inverseWorld.Get(2,3), dlc->inverseWorld.Get(3,3) };
    Matrix4x4f matrix(data);

    const Vector3f vectHitPoint = Vector3f(hitPoint.x, hitPoint.y, hitPoint.z);
    Vector3f hitPointView;

    matrix.MultiplyPoint3(vectHitPoint, hitPointView);
    const Vector2f hitPointView2(hitPointView.x, hitPointView.y);

    const DisplayListEntry* dlBuffer = (DisplayListEntry*)man.getBufferElementDataPtrConstUnsafe<DisplayListEntry>(camera);
    int dlBufferLength = man.getBufferElementDataLength<DisplayListEntry>(camera);

    float bestZ = 0;
    for (int i = 0; i < dlBufferLength; i++) {
        CheckEntry ce;
        DisplayListEntry l = *(dlBuffer + i);
        if (!CreateCheckEntry(man, ce, l))
            continue;
        if (!BoundsContain(ce, hitPointView2))
            continue;
        Vector2f uv;
        float z = 0.0f;
        if (!FineHit(man, ce, hitPointView2, z, uv))
            continue;
        if (result->entityHit.isNone()|| z < bestZ) {
            bestZ = z;
            result->uv.x = uv.x;
            result->uv.y = uv.y;
            result->entityHit = ce.e;
        }
    }
}

void HitBox2DPrivate::rayCast(EntityManager& man, const float3& startPoint, const float3& endPoint, Entity camera, RayCastResult* result)
{
    // result{Entity::NONE, 1.0f};
    Vector2f start2(startPoint.x, startPoint.y);
    Vector2f end2(endPoint.x, endPoint.y);
    if (!man.hasComponent<DisplayListCamera>(camera))
        return;
    const DisplayListEntry* dlBuffer = (DisplayListEntry*)man.getBufferElementDataPtrConstUnsafe<DisplayListEntry>(camera);
    int dlBufferLength = man.getBufferElementDataLength<DisplayListEntry>(camera);

    for (int i = 0; i < dlBufferLength; i++) {
        CheckEntry ce;
        DisplayListEntry l = *(dlBuffer + i);
        if (!CreateCheckEntry(man, ce, l))
            continue;
        if (!CoarseCanRayIntersect(ce, start2, end2))
            continue;
        if (IntersectLine(ce, start2, end2, result->t))
            result->entityHit = l.e;
    }
}


int
HitBox2DPrivate::detailedOverlapInformation(EntityManager& man, Entity e, const HitBoxOverlap& overlap, float2* result)
{
    std::vector<Vector2f> vertices;
    // gather all the components again
    // find entity in camera's dlc
    const DisplayListEntry* dlBuffer = (DisplayListEntry*)man.getBufferElementDataPtrConstUnsafe<DisplayListEntry>(overlap.camera);
    int dlBufferLength = man.getBufferElementDataLength<DisplayListEntry>(overlap.camera);
    CheckEntry ceA, ceB;
    int k = 0;
    for (int i = 0; i < dlBufferLength; i++) {
        DisplayListEntry l = *(dlBuffer + i);
        if (l.e == e) {
            if (!CreateCheckEntry(man, ceA, l))
                return 0;
            k++;
            if (k == 2)
                break;
        } else if (l.e == overlap.otherEntity) {
            if (!CreateCheckEntry(man, ceB, l))
                return 0;
            k++;
            if (k == 2)
                break;
        }
    }
    if (k != 2) {
        Assert(0);
        return 0;
    }

    int maxSize = 4;
    vertices.clear();
    vertices.resize(maxSize);
    // clip B to A
    for (int i = 0; i < maxSize; i++)
    {
        vertices[i] = ceA.viewVerts[i];
    }

    ClipPolygonWithLine(vertices, ceB.viewVerts[0], ceB.viewVerts[1]);
    ClipPolygonWithLine(vertices, ceB.viewVerts[1], ceB.viewVerts[2]);
    ClipPolygonWithLine(vertices, ceB.viewVerts[2], ceB.viewVerts[3]);
    ClipPolygonWithLine(vertices, ceB.viewVerts[3], ceB.viewVerts[0]);
    // clip vertices are in view space now, we should return them in e's space though
    Matrix4x4f finalMatrix;
    std::memcpy(static_cast<void*>(&finalMatrix), static_cast<void*>(&ceA.de.finalMatrix), 16 * sizeof(float));

    Matrix4x4f invMat;
    Matrix4x4f::Invert_General3D(finalMatrix, invMat);

    for (int i = 0; i < (int)vertices.size(); i++)
    {
       Vector2f t = invMat.MultiplyPoint2(vertices[i]);
       result[i].x = t.x;
       result[i].y = t.y;
    }
    return (int)vertices.size();
}

void
HitBox2DPrivate::init()
{
    if(!initialized)
    {
        InitComponentId<DisplayListCamera>();
        InitComponentId<DisplayListEntry>();
        InitComponentId<HitBoxOverlap>();
        InitComponentId<Sprite2DRendererHitBox2D>();
        InitComponentId<Sprite2DRenderer>();
        InitComponentId<Sprite2D>();
        InitComponentId<RectHitBox2D>();
        InitComponentId<Image2DAlphaMask>();
        InitComponentId<Image2DAlphaMaskData>();
        InitComponentId<Image2D>();
        InitComponentId<Shape2DVertex>();
        initialized = true;
    }
}

void HitBox2DPrivate::deInit()
{
    initialized = false;
}

static HitBox2DPrivate sInst;

ZEROPLAYER_EXPORT
void ZEROPLAYER_CALL init_hitbox2d()
{
    sInst.init();
}

ZEROPLAYER_EXPORT
void ZEROPLAYER_CALL deinit_hitbox2d()
{
    sInst.deInit();
}

ZEROPLAYER_EXPORT
int ZEROPLAYER_CALL sweepresults_hitbox2d(Entity **outE, HitBoxOverlap **outHBO)
{
    return sInst.GetSweepResults(outE, outHBO);
}

ZEROPLAYER_EXPORT
void ZEROPLAYER_CALL sweepline_hitbox2d(void *emHandle, Entity entity, Camera2D cam)
{
    EntityManager em(emHandle);
    sInst.sweepLine(em, entity, cam);
}

ZEROPLAYER_EXPORT
void ZEROPLAYER_CALL hittest_hitbox2d(void *emHandle, float3 hitPoint, Entity camera, HitTestResult* result)
{
    EntityManager em(emHandle);
    sInst.hitTest(em, hitPoint, camera, result);
}

ZEROPLAYER_EXPORT
void ZEROPLAYER_CALL raycast_hitbox2d(void *emHandle, float3 startPoint, float3 endPoint, Entity camera, RayCastResult* result)
{
    EntityManager em(emHandle);
    sInst.rayCast(em, startPoint, endPoint, camera, result);
}

ZEROPLAYER_EXPORT
int ZEROPLAYER_CALL detailedOverlapInformation_hitbox2d(void *emHandle, Entity e, HitBoxOverlap overlap, float2* result)
{
    EntityManager em(emHandle);
    return sInst.detailedOverlapInformation(em, e, overlap, result);
}

