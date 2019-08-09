#pragma once
#include <stdint.h>
#include <vector>
#include <string.h>
#include <iostream>
#include <fstream>
#include <string>

#include "zeroplayer.h"
#include "EntityWrappers.h"
#include "Log.h"
#include "GeminiAssert.h"

#define STB_TRUETYPE_IMPLEMENTATION
#include "stb_truetype.h"

#define BUILD_UNITY_FONTSTB

#if defined(BUILD_UNITY_FONTSTB)
#define FONTSTB_EXPORT DLLEXPORT
#else
#define FONTSTB_EXPORT DLLIMPORT
#endif

namespace Unity {
    namespace TextNative {

        class TextImage {
        public:
            TextImage(int w, int h, int i = -1);
            ~TextImage();
            unsigned char* get() { return bitmap; }

            void reset(int w, int h);

            int width, height, glTexId;
            unsigned char * bitmap; //single - channel 8bpp bitmap to render text
        };

        class FontInfoSTB {
        public:

            FontInfoSTB(){}
            FontInfoSTB(FontInfoSTB&& other);
            ~FontInfoSTB();
            FontInfoSTB& operator=(FontInfoSTB&& other);

            bool loadFontInfo(const char* name);
            stbtt_fontinfo* getFontInfo() { return info; }

            std::string fontFileName;
            char* fontBuffer;
            stbtt_fontinfo* info;
        };

        static std::vector<FontInfoSTB*> fontInfos(1);
        static std::vector<TextImage*> textImages;

        FONTSTB_EXPORT TextImage* getTextImage(int glTexId);
        FONTSTB_EXPORT void createOrUpdateTextImage(int fontHandle, int glTexId, const wchar_t* text, int textLength, float size, float width, float height);
    }
}



