using System;
using System.Linq;
using Bee;
using Bee.Core;
using Bee.CSharpSupport;
using Bee.DotNet;
using Bee.Stevedore;
using Bee.Tools;
using Bee.VisualStudioSolution;
using NiceIO;
using Unity.BuildSystem.CSharpSupport;
using Unity.BuildSystem.NativeProgramSupport;

public class BuildProgram
{
    public static NPath BeeRootValue;
    public static NPath LowLevelRoot => BeeRoot.Parent.Combine("LowLevelSupport~");
    public static CSharpProgram UnityLowLevel { get; set; }
    public static DotsRuntimeCSharpProgram ZeroJobs { get; set; }
    public static DotNetAssembly NUnitFramework { get; set; }
    public static DotNetAssembly NUnitLite { get; set; }
    
    public static NPath BeeRoot
    {
        get {
            if (BeeRootValue == null)
                throw new InvalidOperationException("BeeRoot accessed before it has been initialized");
            return BeeRootValue;
        }
    }

    static void Main()
    {
        BeeRootValue = BuildProgramConfigFile.AsmDefDescriptionFor("Unity.Tiny.Text").Path.Parent.Parent.Parent.Combine("DotsPlayer/bee~");

        StevedoreGlobalSettings.Instance = new StevedoreGlobalSettings
        {
            // Manifest entries always override artifact IDs hard-coded in Bee
            // Setting EnforceManifest to true will also ensure no artifacts
            // are used without being listed in a manifest.
            EnforceManifest = true,
            Manifest =
            {
                 BeeRootValue.Combine("manifest.stevedore"),
            },
           
        };
        //The stevedore global manifest will override DownloadableCsc.Csc72 artifacts and use Csc73
        CSharpProgram.DefaultConfig = new CSharpProgramConfiguration(CSharpCodeGen.Release, DownloadableCsc.Csc72);

        UnityLowLevel = new DotsRuntimeCSharpProgram($"{LowLevelRoot}/Unity.LowLevel")
        {
            References = {UnsafeUtility.DotNetAssembly},
            Unsafe = true
        };

        ZeroJobs = new DotsRuntimeCSharpProgram(BeeRoot.Parent.Combine("ZeroJobs"), "Unity.ZeroJobs")
        {
            References = { UnityLowLevel },
            Unsafe = true
        };

        var nunit = new StevedoreArtifact("nunit-framework");
        Backend.Current.Register(nunit);
        NUnitLite = new DotNetAssembly(nunit.Path.Combine("bin", "net40", "nunitlite.dll"), Framework.Framework40);
        NUnitFramework = new DotNetAssembly(nunit.Path.Combine("bin", "net40", "nunit.framework.dll"), Framework.Framework40);

        //any asmdef that sits next to a .project file we will consider a tiny game.
        var asmDefDescriptions = BuildProgramConfigFile.AssemblyDefinitions.ToArray();
        
        var gameAsmDefs =asmDefDescriptions.Where(d => d.Path.Parent.Files("*.project").Any());
        var gamePrograms = gameAsmDefs.Select(SetupGame).ToArray();

        //any asmdef that has .Tests in its name, is going to be our indicator for being a test project for now.
        var testAsmDefs = asmDefDescriptions.Where(ad => ad.Name.EndsWith(".Tests"));
        var testPrograms = testAsmDefs.Where(asm => asm.PackageSource != "BuiltIn" && asm.PackageSource != "Registry")
            .Select(SetupTest)
            .ExcludeNulls()
            .ToArray();
        
        var vs = new VisualStudioSolution()
        {
            Path = BuildProgramConfigFile.UnityProjectPath.Combine($"{BuildProgramConfigFile.ProjectName}-Dots.sln").RelativeTo(NPath.CurrentDirectory),
            DefaultSolutionFolderFor = file => (file.Name.Contains("Unity.") || file.Name == "mscorlib") ? "Unity" : ""
        };

        var unityToolsFolder = "Unity/tools";
        if (BeeRoot.IsChildOf(BuildProgramConfigFile.UnityProjectPath))
            vs.Projects.Add(new CSharpProjectFileReference("buildprogram.gen.csproj"), unityToolsFolder);

        foreach (var gameProgram in gamePrograms)
            vs.Projects.Add(gameProgram);
        foreach (var testProgram in testPrograms)
            vs.Projects.Add(testProgram);

        var toolPrograms = new[]
            {TypeRegistrationTool.EntityBuildUtils, TypeRegistrationTool.TypeRegProgram, BindGem.Instance().Program};
        if (BeeRoot.IsChildOf(BuildProgramConfigFile.UnityProjectPath))
            foreach (var p in toolPrograms)
                vs.Projects.Add(p, unityToolsFolder);

        foreach (var config in DotsConfigs.Configs)
        {
            //we want dotnet to be the default, and we cannot have nice things: https://aras-p.info/blog/2017/03/23/How-does-Visual-Studio-pick-default-config/platform/
            var solutionConfigName = config.Identifier == "dotnet" ? "Debug (dotnet)": config.Identifier;
            
            vs.Configurations.Add(new SolutionConfiguration(solutionConfigName, (configurations, file) =>
            {
                var firstOrDefault = configurations.FirstOrDefault(c => c == config);
                return new Tuple<IProjectConfiguration, bool>(
                    firstOrDefault ?? configurations.First(),
                    firstOrDefault != null || toolPrograms.Any(t=>t.ProjectFile == file));
            }));
        }
        Backend.Current.AddAliasDependency("ProjectFiles", vs.Setup());
        
        EditorToolsBuildProgram.Setup(BeeRoot);
    }
    
