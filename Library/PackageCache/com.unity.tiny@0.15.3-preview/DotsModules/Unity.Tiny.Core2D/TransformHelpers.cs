using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Tiny.Core;
using Unity.Tiny.Debugging;

namespace Unity.Tiny.Core2D
{
    internal class TransformHelpersSystem : ComponentSystem
    {

        public static int FindInString(string s, char c, int startPos)
        {
            for ( int i=startPos; i<s.Length; i++ )
                if (s[i] == c)
                    return i;
            return -1;
        }

        public static string SliceString(string s, int startPos, int endPos)
        {
            string r = "";
            for (int i = startPos; i < endPos; i++)
                r += s[i];
            return r;
        }

        public Entity Find(Entity node, string name)
        {
            if ( name==String.Empty )
                return Entity.Null;
            int subNameStart = 0;
            int subNameEnd = FindInString(name,'/', 0);
            if (subNameEnd == -1)
                return FindNoPath(node, name);
            for (;;)
            {
                string subName = SliceString(name, subNameStart, subNameEnd);
                Entity e = FindNoPath(node, subName);
                if (e == Entity.Null)
                    return Entity.Null;
                if (subNameEnd == name.Length)
                    return e;
                int nextEnd = FindInString(name,'/', subNameEnd + 1);
                if (nextEnd == -1)
                    nextEnd = name.Length;
                subNameStart = subNameEnd + 1;
                subNameEnd = nextEnd;
                node = e;
            }
        }

        private const uint sFindCacheSize = 1024;
        private Entity[] findCache = new Entity[sFindCacheSize];

        private Entity FindNoPath(Entity node, string name)
        {
            var mgr = EntityManager;
            Assert.IsTrue(mgr.HasComponent<Parent>(node));

            var hash = (uint)name.GetHashCode();
            hash ^= (uint)node.GetHashCode();
            hash %= sFindCacheSize;

            Entity eI = findCache[hash];
            if (eI!=Entity.Null && mgr.Exists(eI) && mgr.HasComponent<Parent>(eI) && mgr.HasComponent<EntityName>(eI)) {
                var tx = mgr.GetComponentData<Parent>(eI);
                if (tx.Value == node && mgr.GetBufferAsString<EntityName>(eI) == name)
                        return eI;
            }
            Entity result = Entity.Null;
            Entities.ForEach( (Entity e, ref Parent xformNode, DynamicBuffer<EntityName> ename) => {
                if (xformNode.Value == node)
                {
                    if (mgr.GetBufferAsString<EntityName>(e) == name)
                        result = e;
                }
            });
            findCache[hash] = result;
            return result;
        }

        public void UnlinkAllChildren(Entity parent)
        {
            var mgr = EntityManager;
            Entities.ForEach((ref Parent node) => {
                if (node.Value == parent)
                    node.Value = Entity.Null;
            });
        }

        public void UnlinkAllChildrenDeferred(Entity parent, EntityCommandBuffer ecb)
        {
            Parent noNode = new Parent{ Value = Entity.Null };
            var mgr = EntityManager;
            Entities.ForEach((Entity e, ref Parent node) => {
                if (node.Value == parent)
                    ecb.SetComponent(e, noNode);
            });
        }

        public int CountChildren(Entity parent)
        {
            int n = 0;
            Entities.ForEach((Entity e, ref Parent node) =>
            {
                if (node.Value == parent)
                    n++;
            });
            return n;
        }

        public int GetChildren(Entity parent, ref NativeList<Entity> children, bool includeDisabled)
        {
            int n = 0;
            NativeList<Entity> childrenCopy = children;
            var er = Entities;
            if (includeDisabled)
                er = er.With(EntityQueryOptions.IncludeDisabled); 
            er.ForEach((Entity e, ref Parent node) => {
                if (node.Value == parent) {
                    childrenCopy.Add(e);
                    n++;
                }
            });
            children = childrenCopy;
            return n;
        }

