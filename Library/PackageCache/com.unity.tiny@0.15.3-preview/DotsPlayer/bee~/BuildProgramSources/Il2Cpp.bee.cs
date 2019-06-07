using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text;
using Bee;
using Bee.Core;
using Bee.CSharpSupport;
using Bee.DotNet;
using Bee.NativeProgramSupport.Building;
using Bee.NativeProgramSupport.Building.FluentSyntaxHelpers;
using Bee.Stevedore;
using Bee.Toolchain.Emscripten;
using Bee.Toolchain.GNU;
using Bee.Toolchain.LLVM;
using Bee.Toolchain.VisualStudio;
using Bee.Toolchain.Windows;
using Bee.Toolchain.Xcode;
using Newtonsoft.Json.Linq;
using NiceIO;
using Unity.BuildSystem.CSharpSupport;
using Unity.BuildSystem.NativeProgramSupport;
using Unity.BuildSystem.VisualStudio;
using Unity.BuildTools;

public static class Il2Cpp
{
    //change to tuple when we can finally use tuples
    struct DistributionAndDeps
    {
        public IFileBundle Distribution;
        public IFileBundle Deps;
    }

    static Lazy<DistributionAndDeps> _il2cppAndDeps = new Lazy<DistributionAndDeps>(() =>
    {
        NPath loc;
        if (Il2CppCustomLocation.CustomLocation != null)
        {
            loc = Il2CppCustomLocation.CustomLocation;
            if (!loc.DirectoryExists())
                throw new ArgumentException(
                    $"Il2CppCustomLocation.CustomLocation set to {loc}, but that doesn't exist");
        }
        else
        {
            loc = BuildProgram.BeeRoot.Parent.Parent.Parent.Parent.Parent.Combine("il2cpp");
        }

        if (loc.DirectoryExists() && Environment.GetEnvironmentVariable("IL2CPP_FROM_STEVE") == null)
        {
            var localDeps = loc.Parent.Combine("il2cpp-deps/artifacts/Stevedore");
            if (!localDeps.DirectoryExists())
                throw new ArgumentException(
                    "We found your il2cpp checkout, but not the il2cpp-deps directory next to it.");

            return new DistributionAndDeps()
                {Distribution = new LocalFileBundle(loc), Deps = new LocalFileBundle(localDeps)};
        }

        return new DistributionAndDeps() {Distribution = Il2CppFromSteve(), Deps = Il2CppDepsFromSteve()};
    });

    private static IFileBundle Il2CppDependencies => _il2cppAndDeps.Value.Deps;
    public static IFileBundle Distribution => _il2cppAndDeps.Value.Distribution;

    private static Lazy<CSharpProgram> _tinyCorlib = new Lazy<CSharpProgram>(() =>
    {
        return new CSharpProgram()
        {
            FileName = "mscorlib.dll",
            Sources = {Il2Cpp.Distribution.GetFileList("mscorlib").Where(f => f.HasExtension("cs"))},
            ProjectFilePath = "Unity.TinyCorlib.csproj",
            Framework = {Framework.FrameworkNone},
            Unsafe = true,
            LanguageVersion = "7.3",
            Defines = {c => c.CodeGen == CSharpCodeGen.Debug, "DEBUG"},
            ProjectFile = { RedirectMSBuildBuildTargetToBee = true }
        };
    });

    private static readonly DotNetRunnableProgram Il2CppRunnableProgram =
        new DotNetRunnableProgram(new DotNetAssembly(Distribution.Path.Combine("build/il2cpp.exe"),
            Framework.Framework46));

    public static CSharpProgram TinyCorlib => _tinyCorlib.Value;

    private static IFileBundle Il2CppFromSteve()
    {
        var stevedoreArtifact = new StevedoreArtifact("il2cpp");
        Backend.Current.Register(stevedoreArtifact);
        return stevedoreArtifact;
    }

    private static IFileBundle Il2CppDepsFromSteve()
    {
        var stevedoreArtifact = new StevedoreArtifact("MonoBleedingEdgeSub");
        Backend.Current.Register(stevedoreArtifact);
        return stevedoreArtifact;
    }

