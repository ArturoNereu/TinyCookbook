using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Authoring;
using Unity.Entities;
using UnityEditor;

namespace Unity.Editor.Conversion
{
    internal static class SceneConversion
    {
        private static Dictionary<Type, int> s_Systems;

        public static Dictionary<Type, int> SystemVersions
        {
            get
            {
                if (s_Systems == null)
                {
                    s_Systems = new Dictionary<Type, int>();
                    
                    var types = TypeCache.GetTypesDerivedFrom<ComponentSystemBase>()
                        .Where(IsSceneConversionSystem);
                    
                    foreach (var systemType in types)
                    {
                        var systemVersion = 0;
                        var versionAttributes = systemType.GetCustomAttributes(typeof(SystemVersionAttribute), false);
                        if (versionAttributes.Length > 0)
                        {
                            systemVersion = ((SystemVersionAttribute) versionAttributes[0]).Version;
                        }
                        s_Systems.Add(systemType, systemVersion);
                    }
                }

                return s_Systems;
            }
        }
        
        public static void Convert(World world)
        {
            world.EntityManager.CompleteAllJobs();
            
            // setup conversion group
            var group = world.GetOrCreateSystem<SceneConversionSystemGroup>();

            foreach (var kvp in SystemVersions)
            {
                try
                {
                    group.AddSystemToUpdateList(world.GetOrCreateSystem(kvp.Key));
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
            }
            
            group.SortSystemUpdateList();
            
            // run conversion
            group.Update();
        }

        private static bool IsSceneConversionSystem(Type systemType)
        {
            if (systemType.IsAbstract || 
                systemType.ContainsGenericParameters ||
                systemType == typeof(SceneConversionSystemGroup))
            {
                return false;
            }

            var updateInGroupAttributes = systemType.GetCustomAttributes(typeof(UpdateInGroupAttribute), false);
            if (updateInGroupAttributes.Length == 0)
            {
                return false;
            }

            return updateInGroupAttributes.Cast<UpdateInGroupAttribute>()
                .All(a => a.GroupType == typeof(SceneConversionSystemGroup));
        }
    }
}
