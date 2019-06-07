using System;
using Unity.Tiny.Core;
using Unity.Entities;
using System.Collections.Generic;

namespace Unity.Tiny.Watchers
{
    /// <summary>
    /// One watcher identifies the watched entity and field.
    /// It also provides a reference to the WatchersSystem and its id inside that system.
    /// </summary>
    public struct Watcher
    {
        public WatchersSystem system;
        public int id;
        public Entity watchedEntity;
        public TypeManager.FieldInfo field;
    }

    internal interface IWatcherValueEntry
    {
        bool OnUpdate(WatchersSystem sys); // return false if watcher is to be removed
        Watcher GetTarget();
    }

    internal struct WatcherValueEntryFloat : IWatcherValueEntry
    {
        public float value;
        public Watcher target;
        public WatchersSystem.WatchValueDelegateFloat fn;

        public Watcher GetTarget()
        {
            return target;
        }

        public bool OnUpdate(WatchersSystem sys)
        {
            float newValue = 0.0f;
            if (sys.GetValueFloat(target.watchedEntity, target.field, ref newValue)) {
                if (value != newValue) {
                    bool r = fn(target.watchedEntity, value, newValue, target);
                    value = newValue;
                    return r;
                }
                return true;
            }
            return false; // no value, remove watcher
        }
    }

    internal struct WatcherValueEntryInt : IWatcherValueEntry
    {
        public int value;
        public Watcher target;
        public WatchersSystem.WatchValueDelegateInt fn;

        public Watcher GetTarget()
        {
            return target;
        }

        public bool OnUpdate(WatchersSystem sys)
        {
            int newValue = 0;
            if (sys.GetValueInt(target.watchedEntity, target.field, ref newValue))
            {
                if (value != newValue) {
                    bool r = fn(target.watchedEntity, value, newValue, target);
                    value = newValue;
                    return r;
                }
                return true;
            }
            return false; // no value, remove watcher
        }
    }

    internal struct WatcherValueEntryBool : IWatcherValueEntry
    {
        public bool value;
        public Watcher target;
        public WatchersSystem.WatchValueDelegateBool fn;

        public Watcher GetTarget()
        {
            return target;
        }

        public bool OnUpdate(WatchersSystem sys)
        {
            bool newValue = false;
            if (sys.GetValueBool(target.watchedEntity, target.field, ref newValue)) {
                if (value != newValue) {
                    bool r = fn(target.watchedEntity, value, newValue, target);
                    value = newValue;
                    return r;
                }
                return true;
            }
            return false; // no value, remove watcher
        }
    }


    /// <summary>
    ///  The Watchers provides an optional high-level helper for getting callback-like
    ///  functionality in an Entity Component System (ECS) environment.
    /// </summary>
    /// <remarks>
    ///  <para>
    ///  While callbacks are not a very ECS-friendly concept (Components store only
    ///  data, and systems contain only code that operates on data), callback-like functionality
    ///  can be useful for certain things.
    ///  </para>
    ///  <para>
    ///  For example, you can use watchers for UI event handlers such as OnClick, or to
    ///  get notifications about the state of a tween animation such as OnEnded or OnLoopPoint.
    ///  </para>
    ///  <para>
    ///  The base DefaultWatchersSystem is auto-scheduled and watchers may be added immediately.
    ///  For advanced use cases it is possible to schedule additional WatchersSystem by creating a
    ///  new subclass and scheduling it where desired.
    ///  </para>
    ///  <para>
    ///  WatchersSystems are standalone systems that group multiple watched values.
    ///  A WatchersSystems must be scheduled as a system in order to trigger delegates/callbacks.
    ///  </para>
    ///  <para>
    ///  Callbacks have the same restrictions as code running in ComponentSystem. They are
    ///  called from the watching system, just like code called from any other ComponentSystem.
    ///  They are executed when the watching system runs, and not immediately when
    ///  a value changes.
    ///  </para>
    /// </remarks>
    [DisableAutoCreation]
    public class WatchersSystem : ComponentSystem
    {
        /// <returns>Return true to keep this watcher around, false to remove it.</returns>
        public delegate bool WatchValueDelegateFloat(Entity e, float oldValue, float value, Watcher source);
        /// <returns>Return true to keep this watcher around, false to remove it.</returns>
        public delegate bool WatchValueDelegateInt(Entity e, int oldValue, int value, Watcher source);
        /// <returns>Return true to keep this watcher around, false to remove it.</returns>
        public delegate bool WatchValueDelegateBool(Entity e, bool oldValue, bool value, Watcher source);