        public Entity DebugCheckAllNodes()
        {
            Entity cycleFound = Entity.Null;
            Entities.ForEach((Entity e, ref Parent node) =>
            {
                if (cycleFound!=Entity.Null || node.Value == Entity.Null)
                    return;
                // floyd
                Entity slow = e;
                Entity fast = e;
                for (;;) {
                    Entity fastparent = EntityManager.GetComponentData<Parent>(fast).Value;
                    if (fastparent == Entity.Null)
                        return;
                    Entity fastparentparent = EntityManager.GetComponentData<Parent>(fastparent).Value;
                    if (fastparentparent == Entity.Null)
                        return;
                    Entity slowparent = EntityManager.GetComponentData<Parent>(slow).Value;
                    if (slowparent == Entity.Null)
                        return;
                    if (slowparent == fastparentparent) {
                        cycleFound = e;
                        return;
                    }
                    slow = slowparent;
                    fast = fastparentparent;
                }
            });
            return cycleFound;
        }

        protected override void OnUpdate()
        {
        }
    }

    /// <summary>
    ///  Provides a variety of convenience functions for transforms.
    /// </summary>
    public static class TransformHelpers
    {
        /// <summary>
        ///  Finds a child entity of node with the given name. You can search deeper in the
        ///  hierarchy to find a child of a child entity, a child of that child,
        ///  and so on by separating the levels with forward slashes (/) in the name.
        ///
        ///  For example Find(node, "a/b/c") is the same as Find(Find(Find(node,"a"),"b"),"c");
        ///
        ///  Initial lookups can be slow, but are cached if successful.
        ///  Failed lookups are not cached.
        /// </summary>
        public static Entity Find(ComponentSystem callingSystem, Entity node, string name)
        {
            var ths = callingSystem.World.GetOrCreateSystem<TransformHelpersSystem>();
            return ths.Find(node, name);
        }

        /// <summary>
        ///  Completely removes this node from the hierarchy, and sets the parent
        ///  of all of its children to NONE.
        ///  Avoid using this function in inner loops, where it can be very slow.
        /// </summary>
        public static void Unlink(ComponentSystem callingSystem, Entity node)
        {
            Assert.IsTrue(callingSystem.EntityManager.HasComponent<Parent>(node));
            Parent noNode = new Parent{ Value = Entity.Null };
            callingSystem.EntityManager.SetComponentData(node, noNode );
            var ths = callingSystem.World.GetOrCreateSystem<TransformHelpersSystem>();
            ths.UnlinkAllChildren(node);
        }

        /// <summary>
        ///  Completely removes this node from the hierarchy, and sets the parent
        ///  of all of its children to NONE.
        ///  This function is like Unlink, but takes an explicit entity command buffer.
        ///  Avoid using this function in inner loops, where it can be very slow.
        /// </summary>
        public static void UnlinkDeferred(ComponentSystem callingSystem, EntityCommandBuffer ecb, Entity node)
        {
            Parent noNode = new Parent{ Value = Entity.Null };
            ecb.SetComponent(node, noNode);
            var ths = callingSystem.World.GetOrCreateSystem<TransformHelpersSystem>();
            ths.UnlinkAllChildrenDeferred(node, ecb);
        }

        /// <summary>
        ///  Sets the parent of all of node's child entities to NONE.
        ///  Avoid using this function in inner loops, where it can be very slow.
        /// </summary>
        public static void UnlinkAllChildren(ComponentSystem callingSystem, Entity parent)
        {
            var ths = callingSystem.World.GetOrCreateSystem<TransformHelpersSystem>();
            ths.UnlinkAllChildren(parent);
        }

        /// <summary>
        ///  Sets the parent of all of node's child entities to NONE.
        ///  This function is like UnlinkAllChildren, but takes an explicit entity
        ///  command buffer.
        ///  Avoid using this function in inner loops, where it can be very slow.
        /// </summary>
        public static void UnlinkAllChildrenDeferred(ComponentSystem callingSystem, EntityCommandBuffer ecb, Entity parent)
        {
            var ths = callingSystem.World.GetOrCreateSystem<TransformHelpersSystem>();
            ths.UnlinkAllChildrenDeferred(parent, ecb);
        }

