#include "FontSTB.h"
#include "ThreadPool.h"

using namespace Unity::Entities;
using namespace Unity::Tiny::Core2D;
using namespace Unity::TextNative;
using namespace ut;
using namespace ut::ThreadPool;

static int unsupportedCharacterAdvance = 10;

// ----- TextImage class --------
TextImage::TextImage(int w, int h, int i)
{
    width = w;
    height = h;
    bitmap = new unsigned char[w*h];
    std::memset(bitmap, 0, w*h);
    glTexId = i;
}

TextImage::~TextImage()
{
    if (bitmap)
    {
        delete[] bitmap;
        bitmap = 0;
    }
    glTexId = -1;
}

void TextImage::reset(int w, int h)
{
    width = w;
    height = h;
    if (bitmap)
        delete[] bitmap;
    bitmap = new unsigned char[w*h + 1];
    std::memset(bitmap, 0, w*h);
}

// ----- FontInfoSTB class ------
FontInfoSTB::FontInfoSTB(FontInfoSTB&& other)
{
    fontBuffer = other.fontBuffer;
    other.fontBuffer = 0;
    info = other.info;
    other.info = 0;
    fontFileName = other.fontFileName;
}

FontInfoSTB::~FontInfoSTB()
{
    if (fontBuffer)
    {
        free(fontBuffer);
        fontBuffer = 0;
    }
    if (info)
        delete info;
}

FontInfoSTB& FontInfoSTB::operator=(FontInfoSTB&& other)
{
    if (this == &other) return *this;
    delete[] fontBuffer;
    fontBuffer = other.fontBuffer;
    other.fontBuffer = 0;
    fontFileName = other.fontFileName;
    info = other.info;
    return *this;
}

bool FontInfoSTB::loadFontInfo(const char* name)
{
    fontFileName = name;
    bool res = false;
    FILE* file = fopen(name, "rb");
    if (file)
    {
        fseek(file, 0, SEEK_END);
        int fontBufferSize = (int)ftell(file);
        fseek(file, 0, SEEK_SET);

        fontBuffer = new char[fontBufferSize];
        if (fread(fontBuffer, fontBufferSize, 1, file))
        {
            info = new stbtt_fontinfo();
            if (stbtt_InitFont(info, (unsigned char*)fontBuffer, 0) == 0)
                ut::log("Failed to initialize stb font from file %s\n", fontFileName.c_str());
            res = true;
        }
        else
            ut::log("Failed to read the font file %s\n", fontFileName.c_str());
        fclose(file);
    }
    else {
        ut::log("Failed to open the font file %s\n", fontFileName.c_str());
    }

    return res;
}

static bool LoadFont(FontInfoSTB& info, const char* fontFileName)
{
    return info.loadFontInfo(fontFileName);
}

static void measureText(stbtt_fontinfo* info, char* text, int textLength, float size, float* width, float* height)
{
    float scale = stbtt_ScaleForPixelHeight(info, size);
    float w = 0.0f;

    for (int i = 0; i < textLength; i++)
    {
        int codePoint = (int)(text[i]);
        int glyphIndex = stbtt_FindGlyphIndex(info, codePoint);
        if (glyphIndex == 0) { //unsupported character
            w += unsupportedCharacterAdvance;
            ut::log("Unsupported codepoint %i\n", codePoint);
            continue;
        }

        //Advance
        int advance, bearingX;
        stbtt_GetCodepointHMetrics(info, codePoint, &advance, &bearingX);
        w += (advance * scale);

        //Kerning
        if (i < textLength - 1)
        {
            int nextCodePoint = (int)(text[i + 1]);
            int kern = stbtt_GetCodepointKernAdvance(info, codePoint, nextCodePoint);
            w += kern * scale;
        }
    }
    *width = w;
    *height = size;
}

// ------- Exported functions

TextImage* Unity::TextNative::getTextImage(int glTexId)
{
    for (auto value : textImages) {
        if (value->glTexId == glTexId)
        {
            return value;
        }
    }
    return 0;
}

