using System.Collections.Generic;
using System.Linq;
using Bee.Core;
using Bee.DotNet;
using NiceIO;
using Unity.BuildSystem.CSharpSupport;
using Unity.BuildSystem.NativeProgramSupport;
using Unity.BuildTools;

internal class BindGem
{
    private const string UnityEntitiesDllName = "Unity.Entities.dll";
    private static BindGem _instance;
    public CSharpProgram Program;
    public static BindGem Instance() => _instance ?? (_instance = new BindGem());

    private BindGem()
    {
        Program = new CSharpProgram()
        {
            Sources = {BuildProgram.BeeRoot.Parent.Combine("BindGem~")},
            Path = "artifacts/bindgem/bindgem.exe",
            References = {
                new SystemReference("System.Xml.Linq"), new SystemReference("System.Xml"),
                new NPath($"{BuildProgram.LowLevelRoot}/UnsafeUtilityPatcher").Files("*.dll"),
                StevedoreUnityCecil.Paths,
                TypeRegistrationTool.EntityBuildUtils
            },
            LanguageVersion = "7.3",
            WarningsAsErrors = false,
            CopyReferencesNextToTarget = true,
            ProjectFilePath = $"BindGem.csproj",
        };
        BuiltBindGemProgram = Program.SetupDefault();
    }

    DotNetAssembly BuiltBindGemProgram { get; }

    public void SetupInvocation(DotNetAssembly inputProgram, DotsRuntimeCSharpProgramConfiguration config)
    {
        var result = BindGemOutputFor(inputProgram, config);
        if (result == null) 
            return;

        var assembly = inputProgram;
        
        var args = new List<string>
        {
            "-v",
            "-dots",
            assembly.RuntimeDependencies.Select(rd => $"-r {rd.Path.InQuotes()}"),
            assembly.RuntimeDependencies.Select(r=>BindGemOutputFor(r,config)).ExcludeNulls().Select(bo=>$"-cppInclude {bo.Header}"),
            $"-define_guard BUILD_{assembly.Path.FileName.ToUpper().Replace(".", "_")}",
            assembly.Path.InQuotes(),
            "-o",
            result.Cpp.Parent.Combine(result.Cpp.FileNameWithoutExtension).InQuotes()
        };

        var program = new DotNetRunnableProgram(BuiltBindGemProgram);
        
        var inputs = new List<NPath>
        {
            BuiltBindGemProgram.Path,
            assembly.RecursiveRuntimeDependenciesIncludingSelf.Select(d => d.Path)
        };

        // Note: the MakeAbsolute() below also takes care of changing slashes on Windows,
        // because Windows really hates forward slashes when used as an executable path
        // to cmd.exe
        Backend.Current.AddAction(
            actionName: "BindGem",
            targetFiles: result.Files,
            inputs: inputs.ToArray(),
            executableStringFor: program.InvocationString,
            commandLineArguments: args.ToArray(),
            supportResponseFile: false
        );
    }
    
    static BindGemResult BindGemOutputFor(CSharpProgram inputProgram, NativeProgramConfiguration config) => BindGemOutputFor(inputProgram, ((DotsRuntimeNativeProgramConfiguration) config).CSharpConfig);
    static IEnumerable<BindGemResult> BindGemOutputFor(IEnumerable<CSharpProgram> inputPrograms, NativeProgramConfiguration config) => inputPrograms.Select(p => BindGemOutputFor(p, config)).ExcludeNulls();
    static BindGemResult BindGemOutputFor(DotNetAssembly assembly, DotsRuntimeCSharpProgramConfiguration config) => SupportsBindgem(assembly.Path.FileName) ? MakeBindGemResultFor(assembly.Path.FileName, config) : null;

    private static bool SupportsBindgem(string filename)
    {
        switch (filename)
        {
            case "Unity.Tiny.Image2D.dll":
            case "Unity.Entities.CPlusPlus.dll":
            case "Unity.Tiny.Core2D.dll":
            case "Unity.Tiny.Sprite2D.dll":
            case "Unity.Tiny.Shape2D.dll":
            case "Unity.Tiny.Text.dll":
            case "Unity.Tiny.GLFW.dll":
            case "Unity.Tiny.HitBox2D.dll":
            case "Unity.Tiny.Core2DTypes.dll":
            case "Unity.Tiny.Image2DIOSTB.dll":
            case "Unity.Tiny.Image2DIOHTML.dll":
            case "Unity.Tiny.IO.dll":
            case "Unity.Tiny.RendererGLNative.dll":
            case "Unity.Tiny.RendererGLWebGL.dll":
            case "Unity.Tiny.RendererGLES2.dll":
            case "Unity.Tiny.RendererCanvas.dll":
            case "Unity.Tiny.TextHTML.dll":
            case "Unity.Tiny.HTML.dll":
                return true;
            default:
                return false;
        }
    }

    static BindGemResult BindGemOutputFor(CSharpProgram inputProgram, CSharpProgramConfiguration config) => SupportsBindgem(inputProgram.FileName) ? MakeBindGemResultFor(inputProgram.FileName, config) : null;


    static BindGemResult MakeBindGemResultFor(NPath fileName, CSharpProgramConfiguration config)
    {
        NPath prefix = $"artifacts/bindgen/{fileName.FileNameWithoutExtension}-{config.Identifier}/bind-{fileName.FileNameWithoutExtension.Replace(".", "_")}";
        return new BindGemResult()
        {
            Header = prefix.ChangeExtension("h"),
            Cpp = prefix.ChangeExtension("cpp"),
        };
    }

    public static void ConfigureNativeProgramFor(AsmDefBasedDotsRuntimeCSharpProgram program)
    {
        if (!SupportsBindgem(program.FileName))
            return;
        var nativeProgram = program.GetOrMakeNativeProgram();
        
        nativeProgram.Sources.Add(config => new[] {BindGemOutputFor(program, config)?.Cpp}.ExcludeNulls());
        nativeProgram.PublicIncludeDirectories.Add(config => new[]{BindGemOutputFor(program, config)?.Header.Parent}.ExcludeNulls());
        nativeProgram.ExtraDependenciesForAllObjectFiles.Add(config => BindGemOutputFor(program.ReferencedPrograms.Concat(new[]{program}).Where(p=>p.IsSupportedOn(config.Platform)), config).SelectMany(bo => bo.Files));
    }
    
    class BindGemResult
    {
        public NPath Header;
        public NPath Cpp;

        public NPath[] Files => new[] {Header, Cpp}.Where(p => p != null).ToArray();
    }
}

