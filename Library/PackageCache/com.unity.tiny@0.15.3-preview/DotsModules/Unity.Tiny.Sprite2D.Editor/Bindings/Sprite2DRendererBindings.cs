using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Entities;
using Unity.Tiny.Core2DTypes.Editor;
using Unity.Editor.Bindings;
using UnityEngine;
using RectTransform = Unity.Tiny.UILayout.RectTransform;

namespace Unity.Tiny.Core2D.Editor
{
    [UsedImplicitly]
    internal class Sprite2DRendererBindings: IEntityBinding,
        IComponentBinding<Parent>,
        IComponentBinding<Sprite2DRenderer>,
        IExcludeComponentBinding<RectTransform>,
        IBindingDependency<ParentBindings>
    {
        private static Material s_Material;

        private Material Sprite2DMaterial
        {
            get
            {
                if (null == s_Material || !s_Material)
                {
                    s_Material = new UnityEngine.Material(UnityEngine.Shader.Find("Tiny/Sprite2D"));
                }

                return s_Material;
            }
        }

        private static readonly Dictionary<BlendOp, UnityEngine.Vector2> k_BlendModes = new Dictionary<BlendOp, UnityEngine.Vector2>
        {
            { BlendOp.Alpha, new UnityEngine.Vector2 ((float)UnityEngine.Rendering.BlendMode.SrcAlpha, (float) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha) }, // alpha
            { BlendOp.Add, new UnityEngine.Vector2 ((float)UnityEngine.Rendering.BlendMode.SrcAlpha, (float) UnityEngine.Rendering.BlendMode.One) },              // add
            { BlendOp.Multiply, new UnityEngine.Vector2 ((float)UnityEngine.Rendering.BlendMode.DstColor, (float) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha) }  // multiply
        };

        private static readonly Sprite s_WhiteSprite = UnityEngine.Sprite.Create(UnityEngine.Texture2D.whiteTexture,
            new UnityEngine.Rect(UnityEngine.Vector2.zero,
                new UnityEngine.Vector2(UnityEngine.Texture2D.whiteTexture.width,
                    UnityEngine.Texture2D.whiteTexture.height)), UnityEngine.Vector2.one / 2.0f);

        public void LoadBinding(Entity entity, IBindingContext context)
        {
            context.AddMissingUnityComponent<UnityEngine.SpriteRenderer>(entity, r =>
            {
                var mat = r.sharedMaterial = Sprite2DMaterial;
                mat.renderQueue = 3000;
            });
        }

        public void UnloadBinding(Entity entity, IBindingContext context)
        {
            context.RemoveUnityComponent<UnityEngine.SpriteRenderer>(entity);
        }

        public void TransferToUnityComponents(Entity entity, IBindingContext context)
        {
            try
            {
                var spriteRenderer = context.GetUnityComponent<UnityEngine.SpriteRenderer>(entity);
                var sprite2DRenderer = context.GetComponentData<Sprite2DRenderer>(entity);
                var sprite = context.GetUnityObject<UnityEngine.Sprite>(sprite2DRenderer.sprite);

                var block = new UnityEngine.MaterialPropertyBlock();
                spriteRenderer.GetPropertyBlock(block);
                block.Clear();

                spriteRenderer.color = sprite2DRenderer.color.Convert();
                block.SetColor("_Color", sprite2DRenderer.color.Convert());

                if (sprite)
                {
                    spriteRenderer.sprite = sprite;
                    var blending = sprite2DRenderer.blending;
                    if (k_BlendModes.TryGetValue(blending, out var blendMode))
                    {
                        spriteRenderer.sharedMaterial.SetFloat("_SrcMode", blendMode.x);
                        spriteRenderer.sharedMaterial.SetFloat("_DstMode", blendMode.y);
                    }
                    else
                    {
                        UnityEngine.Debug.Log($"Tiny: Unknown blending mode, of value '{blending}'");
                    }

                    block.SetTexture("_MainTex", sprite.texture);
                }
                else
                {
                    spriteRenderer.sprite = s_WhiteSprite;
                    if (!context.HasComponent<Sprite2DRendererOptions>(entity))
                    {
                        spriteRenderer.size = UnityEngine.Vector2.one;

                    }
                }

                spriteRenderer.SetPropertyBlock(block);

                if (context.HasComponent<Sprite2DRendererOptions>(entity))
                {
                    var options = context.GetComponentData<Sprite2DRendererOptions>(entity);
                    SetDrawMode(spriteRenderer, options.drawMode);
                    spriteRenderer.size = options.size;
                }
                else
                {
                    spriteRenderer.drawMode = UnityEngine.SpriteDrawMode.Simple;
                }
            }
            finally
            {
                UnityEditor.SceneView.RepaintAll();
            }
        }

        public void TransferFromUnityComponents(Entity entity, IBindingContext context)
        {
            var spriteRenderer = context.GetUnityComponent<UnityEngine.SpriteRenderer>(entity);

            context.SetComponentData(entity, new Sprite2DRenderer
            {
                sprite = s_WhiteSprite != spriteRenderer.sprite ? context.GetEntity(spriteRenderer.sprite) : Entity.Null,
                color = spriteRenderer.color.Convert()
            });

            if (spriteRenderer.drawMode != UnityEngine.SpriteDrawMode.Simple)
            {
                var optionsData = new Sprite2DRendererOptions()
                {
                    drawMode = Translate(spriteRenderer.drawMode, spriteRenderer.tileMode),
                    size = spriteRenderer.size
                };

                if (context.HasComponent<Sprite2DRendererOptions>(entity))
                {
                    context.SetComponentData(entity, optionsData);
                }
                else
                {
                    context.AddComponentData(entity, optionsData);
                }
            }
            else
            {
                context.RemoveComponent<Sprite2DRendererOptions>(entity);
            }
        }

        private void SetDrawMode(UnityEngine.SpriteRenderer renderer, DrawMode mode)
        {
            switch (mode)
            {
                case DrawMode.ContinuousTiling:
                    renderer.drawMode = UnityEngine.SpriteDrawMode.Tiled;
                    renderer.tileMode = UnityEngine.SpriteTileMode.Continuous;
                    break;
                case DrawMode.AdaptiveTiling:
                    renderer.drawMode = UnityEngine.SpriteDrawMode.Tiled;
                    renderer.tileMode = UnityEngine.SpriteTileMode.Adaptive;
                    break;
                case DrawMode.Stretch:
                    renderer.drawMode = UnityEngine.SpriteDrawMode.Sliced;
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(mode));
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
