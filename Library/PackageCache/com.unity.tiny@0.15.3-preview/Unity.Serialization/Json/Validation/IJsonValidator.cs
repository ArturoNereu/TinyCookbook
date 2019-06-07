using System;
using Unity.Jobs;

namespace Unity.Serialization.Json
{
    internal interface IJsonValidator : IDisposable
    {
        void Initialize();
        JsonValidationResult GetResult();
        JobHandle ValidateAsync(UnsafeBuffer<char> buffer, int start, int count);
    }
}
