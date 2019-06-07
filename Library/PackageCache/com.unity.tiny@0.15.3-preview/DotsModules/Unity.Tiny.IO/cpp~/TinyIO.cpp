#include "TinyIO.h"

#include <stdio.h>
#include <string.h>
#include <vector>
#include <assert.h>

#if defined __EMSCRIPTEN__
    #include <emscripten.h>
    #include <emscripten/fetch.h>
#else
    #include <iostream>
    #include <fstream>
#endif


namespace Unity { namespace Tiny { namespace IO
{
    struct Request
    {
        Request(int index = -1) : mIndex(index) {}

        int mIndex;
        void* mpPayload = nullptr; // platform specific
        size_t mPayloadSize = 0;
        Status mStatus = Status::NotStarted;
        ErrorStatus mErrorStatus = ErrorStatus::None;
    }; 

    class RequestPool
    {
        const int kGrowSize = 64;

    public:
        RequestPool()
        {
            mRequests.reserve(kGrowSize);
            mFreeRequests.reserve(kGrowSize);

            // Create Request 0 but don't add it to the free list as that index is reserved as invalid
            mRequests.push_back({ 0 });

            for (int i = 1; i < kGrowSize; ++i)
            {
                mRequests.push_back({i});
                mFreeRequests.push_back(i);
            }
        }

        int GetRequestIndex()
        {
            if (mFreeRequests.empty())
            {
                int origSize = (int) mRequests.capacity();
                int newSize = (int) origSize + kGrowSize;

                mRequests.reserve(newSize);
                mFreeRequests.reserve(newSize);

                for (int i = origSize; i < newSize; ++i)
                {
                    mRequests.push_back({i});
                    mFreeRequests.push_back(i);
                }
            }

            int requestIndex = mFreeRequests.back();
            assert(requestIndex != 0); // 0 is reserved so ensure we never return it
            mFreeRequests.pop_back();

            return requestIndex;
        }

        Request& GetRequest(int index)
        {
            return mRequests[index];
        }

        void FreeRequest(int index)
        {
            mFreeRequests.push_back(index);
        }

    private:
        std::vector<Request> mRequests;
        std::vector<int> mFreeRequests;
    };

    static RequestPool sRequestPool;

    ZEROPLAYER_EXPORT
    int ZEROPLAYER_CALL GetStatus(int requestIndex)
    {
        Request& request = sRequestPool.GetRequest(requestIndex);

        return (int)request.mStatus;
    }

    ZEROPLAYER_EXPORT
    int ZEROPLAYER_CALL GetErrorStatus(int requestIndex)
    {
        Request& request = sRequestPool.GetRequest(requestIndex);

        return (int)request.mErrorStatus;
    }

#if defined __EMSCRIPTEN__
    // Fetch Callbacks
    static void OnSuccess(emscripten_fetch_t* fetch)
    {
        int requestIndex = (int)fetch->userData;
        Request& request = sRequestPool.GetRequest(requestIndex);

        request.mpPayload = fetch;
        request.mPayloadSize = fetch->numBytes;
        request.mStatus = Status::Success;
    }

    static void OnError(emscripten_fetch_t* fetch)
    {
        int requestIndex = (int)fetch->userData;
        Request& request = sRequestPool.GetRequest(requestIndex);

        request.mpPayload = nullptr;
        request.mPayloadSize = 0;
        request.mStatus = Status::Failure;

        switch (fetch->status)
        {
            case 404:
                request.mErrorStatus = ErrorStatus::FileNotFound;
                break;
            default:
                request.mErrorStatus = ErrorStatus::Unknown;
                break;
        }
    }

    static void OnProgress(emscripten_fetch_t* fetch)
    {
    }


    // Async API
    /////////////
    ZEROPLAYER_EXPORT
    int RequestAsyncRead(const char* path)
    {
        int requestIndex = sRequestPool.GetRequestIndex();
        Request& request = sRequestPool.GetRequest(requestIndex);

        request.mStatus = Status::InProgress;
        request.mErrorStatus = ErrorStatus::None;

        emscripten_fetch_attr_t attr;
        emscripten_fetch_attr_init(&attr);

        strcpy(attr.requestMethod, "GET");
        attr.attributes = EMSCRIPTEN_FETCH_LOAD_TO_MEMORY;
        attr.onsuccess = OnSuccess;
        attr.onerror = OnError;
        attr.onprogress = OnProgress;
        attr.userData = (void*)requestIndex;

        emscripten_fetch(&attr, path);

        return requestIndex;
    }

    ZEROPLAYER_EXPORT
    void Close(int requestIndex)
    {
        if (requestIndex == 0)
            return;

        Request& request = sRequestPool.GetRequest(requestIndex);
        request.mpPayload = nullptr;
        request.mPayloadSize = 0;
        request.mStatus = Status::NotStarted;
        request.mErrorStatus = ErrorStatus::None;

        emscripten_fetch_close((emscripten_fetch_t*)request.mpPayload);

        assert(request.mIndex == requestIndex);
        sRequestPool.FreeRequest(request.mIndex);
    }

    ZEROPLAYER_EXPORT
    void GetData(int requestIndex, const char** data, int* len)
    {
        Request& request = sRequestPool.GetRequest(requestIndex);

        if (request.mStatus != Status::Success)
        {
            *data = nullptr;
            *len = 0;

            return;
        }

        *data = ((emscripten_fetch_t*) request.mpPayload)->data;
        *len  = ((emscripten_fetch_t*) request.mpPayload)->numBytes;
    }
#else
    // Async API
    /////////////

    ZEROPLAYER_EXPORT
    int ZEROPLAYER_CALL RequestAsyncRead(const char* path)
    {
        int requestIndex = sRequestPool.GetRequestIndex();
        Request& request = sRequestPool.GetRequest(requestIndex);

        request.mStatus = Status::InProgress;
        request.mErrorStatus = ErrorStatus::None;

        // Just do syncrounous IO on native for now
        std::ifstream fs(path, std::ifstream::in | std::ifstream::binary | std::ifstream::ate);

        if (!fs.is_open())
        {
            request.mStatus = Status::Failure;
            request.mErrorStatus = ErrorStatus::FileNotFound;
        }
        else
        {
            int size = (int) fs.tellg();
            void* data = malloc(size);

            fs.seekg(0, std::ifstream::beg);
            fs.read ((char*)data, size);
            fs.close();

            request.mpPayload = data;
            request.mPayloadSize = size;
            request.mStatus = Status::Success;
        }

        return requestIndex;
    }

    ZEROPLAYER_EXPORT
    void ZEROPLAYER_CALL Close(int requestIndex)
    {
        if (requestIndex == 0)
            return;

        Request& request = sRequestPool.GetRequest(requestIndex);

        free(request.mpPayload);
        request.mpPayload = nullptr;
        request.mPayloadSize = 0;
        request.mStatus = Status::NotStarted;
        request.mErrorStatus = ErrorStatus::None;

        assert(request.mIndex == requestIndex);
        sRequestPool.FreeRequest(request.mIndex);
    }

    ZEROPLAYER_EXPORT
    void ZEROPLAYER_CALL GetData(int requestIndex, const char** data, int* len)
    {
        Request& request = sRequestPool.GetRequest(requestIndex);

        *data = (const char*) request.mpPayload;
        *len = (int) request.mPayloadSize;
    }
#endif
}}} // namespace Unity::Tiny::IO