    internal class Il2CppOutputProgram : NativeProgram
    {
        public Il2CppOutputProgram(string name) : base(name)
        {
            Libraries.Add(LibIL2Cpp);
            Libraries.Add(BoehmGCProgram);
            Sources.Add(Distribution.Path.Combine("external").Combine("xxHash/xxhash.c"));

            this.DynamicLinkerSettingsForMsvc()
                .Add(l => l.WithSubSystemType(SubSystemType.Console).WithEntryPoint("wWinMainCRTStartup"));
        
            Libraries.Add(c => c.ToolChain.Platform is WindowsPlatform, new SystemLibrary("kernel32.lib"));
            Defines.Add(c => c.Platform is WebGLPlatform, "IL2CPP_DISABLE_GC=1");

            this.DynamicLinkerSettingsForMsvc().Add(l => l
                .WithSubSystemType(SubSystemType.Console)
                .WithEntryPoint("wWinMainCRTStartup")
            );
            Defines.Add(c => c.ToolChain.DynamicLibraryFormat == null, "FORCE_PINVOKE_INTERNAL=1");

            this.DynamicLinkerSettingsForEmscripten().Add(c =>
                c.WithShellFile(BuildProgram.BeeRoot.Combine("shell.html")));
                    

            Libraries.Add(c => c.Platform is WebGLPlatform,new PreJsLibrary(BuildProgram.BeeRoot.Combine("tiny_runtime.js")));
        }

        

        public void SetupConditionalSourcesAndLibrariesForConfig(DotsRuntimeCSharpProgramConfiguration config, DotNetAssembly setupGame)
        {
            NPath[] il2cppGeneratedFiles = SetupInvocation(setupGame);
            //todo: stop comparing identifier.
            Sources.Add(npc => ((DotsRuntimeNativeProgramConfiguration)npc).CSharpConfig == config, il2cppGeneratedFiles);
            Libraries.Add(npc => ((DotsRuntimeNativeProgramConfiguration)npc).CSharpConfig == config, setupGame.RecursiveRuntimeDependenciesIncludingSelf.SelectMany(r=>r.Deployables.OfType<StaticLibrary>()));
        }
    }
    
    public static NPath[] SetupInvocation(DotNetAssembly inputAssembly)
    {
        var profile = "unitytiny";
        var il2CppTargetDir = inputAssembly.Path.Parent.Combine(inputAssembly.Path.FileName + "-il2cpp-sources");

        var args = new List<string>()
        {
            "--convert-to-cpp",
            "--disable-cpp-chunks",

            //  "--directory", $"{InputAssembly.Path.Parent}",
            "--generatedcppdir",
            $"{il2CppTargetDir}",

            // Make settings out of these
            $"--dotnetprofile={profile}", // Resolve from DotNetAssembly
            "--libil2cpp-static",
            "--emit-null-checks=0",
            "--enable-array-bounds-check=0",
            "--enable-predictable-output",
            //"--enable-stacktrace=1"
            //"--profiler-report",
            //"--enable-stats",
        };

        var iarrdis = MoveExeToFront(inputAssembly.RecursiveRuntimeDependenciesIncludingSelf);
        args.AddRange(
            iarrdis.SelectMany(a =>
                new[] {"--assembly", a.Path.ToString()}));

        var il2cppOutputFiles = new[]
            {
                // static files
                //"Il2CppComCallableWrappers.cpp",
                //"Il2CppProjectedComCallableWrapperMethods.cpp",
                "TinyTypes.cpp", "driver.cpp", "StaticConstructors.cpp", "StringLiterals.cpp",
                "GenericMethods.cpp",
                "Generics.cpp", "ReversePInvokeWrappers.cpp",
                "StaticInitialization.cpp"
            }.Concat(iarrdis.Select(asm => asm.Path.FileNameWithoutExtension + ".cpp"))
            .Select(il2CppTargetDir.Combine)
            .ToArray();

        var il2cppInputs = Distribution.GetFileList("build")
            .Concat(iarrdis.SelectMany(a => a.Paths))
            .Concat(new[] {Distribution.Path.Combine("libil2cpptiny", "libil2cpptiny.icalls")})
            .Concat(new[] {Il2CppDependencies.GetFileList().First()});
                
        Backend.Current.AddAction(
            "Il2Cpp",
            targetFiles: il2cppOutputFiles,
            inputs: il2cppInputs.ToArray(),
            Il2CppRunnableProgram.InvocationString,
            args.ToArray());

        return il2cppOutputFiles;
    }

