#pragma once

#include <stdint.h>
#include <stdio.h>
#include <string>
#include "EntityTypes.h"

#ifdef __EMSCRIPTEN__
#include <emscripten.h>
#endif

#ifdef __clang__
// Disable Clang warning about passing a non-literal string to print format specifier (that is intentional in this file)
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wformat-security"
#endif

#if defined(DEBUG) && !defined(DEVELOPMENT)
#define DEVELOPMENT
#endif

namespace ut {

DLLEXPORT void assertFailed(const char* exprstring, const char* file, uint32_t line, const char* format, ...);

DLLEXPORT inline void
assertFailed(const char* exprstring, const char* file, uint32_t line)
{
    assertFailed(exprstring, file, line, "%s", "");
}

inline void log(char const * const str) noexcept
{
#ifdef DEBUG
    puts(str);
#endif
}

template <typename ... Args>
void log(char const * const format,
         Args const & ... args) noexcept
{
#ifdef DEBUG
    printf(format, args ...);
#endif
}

void logWarningString ( const char *str );

template <typename ... Args>
void logWarning(char const * const format,
         Args const & ... args) noexcept
{
#ifdef DEVELOPMENT
    char buf[1024];
    snprintf(buf, 1024, format, args ...);
    logWarningString(buf);
#endif
}

inline void logRelease(char const * const str) noexcept
{
    puts(str);
}

template <typename ... Args>
void logRelease(char const * const format,
               Args const & ... args) noexcept
{
#ifdef __EMSCRIPTEN__
    js_logf(format, args...);
#else
    printf(format, args ...);
#endif
}

} // namespace ut

#ifdef __clang__
#pragma clang diagnostic pop
#endif
