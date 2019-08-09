#include "GeminiMath.h"

using namespace ut;
using namespace std;

RectInt
ut::Math::RoundRectToInt(const Rectf& rect)
{
    int left = (int)floor(rect.x + 0.5f);
    int top = (int)floor(rect.y + 0.5f);
    int right = (int)floor(rect.GetRight() + 0.5f);
    int bottom = (int)floor(rect.GetBottom() + 0.5f);

    return RectInt(left, top, right-left, bottom-top);
}

Rectf
ut::Math::RoundRect(const Rectf& rect)
{
    float left = floor(rect.x + 0.5f);
    float top = floor(rect.y + 0.5f);
    float right = floor(rect.GetRight() + 0.5f);
    float bottom = floor(rect.GetBottom() + 0.5f);

    return Rectf(left, top, right-left, bottom-top);
}

RectInt
ut::Math::RoundRectWithRegionToInt(const Rectf& rect, const Rectf& region)
{
    // rect is the base rectangle, region is in normalized 0..1 coordinates
    // within rect
    Rectf subRect(rect.x + rect.width*region.x,
                  rect.y + rect.height*region.y,
                  rect.width*region.width,
                  rect.height*region.height);
    return RoundRectToInt(subRect);
}

Rectf
ut::Math::RoundRectWithRegion(const Rectf& rect, const Rectf& region)
{
    Rectf subRect(rect.x + rect.width*region.x,
                  rect.y + rect.height*region.y,
                  rect.width*region.width,
                  rect.height*region.height);
    return RoundRect(subRect);
}