    private static IEnumerable<DotNetAssembly> MoveExeToFront(IEnumerable<DotNetAssembly> assemblies)
    {
        bool foundExe = false;
        var storage = new List<DotNetAssembly>();
        foreach (var a in assemblies)
        {
            if (foundExe)
            {
                yield return a;
                continue;
            }

            if (!a.Path.HasExtension("exe"))
            {
                storage.Add(a);
                continue;
            }
            
            yield return a;
            foreach (var s in storage)
                yield return s;
            foundExe = true;
        }
    }

    public static NativeProgram LibIL2Cpp => _libil2cpp.Value;
    
    static Lazy<NativeProgram> _libil2cpp = new Lazy<NativeProgram>(()=>CreateLibIl2CppProgram(false));
    
    public static NativeProgram BoehmGCProgram => _boehmGCProgram.Value;
    static Lazy<NativeProgram> _boehmGCProgram = new Lazy<NativeProgram>(()=>CreateBoehmGcProgram(Distribution.Path.Combine("external/bdwgc")));

    
    static NativeProgram CreateLibIl2CppProgram(bool useExceptions, NativeProgram boehmGcProgram = null, string libil2cppname = "libil2cpptiny")
    {
        var fileList = Distribution.GetFileList(libil2cppname).ToArray();

        var nPaths = fileList.Where(f => f.HasExtension("cpp")).ToArray();
        var win32Sources = nPaths.Where(p => p.HasDirectory("Win32")).ToArray();
        var posixSources = nPaths.Where(p => p.HasDirectory("Posix")).ToArray();
        nPaths = nPaths.Except(win32Sources).Except(posixSources).ToArray();

        var program = new NativeProgram("libil2cpp")
        {
            Sources =
            {
                nPaths,
                {c => c.Platform.HasPosix, posixSources},
                {c => c.Platform is WindowsPlatform, win32Sources}
            },
            Exceptions = {useExceptions},
            PublicIncludeDirectories =
                {Distribution.Path.Combine(libil2cppname), Distribution.Path.Combine("libil2cpp")},
            PublicDefines =
            {
                "NET_4_0",
                "GC_NOT_DLL",
                "RUNTIME_IL2CPP",

                "LIBIL2CPP_IS_IN_EXECUTABLE=1",
                {c => c.ToolChain is VisualStudioToolchain, "NOMINMAX", "WIN32_THREADS", "IL2CPP_TARGET_WINDOWS=1"},
                {c => c.CodeGen == CodeGen.Debug, "DEBUG", "IL2CPP_DEBUG"}
            },
            Libraries =
            {
                {
                    c => c.Platform is WindowsPlatform,
                    new[]
                    {
                        "user32.lib", "advapi32.lib", "ole32.lib", "oleaut32.lib", "Shell32.lib", "Crypt32.lib",
                        "psapi.lib", "version.lib", "MsWSock.lib", "ws2_32.lib", "Iphlpapi.lib", "Dbghelp.lib"
                    }.Select(s => new SystemLibrary(s))
                },
                {c => c.Platform is MacOSXPlatform, new PrecompiledLibrary[] {new SystemFramework("CoreFoundation")}},
                {c => c.Platform is LinuxPlatform, new SystemLibrary("dl")}
            }
        };
        
        program.Libraries.Add(BoehmGCProgram);
    
        program.RTTI.Set(c => useExceptions && c.ToolChain.EnablingExceptionsRequiresRTTI);

        if (libil2cppname == "libil2cpptiny")
        {
            program.Sources.Add(Distribution.GetFileList("libil2cpp/os"));
            program.Sources.Add(Distribution.GetFileList("libil2cpp/gc"));
            program.Sources.Add(Distribution.GetFileList("libil2cpp/utils"));
            program.Sources.Add(Distribution.GetFileList("libil2cpp/vm-utils"));
            program.PublicIncludeDirectories.Add(Distribution.Path.Combine("libil2cpp"));
            program.PublicIncludeDirectories.Add(Distribution.Path.Combine("external").Combine("xxHash"));
            program.PublicDefines.Add("IL2CPP_TINY");
        }

        //program.CompilerSettingsForMsvc().Add(l => l.WithCompilerRuntimeLibrary(CompilerRuntimeLibrary.None));

        return program;
    }
    
