using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using NiceIO;

static class BuildProgramConfigFile
{
    private static JObject Json { get; }
    
    static readonly Dictionary<string,AsmDefDescription> _namesToAsmDefDescription = new Dictionary<string, AsmDefDescription>();
    public static NPath UnityProjectPath { get; }
    
    static BuildProgramConfigFile()
    {
        Json = JObject.Parse(new NPath("asmdefs.json").MakeAbsolute().ReadAllText());
        UnityProjectPath = Json["UnityProjectPath"].Value<string>();
        ProjectName = Json["ProjectName"].Value<string>();
    }

    public static string ProjectName { get; }
    
    public static AsmDefDescription AsmDefDescriptionFor(string asmdefname)
    {
        if (_namesToAsmDefDescription.TryGetValue(asmdefname, out var result))
            return result;

        var jobject = Json["asmdefs"].Values<JObject>().FirstOrDefault(o => o["AsmdefName"].Value<string>() == asmdefname);
        if (jobject == null)
            return null;
        
        result = new AsmDefDescription(jobject["FullPath"].Value<string>(), jobject["PackageSource"].Value<string>());
        _namesToAsmDefDescription[asmdefname] = result;
        return result;
    }
    
    public static IEnumerable<AsmDefDescription> AssemblyDefinitions
    {
        get
        {
            foreach (var jobject in Json["asmdefs"].Values<JObject>())
                yield return AsmDefDescriptionFor(jobject["AsmdefName"].Value<string>());
        }
    }
}
