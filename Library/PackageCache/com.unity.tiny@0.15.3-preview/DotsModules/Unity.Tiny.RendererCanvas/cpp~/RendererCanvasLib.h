#pragma once
#include <emscripten.h>

extern "C" void js_renderTextTo2DCanvas(const uint16_t* text, const uint16_t* family, float size, int weight, bool italic,
                                        float r, float g, float b, float a,
                                        float width, float height);
extern "C" void js_canvasBlendingAndSmoothing(int blend, bool smooth);
extern "C" void js_canvasRenderNormalSpriteWhite(float txa, float txb, float txc, float txd, float txe, float txf,
                                                 float alpha, int imageIndex, float sx, float sy, float sw, float sh,
                                                 float x, float y, float w, float h);
extern "C" void js_canvasRenderNormalSpriteTinted(float txa, float txb, float txc, float txd, float txe, float txf,
                                                  float alpha, int tintedIndex, float x, float y, float w, float h);
extern "C" int js_canvasMakeTintedSprite(int imageIndex, float sx, float sy, float sw, float sh, int r, int g, int b);
extern "C" void js_canvasReleaseTintedSprite(int tintedIndex);
extern "C" int js_canvasMakePattern(int tintedIndex);
extern "C" void js_canvasSetTransformOnly(float txa, float txb, float txc, float txd, float txe, float txf);
extern "C" void js_canvasRenderNormalSpriteWhiteNoTransform(float alpha, int imageIndex, float sx, float sy, float sw, float sh,
                                                 float x, float y, float w, float h);
extern "C" void js_canvasRenderNormalSpriteTintedNoTransform(float alpha, int tintedIndex, float x, float y, float w, float h);
extern "C" void js_canvasRenderShape(const float * vertices, int nv, const unsigned short *indices, int ni, int r, int g, int b, float a);
extern "C" void js_canvasRenderPatternSprite(int patternIdx, float x, float y, float w, float h, float txa, float txb, float txc, float txd, float txe, float txf, float alpha);
extern "C" void js_canvasRenderMultipleSliced(int tintIndex, int imageIndex, const float *v, int n, float alpha);
extern "C" int js_canvasInit();
extern "C" bool js_canvasSupportsMultiply();
extern "C" bool js_canvasFreeImage(int imageIndex);
extern "C" void js_canvasPushImageAsContext(int imageIndex);
extern "C" void js_canvasPopContext();
extern "C" void js_canvasResizeRenderTarget(int imageIndex, int w, int h);
extern "C" int js_canvasMakeRenderTarget(int w, int h);
extern "C" void js_canvasFreeRenderTarget(int imageIndex);
extern "C" void js_canvasClear(int r, int g, int b, float a, int w, int h);