    public static NativeProgram CreateBoehmGcProgram(NPath boehmGcRoot)
    {
        var program = new NativeProgram("boehm-gc");

        program.Sources.Add($"{boehmGcRoot}/extra/gc.c");
        program.PublicIncludeDirectories.Add($"{boehmGcRoot}/include");
        program.IncludeDirectories.Add($"{boehmGcRoot}/libatomic_ops/src");
        program.Defines.Add(
            "ALL_INTERIOR_POINTERS=1",
            "GC_GCJ_SUPPORT=1",
            "JAVA_FINALIZATION=1",
            "NO_EXECUTE_PERMISSION=1",
            "GC_NO_THREADS_DISCOVERY=1",
            "IGNORE_DYNAMIC_LOADING=1",
            "GC_DONT_REGISTER_MAIN_STATIC_DATA=1",
            "NO_DEBUGGING=1",
            "GC_VERSION_MAJOR=7",
            "GC_VERSION_MINOR=7",
            "GC_VERSION_MICRO=0",
            "HAVE_BDWGC_GC",
            "HAVE_BOEHM_GC",
            "DEFAULT_GC_NAME=\"BDWGC\"",
            "NO_CRT=1",
            "DONT_USE_ATEXIT=1",
            "NO_GETENV=1");

        program.Defines.Add(c => !(c.Platform is WebGLPlatform), "GC_THREADS=1", "USE_MMAP=1", "USE_MUNMAP=1");
        program.Defines.Add(c => c.ToolChain is VisualStudioToolchain, "NOMINMAX", "WIN32_THREADS");
        //program.CompilerSettingsForMsvc().Add(l => l.WithCompilerRuntimeLibrary(CompilerRuntimeLibrary.None));
        return program;
    }



/*

public static BuiltNativeProgram SetupMapFileParser(NPath mapFileParserRoot, CodeGen codegen = CodeGen.Release)
{
    var toolchain = ToolChain.Store.Host();
    var mapFileParserProgram = new NativeProgram("MapFileParser");
    mapFileParserProgram.Sources.Add(mapFileParserRoot.Files("*.cpp", true));
    mapFileParserProgram.Exceptions.Set(true);
    mapFileParserProgram.RTTI.Set(c => c.ToolChain.EnablingExceptionsRequiresRTTI);
    mapFileParserProgram.Libraries.Add(c => c.Platform is WindowsPlatform, new SystemLibrary("Shell32.lib"));
    return mapFileParserProgram.SetupSpecificConfiguration(new NativeProgramConfiguration(codegen, toolchain, false), toolchain.ExecutableFormat);
}

public static BuiltNativeProgram SetupLibIl2CppLackey(NPath libIl2CppLackeyRoot, WindowsToolchain toolchain)
{
    var program = new NativeProgram("libil2cpp-lackey");
    program.Sources.Add($"{libIl2CppLackeyRoot}/DllMain.cpp");
    program.DynamicLinkerSettingsForWindows().Add(l => l.WithEntryPoint("DllMain"));
    return program.SetupSpecificConfiguration(new NativeProgramConfiguration(CodeGen.Release, toolchain, false), toolchain.DynamicLibraryFormat);
}

public static NPath SetupSymbolMap(NPath executableMapFile, NPath mapFileParserExe, ToolChain toolchain)
{
    var mapFileFormat = toolchain.CppCompiler is MsvcCompiler ? "MSVC" :
        toolchain.CppCompiler is ClangCompiler ? "Clang" :
        toolchain.CppCompiler is GccCompiler ? "GCC" : throw new Exception("Unknown map file format");

    var executableSymbolMap = executableMapFile.Parent.Combine("Data/SymbolMap");
    Backend.Current.AddAction(
        "ConvertSymbolMap",
        new[] {executableSymbolMap},
        new[] {mapFileParserExe, executableMapFile},
        mapFileParserExe.InQuotes(),
        new[] {$"-format={mapFileFormat}", executableMapFile.InQuotes(), executableSymbolMap.InQuotes()});
    return executableSymbolMap;
}

public static NativeProgram CreateBoehmGcProgram(NPath boehmGcRoot)
{
    var program = new NativeProgram("boehm-gc");

    program.Sources.Add($"{boehmGcRoot}/extra/gc.c");
    program.PublicIncludeDirectories.Add($"{boehmGcRoot}/include");
    program.IncludeDirectories.Add($"{boehmGcRoot}/libatomic_ops/src");
    program.Defines.Add(
        "ALL_INTERIOR_POINTERS=1",
        "GC_GCJ_SUPPORT=1",
        "JAVA_FINALIZATION=1",
        "NO_EXECUTE_PERMISSION=1",
        "GC_NO_THREADS_DISCOVERY=1",
        "IGNORE_DYNAMIC_LOADING=1",
        "GC_DONT_REGISTER_MAIN_STATIC_DATA=1",
        "NO_DEBUGGING=1",
        "GC_VERSION_MAJOR=7",
        "GC_VERSION_MINOR=7",
        "GC_VERSION_MICRO=0",
        "HAVE_BDWGC_GC",
        "HAVE_BOEHM_GC",
        "DEFAULT_GC_NAME=\"BDWGC\"",
        "NO_CRT=1",
        "DONT_USE_ATEXIT=1",
        "NO_GETENV=1");

    program.Defines.Add(c => !(c.Platform is WebGLPlatform), "GC_THREADS=1", "USE_MMAP=1", "USE_MUNMAP=1");
    program.Defines.Add(c => c.ToolChain is VisualStudioToolchain, "NOMINMAX", "WIN32_THREADS");
    //program.CompilerSettingsForMsvc().Add(l => l.WithCompilerRuntimeLibrary(CompilerRuntimeLibrary.None));
    return program;
}

*/


/*
public static DotNetAssembly SetupLinker(DotNetAssembly inputAssembly, NativeProgramConfiguration nativeProgramConfiguration)
{
    var linkerAssembly = new DotNetAssembly(Distribution.Path.Combine("build/UnityLinker.exe"), Framework.Framework471);
    var linker = new DotNetRunnableProgram(linkerAssembly);

    var outputDir = inputAssembly.Path.Parent.Combine("linkeroutput");

    // combine input files with overrides
    var inputFiles = inputAssembly.RecursiveRuntimeDependenciesIncludingSelf.ToList();
    var nonMainInputs = inputFiles.Exclude(inputAssembly);
    var nonMainOutputs = nonMainInputs.Select(a => Clone(outputDir, a)).ToArray();

    var newDeploy = inputFiles.SelectMany(f => f.Deployables.Where(d=>!(d is DotNetAssembly))).Distinct().ToArray();

    var mainTargetFile = Clone(outputDir, inputAssembly).WithRuntimeDependencies(nonMainOutputs)
        .WithDeployables(newDeploy);

    NPath bclDir = Il2CppDependencies.Path.Combine("MonoBleedingEdge/builds/monodistribution/lib/mono/unityaot");

    var dotNetDeps = new[] {"mscorlib.dll", "System.dll", "System.Configuration.dll", "System.Xml.dll", "System.Core.dll"};
    var isFrameworkNone = inputAssembly.Framework is FrameworkNone;
    var bcl = isFrameworkNone
        ? Array.Empty<DotNetAssembly>()
        : dotNetDeps
        .Select(f => new DotNetAssembly(outputDir.Combine(f), Framework.Framework46)).ToArray();

    var inputPaths = Unity.BuildTools.EnumerableExtensions.Append(inputFiles, linkerAssembly).SelectMany(a => a.Paths);
    inputPaths = Unity.BuildTools.EnumerableExtensions.Append(inputPaths, bcl.Select(d => d.Path).ToArray());

    var linkerArguments = new List<string>
    {
        $"--include-public-assembly={inputAssembly.Path.InQuotes()}",
        $"--out={outputDir.InQuotes()}",
        "--use-dots-options",
        "--dotnetprofile=" + (isFrameworkNone ? "unitydots" : "unityaot"),
        "--rule-set=experimental" // This will enable modification of method bodies to further reduce size.
    };

    foreach (var inputDirectory in inputFiles.Select(f => f.Path.Parent).Distinct())
        linkerArguments.Add($"--include-directory={inputDirectory.InQuotes()}");

    if (!isFrameworkNone)
        linkerArguments.Add($"--search-directory={bclDir.InQuotes()}");

    var targetPlatform = GetTargetPlatformForLinker(nativeProgramConfiguration.Platform);
    if (!string.IsNullOrEmpty(targetPlatform))
        linkerArguments.Add($"--platform={targetPlatform}");

    var targetArchitecture = GetTargetArchitectureForLinker(nativeProgramConfiguration.ToolChain.Architecture);
    if (!string.IsNullOrEmpty(targetPlatform))
        linkerArguments.Add($"--architecture={targetArchitecture}");

    var targetFiles = Unity.BuildTools.EnumerableExtensions.Prepend(nonMainOutputs, mainTargetFile);
    targetFiles = targetFiles.Append(bcl);
    Backend.Current.AddAction(
        "UnityLinker",
        targetFiles: targetFiles.SelectMany(a=>a.Paths).ToArray(),
        inputs: inputPaths.ToArray(),
        executableStringFor: linker.InvocationString,
        commandLineArguments: linkerArguments.ToArray(),
        allowUnwrittenOutputFiles: false,
        allowUnexpectedOutput: false,
        allowedOutputSubstrings: new[] {"Output action"});


    return mainTargetFile.WithRuntimeDependencies(bcl).DeployTo(inputAssembly.Path.Parent.Combine("finaloutput"));
}
*/