        /// <summary>
        ///  Returns the number of children that node has.
        ///  Avoid using this function in inner loops, where it can be very slow.
        /// </summary>
        public static int CountChildren(ComponentSystem callingSystem, Entity parent)
        {
            var ths = callingSystem.World.GetOrCreateSystem<TransformHelpersSystem>();
            return ths.CountChildren(parent);
        }

        /// <summary>
        ///  Returns a child entity at the given index. If the
        ///  index is out of range, returns Entity.NONE.
        ///  Note that because the order of children can change at any time, a given
        ///  index does not return the same child every time. In most cases, you
        ///  can obtain better results by using the Find function or caching children
        ///  locally.
        ///  Avoid using this function in inner loops, where it can be very slow.
        /// </summary>
        public static Entity GetChild(ComponentSystem callingSystem, Entity parent, int index = 0)
        {
            if ( index<0 )
                return  Entity.Null;
            var temp = new NativeList<Entity>(Allocator.Temp);
            int n = GetChildren(callingSystem, parent, ref temp);
            Entity r = Entity.Null;
            if (index < n)
                r = temp[index];
            temp.Dispose();
            return r;
        }

        /// <summary>
        ///  Adds children of a node to the children list and returns
        ///  the number of children added.
        ///  Children are appended to the array, so you can use it for collection.
        ///  Avoid using this function in inner loops, where it can be very slow.
        /// </summary>
        public static int GetChildren(ComponentSystem callingSystem, Entity parent, ref NativeList<Entity> children, bool includeDisabled=false)
        {
            var ths = callingSystem.World.GetOrCreateSystem<TransformHelpersSystem>();
            return ths.GetChildren(parent, ref children, includeDisabled);
        }

        /// <summary>
        /// Checks for cycles in the transform hierarchy.
        /// If there is a cycle, returns the entity at which a cycle was detected.
        /// Otherwise, return Entity.Null.
        /// </summary>
        public static Entity DebugCheckAllNodes(ComponentSystem callingSystem)
        {
            var ths = callingSystem.World.GetOrCreateSystem<TransformHelpersSystem>();
            return ths.DebugCheckAllNodes();
        }

        /// <summary>
        ///  Destroys all of node's children recursively.
        ///  The destroySelf parameter specifies whether to also destroy the entity in node
        ///  along with its children.
        /// </summary>
        /// <remarks>
        /// This function can be very slow and should be used sparingly.
        /// Always prefer grouping entities with different mechanisms than via the hierarchy.
        /// </remarks>
        public static void DestroyTree(ComponentSystem callingSystem, Entity node, bool destroySelf = true)
        {
            ApplyTree(callingSystem, node, (Entity e, ComponentSystem cs) => { cs.EntityManager.DestroyEntity(e);}, destroySelf, true);
        }

        /// <summary>
        ///  Add the Disabled component to all of node's children recursively.
        ///  The disableSelf parameter specifies whether to also add Disabled to the entity in node
        ///  along with its children.
        /// </summary>
        /// <remarks>
        /// There is no error if entities were already Disabled to begin with.
        /// This function can be very slow and should be used sparingly.
        /// Always prefer grouping entities with different mechanisms than via the hierarchy.
        /// </remarks>
        public static void DisableTree(ComponentSystem callingSystem, Entity node, bool disableSelf = true)
        {
            ApplyTree(callingSystem, node, (Entity e, ComponentSystem cs) =>
            {
                if (!cs.EntityManager.HasComponent<Disabled>(e))
                    cs.EntityManager.AddComponentData(e,new Disabled());
            }, disableSelf, true);
        }

        /// <summary>
        ///  Remove the Disabled component from all of node's children recursively.
        ///  The enableSelf parameter specifies whether to also remove the Disabled component from the entity in node
        ///  along with its children.
        /// </summary>
        /// <remarks>
        /// There is no error if entities were not Disabled to begin with.
        /// This function can be very slow and should be used sparingly.
        /// Always prefer grouping entities with different mechanisms than via the hierarchy.
        /// </remarks>
        public static void EnableTree(ComponentSystem callingSystem, Entity node, bool enableSelf = true)
        {
            ApplyTree(callingSystem, node, (Entity e, ComponentSystem cs) =>
            {
                if (cs.EntityManager.HasComponent<Disabled>(e))
                    cs.EntityManager.RemoveComponent<Disabled>(e);
            }, enableSelf, true);
        }

