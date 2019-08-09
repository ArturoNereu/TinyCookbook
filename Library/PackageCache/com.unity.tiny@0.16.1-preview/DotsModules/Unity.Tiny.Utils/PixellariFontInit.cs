using Unity.Entities;
using Unity.Mathematics;
using Unity.Tiny.Core;
using Unity.Tiny.Text;
using Unity.Tiny.Core2D;

namespace Unity.Tiny.Utils
{
    public class PixellariFontInit
    {
        public static Entity CreatePixellariFont(EntityManager em)
        {
            Entity textureEntity = em.CreateEntity(typeof(Image2D), typeof(Image2DLoadFromFile));
            em.SetComponentData(textureEntity, new Image2D
            {
                disableSmoothing = true
            });

            em.AddBufferFromString<Image2DLoadFromFileImageFile>(textureEntity, "Pixellari.png");

            Entity fontEntity = TextService.CreateBitmapFont(em, ref textureEntity, 46, 35, -11);

            TextService.AddGlyph(em, ref fontEntity, 32, 0, 35, 14, 14, 46, new Rect(0.00390625f, -0.1757813f, 0.0546875f, 0.1796875f));
            TextService.AddGlyph(em, ref fontEntity, 33, 2, 31, 11, 7, 32, new Rect(0.9648438f, 0.00390625f, 0.02734375f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 34, 2, 31, 20, 16, 12, new Rect(0.9335938f, 0.5351563f, 0.0625f, 0.046875f));
            TextService.AddGlyph(em, ref fontEntity, 35, 0, 34, 25, 23, 35, new Rect(0.00390625f, 0.5820313f, 0.08984375f, 0.1367188f));
            TextService.AddGlyph(em, ref fontEntity, 36, 5, 31, 25, 18, 35, new Rect(0.00390625f, 0.859375f, 0.0703125f, 0.1367188f));
            TextService.AddGlyph(em, ref fontEntity, 37, 0, 31, 40, 38, 32, new Rect(0.1523438f, 0.00390625f, 0.1484375f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 38, 2, 34, 28, 24, 35, new Rect(0.00390625f, 0.4375f, 0.09375f, 0.1367188f));
            TextService.AddGlyph(em, ref fontEntity, 39, 2, 31, 11, 7, 12, new Rect(0.9335938f, 0.5898438f, 0.02734375f, 0.046875f));
            TextService.AddGlyph(em, ref fontEntity, 40, 2, 31, 14, 10, 35, new Rect(0.1054688f, 0.4375f, 0.0390625f, 0.1367188f));
            TextService.AddGlyph(em, ref fontEntity, 41, 5, 31, 17, 10, 35, new Rect(0.1796875f, 0.5820313f, 0.0390625f, 0.1367188f));
            TextService.AddGlyph(em, ref fontEntity, 42, 0, 34, 20, 18, 18, new Rect(0.71875f, 0.8632813f, 0.0703125f, 0.0703125f));
            TextService.AddGlyph(em, ref fontEntity, 43, 2, 23, 23, 19, 18, new Rect(0.21875f, 0.5f, 0.07421875f, 0.0703125f));
            TextService.AddGlyph(em, ref fontEntity, 44, 2, 5, 11, 7, 12, new Rect(0.96875f, 0.5898438f, 0.02734375f, 0.046875f));
            TextService.AddGlyph(em, ref fontEntity, 45, 2, 17, 23, 19, 7, new Rect(0.5195313f, 0.9648438f, 0.07421875f, 0.02734375f));
            TextService.AddGlyph(em, ref fontEntity, 46, 2, 5, 11, 7, 6, new Rect(0.1523438f, 0.1367188f, 0.02734375f, 0.0234375f));
            TextService.AddGlyph(em, ref fontEntity, 47, 0, 34, 20, 18, 35, new Rect(0.08203125f, 0.859375f, 0.0703125f, 0.1367188f));
            TextService.AddGlyph(em, ref fontEntity, 48, 2, 31, 25, 21, 32, new Rect(0.1054688f, 0.7265625f, 0.08203125f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 49, 5, 31, 25, 18, 32, new Rect(0.125f, 0.3046875f, 0.0703125f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 50, 2, 31, 23, 19, 32, new Rect(0.1289063f, 0.171875f, 0.07421875f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 51, 2, 31, 23, 19, 32, new Rect(0.921875f, 0.1367188f, 0.07421875f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 52, 2, 31, 25, 21, 32, new Rect(0.1953125f, 0.7265625f, 0.08203125f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 53, 2, 31, 23, 19, 32, new Rect(0.2265625f, 0.578125f, 0.07421875f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 54, 2, 31, 23, 19, 32, new Rect(0.3085938f, 0.578125f, 0.07421875f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 55, 2, 31, 25, 21, 32, new Rect(0.90625f, 0.2695313f, 0.08203125f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 56, 2, 31, 23, 19, 32, new Rect(0.3554688f, 0.84375f, 0.07421875f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 57, 2, 31, 23, 19, 32, new Rect(0.375f, 0.7109375f, 0.07421875f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 58, 2, 23, 11, 7, 23, new Rect(0.6015625f, 0.8671875f, 0.02734375f, 0.08984375f));
            TextService.AddGlyph(em, ref fontEntity, 59, 2, 23, 11, 7, 29, new Rect(0.9570313f, 0.765625f, 0.02734375f, 0.1132813f));
            TextService.AddGlyph(em, ref fontEntity, 60, 0, 25, 31, 29, 24, new Rect(0.4609375f, 0.2695313f, 0.1132813f, 0.09375f));
            TextService.AddGlyph(em, ref fontEntity, 61, 2, 23, 23, 19, 18, new Rect(0.3007813f, 0.5f, 0.07421875f, 0.0703125f));
            TextService.AddGlyph(em, ref fontEntity, 62, 0, 25, 31, 29, 24, new Rect(0.5820313f, 0.2695313f, 0.1132813f, 0.09375f));
            TextService.AddGlyph(em, ref fontEntity, 63, 2, 31, 23, 19, 32, new Rect(0.4375f, 0.84375f, 0.07421875f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 64, 2, 31, 40, 36, 41, new Rect(0.00390625f, 0.00390625f, 0.140625f, 0.1601563f));
            TextService.AddGlyph(em, ref fontEntity, 65, 2, 31, 28, 24, 32, new Rect(0.6601563f, 0.00390625f, 0.09375f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 66, 2, 31, 28, 24, 32, new Rect(0.7617188f, 0.00390625f, 0.09375f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 67, 5, 31, 28, 21, 32, new Rect(0.21875f, 0.3671875f, 0.08203125f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 68, 5, 31, 31, 24, 32, new Rect(0.8632813f, 0.00390625f, 0.09375f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 69, 5, 31, 25, 18, 32, new Rect(0.4570313f, 0.6367188f, 0.0703125f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 70, 5, 31, 25, 18, 32, new Rect(0.5351563f, 0.6367188f, 0.0703125f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 71, 5, 31, 28, 21, 32, new Rect(0.3085938f, 0.3671875f, 0.08203125f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 72, 2, 31, 28, 24, 32, new Rect(0.00390625f, 0.7265625f, 0.09375f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 73, 2, 31, 17, 13, 32, new Rect(0.3984375f, 0.3671875f, 0.05078125f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 74, 0, 31, 23, 21, 32, new Rect(0.4570313f, 0.3710938f, 0.08203125f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 75, 2, 31, 28, 24, 32, new Rect(0.2109375f, 0.1367188f, 0.09375f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 76, 2, 31, 23, 19, 32, new Rect(0.7695313f, 0.5351563f, 0.07421875f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 77, 2, 31, 34, 30, 32, new Rect(0.00390625f, 0.171875f, 0.1171875f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 78, 2, 31, 31, 27, 32, new Rect(0.4335938f, 0.00390625f, 0.1054688f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 79, 5, 31, 31, 24, 32, new Rect(0.3125f, 0.1367188f, 0.09375f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 80, 2, 31, 25, 21, 32, new Rect(0.546875f, 0.3710938f, 0.08203125f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 81, 5, 31, 34, 27, 32, new Rect(0.546875f, 0.00390625f, 0.1054688f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 82, 2, 31, 28, 24, 32, new Rect(0.4140625f, 0.1367188f, 0.09375f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 83, 2, 31, 28, 24, 32, new Rect(0.515625f, 0.1367188f, 0.09375f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 84, 2, 31, 28, 24, 32, new Rect(0.6171875f, 0.1367188f, 0.09375f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 85, 5, 31, 31, 24, 32, new Rect(0.71875f, 0.1367188f, 0.09375f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 86, 5, 31, 31, 24, 32, new Rect(0.8203125f, 0.1367188f, 0.09375f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 87, 2, 31, 34, 30, 32, new Rect(0.3085938f, 0.00390625f, 0.1171875f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 88, 2, 31, 28, 24, 32, new Rect(0.703125f, 0.2695313f, 0.09375f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 89, 2, 31, 28, 24, 32, new Rect(0.8046875f, 0.2695313f, 0.09375f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 90, 0, 31, 31, 29, 32, new Rect(0.00390625f, 0.3046875f, 0.1132813f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 91, 2, 31, 14, 10, 35, new Rect(0.2265625f, 0.859375f, 0.0390625f, 0.1367188f));
            TextService.AddGlyph(em, ref fontEntity, 92, 0, 34, 20, 18, 35, new Rect(0.1015625f, 0.5820313f, 0.0703125f, 0.1367188f));
            TextService.AddGlyph(em, ref fontEntity, 93, 5, 31, 17, 10, 35, new Rect(0.2734375f, 0.859375f, 0.0390625f, 0.1367188f));
            TextService.AddGlyph(em, ref fontEntity, 94, 0, 28, 23, 21, 15, new Rect(0.4570313f, 0.7695313f, 0.08203125f, 0.05859375f));
            TextService.AddGlyph(em, ref fontEntity, 95, 0, -3, 25, 23, 7, new Rect(0.796875f, 0.9101563f, 0.08984375f, 0.02734375f));
            TextService.AddGlyph(em, ref fontEntity, 96, 2, 31, 14, 10, 9, new Rect(0.390625f, 0.6679688f, 0.0390625f, 0.03515625f));
            TextService.AddGlyph(em, ref fontEntity, 97, 2, 23, 23, 19, 23, new Rect(0.6484375f, 0.6679688f, 0.07421875f, 0.08984375f));
            TextService.AddGlyph(em, ref fontEntity, 98, 2, 31, 25, 21, 32, new Rect(0.6953125f, 0.4023438f, 0.08203125f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 99, 2, 23, 23, 19, 23, new Rect(0.7304688f, 0.6679688f, 0.07421875f, 0.08984375f));
            TextService.AddGlyph(em, ref fontEntity, 100, 2, 31, 25, 21, 32, new Rect(0.7851563f, 0.4023438f, 0.08203125f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 101, 2, 23, 23, 19, 23, new Rect(0.8125f, 0.6679688f, 0.07421875f, 0.08984375f));
            TextService.AddGlyph(em, ref fontEntity, 102, 2, 31, 17, 13, 32, new Rect(0.6367188f, 0.3710938f, 0.05078125f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 103, 2, 23, 25, 21, 32, new Rect(0.875f, 0.4023438f, 0.08203125f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 104, 2, 31, 25, 21, 32, new Rect(0.2851563f, 0.7109375f, 0.08203125f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 105, 2, 31, 11, 7, 32, new Rect(0.9648438f, 0.4023438f, 0.02734375f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 106, -3, 31, 11, 12, 41, new Rect(0.390625f, 0.5f, 0.046875f, 0.1601563f));
            TextService.AddGlyph(em, ref fontEntity, 107, 0, 31, 23, 21, 32, new Rect(0.4453125f, 0.5039063f, 0.08203125f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 108, 2, 31, 11, 7, 32, new Rect(0.6132813f, 0.6367188f, 0.02734375f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 109, 2, 23, 34, 30, 23, new Rect(0.2109375f, 0.2695313f, 0.1171875f, 0.08984375f));
            TextService.AddGlyph(em, ref fontEntity, 110, 2, 23, 23, 19, 23, new Rect(0.546875f, 0.7695313f, 0.07421875f, 0.08984375f));
            TextService.AddGlyph(em, ref fontEntity, 111, 2, 23, 23, 19, 23, new Rect(0.5195313f, 0.8671875f, 0.07421875f, 0.08984375f));
            TextService.AddGlyph(em, ref fontEntity, 112, 2, 23, 25, 21, 32, new Rect(0.5351563f, 0.5039063f, 0.08203125f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 113, 2, 23, 25, 21, 32, new Rect(0.6796875f, 0.5351563f, 0.08203125f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 114, 2, 23, 23, 19, 23, new Rect(0.8945313f, 0.6679688f, 0.07421875f, 0.08984375f));
            TextService.AddGlyph(em, ref fontEntity, 115, 2, 23, 23, 19, 23, new Rect(0.6289063f, 0.7695313f, 0.07421875f, 0.08984375f));
            TextService.AddGlyph(em, ref fontEntity, 116, 0, 31, 14, 12, 32, new Rect(0.625f, 0.5039063f, 0.046875f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 117, 2, 23, 23, 19, 23, new Rect(0.7109375f, 0.765625f, 0.07421875f, 0.08984375f));
            TextService.AddGlyph(em, ref fontEntity, 118, 2, 23, 23, 19, 23, new Rect(0.7929688f, 0.765625f, 0.07421875f, 0.08984375f));
            TextService.AddGlyph(em, ref fontEntity, 119, 2, 23, 34, 30, 23, new Rect(0.3359375f, 0.2695313f, 0.1171875f, 0.08984375f));
            TextService.AddGlyph(em, ref fontEntity, 120, 2, 23, 23, 19, 23, new Rect(0.875f, 0.765625f, 0.07421875f, 0.08984375f));
            TextService.AddGlyph(em, ref fontEntity, 121, 2, 23, 23, 19, 32, new Rect(0.8515625f, 0.5351563f, 0.07421875f, 0.125f));
            TextService.AddGlyph(em, ref fontEntity, 122, 2, 23, 23, 19, 23, new Rect(0.6367188f, 0.8671875f, 0.07421875f, 0.08984375f));
            TextService.AddGlyph(em, ref fontEntity, 123, 0, 31, 17, 15, 35, new Rect(0.1601563f, 0.859375f, 0.05859375f, 0.1367188f));
            TextService.AddGlyph(em, ref fontEntity, 124, 2, 31, 11, 7, 38, new Rect(0.3203125f, 0.84375f, 0.02734375f, 0.1484375f));
            TextService.AddGlyph(em, ref fontEntity, 125, 0, 31, 17, 15, 35, new Rect(0.1523438f, 0.4375f, 0.05859375f, 0.1367188f));
            TextService.AddGlyph(em, ref fontEntity, 126, 2, 20, 28, 24, 10, new Rect(0.796875f, 0.8632813f, 0.09375f, 0.0390625f));
            TextService.AddGlyph(em, ref fontEntity, 160, 0, 0, 14, 0, 0, new Rect(0.00390625f, 0.00390625f, 0.0f, 0.0f));
            TextService.AddGlyph(em, ref fontEntity, 32, 0, 35, 0, 0, 46, new Rect(0.00390625f, -0.1757813f, 0.0f, 0.1796875f));
            TextService.AddGlyph(em, ref fontEntity, 32, 0, 35, 0, 0, 46, new Rect(0.00390625f, -0.1757813f, 0.0f, 0.1796875f));
            TextService.AddGlyph(em, ref fontEntity, 10, 0, 35, 0, 10, 46, new Rect(0.0f, 0.8203125f, 0.0390625f, 0.1796875f));
            TextService.AddGlyph(em, ref fontEntity, 10, 0, 35, 0, 10, 46, new Rect(0.0f, 0.8203125f, 0.0390625f, 0.1796875f));
            TextService.AddGlyph(em, ref fontEntity, 9, 0, 35, 140, 140, 46, new Rect(0.00390625f, -0.1757813f, 0.546875f, 0.1796875f));
            return fontEntity;
        }
    }
}

