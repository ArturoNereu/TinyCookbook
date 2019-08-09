#pragma once

#define HAVE_UTINY_MATH

#include "../src/UnityMathTypes.h"
#include "../src/TRS.h"

namespace ut {
namespace Math {

// Round the float rectangle to ints; rounds towards infinity
RectInt RoundRectToInt(const Rectf& rect);
Rectf RoundRect(const Rectf& rect);

// region is 0..1 range
RectInt RoundRectWithRegionToInt(const Rectf& rect, const Rectf& region);
Rectf RoundRectWithRegion(const Rectf& rect, const Rectf& region);

}
}

