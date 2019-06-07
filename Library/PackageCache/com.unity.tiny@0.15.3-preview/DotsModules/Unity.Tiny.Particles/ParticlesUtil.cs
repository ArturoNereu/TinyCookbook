// using System;
using Unity.Mathematics;
using Unity.Tiny.Core2D;

namespace Unity.Tiny.Particles
{
    public static class ParticlesUtil
    {
        private static Random m_rand = new Random(1);

        public static float Random01()
        {
            return m_rand.NextFloat(0.0f, 1.0f);
        }

        public static float2 RandomPointInRect(Rect rect)
        {
            return m_rand.NextFloat2(
                new float2(rect.x, rect.y),
                new float2(rect.x + rect.width, rect.y + rect.height));
        }

        public static float RandomRange(Range range)
        {
            return m_rand.NextFloat(range.start, range.end);
        }
    }
}
