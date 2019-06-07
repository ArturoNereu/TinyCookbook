using Unity.Authoring;
using Unity.Editor.Bindings;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using RectTransform = Unity.Tiny.UILayout.RectTransform;

namespace Unity.Editor
{
    public class RectTransformInspector : IComponentInspector<RectTransform>, IInspectorTemplateProvider
    {
        private const float k_DropdownSize = 49;
        private const string k_TemplatePath = "Packages/com.unity.tiny/Editor Default Resources/{0}/rect-transform-inspector.{0}";

        private InspectorDataProxy<RectTransform> m_Proxy;
        private EntityManager m_EntityManager;

        private FloatField m_PosX;
        private FloatField m_PosY;
        private FloatField m_Left;
        private FloatField m_Right;
        private FloatField m_Top;
        private FloatField m_Bottom;
        private FloatField m_Width;
        private FloatField m_Height;
        private Vector2Field m_AnchorMin;
        private Vector2Field m_AnchorMax;
        private Vector2Field m_Pivot;
        private Button m_BlueprintButton;
        private Button m_RawEditButton;

        private bool RawEditMode
        {
            get => EditorPrefs.GetBool(k_LockRectPrefName, false);
            set => EditorPrefs.SetBool(k_LockRectPrefName, value);
        }

        private static Vector2 s_StartDragAnchorMin = Vector2.zero;
        private static Vector2 s_StartDragAnchorMax = Vector2.zero;
        private const string k_LockRectPrefName = "RectTransformEditor.lockRect";

        public VisualElement Build(InspectorDataProxy<RectTransform> proxy)
        {
            m_EntityManager = proxy.Session.GetManager<IWorldManager>().EntityManager;
            m_Proxy = proxy;

            var root = this.BuildFromTemplate();
            var foldout = root.Q<Foldout>();
            foldout.text = "Anchor";

            FindAndRegisterCallbacks<FloatField, float>(root, out m_PosX, nameof(m_PosX), PositionXChanged);
            FindAndRegisterCallbacks<FloatField, float>(root, out m_PosY, nameof(m_PosY), PositionYChanged);
            FindAndRegisterCallbacks<FloatField, float>(root, out m_Left, nameof(m_Left), LeftChanged);
            FindAndRegisterCallbacks<FloatField, float>(root, out m_Right, nameof(m_Right), RightChanged);
            FindAndRegisterCallbacks<FloatField, float>(root, out m_Top, nameof(m_Top), TopChanged);
            FindAndRegisterCallbacks<FloatField, float>(root, out m_Bottom, nameof(m_Bottom), BottomChanged);
            FindAndRegisterCallbacks<FloatField, float>(root, out m_Width, nameof(m_Width), WidthChanged);
            FindAndRegisterCallbacks<FloatField, float>(root, out m_Height, nameof(m_Height), HeightChanged);
            FindAndRegisterCallbacks<Vector2Field, Vector2>(root, out m_AnchorMin, nameof(m_AnchorMin), AnchorMinChanged);
            FindAndRegisterCallbacks<Vector2Field, Vector2>(root, out m_AnchorMax, nameof(m_AnchorMax), AnchorMaxChanged);
            FindAndRegisterCallbacks<Vector2Field, Vector2>(root, out m_Pivot, nameof(m_Pivot), PivotChanged);
            FindAndRegisterCallbacks(root, out m_BlueprintButton, nameof(m_BlueprintButton), BlueprintButtonClicked, "RectTransformBlueprint", "blueprint-image");
            FindAndRegisterCallbacks(root, out m_RawEditButton, nameof(m_RawEditButton), RawEditButtonClicked, "RectTransformRaw", "raw-edit-image");

            var topImage = root.Q<VisualElement>("LayoutDropdown");
            topImage.RegisterCallback<MouseUpEvent>(ShowDropdownWindow);

            var popup = root.Q<IMGUIContainer>("CurrentLayoutImage");
            popup.onGUIHandler = LayoutDropdownButton;
            return root;
        }

        private static void FindAndRegisterCallbacks<TField, TValue>(VisualElement root, out TField field, string name,
            EventCallback<ChangeEvent<TValue>> handler)
            where TField : BaseField<TValue>
        {
            field = root.Q<TField>(name);
            field.RegisterValueChangedCallback(handler);
        }

        private static void FindAndRegisterCallbacks(VisualElement root, out Button button, string name, EventCallback<ChangeEvent<MouseUpEvent>> handler, string iconName, string className)
        {
            button = root.Q<Button>(name);
            button.RegisterCallback(handler);

            var image = new Image
            {
                image = EditorGUIUtility.IconContent(iconName).image,
                scaleMode = ScaleMode.ScaleToFit
            };
            image.AddToClassList(className);
            button.contentContainer.Add(image);
        }

