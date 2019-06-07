using System;
using System.Runtime.InteropServices;
using Unity.Entities;

namespace Unity.Entities
{
    [AttributeUsage(AttributeTargets.Method)]
    class MonoPInvokeCallbackAttribute : Attribute
    {
    }

    unsafe struct EntityManagerDelegates
    {
        public delegate void AddComponentRawType(void* emHandle, Entity e, int cid);

        [MonoPInvokeCallback]
        internal static void CallAddComponentRaw(void* emHandle, Entity e, int cid)
        {
            ((EntityManager) GCHandle.FromIntPtr((IntPtr) emHandle).Target).AddComponentRaw(e, cid);
        }

        public delegate void RemoveComponentRawType(void* emHandle, Entity e, int cid);

        [MonoPInvokeCallback]
        internal static void CallRemoveComponentRaw(void* emHandle, Entity e, int cid)
        {
            ((EntityManager) GCHandle.FromIntPtr((IntPtr) emHandle).Target).RemoveComponentRaw(e, cid);
        }

        public delegate bool HasComponentDataRawType(void* emHandle, Entity e, int cid);

        [MonoPInvokeCallback]
        internal static bool CallHasComponentRaw(void* emHandle, Entity e, int cid)
        {
            return ((EntityManager) GCHandle.FromIntPtr((IntPtr) emHandle).Target).HasComponentRaw(e, cid);
        }

        public delegate byte* GetComponentDataPtrRawROType(void* emHandle, Entity e, int cid);

        [MonoPInvokeCallback]
        internal static byte* CallGetComponentDataPtrRawRO(void* emHandle, Entity e, int cid)
        {
            return (byte*)((EntityManager) GCHandle.FromIntPtr((IntPtr) emHandle).Target).GetComponentDataRawRO(e, cid);
        }

        public delegate void* GetBufferElementDataPtrRawROType(void* emHandle, Entity e, int cid);

        [MonoPInvokeCallback]
        internal static void* CallGetBufferElementDataPtrRawRO(void* emHandle, Entity e, int cid)
        {
            return (void*)((EntityManager) GCHandle.FromIntPtr((IntPtr) emHandle).Target).GetBufferRawRO(e, cid);
        }

        public delegate void* GetBufferElementDataPtrRawRWType(void* emHandle, Entity e, int cid);

        [MonoPInvokeCallback]
        internal static void* CallGetBufferElementDataPtrRawRW(void* emHandle, Entity e, int cid)
        {
            return (void*)((EntityManager)GCHandle.FromIntPtr((IntPtr)emHandle).Target).GetBufferRawRW(e, cid);
        }

        public delegate int GetBufferElementDataLengthType(void* emHandle, Entity e, int cid);

        [MonoPInvokeCallback]
        internal static int CallGetBufferElementDataLength(void* emHandle, Entity e, int cid)
        {
            return (int) ((EntityManager) GCHandle.FromIntPtr((IntPtr) emHandle).Target).GetBufferLength(e, cid);
        }

        public delegate byte* GetComponentDataPtrRawRWType(void* emHandle, Entity e, int cid);

        [MonoPInvokeCallback]
        internal static byte* CallGetComponentDataPtrRawRW(void* emHandle, Entity e, int cid)
        {
            return (byte*)((EntityManager) GCHandle.FromIntPtr((IntPtr) emHandle).Target).GetComponentDataRawRW(e, cid);
        }

        public delegate Entity CreateEntityType(void* emHandle, EntityArchetype arch);

        [MonoPInvokeCallback]
        internal static Entity CallCreateEntity(void* emHandle, EntityArchetype arch)
        {
            return ((EntityManager) GCHandle.FromIntPtr((IntPtr) emHandle).Target).CreateEntity(arch);
        }

        public delegate void DestroyEntityType(void* emHandle, Entity* e, int count);

        [MonoPInvokeCallback]
        internal static void CallDestroyEntity(void* emHandle, Entity* e, int count)
        {
            ((EntityManager) GCHandle.FromIntPtr((IntPtr) emHandle).Target).DestroyEntityInternal(e, count);
        }

        public delegate EntityArchetype CreateArchetypeRawType(void* emHandle, int* typeIndices, int count);

        [MonoPInvokeCallback]
        internal static EntityArchetype CallCreateArchetypeRaw(void* emHandle, int* typeIndices, int count)
        {
            return ((EntityManager) GCHandle.FromIntPtr((IntPtr) emHandle).Target).CreateArchetypeRaw(typeIndices,
                count);
        }

        public delegate int TypeIndexForStableTypeHashType(ulong typeHash);

        [MonoPInvokeCallback]
        internal static int CallTypeIndexForStableTypeHash(ulong typeHash)
        {
            for (int i = 0; i < TypeManager.GetTypeCount(); ++i) {
                var t = TypeManager.GetTypeInfo(i);
                if (typeHash == t.StableTypeHash)
                    return t.TypeIndex;
            }

            return -1;
        }

        public AddComponentRawType AddComponentRaw;
        public RemoveComponentRawType RemoveComponentRaw;
        public HasComponentDataRawType HasComponentDataRaw;
        public GetComponentDataPtrRawROType GetComponentDataPtrRawRO;
        public GetComponentDataPtrRawRWType GetComponentDataPtrRawRW;
        public CreateEntityType CreateEntity;
        public DestroyEntityType DestroyEntity;
        public CreateArchetypeRawType CreateArchetypeRaw;
        public TypeIndexForStableTypeHashType TypeIndexForStableTypeHash;
        public GetBufferElementDataPtrRawROType GetBufferElementDataPtrRawRO;
        public GetBufferElementDataPtrRawRWType GetBufferElementDataPtrRawRW;
        public GetBufferElementDataLengthType GetBufferElementDataLength;

        public static IntPtr HandleForEntityManager(EntityManager em)
        {
            return (IntPtr) GCHandle.Alloc(em, GCHandleType.Normal);
        }

        internal static EntityManagerDelegates Instance;

        [DllImport("lib_unity_entities_cplusplus")]
        extern static void SetEntityManagerDelegates(EntityManagerDelegates delegates);

        internal static void Initialize()
        {
            if (Instance.AddComponentRaw == null) {
                Instance.AddComponentRaw = CallAddComponentRaw;
                Instance.RemoveComponentRaw = CallRemoveComponentRaw;
                Instance.HasComponentDataRaw = CallHasComponentRaw;
                Instance.GetComponentDataPtrRawRO = CallGetComponentDataPtrRawRO;
                Instance.GetComponentDataPtrRawRW = CallGetComponentDataPtrRawRW;
                Instance.CreateEntity = CallCreateEntity;
                Instance.DestroyEntity = CallDestroyEntity;
                Instance.CreateArchetypeRaw = CallCreateArchetypeRaw;
                Instance.TypeIndexForStableTypeHash = CallTypeIndexForStableTypeHash;
                Instance.GetBufferElementDataPtrRawRO = CallGetBufferElementDataPtrRawRO;
                Instance.GetBufferElementDataPtrRawRW = CallGetBufferElementDataPtrRawRW;
                Instance.GetBufferElementDataLength = CallGetBufferElementDataLength;

                SetEntityManagerDelegates(Instance);
            }
        }
    }

    public static class CPlusPlus {
        public static IntPtr WrapEntityManager(EntityManager em)
        {
            EntityManagerDelegates.Initialize();
            return EntityManagerDelegates.HandleForEntityManager(em);
        }

        public static void ReleaseHandleForEntityManager(IntPtr wrapper)
        {
            ((GCHandle)wrapper).Free();
        }
    }
}

