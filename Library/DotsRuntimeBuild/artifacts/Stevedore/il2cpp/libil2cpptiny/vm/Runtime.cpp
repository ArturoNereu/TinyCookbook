#include "il2cpp-config.h"
#include "Runtime.h"
#include "TypeUniverse.h"
#include "os/Image.h"
#include "os/Memory.h"
#include "gc/GarbageCollector.h"
#include "vm/DebugMetadata.h"
#include "vm/StackTrace.h"

void Il2CppCallStaticConstructors();
extern const void** GetStaticFieldsStorageArray();

namespace tiny
{
namespace vm
{
    void Runtime::Init()
    {
#if IL2CPP_ENABLE_STACKTRACES
        vm::StackTrace::InitializeStackTracesForCurrentThread();
#endif
        il2cpp::gc::GarbageCollector::Initialize();
        TypeUniverse::Initialize();
        AllocateStaticFieldsStorage();
        Il2CppCallStaticConstructors();
#if IL2CPP_ENABLE_STACKTRACES
        il2cpp::os::Image::Initialize();
        vm::DebugMetadata::InitializeMethodsForStackTraces();
#endif
    }

    void Runtime::Shutdown()
    {
        FreeStaticFieldsStorage();
        il2cpp::gc::GarbageCollector::UninitializeGC();
#if IL2CPP_ENABLE_STACKTRACES
        vm::StackTrace::CleanupStackTracesForCurrentThread();
#endif
    }

    void Runtime::AllocateStaticFieldsStorage()
    {
        const void** StaticFieldsStorage = GetStaticFieldsStorageArray();
        int i = 0;
        while (StaticFieldsStorage[i] != NULL)
        {
            *(void**)StaticFieldsStorage[i] = il2cpp::gc::GarbageCollector::AllocateFixed(*(size_t*)StaticFieldsStorage[i], NULL);
            i++;
        }
    }

    void Runtime::FreeStaticFieldsStorage()
    {
        const void** StaticFieldsStorage = GetStaticFieldsStorageArray();
        int i = 0;
        while (StaticFieldsStorage[i] != NULL)
        {
            il2cpp::gc::GarbageCollector::FreeFixed(*(void**)StaticFieldsStorage[i]);
            i++;
        }
    }
}
}