        public void Update(InspectorDataProxy<RectTransform> proxy)
        {
            var rt = m_Proxy.Data;
            m_PosX.SetValueWithoutNotify(rt.anchoredPosition.x);
            m_Left.SetValueWithoutNotify(rt.offsetMin.x);

            m_PosY.SetValueWithoutNotify(rt.anchoredPosition.y);
            m_Top.SetValueWithoutNotify(-rt.offsetMax.x);

            m_Width.SetValueWithoutNotify(rt.sizeDelta.x);
            m_Right.SetValueWithoutNotify(-rt.offsetMax.x);

            m_Height.SetValueWithoutNotify(rt.sizeDelta.y);
            m_Bottom.SetValueWithoutNotify(rt.offsetMin.x);

            m_AnchorMin.SetValueWithoutNotify(rt.anchorMin);
            m_AnchorMax.SetValueWithoutNotify(rt.anchorMax);
            m_Pivot.SetValueWithoutNotify(rt.pivot);

            SetButtonActiveState(m_BlueprintButton, UnityEditor.Tools.rectBlueprintMode);
            SetButtonActiveState(m_RawEditButton, RawEditMode);

            UpdateVisibility(rt);
        }

        public VisualTreeAsset UxmlAsset { get; } = EditorGUIUtility.Load(string.Format(k_TemplatePath, "uxml")) as VisualTreeAsset;
        public StyleSheet UssAsset { get; }= AssetDatabase.LoadAssetAtPath<StyleSheet>(string.Format(k_TemplatePath, "uss"));
        public bool AutoRegisterBindings { get; } = false;

        public static void SetAnchorSmart(Entity entity, ref RectTransform rt, float value, int axis, bool isMax, bool smart, bool enforceExactValue)
        {
            SetAnchorSmart(entity, ref rt, value, axis, isMax, smart, enforceExactValue, false, false);
        }

        private static void SetAnchorSmart(Entity entity, ref RectTransform rt, float value, int axis, bool isMax, bool smart, bool enforceExactValue, bool enforceMinNoLargerThanMax, bool moveTogether)
        {
            UnityEngine.RectTransform parent = null;
            var cache = Application.AuthoringProject.Session.GetManager<IUnityComponentCacheManager>();
            var reference = cache.GetEntityReference(entity);
            var parentTransform = reference.transform.parent;
            if (parentTransform == null)
            {
                smart = false;
            }
            else
            {
                parent = parentTransform.GetComponent<UnityEngine.RectTransform>();
                if (parent == null)
                    smart = false;
            }

            var clampToParent = !AnchorAllowedOutsideParent(axis, isMax ? 1 : 0);
            if (clampToParent)
            {
                value = Mathf.Clamp01(value);
            }

            if (enforceMinNoLargerThanMax)
            {
                value = isMax ? Mathf.Max(value, rt.anchorMin[axis]) : Mathf.Min(value, rt.anchorMax[axis]);
            }

            float offsetSizePixels = 0;
            float offsetPositionPixels = 0;
            if (smart)
            {
                var oldValue = isMax ? rt.anchorMax[axis] : rt.anchorMin[axis];

                offsetSizePixels = (value - oldValue) * parent.rect.size[axis];

                // Ensure offset is in whole pixels.
                // Note: In this particular instance we want to use Mathf.Round (which rounds towards nearest even number)
                // instead of Round from this class which always rounds down.
                // This makes the position of rect more stable when their anchors are changed.
                float roundingDelta = 0;
                if (ShouldDoIntSnapping())
                    roundingDelta = Mathf.Round(offsetSizePixels) - offsetSizePixels;
                offsetSizePixels += roundingDelta;

                if (!enforceExactValue)
                {
                    value += roundingDelta / parent.rect.size[axis];

                    // Snap value to whole percent if close
                    if (Mathf.Abs(Round(value * 1000) - value * 1000) < 0.1f)
                        value = Round(value * 1000) * 0.001f;

                    if (clampToParent)
                        value = Mathf.Clamp01(value);
                    if (enforceMinNoLargerThanMax)
                    {
                        value = isMax ? Mathf.Max(value, rt.anchorMin[axis]) : Mathf.Min(value, rt.anchorMax[axis]);
                    }
                }

                if (moveTogether)
                    offsetPositionPixels = offsetSizePixels;
                else
                    offsetPositionPixels = (isMax ? offsetSizePixels * rt.pivot[axis] : (offsetSizePixels * (1 - rt.pivot[axis])));
            }

            if (isMax)
            {
                var rectAnchorMax = rt.anchorMax;
                rectAnchorMax[axis] = value;
                rt.anchorMax = rectAnchorMax;

                var other = rt.anchorMin;
                if (moveTogether)
                    other[axis] = s_StartDragAnchorMin[axis] + rectAnchorMax[axis] - s_StartDragAnchorMax[axis];
                rt.anchorMin = other;
            }
            else
            {
                var rectAnchorMin = rt.anchorMin;
                rectAnchorMin[axis] = value;
                rt.anchorMin = rectAnchorMin;

                var other = rt.anchorMax;
                if (moveTogether)
                    other[axis] = s_StartDragAnchorMax[axis] + rectAnchorMin[axis] - s_StartDragAnchorMin[axis];
                rt.anchorMax = other;
            }

            if (smart)
            {
                var rectPosition = rt.anchoredPosition;
                rectPosition[axis] -= offsetPositionPixels;
                rt.anchoredPosition = rectPosition;

                if (!moveTogether)
                {
                    var rectSizeDelta = rt.sizeDelta;
                    rectSizeDelta[axis] += offsetSizePixels * (isMax ? -1 : 1);
                    rt.sizeDelta = rectSizeDelta;
                }
            }
        }

