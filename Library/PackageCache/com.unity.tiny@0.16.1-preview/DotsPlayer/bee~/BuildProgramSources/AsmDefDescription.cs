using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using NiceIO;
using Unity.BuildSystem.NativeProgramSupport;


public class AsmDefDescription
{
    public NPath Path { get; }
    public string PackageSource { get; }
    private JObject Json;
    
    public AsmDefDescription(NPath path, string packageSource)
    {
        Path = path;
        PackageSource = packageSource;
        Json = JObject.Parse(path.ReadAllText());
    }

    public string Name => Json["name"].Value<string>();

    public AsmDefDescription[] References => (Json["references"]?.Values<string>() ?? Array.Empty<string>()).Select(BuildProgramConfigFile.AsmDefDescriptionFor).Where(d => d != null).ToArray();
    
    public Platform[] IncludePlatforms => ReadPlatformList(Json["includePlatforms"]);
    public Platform[] ExcludePlatforms => ReadPlatformList(Json["excludePlatforms"]);
    public bool Unsafe => Json["allowUnsafeCode"]?.Value<bool>() == true;
    public NPath Directory => Path.Parent;

    public string[] DefineConstraints => Json["defineConstraints"]?.Values<string>().ToArray() ?? Array.Empty<string>();

    public string[] OptionalUnityReferences => Json["optionalUnityReferences"]?.Values<string>()?.ToArray() ?? Array.Empty<string>();

    private static Platform[] ReadPlatformList(JToken platformList)
    {
        if (platformList == null)
            return Array.Empty<Platform>();

        return platformList.Select(token => PlatformFromAsmDefPlatformName(token.ToString())).Where(p => p != null).ToArray();
    }

    private static Platform PlatformFromAsmDefPlatformName(string name)
    {
        switch(name)
        {
            case "macOSStandalone":
                return new MacOSXPlatform();
            case "WindowsStandalone32":
            case "WindowsStandalone64":
                return new WindowsPlatform();
            case "Editor":
                return null;
            default:
            {
                var typeName = $"{name}Platform";
                var type = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));
                if (type == null)
                {
                    Console.WriteLine($"Couldn't find Platform for {name} (tried {name}Platform), ignoring it.");
                    return null;
                }
                return (Platform)Activator.CreateInstance(type);
            }
        }
    }
}