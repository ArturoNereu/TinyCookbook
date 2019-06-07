#include "Image2DHelpers.h"

using namespace ut;

static bool
IsValidPremultiplied(const uint32_t* mem, int w, int h)
{
    for (int i = 0; i < w * h; i++) {
        uint32_t v = mem[i];
        uint32_t a = v >> 24;
        if (((v >> 0) & 0xff) > a || ((v >> 8) & 0xff) > a || ((v >> 16) & 0xff) > a)
            return false;
    }
    return true;
}

void
Image2DHelpers::ExpandAlphaCopy(uint32_t* dest, const uint8_t* src, int w, int h)
{
    for (int i = 0; i < w * h; i++) {
        uint32_t v = src[i];
        v = v | (v << 8);
        v = v | (v << 16);
        dest[i] = v;
    }
}

void
Image2DHelpers::ExpandAlphaWhiteCopy(uint32_t* dest, const uint8_t* src, int w, int h)
{
    for (int i = 0; i < w * h; i++) {
        uint32_t v = src[i];
        dest[i] = (v << 24) | 0xffffff;
    }
}

uint32_t
Image2DHelpers::PremultiplyAlpha(uint32_t c)
{
    int32_t a = c >> 24;
    if (a == 0)
        return 0;
    if (a == 0xff)
        return c;
    uint32_t r = ((c & 0xff) * a) / 255;
    uint32_t g = (((c >> 8) & 0xff) * a) / 255;
    uint32_t b = (((c >> 16) & 0xff) * a) / 255;
    return r | (g << 8) | (b << 16) | (a << 24);
}

void
Image2DHelpers::PremultiplyAlpha(uint32_t* mem, int w, int h)
{
     for (int i = 0; i < w * h; i++)
         mem[i] = PremultiplyAlpha(mem[i]);
}

uint32_t
Image2DHelpers::UnmultiplyAlpha(uint32_t c)
{
    int32_t a = c >> 24;
    if (a == 0)
        return 0;
    if (a == 0xff)
        return c;
    uint32_t r = ((c & 0xff)*255) / a;
    uint32_t g = (((c >> 8) & 0xff)*255) / a;
    uint32_t b = (((c >> 16) & 0xff)*255) / a;
    //Assert(r<=0xff && g<=0xff && b<=0xff);
    return r | (g << 8) | (b << 16) | (a << 24);
}

void 
Image2DHelpers::UnmultiplyAlpha(uint32_t* mem, int w, int h)
{
     for (int i = 0; i < w * h; i++)
         mem[i] = UnmultiplyAlpha(mem[i]);
}

bool
Image2DHelpers::UnmultiplyAlphaAndCheckCopy(uint32_t* dest, const uint32_t* src, int w, int h)
{
    bool r = false;
    for (int i = 0; i < w * h; i++) {
        uint32_t v = src[i];
        if ((v & 0xff000000) != 0xff000000) {
            v = UnmultiplyAlpha(v);
            r = true;
        }
        dest[i] = v;
    }
    return r;
}

bool
Image2DHelpers::PremultiplyAlphaAndCheckCopy(uint32_t* dest, const uint32_t* src, int w, int h)
{
    bool r = false;
    for (int i = 0; i < w * h; i++) {
        uint32_t v = src[i];
        if ((v & 0xff000000) != 0xff000000) {
            v = PremultiplyAlpha(v);
            r = true;
        }
        dest[i] = v;
    }
    return r;
}