    private static bool IsTestProgramDotsRuntimeCompatible(DotsRuntimeCSharpProgram arg)
    {
        //We need a better way of knowing which asmdefs are supposed to work on dots-runtime, and which do not.  for now use a simple heuristic of "is it called Editor or is it called Hybrid"
        var allFileNames = arg.References.ForAny().OfType<CSharpProgram>().Select(r => r.FileName).Append(arg.FileName).ToArray();
        if (allFileNames.Any(f=>f.Contains("Editor")))
            return false;
        if (allFileNames.Any(f=>f.Contains("Hybrid")))
            return false;
        if (allFileNames.Any(f=>f.Contains("Unity.TextMeshPro")))
            return false;
        if (allFileNames.Any(f => f.Contains("Unity.ugui")))
            return false;
        //in theory, all tests for assemblies that are used by dotsruntime targetting programs should be dots runtime compatible
        //unfortunately we have some tests today that test dotsruntime compatible code,  but the testcode itself is not dotsruntime compatible.
        //blacklist these for now
        if (arg.FileName.Contains("Unity.Scenes.Tests"))
            return false;
        if (arg.FileName.Contains("Unity.Authoring"))
            return false;
        if (arg.FileName.Contains("Unity.Serialization"))
            return false;
        if (arg.FileName.Contains("Unity.Entities.Reflection.Tests"))
            return false;
        if (arg.FileName.Contains("Unity.Properties"))
            return false;
        if (arg.FileName.Contains("Unity.Entities.Properties"))
            return false;
        if (arg.FileName.Contains("Unity.Burst.Tests"))
            return false;
        if (arg.FileName.Contains("Unity.jobs.Tests"))
            return false;
        if (arg.FileName.Contains("Unity.Collections.Tests"))
            return false;
        return true;
    }

    private static DotsRuntimeCSharpProgram SetupTest(AsmDefDescription test)
    {
        var testProgram = GetOrMakeDotsRuntimeCSharpProgramFor(test);
        if (!IsTestProgramDotsRuntimeCompatible(testProgram))
            return null;

        var config = DotsConfigs.HostDotnet;
        var builtTest = testProgram.SetupSpecificConfiguration(config);

        builtTest = TypeRegistrationTool.SetupInvocation(builtTest, config);

        NPath deployDirectory = $"build/{test.Name}/{test.Name}-{config.Identifier}";
        var deployed = builtTest.DeployTo(deployDirectory);

        testProgram.ProjectFile.OutputPath.Add(c => c == config, deployDirectory);
        testProgram.ProjectFile.BuildCommand.Add(c => c == config,
            new BeeBuildCommand(deployed.Path.ToString(), false, false).ToExecuteArgs());

        Backend.Current.AddAliasDependency(test.Name.Replace(".", "-").ToLower(), deployed.Path);
        Backend.Current.AddAliasDependency("tests", deployed.Path);

        return testProgram;
    }