    public static class UnityLinker
    {
        public static DotNetAssembly SetupInvocation(DotNetAssembly inputGame, NPath outputPath, NativeProgramConfiguration config)
        {
            return inputGame.ApplyDotNetAssembliesPostProcessor(outputPath,(inputAssemblies, targetDir) => AddActions(inputAssemblies, targetDir, config)
            );
        }
        
        static void AddActions(DotNetAssembly[] inputAssemblies, NPath targetDirectory, NativeProgramConfiguration nativeProgramConfiguration)
        {
            var linkerAssembly = new DotNetAssembly(Distribution.Path.Combine("build/UnityLinker.exe"), Framework.Framework471);
            var linker = new DotNetRunnableProgram(linkerAssembly);
            
            var outputDir = targetDirectory;
            var isFrameworkNone = inputAssemblies.First().Framework == Framework.FrameworkNone;

            var rootAssemblies = inputAssemblies.Where(a => a.Path.HasExtension("exe")).Concat(new[]{inputAssemblies.First()}).Distinct();
            
            var linkerArguments = new List<string>
            {
                $"--out={outputDir.InQuotes()}",
                "--use-dots-options",
                "--dotnetprofile=" + (isFrameworkNone ? "unitytiny" : "unityaot"),
                "--rule-set=experimental", // This will enable modification of method bodies to further reduce size.
                inputAssemblies.Select(a=>$"--include-directory={a.Path.Parent.InQuotes()}")
            };

            linkerArguments.AddRange(rootAssemblies.Select(rootAssembly => $"--include-public-assembly={rootAssembly.Path.InQuotes()}"));
            
//            foreach (var inputDirectory in inputFiles.Select(f => f.Path.Parent).Distinct())
//                linkerArguments.Add($"--include-directory={inputDirectory.InQuotes()}");

            NPath bclDir = Il2CppDependencies.Path.Combine("MonoBleedingEdge/builds/monodistribution/lib/mono/unityaot");

            if (!isFrameworkNone)
                linkerArguments.Add($"--search-directory={bclDir.InQuotes()}");

            var targetPlatform = GetTargetPlatformForLinker(nativeProgramConfiguration.Platform);
            if (!string.IsNullOrEmpty(targetPlatform))
                linkerArguments.Add($"--platform={targetPlatform}");

            var targetArchitecture = GetTargetArchitectureForLinker(nativeProgramConfiguration.ToolChain.Architecture);
            if (!string.IsNullOrEmpty(targetPlatform))
                linkerArguments.Add($"--architecture={targetArchitecture}");

  //          var targetFiles = Unity.BuildTools.EnumerableExtensions.Prepend(nonMainOutputs, mainTargetFile);
  //          targetFiles = targetFiles.Append(bcl);
              var targetFiles = inputAssemblies.SelectMany(a=>a.Paths).Select(i => targetDirectory.Combine(i.FileName)).ToArray();
            
              Backend.Current.AddAction(
                "UnityLinker",
                targetFiles: targetFiles,
                inputs: inputAssemblies.SelectMany(a=>a.Paths).Concat(linkerAssembly.Paths).ToArray(),
                executableStringFor: linker.InvocationString,
                commandLineArguments: linkerArguments.ToArray(),
                allowUnwrittenOutputFiles: false,
                allowUnexpectedOutput: false,
                allowedOutputSubstrings: new[] {"Output action"});
        }
    }