/*
bool
Image2DHelpers::CheckMemoryImage(ManagerWorld& world, Entity e, Image2DLoadFromMemory& fspec)
{
#ifdef DEVELOPMENT
    int maxSize = sMaxImageSize; // workaround clang needing an address to static const int in log
    int bypp = MemoryFormatToBytesPerPixel(fspec.format);
    if (fspec.height <= 0 || fspec.width <= 0 || fspec.height > sMaxImageSize || fspec.width > sMaxImageSize) {
        ut::logWarning("The memory image load on %s has an invalid pixel size"
                       "%i*%i. The minimum size is 1*1, the maximum size is %i*%i. "
                       "Error detected in DEVELOPMENT builds only.",
                       world.formatEntity(e).c_str(), fspec.width, fspec.height, maxSize, maxSize);
        return false;
    }
    if (fspec.height * fspec.width * bypp != fspec.pixelData.size()) {
        ut::logWarning("The memory image load on %s has a pixel size mismatch. The expected array size for "
                       "%i*%i is %i bytes, but there are %i bytes in the pixelData array. "
                       "Error detected in DEVELOPMENT builds only.",
                       world.formatEntity(e).c_str(), fspec.width, fspec.height, fspec.width * fspec.height * bypp,
                       (int)fspec.pixelData.size());
        return false;
    }
#endif
    return true;
}

bool
Image2DHelpers::ConvertMemoryImageToRGBA8(ManagerWorld& world, Entity e, Image2DLoadFromMemory& fspec,
                                             std::vector<uint8_t>& temp, bool& hasalpha)
{
    if (!CheckMemoryImage(world, e, fspec))
        return false;
    switch (fspec.format) {
    case Image2DMemoryFormat::RGBA8Premultiplied:
        // unmultiply :/
        temp.resize(fspec.width * fspec.height * 4);
        hasalpha = UnmultiplyAlphaAndCheckCopy((uint32_t*)temp.data(), (const uint32_t*)fspec.pixelData.data(), fspec.width, fspec.height);
        return true;
    case Image2DMemoryFormat::RGBA8:
        // nothing to do
        hasalpha = true;
        return true;
    case Image2DMemoryFormat::A8:
        // expand
        temp.resize(fspec.width * fspec.height * 4);
        ExpandAlphaWhiteCopy((uint32_t*)temp.data(), (const uint8_t*)fspec.pixelData.data(), fspec.width, fspec.height);
        hasalpha = true;
        return true;
    default:
        AssertNotReached();
        return false;
    }
}

bool
Image2DHelpers::ConvertMemoryImageToRGBA8Pre(ManagerWorld& world, Entity e, Image2DLoadFromMemory& fspec,
                                          std::vector<uint8_t>& temp, bool& hasalpha)
{
    if (!CheckMemoryImage(world, e, fspec))
        return false;
    switch (fspec.format) {
    case Image2DMemoryFormat::RGBA8Premultiplied:
#ifdef DEVELOPMENT
        if (!IsValidPremultiplied((const uint32_t*)fspec.pixelData.data(), fspec.width, fspec.height)) {
            ut::logWarning("The memory image load on %s specifies pre-multiplied alpha but has invalid pixels "
                           "where color values are larger than alpha values.",
                           world.formatEntity(e).c_str());
            return false;
        }
#endif
        // no copy needed
        hasalpha = true;
        return true;
    case Image2DMemoryFormat::RGBA8:
        // premultiply, expand & check if alpha != 255 anywhere
        temp.resize(fspec.width * fspec.height * 4);
        hasalpha = PremultiplyAlphaAndCheckCopy((uint32_t*)temp.data(), (const uint32_t*)fspec.pixelData.data(),
                                                fspec.width, fspec.height);
        return true;
    case Image2DMemoryFormat::A8:
        // expand
        temp.resize(fspec.width * fspec.height * 4);
        ExpandAlphaCopy((uint32_t*)temp.data(), (const uint8_t*)fspec.pixelData.data(), fspec.width, fspec.height);
        hasalpha = true;
        return true;
    default:
        AssertNotReached();
        return false;
    }
}

int
Image2DHelpers::MemoryFormatToBytesPerPixel(Image2DMemoryFormat fmt)
{
    switch (fmt) {
    case Image2DMemoryFormat::RGBA8:
    case Image2DMemoryFormat::RGBA8Premultiplied:
        return 4;
    case Image2DMemoryFormat::A8:
        return 1;
    default:
        AssertNotReached();
        return 0;
    }
}

static std::string
TrimSourceString(NativeString& s)
{
    if (s.length() > 128) {
        std::string r(s.data(), 128);
        r += "...";
        return r;
    } else {
        return std::string(s);
    }
}

NativeString
Image2DHelpers::FormatSourceName(Image2DLoadFromFile& fspec)
{
    std::string sourceName;
    if (!fspec.imageFile.empty())
        sourceName += TrimSourceString(fspec.imageFile);
    if (!fspec.maskFile.empty())
        sourceName += " Alpha Mask=" + TrimSourceString(fspec.maskFile);
    return sourceName;
}*/