    private static DotsRuntimeCSharpProgram SetupGame(AsmDefDescription game)
    {
        DotsRuntimeCSharpProgram gameProgram = GetOrMakeDotsRuntimeCSharpProgramFor(game);

        var withoutExt = new NPath(gameProgram.FileName).FileNameWithoutExtension;
        NPath exportManifest = new NPath(withoutExt + "/export.manifest");
        Backend.Current.RegisterFileInfluencingGraph(exportManifest);
        if (exportManifest.FileExists())
        {
            var dataFiles = exportManifest.MakeAbsolute().ReadAllLines();
            foreach (var dataFile in dataFiles.Select(d=>new NPath(d)))
                gameProgram.SupportFiles.Add(new DeployableFile(dataFile, "Data/"+dataFile.FileName));
        }
        
        var configToSetupGame = DotsConfigs.Configs.ToDictionary(config => config, config =>
        {
            DotNetAssembly setupGame = gameProgram.SetupSpecificConfiguration(config);
            return TypeRegistrationTool.SetupInvocation(setupGame, config);
        });

        var il2CppOutputProgram = new Il2Cpp.Il2CppOutputProgram(gameProgram.FileName + "_il2cpp");
        foreach (var kvp in configToSetupGame)
        {
            var config = kvp.Key;
            var setupGame = kvp.Value;

            if (config.ScriptingBackend == ScriptingBackend.TinyIl2cpp)
            {
                setupGame = Il2Cpp.UnityLinker.SetupInvocation(setupGame, $"artifacts/{game.Name}/{config.Identifier}_stripped", config.NativeProgramConfiguration);
                il2CppOutputProgram.SetupConditionalSourcesAndLibrariesForConfig(config, setupGame);
            }
        }

        foreach (var kvp in configToSetupGame)
        {
            var config = kvp.Key;
            var setupGame = kvp.Value;
            NPath deployPath = $"build/{game.Name}/{game.Name}-{config.Identifier}";
            
            IDeployable deployedGame;

            if (config.ScriptingBackend == ScriptingBackend.TinyIl2cpp)
            {
                var builtNativeProgram = il2CppOutputProgram.SetupSpecificConfiguration(
                        config.NativeProgramConfiguration,
                        config.NativeProgramConfiguration.ExecutableFormat
                        )
                        .WithDeployables(setupGame.RecursiveRuntimeDependenciesIncludingSelf.SelectMany(a => a.Deployables.Where(d=>!(d is DotNetAssembly) && !(d is StaticLibrary)))
                        .ToArray());

                deployedGame = builtNativeProgram.DeployTo(deployPath);
            }
            else
            {
                deployedGame  = setupGame.DeployTo(deployPath);

                var dotNetAssembly = (DotNetAssembly) deployedGame;
                
                //Usually a dotnet runtime game does not have a static void main(), and instead references another "entrypoint asmdef" that provides it.
                //This is convenient, but what makes it weird is that you have to start YourEntryPoint.exe  instead of YourGame.exe.   Until we have a better
                //solution for this, we're going to copy YourEntryPoint.exe to YourGame.exe, so that it's easier to find, and so that when it runs and you look
                //at the process name you understand what it is.
                if (deployedGame.Path.HasExtension("dll"))
                {
                    var to = deployPath.Combine(deployedGame.Path.ChangeExtension("exe").FileName);
                    var from = dotNetAssembly.RecursiveRuntimeDependenciesIncludingSelf.Single(a=>a.Path.HasExtension("exe")).Path;
                    Backend.Current.AddDependency(deployedGame.Path, CopyTool.Instance().Setup(to, from));
                }
            }

            NPath deployedGamePath = deployedGame.Path;
            
            gameProgram.ProjectFile.StartInfo.Add(c=>c==config, StartInfoFor(config, deployedGame));
            gameProgram.ProjectFile.BuildCommand.Add(c=>c == config, new BeeBuildCommand(deployedGamePath.ToString(), false, false).ToExecuteArgs());

            Backend.Current.AddAliasDependency($"{game.Name.ToLower()}-{config.Identifier}", deployedGamePath);
            Backend.Current.AddAliasDependency($"{game.Name.ToLower()}-all", deployedGamePath);
        }

        return gameProgram;
    }

    private static StartInfo StartInfoFor(DotsRuntimeCSharpProgramConfiguration config, IDeployable deployedGame)
    {
        var exe = deployedGame.Path;
        if (deployedGame is DotNetAssembly dotNetGame)
            exe = dotNetGame.Path.ChangeExtension("exe");
        
        if (config.Platform is WebGLPlatform)
            return new BrowserStartInfo(new Uri(deployedGame.Path.MakeAbsolute().ToString(SlashMode.Native)).AbsoluteUri);
        
        return new ExecutableStartInfo(new Shell.ExecuteArgs() {Executable = exe, WorkingDirectory = exe.Parent }, true);
    }

    static readonly Cache<DotsRuntimeCSharpProgram, AsmDefDescription> _cache = new Cache<DotsRuntimeCSharpProgram, AsmDefDescription>();

    public static DotsRuntimeCSharpProgram GetOrMakeDotsRuntimeCSharpProgramFor(AsmDefDescription asmDefDescription) => 
        _cache.GetOrMake(asmDefDescription, () => new AsmDefBasedDotsRuntimeCSharpProgram(asmDefDescription));

    public static bool LivesInProject(NPath file) => file.IsChildOf(BuildProgramConfigFile.UnityProjectPath) &&
                                                     !(file.HasDirectory("PackageCache") &&
                                                       file.HasDirectory("Library"));
}