        public delegate void ApplyTreeDelegate(Entity e, ComponentSystem callingSystem);

        /// <summary>
        /// Apply a delegate function to a sub-tree in the transform hierarchy.
        /// </summary>
        /// <remarks>
        /// This function can be very slow and should be used sparingly.
        /// Always prefer grouping entities with different mechanisms than via the hierarchy.
        /// </remarks>
        public static void ApplyTree(ComponentSystem callingSystem, Entity node, ApplyTreeDelegate d, bool actOnSelf = true, bool includeDisabled = false)
        {
            var ths = callingSystem.World.GetOrCreateSystem<TransformHelpersSystem>();
            var tempList = new NativeList<Entity>(Allocator.Temp);
            GetTree(callingSystem, node, ref tempList, includeDisabled);
            for (var i = 0; i < tempList.Length; i++)
                d(tempList[i], callingSystem);
            if (actOnSelf)
                d(node, callingSystem);
            tempList.Dispose();
        }

        /// <summary>
        ///  Destroys all of node's children recursively.
        ///  The destroySelf parameter specifies whether to also destroy the entity
        ///  along with its children.
        ///  This function is like DestroyTree, but takes an explicit entity command buffer.
        ///  This function can be slow. Use it sparingly.
        /// </summary>
        public static void DestroyTreeDeferred(ComponentSystem callingSystem, EntityCommandBuffer ecb, Entity node, bool destroySelf = true)
        {
            var ths = callingSystem.World.GetOrCreateSystem<TransformHelpersSystem>();
            var tempList = new NativeList<Entity>(Allocator.Temp);
            ths.GetChildren(node, ref tempList, true);
            for ( int i=0; i<tempList.Length; i++ )
                DestroyTree(callingSystem, tempList[i], true);
            if ( destroySelf )
                ecb.DestroyEntity(node);
            tempList.Dispose();
        }

        /// <summary>
        ///  Clones the entire hierarchy tree, including node.
        ///  Returns the clone of node. All cloned descendants are in the new hierarchy.
        ///  This function can be slow. Use it sparingly.
        /// </summary>
        public static Entity CloneTree(ComponentSystem callingSystem, Entity node)
        {
            var mgr = callingSystem.EntityManager;
            NativeList<Entity> children = new NativeList<Entity>(Allocator.Temp);
            GetChildren(callingSystem, node, ref children);
            var theClone = mgr.Instantiate(node);
            Parent tx = new Parent {Value = theClone};
            for (int i = 0; i < children.Length; i++)
            {
                Entity childClone = CloneTree(callingSystem, children[i]);
                Assert.IsTrue(mgr.GetComponentData<Parent>(childClone).Value == node);
                mgr.SetComponentData(childClone, tx);
            }
            children.Dispose();
            return theClone;
        }

        /// <summary>
        ///  Clones the entire hierarchy tree, including node.
        ///  Returns the clone of node. All cloned descendants are in the new hierarchy.
        ///  This function is like CloneTree, but takes an explicit entity command buffer.
        ///  This function can be slow. Use it sparingly.
        /// </summary>
        public static Entity CloneTreeDeferred(ComponentSystem callingSystem, EntityCommandBuffer ecb, Entity node)
        {
            var mgr = callingSystem.EntityManager;
            NativeList<Entity> children = new NativeList<Entity>(Allocator.Temp);
            GetChildren(callingSystem, node, ref children);
            var theClone = ecb.Instantiate(node);
            Parent tx = new Parent {Value = theClone};
            for (int i = 0; i < children.Length; i++)
            {
                Entity childClone = CloneTreeDeferred(callingSystem, ecb, children[i]);
                ecb.SetComponent(childClone, tx);
            }
            children.Dispose();
            return theClone;
        }

