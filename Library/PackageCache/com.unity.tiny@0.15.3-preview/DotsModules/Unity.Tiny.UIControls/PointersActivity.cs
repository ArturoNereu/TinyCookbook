using System.Collections.Generic;
using Unity.Entities;

namespace Unity.Tiny.UIControls
{
    class PointerControlInteraction
    {
        public PointerID id;
        public Entity pressed;
        public Entity hover;
    };

    public class PointersActivity
    {
        List<PointerControlInteraction> pointers = new List<PointerControlInteraction>();

        private static PointersActivity instance;

        public static PointersActivity Get()
        {
            if (instance == null)
                instance = new PointersActivity();

            return instance;
        }

        private PointerControlInteraction GetPointer(PointerID id)
        {
            foreach (var item in pointers)
            {
                if (item.id.Equals(id))
                    return item;
            }

            return null;
        }

        public void ClearInvalidControls(EntityManager mgr)
        {
            foreach (var pointer in pointers)
            {
                if (pointer.pressed != Entity.Null && !mgr.Exists(pointer.pressed))
                    pointer.pressed = Entity.Null;

                if (pointer.hover != Entity.Null && !mgr.Exists(pointer.hover))
                    pointer.hover = Entity.Null;
            }
        }

        public Entity GetPressedControl(PointerID id)
        {
            var pointer = GetPointer(id);
            if (pointer == null)
                return Entity.Null;

            return pointer.pressed;
        }

        public Entity GetHoverControl(PointerID id)
        {
            var pointer = GetPointer(id);
            if (pointer == null)
                return Entity.Null;

            return pointer.hover;
        }

        public bool IsPressed(Entity control)
        {
            foreach (var item in pointers)
            {
                if (item.pressed == control)
                    return true;
            }

            return false;
        }

        public int GetPressCount(Entity control)
        {
            int count = 0;
            foreach (var item in pointers)
            {
                if (item.pressed == control)
                    count++;
            }

            return count;
        }

        public bool IsHover(Entity control)
        {
            foreach (var item in pointers)
            {
                if (item.hover == control)
                    return true;
            }

            return false;
        }

        public void ClearPressed(PointerID id)
        {
            SetPressed(id, Entity.Null);
        }

        public void ClearHover(PointerID id)
        {
            SetHover(id, Entity.Null);
        }

        public void SetPressed(PointerID id, Entity control)
        {
            var pointer = GetPointer(id);
            if (pointer != null)
                pointer.pressed = control;
            else if (control != Entity.Null)
                AddNewPointer(id, control, Entity.Null);
        }

        public void SetHover(PointerID id, Entity control)
        {
            var pointer = GetPointer(id);
            if (pointer != null)
                pointer.hover = control;
            else if (control != Entity.Null)
                AddNewPointer(id, Entity.Null, control);
        }

        private void AddNewPointer(PointerID id, Entity pressed, Entity hover)
        {
            Debugging.Assert.IsTrue(GetPointer(id) == null);
            pointers.Add(new PointerControlInteraction { id = id, pressed = pressed, hover = hover });
        }
    }
}
