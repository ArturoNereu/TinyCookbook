using Unity.Mathematics;
using Unity.Tiny.Core2D;

namespace Unity.Tiny.Interpolation
{
    public static class TimeLooper
    {
        public static float LoopTime(float time, float startTime, float endTime, LoopMode loopMode, out bool done)
        {
            float duration = endTime - startTime;

            done = ((loopMode == LoopMode.Once) && (time >= endTime)) ||
                   ((loopMode == LoopMode.PingPongOnce) && ((time - startTime) >= duration * 2.0f));

            if (time >= startTime && time <= endTime)
                return time;

            switch (loopMode)
            {
                case LoopMode.Once:
                case LoopMode.ClampForever:
                    return math.clamp(time, startTime, endTime);

                case LoopMode.Loop:
                    time = math.fmod(time - startTime, duration) + startTime;
                    if (time < startTime)
                        time = endTime - (startTime - time);
                    return time;

                case LoopMode.PingPongOnce:
                    time = math.clamp(time, startTime, endTime + duration);
                    if (time > endTime)
                        time -= (time - endTime) * 2.0f;
                    return time;

                case LoopMode.PingPong:
                    time = math.fmod(math.abs(time - startTime), duration * 2.0f) + startTime;
                    if (time > endTime)
                        time -= (time - endTime) * 2.0f;
                    return time;
            }

            return time;
        }
    }
}
