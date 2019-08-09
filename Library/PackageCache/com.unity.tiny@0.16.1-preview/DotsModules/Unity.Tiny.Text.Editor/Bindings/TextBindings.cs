using System;
using TMPro;
using Unity.Editor.Bindings;
using Unity.Entities;
using Unity.Tiny.Core;
using Unity.Tiny.Core2D;
using UnityEngine;
using UnityEngine.UI;
using Color = UnityEngine.Color;
using Unity.Editor.Extensions;

namespace Unity.Tiny.Text.Editor
{
    internal abstract class TextBindingsBase<TText> : IEntityBinding
        , IComponentBinding<Text2DRenderer, Text2DStyle>
        where TText : TMP_Text
    {
        protected virtual float SizeFactor => 1.0f;

        public void LoadBinding(Entity entity, IBindingContext context)
        {
            context.AddMissingUnityComponent<MeshRenderer>(entity,
                renderer => { renderer.sharedMaterial = new Material(Shader.Find("GUI/Text Shader")); });
            context.AddMissingUnityComponent<TText>(entity);
        }

        public void UnloadBinding(Entity entity, IBindingContext context)
        {
            context.RemoveUnityComponent<TText>(entity);
            context.RemoveUnityComponent<MeshRenderer>(entity);
        }

        public virtual void TransferToUnityComponents(Entity entity, IBindingContext context)
        {
            var text2DRenderer = context.GetComponentData<Text2DRenderer>(entity);
            var text2DStyle = context.GetComponentData<Text2DStyle>(entity);

            var text = context.GetUnityComponent<TText>(entity);
            if (context.HasComponent<TextString>(entity))
            {
                var textString = context.GetBufferRO<TextString>(entity).Reinterpret<char>().AsString();
                text.text = textString;
            }
            else
            {
                text.text = string.Empty;
            }

            text.fontStyle = FontStyles.Normal;
            text.lineSpacing = 1;
            text.richText = false;
            text.alignment = Fonts.GetTextAlignmentFromPivot(text2DRenderer.pivot);
            var c = text2DStyle.color;
            text.color = new Color(c.r, c.g, c.b, c.a);;
            text.fontSize = text2DStyle.size * SizeFactor;
            text.isOrthographic = true;
            text.enableWordWrapping = false;

            Transfer(entity, text, context);
        }


        protected virtual void Transfer(Entity entity, TText text, IBindingContext context)
        {
        }

        public void TransferFromUnityComponents(Entity entity, IBindingContext context)
        {
        }
    }

    internal class BitmapFontBindings<TText> : TextBindingsBase<TText>
        , IComponentBinding<Text2DStyleBitmapFont>
        where TText : TMP_Text
    {
        protected override void Transfer(Entity entity, TText text, IBindingContext context)
        {
            var text2DStyleBitmapFont = context.GetComponentData<Text2DStyleBitmapFont>(entity);
            try
            {
                text.font = context.GetUnityObject<TMP_FontAsset>(text2DStyleBitmapFont.font);
            }
            catch(NullReferenceException)
            {
                //TMP throws a NullReferenceException if TMP Essentials has not been imported. Catch the exception for now to ask to import it again
                Debug.LogError("This project contains some TextMeshPro assets and requires TMP Essentials. Make sure to import TMP Essentials: Close the project first, and go to Window->TextMeshPro->Import TMP Essential resources");
            }
        }
    }

