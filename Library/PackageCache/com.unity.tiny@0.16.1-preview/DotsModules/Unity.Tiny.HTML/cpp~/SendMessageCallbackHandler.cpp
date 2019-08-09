#include "zeroplayer.h"
#if defined(__EMSCRIPTEN__)
#include <emscripten.h>
#endif

typedef void (*SendMessageDelegate)(const char* message, const int* intArray, const int intArrayLength, const float* floatArray, const int floatArrayLength, const unsigned char* byteArray, const int byteArrayLength);

static SendMessageDelegate sSendMessageDelegate;

ZEROPLAYER_EXPORT
void RegisterSendMessage(SendMessageDelegate delegate)
{
    sSendMessageDelegate = delegate;
}

#if defined(__EMSCRIPTEN__)
extern "C"
{
    EMSCRIPTEN_KEEPALIVE
    void SendMessage (const char* message, const int* intArray, const int intArrayLength, const float* floatArray, const int floatArrayLength, const unsigned char* byteArray, const int byteArrayLength) {
        sSendMessageDelegate(message, intArray, intArrayLength, floatArray, floatArrayLength, byteArray, byteArrayLength);
    }
}
#endif