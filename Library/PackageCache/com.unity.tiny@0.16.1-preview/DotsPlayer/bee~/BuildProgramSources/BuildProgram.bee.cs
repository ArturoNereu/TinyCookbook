using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using Bee;
using Bee.Core;
using Bee.CSharpSupport;
using Bee.DotNet;
using Bee.Stevedore;
using Bee.Tools;
using Bee.TundraBackend;
using Bee.VisualStudioSolution;
using NiceIO;
using Unity.BuildSystem.CSharpSupport;
using Unity.BuildSystem.NativeProgramSupport;
using Unity.BuildTools;

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

    static bool CanSkipSetupOf(string programName, DotsRuntimeCSharpProgramConfiguration config)
    {
        if (IsRequestedTargetExactlyProjectFiles())
            return true;
        
        if (!IsRequestedTargetExactlySingleAppSingleConfig()) 
            return false;
        
        var configIdentifier = (programName.ToLower() + "-" + config.Identifier);
        var canSkipSetupOf = configIdentifier != StandaloneBeeDriver.GetCommandLineTargets().Single();
            
        return canSkipSetupOf;
    }

    public static bool IsRequestedTargetExactlyProjectFiles()
    {
        var commandLineTargets = StandaloneBeeDriver.GetCommandLineTargets();
        if (commandLineTargets.Count() != 1)
            return false;

        return commandLineTargets.Single() == "ProjectFiles";
    }
    
    private static bool IsRequestedTargetExactlySingleAppSingleConfig()
    {
        var commandLineTargets = StandaloneBeeDriver.GetCommandLineTargets();
        if (commandLineTargets.Count() != 1)
            return false;

        var commandLineTarget = commandLineTargets.Single();
        return DotsConfigs.Configs.Any(c => commandLineTarget.EndsWith(c.Identifier));
    }

    static void Main()
    {
        if (!(Backend.Current is TundraBackend))
        {
            StandaloneBeeDriver.RunBuildProgramInBeeEnvironment("dummy.json", Main);
            return;
        }
        
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

        ZeroJobs = new DotsRuntimeCSharpProgram($"{LowLevelRoot}/Unity.ZeroJobs")
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



        var gameAsmDefs = asmDefDescriptions.Where(d => d.Path.Parent.Files("*.project").Any());
        var gamePrograms = gameAsmDefs.Select(SetupGame).ToArray();

        //any asmdef that has .Tests in its name, is going to be our indicator for being a test project for now.
        var testAsmDefs = asmDefDescriptions.Where(ad => ad.Name.EndsWith(".Tests"));
        
        var testPrograms = testAsmDefs
            .Where(t=>DotsRuntimeCSharpProgram.DoesPackageSourceIndicateUserHasControlOverSource(t.PackageSource))
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

        foreach (var config in new[] { DotsConfigs.ProjectFileConfig})
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
        
        if (!IsRequestedTargetExactlySingleAppSingleConfig())
            Backend.Current.AddAliasDependency("ProjectFiles", vs.Setup());
        
        EditorToolsBuildProgram.Setup(BeeRoot);
    }
    
    private static bool IsTestProgramDotsRuntimeCompatible(DotsRuntimeCSharpProgram arg)
    {
        //We need a better way of knowing which asmdefs are supposed to work on dots-runtime, and which do not.  for now use a simple heuristic of "is it called Editor or is it called Hybrid"
        var allFileNames = Enumerable.Append(arg.References.ForAny().OfType<CSharpProgram>().Select(r => r.FileName), arg.FileName).ToArray();
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

        var name = test.Name;
        
        SetupTestForConfig(name, testProgram, DotsConfigs.HostDotnet);
        SetupTestForConfig(name, testProgram, DotsConfigs.MultithreadedJobsTestConfig);

        return testProgram;
    }

    static void SetupTestForConfig(string name, AsmDefBasedDotsRuntimeCSharpProgram testProgram, DotsRuntimeCSharpProgramConfiguration config)
    {
        var builtTest = testProgram.SetupSpecificConfiguration(config);

        builtTest = TypeRegistrationTool.SetupInvocation(builtTest, config);

        NPath deployDirectory = $"build/{name}/{name}-{config.Identifier}";
        var deployed = builtTest.DeployTo(deployDirectory);

        testProgram.ProjectFile.OutputPath.Add(c => c == config, deployDirectory);
        testProgram.ProjectFile.BuildCommand.Add(c => c == config, new BeeBuildCommand(deployed.Path.ToString(), false, false).ToExecuteArgs());

        var testAlias = name.Replace(".", "-").ToLower();
        if (config.MultiThreadedJobs)
            testAlias += "-mt";

        Backend.Current.AddAliasDependency(testAlias, deployed.Path);
        Backend.Current.AddAliasDependency("tests", deployed.Path);
    }

    private static DotsRuntimeCSharpProgram SetupGame(AsmDefDescription game)
    {
        var gameProgram = GetOrMakeDotsRuntimeCSharpProgramFor(game);

        var withoutExt = new NPath(gameProgram.FileName).FileNameWithoutExtension;
        NPath exportManifest = new NPath(withoutExt + "/export.manifest");
        Backend.Current.RegisterFileInfluencingGraph(exportManifest);
        if (exportManifest.FileExists())
        {
            var dataFiles = exportManifest.MakeAbsolute().ReadAllLines();
            foreach (var dataFile in dataFiles.Select(d=>new NPath(d)))
                gameProgram.SupportFiles.Add(new DeployableFile(dataFile, "Data/"+dataFile.FileName));
        }

        {
            var config = DotsConfigs.ProjectFileConfig;
            gameProgram.ProjectFile.StartInfo.Add(c => c == config, StartInfoFor(config, EntryPointExecutableFor(gameProgram,config)));
            gameProgram.ProjectFile.BuildCommand.Add(c => c == config,new BeeBuildCommand(GameDeployBinaryFor(gameProgram, config).ToString(), false, false).ToExecuteArgs());

        }

        var configToSetupGame = DotsConfigs.Configs.Where(c=>!CanSkipSetupOf(game.Name,c)).ToDictionary(config => config, config =>
        {
            DotNetAssembly setupGame = gameProgram.SetupSpecificConfiguration(config);
            return TypeRegistrationTool.SetupInvocation(setupGame, config);
        });

        var il2CppOutputProgram = new Il2Cpp.Il2CppOutputProgram(gameProgram.FileName + "_il2cpp");
        var configToSetupGameStripped = new Dictionary<DotsRuntimeCSharpProgramConfiguration, DotNetAssembly>();
        foreach (var kvp in configToSetupGame)
        {
            var config = kvp.Key;
            var setupGame = kvp.Value;

            if (config.ScriptingBackend == ScriptingBackend.TinyIl2cpp)
            {
                setupGame = Il2Cpp.UnityLinker.SetupInvocation(setupGame, $"artifacts/{game.Name}/{config.Identifier}_stripped", config.NativeProgramConfiguration);
                il2CppOutputProgram.SetupConditionalSourcesAndLibrariesForConfig(config, setupGame);
                configToSetupGameStripped[kvp.Key] = setupGame;
            }
            else
            {
                configToSetupGameStripped[kvp.Key] = kvp.Value;
            }
        }

        foreach (var kvp in configToSetupGameStripped)
        {
            var config = kvp.Key;
            var setupGame = kvp.Value;
            NPath deployPath = GameDeployDirectoryFor(gameProgram, config);
            
            IDeployable deployedGame;
            NPath entryPointExecutable = null;
            
            if (config.ScriptingBackend == ScriptingBackend.TinyIl2cpp)
            {
                var builtNativeProgram = il2CppOutputProgram.SetupSpecificConfiguration(
                        config.NativeProgramConfiguration,
                        config.NativeProgramConfiguration.ExecutableFormat
                        )
                        .WithDeployables(setupGame.RecursiveRuntimeDependenciesIncludingSelf.SelectMany(a => a.Deployables.Where(d=>!(d is DotNetAssembly) && !(d is StaticLibrary)))
                        .ToArray());

                deployedGame = builtNativeProgram.DeployTo(deployPath);
                entryPointExecutable = deployedGame.Path;
                if (config.EnableManagedDebugging)
                    Backend.Current.AddDependency(deployedGame.Path, Il2Cpp.CopyIL2CPPMetadataFile(deployPath, setupGame));
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
                    entryPointExecutable = to;
                }
                else
                {
                    entryPointExecutable = deployedGame.Path;
                }
            }

            
            //Because we use multidag, and try to not run all the setupcode when we just want to create projectfiles, we have a bit of a challenge.
            //Projectfiles require exact start and build commands. So we need to have a cheap way to calculate those. However, it's important that they
            //exactly match the actual place where the buildprogram is going to place our files. If these don't match things break down. The checks
            //in this block, they compare the "quick way to determine where the binary will be placed, and what the start executable is",  with the
            //actual return values returned from .DeployTo(), when we do run the actual buildcode.
            NPath deployedGamePath = GameDeployBinaryFor(gameProgram, config);
            if (deployedGame.Path != deployedGamePath)
                throw new InvalidProgramException($"We expected deployPath to be {deployedGamePath}, but in reality it was {deployedGame.Path}");
            var expectedEntryPointExecutable = EntryPointExecutableFor(gameProgram, config);
            if (entryPointExecutable != expectedEntryPointExecutable)
                throw new InvalidProgramException($"We expected entryPointExecutable to be {expectedEntryPointExecutable}, but in reality it was {entryPointExecutable}");
            
            Backend.Current.AddAliasDependency($"{game.Name.ToLower()}-{config.Identifier}", deployedGamePath);
            Backend.Current.AddAliasDependency($"{game.Name.ToLower()}-all", deployedGamePath);
        }

        return gameProgram;
    }

    private static NPath EntryPointExecutableFor(AsmDefBasedDotsRuntimeCSharpProgram gameProgram, DotsRuntimeCSharpProgramConfiguration config)
    {
        if (gameProgram.FileName.EndsWith(".exe") || config.ScriptingBackend != ScriptingBackend.Dotnet)
            return GameDeployBinaryFor(gameProgram,config);
       
        return GameDeployDirectoryFor(gameProgram, config).Combine(new NPath(gameProgram.FileName).FileNameWithoutExtension+".exe");
    }

    private static NPath GameDeployBinaryFor(AsmDefBasedDotsRuntimeCSharpProgram game, DotsRuntimeCSharpProgramConfiguration config)
    {
        var fileName = config.ScriptingBackend == ScriptingBackend.Dotnet ? 
            game.FileName
            : new NPath(game.FileName).ChangeExtension(config.NativeProgramConfiguration.ExecutableFormat.Extension);
        
        return GameDeployDirectoryFor(game, config).Combine(fileName);
    }

    private static NPath GameDeployDirectoryFor(AsmDefBasedDotsRuntimeCSharpProgram game, DotsRuntimeCSharpProgramConfiguration config)
    {
        return $"build/{game.AsmDefDescription.Name}/{game.AsmDefDescription.Name}-{config.Identifier}";
    }

    private static StartInfo StartInfoFor(DotsRuntimeCSharpProgramConfiguration config, NPath deployedGamePath)
    {
        if (config.Platform is WebGLPlatform)
            return new BrowserStartInfo(new Uri(deployedGamePath.MakeAbsolute().ToString(SlashMode.Native)).AbsoluteUri);
        
        return new ExecutableStartInfo(new Shell.ExecuteArgs() {Executable = deployedGamePath, WorkingDirectory = deployedGamePath.Parent }, true);
    }

    static readonly Cache<AsmDefBasedDotsRuntimeCSharpProgram, AsmDefDescription> _cache = new Cache<AsmDefBasedDotsRuntimeCSharpProgram, AsmDefDescription>();

    public static AsmDefBasedDotsRuntimeCSharpProgram GetOrMakeDotsRuntimeCSharpProgramFor(AsmDefDescription asmDefDescription) => 
        _cache.GetOrMake(asmDefDescription, () => new AsmDefBasedDotsRuntimeCSharpProgram(asmDefDescription));

}
