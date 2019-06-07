using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Unity.Editor
{
    internal delegate TData DataGetter<TData>(Entity entity) where TData: struct;
    internal delegate void DataSetter<TData>(Entity entity, TData data) where TData: struct;

    internal readonly unsafe struct ChangePropagator<TData>
        where TData : struct /*, blittable */
    {
        private readonly NativeArray<Entity> m_Targets;
        private readonly DataGetter<TData> m_Getter;
        private readonly DataSetter<TData> m_Setter;

         public ChangePropagator(NativeArray<Entity> targets, DataGetter<TData> getter, DataSetter<TData> setter)
        {
            m_Targets = targets;
            m_Getter = getter;
            m_Setter = setter;
        }

         public bool IsDifferent<T>(T baseData, T newData)
             where T : struct
         {
             var lhs = (byte*) UnsafeUtility.AddressOf(ref baseData);
             var rhs = (byte*) UnsafeUtility.AddressOf(ref newData);
             var size = UnsafeUtility.SizeOf<T>();

             for (var i = 0; i < size; ++i)
             {
                 if (lhs[i] != rhs[i])
                 {
                     return true;
                 }
             }

             return false;
         }

         public bool Apply<T>(T baseData, T newData)
             where T : struct
         {
             var lhs = (byte*) UnsafeUtility.AddressOf(ref baseData);
             var rhs = (byte*) UnsafeUtility.AddressOf(ref newData);
             var size = UnsafeUtility.SizeOf<T>();
             var mask = stackalloc byte[size];
             var changed = false;

             for (var i = 0; i < size; ++i)
             {
                 if (lhs[i] != rhs[i])
                 {
                     mask[i] = 1;
                     changed = true;
                 }
                 else
                 {
                     mask[i] = 0;
                 }
             }

             if (!changed)
             {
                 return false;
             }

             foreach (var entity in m_Targets)
             {
                 var other = m_Getter(entity);
                 var apply = (byte*) UnsafeUtility.AddressOf(ref other);
                 for (var i = 0; i < size; ++i)
                 {
                     if (mask[i] == 1)
                     {
                         apply[i] = rhs[i];
                     }
                 }

                 m_Setter(entity, other);
             }

             return true;
         }

         public bool SetDataAtOffset<T>(T baseData, T newData, int offset)
             where T : struct
         {
             var lhs = (byte*) UnsafeUtility.AddressOf(ref baseData);
             var rhs = (byte*) UnsafeUtility.AddressOf(ref newData);
             var size = UnsafeUtility.SizeOf<T>();
             var mask = stackalloc byte[size];
             var changed = false;

             for (var i = 0; i < size; ++i)
             {
                 if (lhs[i] != rhs[i])
                 {
                     mask[i] = 1;
                     changed = true;
                 }
                 else
                 {
                     mask[i] = 0;
                 }
             }

             if (!changed)
             {
                 return false;
             }

             foreach (var entity in m_Targets)
             {
                 var other = m_Getter(entity);
                 var apply = (byte*) UnsafeUtility.AddressOf(ref other) + offset;
                 for (var i = 0; i < size; ++i)
                 {
                     if (mask[i] == 1)
                     {
                         apply[i] = rhs[i];
                     }
                 }

                 m_Setter(entity, other);
             }

             return true;
         }
    }
}
