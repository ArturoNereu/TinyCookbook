#pragma once

#if 0
#define LOGE(fmt, ...) printf("[audio] " #fmt "\n", ##__VA_ARGS__); fflush(stdout);
#define ASSERT(x) { if (!(x)) printf("[audio] ASSERT: " #x); }
#else
#define LOGE(fmt, ...);
#define ASSERT(x)
#endif