        /// <summary>
        ///  Appends all children of node to the children array.
        ///  Returns number of children added to the array.
        ///  This function can be slow. Use it sparingly.
        /// </summary>
        public static int GetTree(ComponentSystem callingSystem, Entity node, ref NativeList<Entity> children, bool includeDisabled=false)
        {
            int baseI = children.Length;
            int nadded = GetChildren(callingSystem, node, ref children, includeDisabled);
            int n = nadded;
            for (int i = 0; i < nadded; i++)
                n += GetTree(callingSystem, children[i + baseI], ref children, includeDisabled);
            return n;
        }

        /// <summary>
        ///  Computes a new local matrix of the kind produced in the LocalToParent
        ///  component.
        ///  This function does not use LocalToParent, and can be quite resource
        ///  intensive. Use it only if the cached result in LocalToParent is not available.
        /// </summary>
        public static float4x4 ComputeLocalMatrix(ComponentSystem callingSystem, Entity node)
        {
            var em = callingSystem.EntityManager;
            float4x4 m;
            if (em.HasComponent<Translation>(node))
            {
                var T = em.GetComponentData<Translation>(node);
                if (em.HasComponent<Rotation>(node))
                {
                    var R = em.GetComponentData<Rotation>(node);
                    if (em.HasComponent<NonUniformScale>(node))
                    { // TRS
                        var S = em.GetComponentData<NonUniformScale>(node);
#if UNITY_USE_TINYMATH
                        m = tinymath.TranslationRotationScale(T.Value, R.Value, S.Value);
#else
                        m = math.mul(new float4x4(R.Value, T.Value), float4x4.Scale(S.Value));
#endif
                    }
                    else // TR
                    {
#if UNITY_USE_TINYMATH
                        m = tinymath.TranslationRotation(T.Value, R.Value);
#else
                        m = new float4x4(R.Value, T.Value);
#endif
                    }
                }
                else
                {
                    if (em.HasComponent<NonUniformScale>(node))
                    { // TS
                        var S = em.GetComponentData<NonUniformScale>(node);
#if UNITY_USE_TINYMATH
                        m = tinymath.TranslationScale(T.Value, S.Value);
#else
                        m = math.mul(float4x4.Translate(T.Value), float4x4.Scale(S.Value));
#endif
                    }
                    else // T
                    {
#if UNITY_USE_TINYMATH
                        m = tinymath.Translation(T.Value);
#else
                        m = float4x4.Translate(T.Value);
#endif
                    }
                }
            }
            else
            {
                if (em.HasComponent<Rotation>(node))
                {
                    var R = em.GetComponentData<Rotation>(node);
                    if (em.HasComponent<NonUniformScale>(node))
                    { // RS
                        var S = em.GetComponentData<NonUniformScale>(node);
#if UNITY_USE_TINYMATH
                        m = tinymath.RotationScale(R.Value, S.Value);
#else
                        m = math.mul(new float4x4(R.Value, float3.zero), float4x4.Scale(S.Value));
#endif
                    }
                    else // R
                    {
#if UNITY_USE_TINYMATH
                        m = tinymath.Rotation(R.Value);
#else
                        m = new float4x4(R.Value, float3.zero);
#endif
                    }
                }
                else
                {
                    if (em.HasComponent<NonUniformScale>(node))
                    { // S
                        var S = em.GetComponentData<NonUniformScale>(node);
#if UNITY_USE_TINYMATH
                        m = tinymath.Scale(S.Value);
#else
                        m = float4x4.Scale(S.Value);
#endif
                    }
                    else //
                    {
                        m = float4x4.identity;
                    }
                }
            }
            return m;
        }

        /// <summary>
        ///  Computes a new Object to World matrix of the kind produced in the LocalToWorld
        ///  component. This function does not use LocalToWorld, and can be quite
        ///  resource intensive. Use it only if the cached result in LocalToParent
        ///  is not available.
        /// </summary>
        public static float4x4 ComputeWorldMatrix(ComponentSystem callingSystem, Entity node)
        {
            var wm = ComputeLocalMatrix(callingSystem, node);
            for (;;)
            {
                var n = callingSystem.EntityManager.GetComponentData<Parent>(node);
                if (n.Value == Entity.Null)
                    break;
                var pm = ComputeLocalMatrix(callingSystem, n.Value);
                wm = math.mul(pm, wm);
                node = n.Value;
            }
            return wm;
        }

