#pragma once
#include <emscripten.h>

extern "C" {
    void js_texImage2D_from_html_image(int htmlImageId);
    void js_texImage2D_from_html_text(const uint16_t* text, const uint16_t* family, float fontSize, int weight, bool italic, float labelWidth, float labelHeight);
}
