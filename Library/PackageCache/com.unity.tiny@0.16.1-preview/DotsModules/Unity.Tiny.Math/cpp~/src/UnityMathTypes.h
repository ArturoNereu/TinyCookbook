#pragma once

// the order of these #includes matters (specifically UnityPrefix.h)
// clang-format off

#include "unity/UnityPrefix.h"

#include "unity/Vector2.h"
#include "unity/Vector3.h"
#include "unity/Vector4.h"
#include "unity/Rect.h"
#include "unity/Quaternion.h"
#include "unity/Matrix3x3.h"
#include "unity/Matrix4x4.h"

// and then move them into the proper namespace

namespace ut {
namespace Math {

using ::Vector2f;
using ::Vector3f;
using ::Vector4f;
using ::Rectf;
using ::RectInt;
using ::Quaternionf;
using ::Matrix3x3f;
using ::Matrix4x4f;

}
}

// clang-format on

