#include "UnityPrefix.h"
#include "Vector4.h"

const float     Vector4f::infinity = std::numeric_limits<float>::infinity();
const Vector4f  Vector4f::infinityVec = Vector4f(std::numeric_limits<float>::infinity(), std::numeric_limits<float>::infinity(), std::numeric_limits<float>::infinity(), std::numeric_limits<float>::infinity());

const Vector4f  Vector4f::zero = Vector4f(0, 0, 0, 0);
const Vector4f  Vector4f::one = Vector4f(1, 1, 1, 1);
