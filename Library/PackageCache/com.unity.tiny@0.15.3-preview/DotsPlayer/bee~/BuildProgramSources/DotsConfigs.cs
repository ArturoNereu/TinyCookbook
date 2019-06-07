using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotsBuildTargets;

static class DotsConfigs
{
    static IEnumerable<DotsRuntimeCSharpProgramConfiguration> MakeConfigs()
    {
        var platformList = new List<DotsBuildSystemTarget>();

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;

            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types;
            }

            foreach (var type in types)
            {
                if (type.IsAbstract)
                    continue;

                if (!type.IsSubclassOf(typeof(DotsBuildSystemTarget)))
                    continue;

                platformList.Add((DotsBuildSystemTarget)Activator.CreateInstance(type));
            }
        }

        foreach (var platform in platformList)
        {
            foreach (var config in platform.GetConfigs())
                yield return config;
        }
    }

    private static readonly Lazy<DotsRuntimeCSharpProgramConfiguration[]> _configs = new Lazy<DotsRuntimeCSharpProgramConfiguration[]>(() => MakeConfigs().ToArray());

    public static DotsRuntimeCSharpProgramConfiguration HostDotnet => Configs.First(c => c.ScriptingBackend == ScriptingBackend.Dotnet && c.Platform.GetType() == Unity.BuildSystem.NativeProgramSupport.Platform.HostPlatform.GetType());
    public static DotsRuntimeCSharpProgramConfiguration[] Configs => _configs.Value;
}