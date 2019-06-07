#pragma once

namespace math
{
    // This Enum needs to stay synchronized with the one in the bindings Runtime\Export\Transform.bindings
    // and in uTiny Geometry.cs
    enum RotationOrder
    {
        kOrderXYZ,
        kOrderXZY,
        kOrderYZX,
        kOrderYXZ,
        kOrderZXY,
        kOrderZYX,
        kRotationOrderLast = kOrderZYX,
        kOrderUnityDefault = kOrderZXY
    };

    const int kRotationOrderCount = kRotationOrderLast + 1;
}