        /// <summary>
        ///  Removes a watcher with a specific id.
        /// </summary>
        /// <remarks>
        ///  Ids are returned by the various Watch* functions.
        ///  Watchers can also be removed by returning false from the delegate callback.
        /// </remarks>
        /// <returns>Returns true if the watcher was found and removed.</returns>
        public bool RemoveWatcher(int id)
        {
            if (inUpdate)
                throw new InvalidOperationException(
                    "Can not modify watchers during delegate. Return false to remove it instead.");
            for (int i = 0; i < watchers.Count; i++)
            {
                if (watchers[i].GetTarget().id == id)
                {
                    watchers.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>Removes all watchers in this system</summary>
        public void RemoveAllWatchers()
        {
            if (inUpdate)
                throw new InvalidOperationException("Can not modify watchers during delegate. Return false to remove it instead.");
            watchers.Clear();
        }

        /// <summary>
        ///  Watches an entity for changes to a float component.
        /// </summary>
        /// <remarks>
        ///  Note that all watching callbacks are called when the watching system
        ///  runs, not when the value is changed.
        /// </remarks>
        /// <param name="e">Target entity to watch.</param>
        /// <param name="fieldInfo">The field to watch; generally implicitly cast from the supplied string, for example
        /// "Translation.position.x", "TweenComponent.ended", or "SequencePlayer.playing".</param>
        /// <param name="callback">Delegate method to call when the value changes.</param>
        /// <returns>An id that can be passed to <see cref="RemoveWatcher"/></returns>
        public int WatchChanged(Entity e, TypeManager.FieldInfo fieldInfo, WatchValueDelegateFloat callback)
        {
            Watcher w = new Watcher { watchedEntity = e, field = fieldInfo, id = nextId++, system = this };
            if (fieldInfo.primitiveType!= PrimitiveFieldTypes.Float)
                throw new ArgumentException("A float field must be watched by a float delegate.");
            var v = new WatcherValueEntryFloat { fn = callback, target = w, value = 0 };
            if (!GetValueFloat(e,fieldInfo,ref v.value))
                throw new ArgumentException("Could not get initial value to watch. Entity or component is missing.");
            watchers.Add(v);
            return w.id;
        }

        /// <summary>
        ///  Watches an entity for changes to a int component.
        /// </summary>
        /// <seealso cref="WatchChanged(Unity.Entities.Entity,Unity.Entities.TypeManager.FieldInfo,Unity.Tiny.Watchers.WatchersSystem.WatchValueDelegateFloat)"/>
        public int WatchChanged(Entity e, TypeManager.FieldInfo fieldInfo, WatchValueDelegateInt callback)
        {
            Watcher w = new Watcher { watchedEntity = e, field = fieldInfo, id = nextId++, system = this };
            if (fieldInfo.primitiveType != PrimitiveFieldTypes.Int)
                throw new ArgumentException("An int field must be watched by an int delegate.");
            var v = new WatcherValueEntryInt { fn = callback, target = w, value = 0 };
            if (!GetValueInt(e,fieldInfo,ref v.value))
                throw new ArgumentException("Could not get initial value to watch. Entity or component is missing.");
            watchers.Add(v);
            return w.id;
        }

        /// <summary>
        ///  Watches an entity for changes to a bool component.
        /// </summary>
        /// <seealso cref="WatchChanged(Unity.Entities.Entity,Unity.Entities.TypeManager.FieldInfo,Unity.Tiny.Watchers.WatchersSystem.WatchValueDelegateFloat)"/>
        public int WatchChanged(Entity e, TypeManager.FieldInfo fieldInfo, WatchValueDelegateBool callback)
        {
            Watcher w = new Watcher { watchedEntity = e, field = fieldInfo, id = nextId++, system = this };
            if (fieldInfo.primitiveType != PrimitiveFieldTypes.Bool)
                throw new ArgumentException("A bool field must be watched by a bool delegate.");
            var v = new WatcherValueEntryBool { fn = callback, target = w, value = false };
            if (!GetValueBool(e,fieldInfo,ref v.value))
                throw new ArgumentException("Could not get initial value to watch. Entity or component is missing.");
            watchers.Add(v);
            return w.id;
        }


        internal bool GetValueFloat(Entity e, TypeManager.FieldInfo field, ref float value)
        {
            if (!EntityManager.Exists(e))
                return false;
            if (!EntityManager.HasComponentRaw(e, field.componentTypeIndex))
                return false;
            float temp;
            unsafe {
                byte* ptrDest = (byte*)&temp;
                byte* ptrSrc = EntityManager.GetComponentDataWithTypeRO(e, field.componentTypeIndex) + field.byteOffsetInComponent;
                for (int i = 0; i < sizeof(float); i++)
                    ptrDest[i] = ptrSrc[i];
            }
            value = temp;
            return true;
        }

        internal bool GetValueInt(Entity e, TypeManager.FieldInfo field, ref int value)
        {
            if (!EntityManager.Exists(e))
                return false;
            if (!EntityManager.HasComponentRaw(e, field.componentTypeIndex))
                return false;
            int temp;
            unsafe
            {
                byte* ptrDest = (byte*)&temp;
                byte* ptrSrc = EntityManager.GetComponentDataWithTypeRO(e, field.componentTypeIndex) + field.byteOffsetInComponent;
                for (int i = 0; i < sizeof(int); i++)
                    ptrDest[i] = ptrSrc[i];
            }
            value = temp;
            return true;
        }

        internal bool GetValueBool(Entity e, TypeManager.FieldInfo field, ref bool value)
        {
            if (!EntityManager.Exists(e))
                return false;
            if (!EntityManager.HasComponentRaw(e, field.componentTypeIndex))
                return false;
            bool temp;
            unsafe
            {
                byte* ptrDest = (byte*)&temp;
                byte* ptrSrc = EntityManager.GetComponentDataWithTypeRO(e, field.componentTypeIndex) + field.byteOffsetInComponent;
                for (int i = 0; i < sizeof(bool); i++)
                    ptrDest[i] = ptrSrc[i];
            }
            value = temp;
            return true;
        }

        private List<IWatcherValueEntry> watchers = new List<IWatcherValueEntry>();
        private int nextId;
        private bool inUpdate;

        protected override void OnUpdate()
        {
            inUpdate = true;
            var l = watchers.Count;
            for (int i=0; i<l; i++) {
                if ( !watchers[i].OnUpdate(this) ) {
                    watchers[i] = watchers[l - 1];
                    l--;
                    i--;
                }
            }
            int nremove = watchers.Count - l;
            for (int i=0; i<nremove; i++)
                watchers.RemoveAt(watchers.Count-1); // no RemoveRange in tiny.
            inUpdate = false;
        }
    }

    public class DefaultWatchersSystem : WatchersSystem
    {
    }
}