        /// <summary>
        ///  Computes a transform node's world position.
        ///  This function uses the computeWorldMatrix function to compute the world
        ///  matrix (bypassing caching), and returns the position of that.
        ///
        ///  This function can be quite resource intensive. When possible, read the
        ///  LocalToWorld component instead.
        /// </summary>
        public static float3 ComputeWorldPosition(ComponentSystem callingSystem, Entity node)
        {
            float4x4 wm = ComputeWorldMatrix(callingSystem, node);
            return wm[3].xyz;
        }

        /// <summary>
        ///  Computes a transform node's world rotation.
        ///  This function uses the computeWorldMatrix function to compute the world
        ///  matrix (bypassing caching), and returns the rotation of that.
        ///
        ///  This function can be quite resource intensive. When possible, read the
        ///  LocalToWorld component instead.
        /// </summary>
        public static quaternion ComputeWorldRotation(ComponentSystem callingSystem, Entity node)
        {
            float4x4 wm = ComputeWorldMatrix(callingSystem, node);
            float3x3 rotpart = new float3x3(wm[0].xyz, wm[1].xyz, wm[2].xyz);
            return new quaternion(rotpart);
        }

        /// <summary>
        ///  Computes a transform node's world scale.
        ///  This function uses the computeWorldMatrix function to compute the world
        ///  matrix (bypassing caching), and returns the scale of that.
        ///
        ///  Because lossy scale does not include skew, it does not include enough
        ///  information to recreate a full 4x4 matrix from rotation, position, and
        ///  lossy scale in 3D.
        ///
        ///  This function can be quite resource intensive. If possible, read the
        ///  LocalToWorld component instead.
        /// </summary>
        public static float3 ComputeWorldScaleLossy(ComponentSystem callingSystem, Entity node)
        {
            float4x4 wm = ComputeWorldMatrix(callingSystem, node);
            float3 s = new float3(math.length(wm[0].xyz), math.length(wm[1].xyz), math.length(wm[2].xyz));
            return s;
        }

        private static float3 InverseTransformPoint(ComponentSystem callingSystem, Entity node, float3 p)
        {
            float4x4 wm = ComputeWorldMatrix(callingSystem, node);
            wm = math.inverse(wm);
            return math.transform(wm, p);
        }

        private static float3 InverseTransformScale(ComponentSystem callingSystem, Entity node, float3 s)
        {
            float4x4 wm = ComputeWorldMatrix(callingSystem, node);
            wm = math.inverse(wm);
            float3 sm = new float3(math.length(wm[0].xyz), math.length(wm[1].xyz), math.length(wm[2].xyz));
            return sm * s;
        }

        private static quaternion InverseTransformRotation(ComponentSystem callingSystem, Entity node, quaternion r)
        {
            float4x4 wm = ComputeWorldMatrix(callingSystem, node);
            wm = math.inverse(wm);
            float3x3 rot = new float3x3(wm.c0.xyz, wm.c1.xyz, wm.c2.xyz);
            rot = math.orthonormalize(rot);
            quaternion qrot = new quaternion(rot);
            return math.mul(r, qrot);
        }

        // Helpers for world-to-local transforms

        /// <summary>
        ///  Inverse-transforms a position by the LocalToWorld matrix.
        ///  This function uses the computeWorldMatrix function to compute the world
        ///  matrix (bypassing caching), then transforms by that.
        ///
        ///  This function can be quite resource intensive. When possible, read the
        ///  LocalToWorld component instead.
        /// </summary>
        public static float3 LocalPositionFromWorldPosition(ComponentSystem callingSystem, Entity node, float3 position)
        {
            Parent tn = callingSystem.EntityManager.GetComponentData<Parent>(node);
            if (tn.Value == Entity.Null)
                return position;
            else
                return InverseTransformPoint(callingSystem, tn.Value, position);
        }