    internal class NativeFontBindings<TText> : TextBindingsBase<TText>
        , IComponentBinding<Text2DStyleNativeFont>
        where TText : TMP_Text
    {
        protected override float SizeFactor => 4.0f/3.0f;

        protected override void Transfer(Entity entity, TText text, IBindingContext context)
        {
            var textMesh = context.GetUnityComponent<TText>(entity);
            var text2DStyleNativeFont = context.GetComponentData<Text2DStyleNativeFont>(entity);
            var nativeFont = context.GetComponentData<NativeFont>(entity);

            var italic = text2DStyleNativeFont.italic;

            if (italic)
            {
                textMesh.fontStyle = FontStyles.Italic;
            }
            else
            {
                textMesh.fontStyle = FontStyles.Normal;
            }

            switch (nativeFont.name)
            {
                case FontName.SansSerif:
                    textMesh.font = Fonts.GetSansSerifFont(italic);
                    break;
                case FontName.Serif:
                    textMesh.font = Fonts.GetSerifFont(italic);
                    break;
                case FontName.Monospace:
                    textMesh.font = Fonts.GetMonoSpaceFont(italic);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            //For the weight, let's update the dilate shader property, it should be present for font asset using a SDF Text mesh pro shader
            Material mat = textMesh.fontMaterial;
            float nativeFontWeight = Mathf.Clamp((float)text2DStyleNativeFont.weight, 400.0f, 700.0f);
            if (mat.HasProperty("_FaceDilate"))
            {
                //400 is regular(min) and 700 bold(max). font.boldStyle is the initial dilatation used when creating the font
                float value = (nativeFontWeight - 400) * textMesh.font.boldStyle / 700.0f;
                value = Mathf.Clamp(value, 0.0f, 1.0f);
                mat.SetFloat("_FaceDilate", value);
                textMesh.fontMaterial = mat;
            }

            //Add components for native builds if missing
            if(!context.HasComponent<NativeFontLoadFromFile>(entity))
                context.AddComponentData(entity, new NativeFontLoadFromFile());

            if (!context.HasComponent<NativeFontLoadFromFileName>(entity))
                context.AddBuffer<NativeFontLoadFromFileName>(entity);

            bool bold = text2DStyleNativeFont.weight >= 700;
            var packagePath = Unity.Editor.Application.PackageDirectory.FullName.ToForwardSlash();
            switch (nativeFont.name)
            {
                case FontName.SansSerif:
                    context.SetBufferFromString<NativeFontLoadFromFileName>(entity, TtfFonts.GetSansTtfFontPath(packagePath, italic, bold));
                    break;
                case FontName.Serif:
                    context.SetBufferFromString<NativeFontLoadFromFileName>(entity, TtfFonts.GetSerifTtfFontPath(packagePath, italic, bold));
                    break;
                case FontName.Monospace:
                    context.SetBufferFromString<NativeFontLoadFromFileName>(entity, TtfFonts.GetMonoTtfFontPath(packagePath, italic, bold));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    internal sealed class BitmapTextBaseBindings : BitmapFontBindings<TextMeshPro>
        , IBindingDependency<Core2D.Editor.ParentBindings>
        , IExcludeComponentBinding<UILayout.RectTransform>
    {
        public override void TransferToUnityComponents(Entity entity, IBindingContext context)
        {
            base.TransferToUnityComponents(entity, context);
            var rt = context.GetUnityComponent<RectTransform>(entity);
            rt.sizeDelta = Vector2.zero;
        }
    }

    internal sealed class UiBitmapTextBaseBindings : BitmapFontBindings<TextMeshProUGUI>
         , IComponentBinding<UILayout.RectTransform>
         , IBindingDependency<UILayout.Editor.RectTransformBindings>
    {
        protected override void Transfer(Entity entity, TextMeshProUGUI text, IBindingContext context)
        {
            base.Transfer(entity, text, context);
            try
            {
                if (context.HasComponent<Text2DAutoFit>(entity))
                {
                    var autoFit = context.GetComponentData<Text2DAutoFit>(entity);
                    text.enableAutoSizing = true;
                    text.fontSizeMin = autoFit.minSize * SizeFactor;
                    text.fontSizeMax = autoFit.maxSize * SizeFactor;
                    text.enableWordWrapping = false;
                }
                else
                {
                    text.enableAutoSizing = false;
                    text.enableWordWrapping = false;
                }
            }
            finally
            {
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(context.GetUnityComponent<RectTransform>(entity).root as RectTransform);
            }
        }
    }

    internal sealed class NativeTextBaseBindings : NativeFontBindings<TextMeshPro>
        , IExcludeComponentBinding<UILayout.RectTransform>
        , IBindingDependency<Core2D.Editor.ParentBindings>
    {
        public override void TransferToUnityComponents(Entity entity, IBindingContext context)
        {
            base.TransferToUnityComponents(entity, context);
            var rt = context.GetUnityComponent<RectTransform>(entity);
            rt.sizeDelta = Vector2.zero;
        }
    }

    internal sealed class UiNativeTextBaseBindings : NativeFontBindings<TextMeshProUGUI>
        , IComponentBinding<UILayout.RectTransform>
        , IBindingDependency<UILayout.Editor.RectTransformBindings>
    {
        protected override void Transfer(Entity entity, TextMeshProUGUI text, IBindingContext context)
        {
            base.Transfer(entity, text, context);
            try
            {
                if (context.HasComponent<Text2DAutoFit>(entity))
                {
                    var autoFit = context.GetComponentData<Text2DAutoFit>(entity);
                    text.enableAutoSizing = true;
                    text.fontSizeMin = autoFit.minSize * SizeFactor;
                    text.fontSizeMax = autoFit.maxSize * SizeFactor;
                    text.enableWordWrapping = false;
                }
                else
                {
                    text.enableAutoSizing = false;
                    text.enableWordWrapping = false;
                }
            }
            finally
            {
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(context.GetUnityComponent<RectTransform>(entity).root as RectTransform);
            }
        }
    }
}
