using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Editor.Bindings;
using Unity.Tiny.Core2D;
using Unity.Entities;
using Unity.Tiny.Core2DTypes.Editor;

namespace Unity.Tiny.UILayout.Editor
{
    [UsedImplicitly]
    internal class UISprite2DRendererBindings : IEntityBinding,
        IComponentBinding<Parent, Translation, RectTransform, Sprite2DRenderer>,
        IBindingDependency<RectTransformBindings>
    {
        private static readonly Dictionary<BlendOp, UnityEngine.Vector2> k_BlendModes = new Dictionary<BlendOp, UnityEngine.Vector2>
        {
            { BlendOp.Alpha, new UnityEngine.Vector2 ((float)UnityEngine.Rendering.BlendMode.SrcAlpha, (float) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha) }, // alpha
            { BlendOp.Add, new UnityEngine.Vector2 ((float)UnityEngine.Rendering.BlendMode.SrcAlpha, (float) UnityEngine.Rendering.BlendMode.One) },              // add
            { BlendOp.Multiply, new UnityEngine.Vector2 ((float)UnityEngine.Rendering.BlendMode.DstColor, (float) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha) }  // multiply
        };

        public void LoadBinding(Entity entity, IBindingContext context)
        {
            context.AddMissingUnityComponent<UnityEngine.CanvasRenderer>(entity);
            context.AddMissingUnityComponent<UnityEngine.UI.Image>(entity, image =>
            {
                var mat = image.material = new UnityEngine.Material(UnityEngine.Shader.Find("Tiny/Sprite2D"));
                mat.renderQueue = 3000;
            });
        }

        public void UnloadBinding(Entity entity, IBindingContext context)
        {
            context.RemoveUnityComponent<UnityEngine.UI.Image>(entity);
            context.RemoveUnityComponent<UnityEngine.CanvasRenderer>(entity);
        }

        public void TransferToUnityComponents(Entity entity, IBindingContext context)
        {
            var image = context.GetUnityComponent<UnityEngine.UI.Image>(entity);

            var sprite2DRenderer = context.GetComponentData<Sprite2DRenderer>(entity);

            image.sprite = context.GetUnityObject<UnityEngine.Sprite>(sprite2DRenderer.sprite);
            image.color = sprite2DRenderer.color.Convert();

            var rt = context.GetUnityComponent<UnityEngine.RectTransform>(entity);
            if (null != rt && rt)
            {
                if (context.HasComponent<Sprite2DRendererOptions>(entity))
                {
                    var tinyOptions = context.GetComponentData<Sprite2DRendererOptions>(entity);
                    tinyOptions.size =  rt.rect.size;
                    var drawMode = tinyOptions.drawMode;
                    switch (drawMode)
                    {
                        case DrawMode.Stretch:
                        {
                            image.type = UnityEngine.UI.Image.Type.Sliced;
                            break;
                        }
                        case DrawMode.AdaptiveTiling:
                        case DrawMode.ContinuousTiling:
                        {
                            image.type = UnityEngine.UI.Image.Type.Tiled;
                            break;
                        }
                    }
                }
            }

            if (k_BlendModes.TryGetValue(sprite2DRenderer.blending, out var blendMode))
            {
                var mat = image.material;
                mat.SetFloat("_SrcMode", blendMode.x);
                mat.SetFloat("_DstMode", blendMode.y);
                mat.SetColor("_Color", image.color);
            }
            else
            {
                UnityEngine.Debug.Log($"Tiny: Unknown blending mode, of value '{sprite2DRenderer.blending}'");
            }
        }

        public void TransferFromUnityComponents(Entity entity, IBindingContext context)
        {
            var image = context.GetUnityComponent<UnityEngine.UI.Image>(entity);

            context.SetComponentData(entity, new Sprite2DRenderer()
            {
                sprite = context.GetEntity(image.sprite),
                color = image.color.Convert()
            });

            var rt = context.GetUnityComponent<UnityEngine.RectTransform>(entity);

            var optionsData = new Sprite2DRendererOptions()
            {
                size = rt.rect.size
            };


            if (image.type == UnityEngine.UI.Image.Type.Simple || image.type == UnityEngine.UI.Image.Type.Sliced)
            {
                optionsData.drawMode = DrawMode.Stretch;
            }
            else
            {
                optionsData.drawMode = DrawMode.ContinuousTiling;
            }
            if (context.HasComponent<Sprite2DRendererOptions>(entity))
            {
                context.SetComponentData(entity, optionsData);
            }
            else
            {
                context.AddComponentData(entity, optionsData);
            }
        }

        private static DrawMode Translate(UnityEngine.SpriteDrawMode drawMode, UnityEngine.SpriteTileMode tileMode)
        {
            switch (drawMode)
            {
                case UnityEngine.SpriteDrawMode.Sliced:
                    return DrawMode.Stretch;
                case UnityEngine.SpriteDrawMode.Tiled:
                    return tileMode == UnityEngine.SpriteTileMode.Continuous ? DrawMode.ContinuousTiling : DrawMode.AdaptiveTiling;
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(drawMode));
            }
        }
    }
}