    static string GetTargetPlatformForLinker(Platform platform)
    {
    // Desktop platforms
    if (platform is WindowsPlatform)
        return "WindowsDesktop";
    if (platform is MacOSXPlatform)
        return "MacOSX";
    if (platform is LinuxPlatform)
        return "Linux";
    if (platform is UniversalWindowsPlatform)
        return "WinRT";

    // mobile
    if (platform is AndroidPlatform)
        return "Android";
    if (platform is IosPlatform)
        return "iOS";

    // consoles
    if (platform is XboxOnePlatform)
        return "XboxOne";
    if (platform is PS4Platform)
        return "PS4";
    if (platform is SwitchPlatform)
        return "Switch";

    // other
    if (platform is WebGLPlatform)
        return "WebGL";
    if (platform is LuminPlatform)
        return "Lumin";

    return null;
}

static string GetTargetArchitectureForLinker(Architecture arch)
{
    if (arch is x64Architecture)
        return "x64";
    if (arch is x86Architecture)
        return "x86";
    if (arch is ARMv7Architecture)
        return "ARMv7";
    if (arch is Arm64Architecture)
        return "ARM64";
    if (arch is EmscriptenArchitecture)
        return "EmscriptenJavaScript";

    return null;
}

private static DotNetAssembly Clone(NPath outputDir, DotNetAssembly a)
{
    var debugSymbolPath = a.DebugSymbolPath == null ? null : outputDir.Combine(a.DebugSymbolPath.FileName);
    return new DotNetAssembly(outputDir.Combine(a.Path.FileName), a.Framework,a.DebugFormat, debugSymbolPath);
}

public static NPath SetupBurst(NPath burstPackage, DotNetAssembly inputAssembly, NPath responseFile, ToolChain toolChain)
{
    var bcl = new DotNetRunnableProgram(new DotNetAssembly(burstPackage.Combine(".Runtime/bcl.exe"), Framework.Framework471));

    var targetFile = inputAssembly.Path.Parent.Combine($"burst_output.{toolChain.CppCompiler.ObjectExtension}");
    var inputs = Unity.BuildTools.EnumerableExtensions.Append(inputAssembly.RecursiveRuntimeDependenciesIncludingSelf.SelectMany(a => a.Paths), responseFile);

    Backend.Current.AddAction(
        "Burst",
        targetFiles: new[] {targetFile},
        inputs: inputs.ToArray(),
        executableStringFor: bcl.InvocationString,
        commandLineArguments: new[] {$"--assembly-folder={inputAssembly.Path.Parent}", $"--output={targetFile}", "--keep-intermediate-files", $"@{responseFile.ToString(SlashMode.Native)}"},
        allowUnexpectedOutput: false,
        allowedOutputSubstrings: new[] {"Link succesful", "Method:"});
    return targetFile;
}
}
