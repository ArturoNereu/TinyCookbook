using Unity.Entities;
using Unity.Tiny;
using Unity.Tiny.Core2D;

/**
 * UIControls module that allows you to create user interfaces. This module contains
 * components for adding UI elements such as buttons and toggles, and defines the
 * interaction with the pointer.
 * @module
 * @name Unity.Tiny
 */
[assembly: ModuleDescription("Unity.Tiny.UIControls", "Basic UI controls")]
namespace Unity.Tiny.UIControls
{
    public delegate void EventCallback(Entity e);

    /// <summary>
    ///  Captures the interaction between the mouse/touch and the UI control.
    ///  This component requires the RectTransform component, from the UILayout module.
    /// </summary>
    public struct PointerInteraction : IComponentData
    {
        /// <summary>
        ///  True if the mouse button is pressed and the press started when the
        ///  cursor was over the UI control.
        /// </summary>
        public bool down;

        /// <summary>
        ///  True if the cursor is inside the bounds of the UI control.
        /// </summary>
        public bool over;

        /// <summary>
        ///  True if the UI control is clicked. A click consists of a mouse-down
        ///  action and a corresponding mouse-up action while the cursor is inside
        ///  the control's bounds.
        /// </summary>
        public bool clicked;
    }

    /// <summary>
    ///  Component for UI buttons.
    /// </summary>
    public struct Button : IComponentData
    {
        /// <summary>
        ///  Reference to an entity with a Sprite2DRenderer component that represents
        ///  the button's default state. Mouse/touch interaction, captured by the
        ///  PointerInteraction component, swaps or modifies the sprite based on the
        ///  type of transition you apply.
        ///  If this is set to NONE, it assumes that the underlying entity (the one
        ///  that the Button component is attached to) also has a Sprite2DRenderer
        ///  component, and uses that.
        /// </summary>
        public Entity sprite2DRenderer;

        /// <summary>
        ///  Reference to an entity that defines visual transitions based on mouse/
        ///  touch interaction captured by the PointerInteraction component. For example,
        ///  A SpriteTransition or ColorTintTransition component.
        /// </summary>
        public Entity transition;
    }

    /// <summary>
    /// Component for toggle buttons.
    /// </summary>
    public struct Toggle : IComponentData
    {
        /// <summary>
        ///  True if the toggle is on (for example, checked).
        /// </summary>
        public bool isOn;

        /// <summary>
        ///  Reference to an entity with a Sprite2DRenderer component that represents
        ///  the button's toggle's state. Mouse/touch interaction, captured by the
        ///  PointerInteraction component, swaps or modifies the sprite based on the
        ///  type of transitions you apply.
        ///  If this is set to NONE, it assumes that the underlying entity (the one
        ///  that the Toggle component is attached to) also has a Sprite2DRenderer
        ///  component, and uses that.
        /// </summary>
        public Entity sprite2DRenderer;

        /// <summary>
        ///  Reference to an entity that defines visual transitions based on mouse/
        ///  touch interaction captured by the PointerInteraction component. For example,
        ///  A SpriteTransition or ColorTintTransition component.
        ///  Used when isOn is true.
        /// </summary>
        public Entity transition;

        /// <summary>
        ///  Reference to an entity that defines visual transitions based on mouse/
        ///  touch interaction captured by the PointerInteraction component. For example,
        ///  A SpriteTransition or ColorTintTransition component.
        ///  Used when isOn is false.
        /// </summary>
        public Entity transitionChecked;
    }

    /// <summary>
    ///  Applies standard sprite-swap effect on controls that have a PointerInteraction
    ///  component.
    /// </summary>
    public struct SpriteTransition : IComponentData
    {
        /// <summary>
        ///  The sprite used when PointerInteraction.down = false and PointerInteraction.over = false.
        /// </summary>
        [EntityWithComponents(typeof(Sprite2D))] public Entity normal;

        /// <summary>
        ///  The sprite used when PointerInteraction.down = false and PointerInteraction.over = false.
        /// </summary>
        [EntityWithComponents(typeof(Sprite2D))] public Entity hover;

        /// <summary>
        ///  The sprite used when PointerInteraction.down = true.
        /// </summary>
        [EntityWithComponents(typeof(Sprite2D))] public Entity pressed;

        /// <summary>
        ///  The sprite used when the entity has an InactiveUIElement component.
        /// </summary>
        [EntityWithComponents(typeof(Sprite2D))] public Entity disabled;
    };

    /// <summary>
    /// Applies a standard color-tint effect on controls that have a PointerInteraction
    /// component.
    /// </summary>
    public struct ColorTintTransition : IComponentData
    {
        /// <summary>
        ///  The color used when PointerInteraction.down = false and PointerInteraction.over = false.
        /// </summary>
        public Color normal;

        /// <summary>
        ///  The color used when PointerInteraction.down = false and PointerInteraction.over = false.
        /// </summary>
        public Color hover;

        /// <summary>
        ///  The color used when PointerInteraction.down = true.
        /// </summary>
        public Color pressed;

        /// <summary>
        ///  The color used when the entity has InactiveUIElement component.
        /// </summary>
        public Color disabled;
    }

    /// <summary>
    ///  Disables the UI control and resets the PointerInteraction component.
    /// </summary>
    public struct InactiveUIControl : IComponentData
    {
    };

    struct InitialColor : IComponentData
    {
        public Color color;
    };
}
