using Unity.Entities;
using Unity.Tiny.Watchers;

namespace Unity.Tiny.UIControls
{
    /// <summary>
    ///  Util class for UI controls.
    /// </summary>
    public static class UIControlsService
    {
        public static void SetEnabled(ComponentSystem sys, Entity uiControl, bool enabled)
        {
            var mgr = sys.World.EntityManager;

            if (!mgr.Exists(uiControl))
                return;

            bool hasInactive = mgr.HasComponent<InactiveUIControl>(uiControl);
            if (enabled && hasInactive)
                mgr.RemoveComponent<InactiveUIControl>(uiControl);
            else if (!enabled && !hasInactive)
                mgr.AddComponent(uiControl, typeof(InactiveUIControl));
        }

        public static bool IsEnabled(ComponentSystem sys, Entity uiControl)
        {
            var mgr = sys.World.EntityManager;

            if (!mgr.Exists(uiControl))
                return false;

            return !mgr.HasComponent<InactiveUIControl>(uiControl);
        }

        /// <summary>
        /// Sets the callback that is called when the control is clicked.
        /// </summary>
        public static int AddOnClickCallback(ComponentSystem sys, Entity uiControl, EventCallback callback)
        {
            return AddCallback(sys, uiControl, "PointerInteraction.clicked", false, callback);
        }

        /// <summary>
        ///  Sets the callback that is called when the control is pressed.
        /// </summary>
        public static int AddOnDownCallback(ComponentSystem sys, Entity uiControl, EventCallback callback)
        {
            return AddCallback(sys, uiControl, "PointerInteraction.down", false, callback);
        }

        /// <summary>
        ///  Sets the callback that is called when the control is released.
        /// </summary>
        public static int AddOnUpCallback(ComponentSystem sys, Entity uiControl, EventCallback callback)
        {
            return AddCallback(sys, uiControl, "PointerInteraction.down", true, callback);
        }

        /// <summary>
        ///  Sets the callback that is called when the mouse cursor enters the control's bounds.
        /// </summary>
        public static int AddOnEnterCallback(ComponentSystem sys, Entity uiControl, EventCallback callback)
        {
            return AddCallback(sys, uiControl, "PointerInteraction.over", false, callback);
        }

        /// <summary>
        ///  Sets the callback that is called when the mouse cursor leaves the control's bounds.
        /// </summary>
        public static int AddOnLeaveCallback(ComponentSystem sys, Entity uiControl, EventCallback callback)
        {
            return AddCallback(sys, uiControl, "PointerInteraction.over", true, callback);
        }

        /// <summary>
        /// </summary>
        public static void RemoveCallback(ComponentSystem sys, int callbackId)
        {
            var watchers = sys.World.GetExistingSystem<UIControlsWatchersSystem>();
            watchers.RemoveWatcher(callbackId);
        }

        private static int AddCallback(ComponentSystem sys, Entity uiControl, TypeManager.FieldInfo field, bool watchNegative, EventCallback callback)
        {
            if (!sys.World.EntityManager.Exists(uiControl))
                return -1;

            var watchers = sys.World.GetExistingSystem<UIControlsWatchersSystem>();

            return watchers.WatchChanged(uiControl, field, (Entity e, bool oldValue, bool value, Watcher source) =>
            {
                if (value ^ watchNegative)
                    callback?.Invoke(e);

                return true;
            });
        }
    }
}
