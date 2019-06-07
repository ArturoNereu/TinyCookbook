#include "il2cpp-config.h"
#include "os/File.h"
#include "os/Memory.h"
#include "vm/PlatformInvoke.h"
#include "utils/StringUtils.h"

extern "C" void STDCALL Console_Write(const char* message, int newline)
{
    il2cpp::os::FileHandle* fileHandle = il2cpp::os::File::GetStdOutput();
    size_t length = il2cpp::utils::StringUtils::StrLen(message);

    int error;
    il2cpp::os::File::Write(fileHandle, message, static_cast<int>(length), &error);
    if (newline)
    {
#if IL2CPP_TARGET_WINDOWS
        il2cpp::os::File::Write(fileHandle, "\r\n", 2, &error);
#else
        il2cpp::os::File::Write(fileHandle, "\n", 1, &error);
#endif
    }
}
