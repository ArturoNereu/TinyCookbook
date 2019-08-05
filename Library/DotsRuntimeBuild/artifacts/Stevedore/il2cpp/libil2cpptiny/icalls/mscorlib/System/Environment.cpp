#include "il2cpp-config.h"

#include "il2cpp-object-internals.h"
#include "il2cpp-class-internals.h"
#include "il2cpp-string-types.h"
#include "Environment.h"
#include "os/CrashHelpers.h"
#include "os/StackTrace.h"
#include "utils/Logging.h"
#include "utils/StringUtils.h"
#include "vm/String.h"
#include "vm/StackTrace.h"

#include <string>

namespace tiny
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    Il2CppString* Environment::GetStackTrace_internal()
    {
        std::string stackTrace = tiny::vm::StackTrace::GetStackTrace();
#if IL2CPP_TINY_WITHOUT_DEBUGGER
        UTF16String utf16Chars = il2cpp::utils::StringUtils::Utf8ToUtf16(stackTrace.c_str(), stackTrace.length());
        return vm::String::NewLen(utf16Chars.c_str(), (uint32_t)stackTrace.length());
#else
        return vm::String::NewLen(stackTrace.c_str(), (uint32_t)stackTrace.length());
#endif
    }

    void Environment::FailFast_internal(Il2CppString* message)
    {
        bool messageWritten = false;
        if (message != NULL)
        {
            std::string messageUtf8 = il2cpp::utils::StringUtils::Utf16ToUtf8(message->chars, message->length);
            if (!messageUtf8.empty())
            {
                il2cpp::utils::Logging::Write(messageUtf8.c_str());
                messageWritten = true;
            }
        }

        if (!messageWritten)
            il2cpp::utils::Logging::Write("No error message was provided. Hopefully the stack trace can provide some information.");

        std::string managedStackTrace = vm::StackTrace::GetStackTrace();
        if (!managedStackTrace.empty())
        {
            std::string managedStackTraceMessage = "Managed stack trace:\n" + managedStackTrace;
            il2cpp::utils::Logging::Write(managedStackTraceMessage.c_str());
        }
        else
        {
            il2cpp::utils::Logging::Write("No managed stack trace exists. Make sure this is a development build to enable managed stack traces.");
        }

        il2cpp::os::CrashHelpers::Crash();
    }
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace tiny */