        private static bool AnchorAllowedOutsideParent(int axis, int minmax)
        {
            // Allow dragging outside if action key is held down (same key that disables snapping).
            // Also allow when not dragging at all - for e.g. typing values into the Inspector.
            if (EditorGUI.actionKey || EditorGUIUtility.hotControl == 0)
                return true;
            // Also allow if drag started outside of range to begin with.
            var value = (minmax == 0 ? s_StartDragAnchorMin[axis] : s_StartDragAnchorMax[axis]);
            return (value < -0.001f || value > 1.001f);
        }

        private static float Round(float value)
        {
            return Mathf.Floor(0.5f + value);
        }

        private static bool ShouldDoIntSnapping()
        {
            // We don't support WorldSpace as of yet.
            return true;
        }

        public static void SetPivotSmart(Entity entity, ref RectTransform rt, float value, int axis, bool smart)
        {
            var cache = Application.AuthoringProject.Session.GetManager<IUnityComponentCacheManager>();
            var reference = cache.GetEntityReference(entity);
            var rect = reference.GetComponent<UnityEngine.RectTransform>();
            if (rect == null)
            {
                smart = false;
            }

            var cornerBefore = GetRectReferenceCorner(rect);

            var rectPivot = rt.pivot;
            rectPivot[axis] = value;
            rt.pivot = rectPivot;

            rectPivot = rect.pivot;
            rectPivot[axis] = value;
            rect.pivot = rectPivot;

            if (smart)
            {
                var cornerAfter = GetRectReferenceCorner(rect);
                var cornerOffset = cornerAfter - cornerBefore;
                var anchoredPosition = rt.anchoredPosition;
                anchoredPosition = anchoredPosition - (float2)(Vector2)cornerOffset;
                rt.anchoredPosition = anchoredPosition;

                var pos = rect.transform.position;
                pos.z -= cornerOffset.z;
                rect.transform.position = pos;
            }
        }

        private static Vector3 GetRectReferenceCorner(UnityEngine.RectTransform gui)
        {
            return (Vector3)gui.rect.min + gui.transform.localPosition;
        }

        private void LayoutDropdownButton()
        {
            var anyWithoutParent = false;
            var dropdownPosition = GUILayoutUtility.GetRect(0, 0);
            dropdownPosition.x += 17;
            dropdownPosition.y += 17;
            dropdownPosition.height = k_DropdownSize;
            dropdownPosition.width = k_DropdownSize;

            if (!anyWithoutParent)
            {
                var data = m_Proxy.Data;
                LayoutDropdownWindow.DrawLayoutMode(new RectOffset(7, 7, 7, 7).Remove(dropdownPosition), data.anchorMin, data.anchorMax);
                LayoutDropdownWindow.DrawLayoutModeHeadersOutsideRect(dropdownPosition, data.anchorMin, data.anchorMax);
            }
        }

        private void ShowDropdownWindow(MouseUpEvent evt)
        {
            var window = new LayoutDropdownWindow(m_EntityManager, m_Proxy.Targets);
            Bridge.PopupWindow.Show((evt.target as VisualElement).worldBound, window);
        }

        private void PositionXChanged(ChangeEvent<float> evt)
        {
            var data = m_Proxy.Data;
            data.anchoredPosition.x = evt.newValue;
            m_Proxy.Data = data;
            Update(m_Proxy);
            UpdateVisibility(data);
        }

        private void LeftChanged(ChangeEvent<float> evt)
        {
            var data = m_Proxy.Data;
            var offsetMin = data.offsetMin;
            offsetMin.x = evt.newValue;
            data.offsetMin = offsetMin;
            m_Proxy.Data = data;
            Update(m_Proxy);
            UpdateVisibility(data);
        }