        /// <summary>
        ///  Inverse-transforms a scale by the LocalToWorld matrix.
        ///  This function uses the computeWorldMatrix function to compute the world
        ///  matrix (bypassing caching), then transforms by that.
        ///
        ///  This function can be quite resource intensive. When possible, read the
        ///  LocalToWorld component instead.
        /// </summary>
        public static float3 LocalScaleFromWorldScale(ComponentSystem callingSystem, Entity node, float3 scale)
        {
            Parent tn = callingSystem.EntityManager.GetComponentData<Parent>(node);
            if (tn.Value == Entity.Null)
                return scale;
            else
                return InverseTransformScale(callingSystem, tn.Value, scale);
        }

        /// <summary>
        ///  Inverse-transforms a rotation by the LocalToWorld matrix.
        ///  This function uses the computeWorldMatrix function to compute the world
        ///  matrix (bypassing caching), then transforms by that.
        ///
        ///  This function can be quite resource intensive. When possible, read the
        ///  LocalToWorld component instead.
        /// </summary>
        public static quaternion LocalRotationFromWorldRotation(ComponentSystem callingSystem, Entity node, quaternion rotation)
        {
            Parent tn = callingSystem.EntityManager.GetComponentData<Parent>(node);
            if (tn.Value==Entity.Null)
                return rotation;
            else
                return InverseTransformRotation(callingSystem, tn.Value, rotation);
        }

        /// <summary>
        ///  Helper function for transforming from window to world coordinates.
        ///  This only works if cameraEntity has a valid LocalToWorld
        ///  component. The windowPos (window position) and windowSize (window size) should be
        ///  in the same coordinates. The Z coordinate is set to 0.
        /// </summary>
        public static float3 WindowToWorld(ComponentSystem callingSystem, Entity cameraEntity, float2 windowPos, float2 windowSize)
        {
            var mgr = callingSystem.EntityManager;
            if (!mgr.HasComponent<DisplayListCamera>(cameraEntity) ||
                !mgr.HasComponent<Camera2D>(cameraEntity))
                return float3.zero;

            DisplayListCamera dlc = mgr.GetComponentData<DisplayListCamera>(cameraEntity);
            Camera2D c2d = mgr.GetComponentData<Camera2D>(cameraEntity);

            float aspect = windowSize.x / windowSize.y;
            float2 camsize;
            camsize.x = c2d.halfVerticalSize * aspect;
            camsize.y = c2d.halfVerticalSize;
            float2 normalizedWindow;
            normalizedWindow.x = (windowPos.x / windowSize.x) * 2.0f - 1.0f;
            normalizedWindow.y = (windowPos.y / windowSize.y) * 2.0f - 1.0f;
            float3 viewpos;
            viewpos.x = normalizedWindow.x * camsize.x;
            viewpos.y = normalizedWindow.y * camsize.y;
            viewpos.z = 0;
            float3 worldPos = math.transform(dlc.world, viewpos);
            return worldPos;
        }

        /// <summary>
        ///  Helper function for transforming from world to window coordinates.
        ///  This only works if the cameraEntity has a valid LocalToWorld
        ///  component. Returns windowPos (window position) and windowSize (window size) in
        ///  the same coordinates.
        /// </summary>
        public static float2 WorldToWindow(ComponentSystem callingSystem, Entity cameraEntity, float3 worldPos, float2 windowSize)
        {
            var mgr = callingSystem.EntityManager;
            DisplayListCamera dlc = mgr.GetComponentData<DisplayListCamera>(cameraEntity);
            Camera2D c2d = mgr.GetComponentData<Camera2D>(cameraEntity);

            float aspect = windowSize.x / windowSize.y;
            float2 camsize;
            camsize.x = c2d.halfVerticalSize * aspect;
            camsize.y = c2d.halfVerticalSize;
            // transform from world to window
            float3 viewpos;
            viewpos = math.transform(dlc.inverseWorld, worldPos);
            // to normalized window
            float2 windowPos;
            windowPos.x = viewpos.x / camsize.x;
            windowPos.y = viewpos.y / camsize.y;
            // to output window
            windowPos.x = (windowPos.x * .5f + .5f) * windowSize.x;
            windowPos.y = (windowPos.y * .5f + .5f) * windowSize.y;
            return windowPos;
        }
    }
}
