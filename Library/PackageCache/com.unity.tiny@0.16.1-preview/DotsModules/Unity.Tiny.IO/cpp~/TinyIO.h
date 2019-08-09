#pragma once

#include "zeroplayer.h"
#include "bind-Unity_Tiny_IO.h"

namespace Unity { namespace Tiny { namespace IO {
    ZEROPLAYER_EXPORT int ZEROPLAYER_CALL RequestAsyncRead(const char* path);
    ZEROPLAYER_EXPORT int ZEROPLAYER_CALL GetStatus(int requestIndex);
    ZEROPLAYER_EXPORT int ZEROPLAYER_CALL GetErrorStatus(int requestIndex);
    ZEROPLAYER_EXPORT void ZEROPLAYER_CALL Close(int requestIndex);
    ZEROPLAYER_EXPORT void ZEROPLAYER_CALL GetData(int requestIndex, const char** data, int* len);
}}} // namespace Unity::Tiny::IO