        private void PositionYChanged(ChangeEvent<float> evt)
        {
            var data = m_Proxy.Data;
            data.anchoredPosition.y = evt.newValue;
            m_Proxy.Data = data;
            Update(m_Proxy);
            UpdateVisibility(data);
        }

        private void TopChanged(ChangeEvent<float> evt)
        {
            var data = m_Proxy.Data;
            var offsetMax = data.offsetMax;
            offsetMax.x = -evt.newValue;
            data.offsetMax = offsetMax;
            m_Proxy.Data = data;
            Update(m_Proxy);
            UpdateVisibility(data);
        }

        private void WidthChanged(ChangeEvent<float> evt)
        {
            var data = m_Proxy.Data;
            var sizeDelta = data.sizeDelta;
            sizeDelta.x = evt.newValue;
            data.sizeDelta = sizeDelta;
            m_Proxy.Data = data;
            UpdateVisibility(data);
        }

        private void RightChanged(ChangeEvent<float> evt)
        {
            var data = m_Proxy.Data;
            var offsetMax = data.offsetMax;
            offsetMax.x = -evt.newValue;
            data.offsetMax = offsetMax;
            m_Proxy.Data = data;
            Update(m_Proxy);
            UpdateVisibility(data);
        }

        private void HeightChanged(ChangeEvent<float> evt)
        {
            var data = m_Proxy.Data;
            var sizeDelta = data.sizeDelta;
            sizeDelta.y = evt.newValue;
            data.sizeDelta = sizeDelta;
            m_Proxy.Data = data;
            UpdateVisibility(data);
        }

        private void BottomChanged(ChangeEvent<float> evt)
        {
            var data = m_Proxy.Data;
            var offsetMin = data.offsetMin;
            offsetMin.y = evt.newValue;
            data.offsetMin = offsetMin;
            m_Proxy.Data = data;
            Update(m_Proxy);
            UpdateVisibility(data);
        }

        private void AnchorMinChanged(ChangeEvent<Vector2> evt)
        {
            var data = m_Proxy.Data;
            SetAnchorSmart(m_Proxy.MainTarget, ref data, evt.newValue.x, 0, false, !RawEditMode, true);
            SetAnchorSmart(m_Proxy.MainTarget, ref data, evt.newValue.y, 1, false, !RawEditMode, true);
            m_Proxy.Data = data;
            Update(m_Proxy);
            UpdateVisibility(data);
        }

        private void AnchorMaxChanged(ChangeEvent<Vector2> evt)
        {
            var data = m_Proxy.Data;
            SetAnchorSmart(m_Proxy.MainTarget, ref data, evt.newValue.x, 0, true, !RawEditMode, true);
            SetAnchorSmart(m_Proxy.MainTarget, ref data, evt.newValue.y, 1, true, !RawEditMode, true);
            m_Proxy.Data = data;
            Update(m_Proxy);
            UpdateVisibility(data);
        }

        private void PivotChanged(ChangeEvent<Vector2> evt)
        {
            var data = m_Proxy.Data;
            SetPivotSmart(m_Proxy.MainTarget, ref data, evt.newValue.x, 0, !RawEditMode);
            SetPivotSmart(m_Proxy.MainTarget, ref data, evt.newValue.y, 1, !RawEditMode);
            m_Proxy.Data = data;
            Update(m_Proxy);
            UpdateVisibility(data);
        }

        private void BlueprintButtonClicked(ChangeEvent<MouseUpEvent> evt)
        {
            UnityEditor.Tools.rectBlueprintMode = !UnityEditor.Tools.rectBlueprintMode;
            Update(m_Proxy);
            UpdateVisibility(m_Proxy.Data);
        }

        private void RawEditButtonClicked(ChangeEvent<MouseUpEvent> evt)
        {
            RawEditMode = !RawEditMode;
            Update(m_Proxy);
            UpdateVisibility(m_Proxy.Data);
        }

        private void UpdateVisibility(RectTransform rt)
        {
            var stretchX = !Mathf.Approximately(rt.anchorMin.x, rt.anchorMax.x);
            var stretchY = !Mathf.Approximately(rt.anchorMin.y, rt.anchorMax.y);
            m_Width.style.display = m_PosX.style.display = !stretchX ? DisplayStyle.Flex : DisplayStyle.None;
            m_Height.style.display = m_PosY.style.display = !stretchY ? DisplayStyle.Flex : DisplayStyle.None;
            m_Right.style.display = m_Left.style.display =  stretchX? DisplayStyle.Flex : DisplayStyle.None;
            m_Bottom.style.display = m_Top.style.display = stretchY ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private static void SetButtonActiveState(Button button, bool isActive)
        {
            if (isActive)
            {
                button.AddToClassList("mode-button-active");
            }
            else
            {
                button.RemoveFromClassList("mode-button-active");
            }
        }
    }
}