void Unity::TextNative::createOrUpdateTextImage(int fontHandle, int glTexId, const wchar_t* text, int textLength, float size, float width, float height)
{
    Assert(fontHandle>0);

    // Get STB Font info
    stbtt_fontinfo* info = fontInfos[fontHandle]->getFontInfo();
    
    //Create/Get bitmap
    TextImage* t = getTextImage(glTexId);
    if (!t)
    {
        t = new TextImage((int)(width + 0.5f), (int)(height + 0.5f), glTexId);
        textImages.push_back(t);
    }
    else {
        t->reset((int)(width + 0.5f), (int)(height + 0.5f));
    }

    //Get font metrics
    int ascent, descent;
    stbtt_GetFontVMetrics(info, &ascent, &descent, 0); 
    
    //Get scale 
    float scale = stbtt_ScaleForPixelHeight(info, height);
    ascent *= scale;
    descent *= scale;

    // Create bitmap
    int xpos = 0;
    int bitmapSize = (t->width * t->height);
    for (int i = 0; i < textLength; i++)
    {
        int codePoint = (int)(text[i]);
        int glyphIndex = stbtt_FindGlyphIndex(info, codePoint); 
        if (glyphIndex == 0)
        {
            xpos += unsupportedCharacterAdvance;
            continue;
        }

        //Get the bbox of the bitmap centered around the glyph origin
        int x0, y0, x1, y1 = 0;
        stbtt_GetCodepointBitmapBox(info, codePoint, scale, scale, &x0, &y0, &x1, &y1); 

        int y = ascent + y0;

        //Update bitmap with new character
        int offset = xpos + y * (int)width;
        if (offset < bitmapSize)
            stbtt_MakeCodepointBitmap(info, t->get() + offset, x1 - x0, y1 - y0, (int)width, scale, scale, codePoint);

        //Get glyph metrics 
        int advance, bearingX = 0;
        stbtt_GetCodepointHMetrics(info, codePoint, &advance, &bearingX); 

        //Move on x cursor xpos with advance
        xpos += (int)(advance * scale);

        //Move on x cursor xpos with kerning
        if (i < textLength - 1)
        {
            int nextCodePoint = (int)(text[i + 1]);
            int kern = stbtt_GetCodepointKernAdvance(info, codePoint, nextCodePoint);
            xpos += (int)(kern * scale);
        }
    }
}

class AsyncNativeFontLoader : public ThreadPool::Job {
public:
    // state needed for Do()
    FontInfoSTB fontInfo;
    std::string fontFile;

    AsyncNativeFontLoader() {}

    virtual bool Do()
    {
        progress = 0;
        // simulate being slow
#if 0
        for (int i = 0; i<20; i++) {
            std::this_thread::sleep_for(std::chrono::milliseconds(20));
            progress = i;
            if (abort)
                return false;
        }
#endif
        // actual work
        return LoadFont(fontInfo, fontFile.c_str());
    }
};

ZEROPLAYER_EXPORT
int64_t ZEROPLAYER_CALL startload_font_stb(const char *fontFile)
{
    //Check first if we didn't already load the same font in case 2 font entities are using the same font name
    for (int i = 1; i < (int)fontInfos.size(); i++) {
        if (strcmp(fontInfos[i]->fontFileName.c_str(), fontFile) == 0)
           return -1;
    }

    std::unique_ptr<AsyncNativeFontLoader> loader(new AsyncNativeFontLoader);
    loader->fontFile = fontFile;
    return Pool::GetInstance()->Enqueue(std::move(loader));
}

ZEROPLAYER_EXPORT
int ZEROPLAYER_CALL checkload_font_stb(int64_t loadId, int *fontHandle)
{
    *fontHandle = -1;
    std::unique_ptr<ThreadPool::Job> resultTemp = Pool::GetInstance()->CheckAndRemove(loadId);
    if (!resultTemp)
        return 0; // still loading
    if (!resultTemp->GetReturnValue()) {
        resultTemp.reset(0);
        return 2; // failed
    }
    // put it into a local copy
    int found = -1;
    for (int i = 1; i<(int)fontInfos.size(); i++) {
        if (!fontInfos[i]) {
            found = i;
            break;
        }
    }
    AsyncNativeFontLoader* res = (AsyncNativeFontLoader*)resultTemp.get();
    FontInfoSTB *font = new FontInfoSTB(std::move(res->fontInfo));
    if (found == -1) {
        fontInfos.push_back(font);
        *fontHandle = (int)fontInfos.size() - 1;
    }
    else {
        fontInfos[found] = font;
        *fontHandle = found;
    }
    return 1; // ok
}

ZEROPLAYER_EXPORT
void ZEROPLAYER_CALL abortload_stb(int64_t loadId)
{
    Pool::GetInstance()->Abort(loadId);
}

ZEROPLAYER_EXPORT
void ZEROPLAYER_CALL freeFont_stb(int handle)
{
    if (handle<0 || handle >= (int)fontInfos.size())
        return;
    delete fontInfos[handle];
    fontInfos[handle] = 0;
}

ZEROPLAYER_EXPORT
void ZEROPLAYER_CALL measureText_stb(char* text, int fontHandle, int textLength, float textSize, float* width, float* height)
{
    if (fontHandle<0 || fontHandle >= (int)fontInfos.size())
        return;

    stbtt_fontinfo* info = fontInfos[fontHandle]->getFontInfo();
    measureText(info, text, textLength, textSize, width, height);
}
