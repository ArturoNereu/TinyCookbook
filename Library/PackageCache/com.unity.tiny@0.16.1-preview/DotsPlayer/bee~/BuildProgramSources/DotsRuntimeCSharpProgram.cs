using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using Bee;
using Bee.Core;
using Bee.CSharpSupport;
using Bee.DotNet;
using Bee.NativeProgramSupport.Building;
using Bee.Toolchain.Emscripten;
using Bee.Toolchain.Xcode;
using Bee.VisualStudioSolution;
using NiceIO;
using Unity.BuildSystem.CSharpSupport;
using Unity.BuildSystem.NativeProgramSupport;
using Unity.BuildTools;

/// <summary>
/// DotsRuntimeCSharpProgram is a csharp program that targets dots-runtime. It follows a particular file structure. It always has a folder
/// that folder can have *.cs files, which will be part of the csharp program. The folder can also have a .cpp~ and .js~ folder.  If any
/// of those are present, DotsRuntimeCSharpProgram will build a NativeProgram with those .cpp files and .js libraries side by side. The common
/// usecase for this is for the c# code to [DllImport] pinvoke into the c++ code.
///
/// A DotsRuntimeCSharpProgram does not know about asmdefs (e.g. Unity.LowLevel)
/// </summary>
public class DotsRuntimeCSharpProgram : CSharpProgram
{
    private bool _doneEnsureNativeProgramLinksToReferences;
    public NPath SourcePath { get; }
    public NativeProgram NativeProgram { get; set; }
    public Platform[] IncludePlatforms { get; set; }
    public Platform[] ExcludePlatforms { get; set; }

    public bool IsSupportedOn(Platform platform)
    {
        if (IncludePlatforms.Any(p => p.GetType().IsInstanceOfType(platform)))
            return true;

        if (IncludePlatforms.Any())
            return false;
            
        if (!ExcludePlatforms.Any())
            return true;

        return !ExcludePlatforms.Any(p => p.GetType().IsInstanceOfType(platform));
    }

    public DotsRuntimeCSharpProgram(NPath sourcePath, string name = null, bool isExe = false, bool deferConstruction = false)
    {
        SourcePath = sourcePath;
        name = name ?? sourcePath.FileName;
        
        if (!deferConstruction)
            Construct(name, isExe);

        ProjectFile.ExplicitConfigurationsToUse = new CSharpProgramConfiguration[] {DotsConfigs.ProjectFileConfig};
        
        ProjectFile.IntermediateOutputPath.Set(config => Configuration.RootArtifactsPath.Combine(ArtifactsGroup ?? "Bee.CSharpSupport").Combine("MSBuildIntermediateOutputPath", config.Identifier));
    }

    protected void Construct(string name, bool isExe)
    {
        FileName = name + (isExe ? ".exe" : ".dll");

        Framework.Add(c=> ShouldTargetTinyCorlib(c, this),Bee.DotNet.Framework.FrameworkNone);
        References.Add(c=>ShouldTargetTinyCorlib(c, this),Il2Cpp.TinyCorlib);
        
        Framework.Add(c=>!ShouldTargetTinyCorlib(c, this),Bee.DotNet.Framework.Framework471);
        References.Add(c=>!ShouldTargetTinyCorlib(c, this), new SystemReference("System"));
        
        ProjectFile.Path = DeterminePathForProjectFile();

        ProjectFile.ReferenceModeCallback = arg =>
        {
            if (arg == Il2Cpp.TinyCorlib)
                return ProjectFile.ReferenceMode.ByCSProj;

            // Most projects are AsmDefBasedDotsRuntimeCSharpProgram. For everything else we'll look up their
            // packagestatus by the fact that we know it's in the same package as Unity.Entities.CPlusPlus
            var asmdefDotsProgram = (arg as AsmDefBasedDotsRuntimeCSharpProgram)?.AsmDefDescription ?? BuildProgramConfigFile.AsmDefDescriptionFor("Unity.Entities.CPlusPlus");

            if (DoesPackageSourceIndicateUserHasControlOverSource(asmdefDotsProgram.PackageSource))
                return ProjectFile.ReferenceMode.ByCSProj;
            else
                return ProjectFile.ReferenceMode.ByDotNetAssembly;
        };
        
        LanguageVersion = "7.3";
        Defines.Add(
            "UNITY_2018_3_OR_NEWER",
            "UNITY_DOTSPLAYER",
            "UNITY_ZEROPLAYER", //<-- this was used for a while, let's keep it around to not break people's incoming PR's.
            "NET_TINY",
            "NET_DOTS",
            "UNITY_USE_TINYMATH",
            "UNITY_BINDGEM",
            
            //today, in dots-runtime this is always the case. There will likely be situations going forward where
            //a user targets full dotnet, and they actually want to use our reflection based codepath, instead of the codegenerated one
            "UNITY_AVOID_REFLECTION"
        );
        
        Defines.Add(c => (c as DotsRuntimeCSharpProgramConfiguration)?.Platform is WebGLPlatform, "UNITY_WEBGL");
        Defines.Add(c =>(c as DotsRuntimeCSharpProgramConfiguration)?.Platform is WindowsPlatform, "UNITY_WINDOWS");
        Defines.Add(c =>(c as DotsRuntimeCSharpProgramConfiguration)?.Platform is MacOSXPlatform, "UNITY_MACOSX");
        Defines.Add(c => (c as DotsRuntimeCSharpProgramConfiguration)?.Platform is LinuxPlatform, "UNITY_LINUX");
        Defines.Add(c =>(c as DotsRuntimeCSharpProgramConfiguration)?.Platform is IosPlatform, "UNITY_IOS");
        Defines.Add(c => (c as DotsRuntimeCSharpProgramConfiguration)?.Platform is AndroidPlatform, "UNITY_ANDROID");
        Defines.Add(c => !((DotsRuntimeCSharpProgramConfiguration) c).MultiThreadedJobs, "UNITY_SINGLETHREADED_JOBS");
        
        CopyReferencesNextToTarget = false;

        WarningsAsErrors = false;
        //hack, fix this in unity.mathematics
        if (SourcePath.FileName == "Unity.Mathematics")
            Sources.Add(SourcePath.Files("*.cs",true).Where(f=>f.FileName != "math_unity_conversion.cs" && f.FileName != "PropertyAttributes.cs"));
        else
        {
            Sources.Add(new CustomProvideFiles(SourcePath));
        }

        var cppFolder = SourcePath.Combine("cpp~");
        var prejsFolder = SourcePath.Combine("prejs~");
        var jsFolder = SourcePath.Combine("js~"); 
        var postjsFolder = SourcePath.Combine("postjs~");
        var beeFolder = SourcePath.Combine("bee~");
        var includeFolder = cppFolder.Combine("include");

        NPath[] cppFiles = Array.Empty<NPath>();
        if (cppFolder.DirectoryExists())
        {
            cppFiles = cppFolder.Files("*.c*", true);
            ProjectFile.AdditionalFiles.AddRange(cppFolder.Files(true));
            GetOrMakeNativeProgram().Sources.Add(cppFiles);
        }

        if (prejsFolder.DirectoryExists())
        {
            var jsFiles = prejsFolder.Files("*.js", true);
            ProjectFile.AdditionalFiles.AddRange(prejsFolder.Files(true));
            GetOrMakeNativeProgram().Libraries.Add(jsFiles.Select(jsFile => new PreJsLibrary(jsFile)));
        }

        //todo: get rid of having both a regular js and a prejs folder
        if (jsFolder.DirectoryExists())
        {
            var jsFiles = jsFolder.Files("*.js", true);
            ProjectFile.AdditionalFiles.AddRange(jsFolder.Files(true));
            GetOrMakeNativeProgram().Libraries.Add(jsFiles.Select(jsFile => new JavascriptLibrary(jsFile)));
        }

        if (postjsFolder.DirectoryExists())
        {
            var jsFiles = postjsFolder.Files("*.js", true);
            ProjectFile.AdditionalFiles.AddRange(postjsFolder.Files(true));
            GetOrMakeNativeProgram().Libraries.Add(jsFiles.Select(jsFile => new PostJsLibrary(jsFile)));
        }
        
        if (beeFolder.DirectoryExists())
            ProjectFile.AdditionalFiles.AddRange(beeFolder.Files("*.cs"));
        
        if (includeFolder.DirectoryExists())
            GetOrMakeNativeProgram().PublicIncludeDirectories.Add(includeFolder);

        SupportFiles.Add(SourcePath.Files().Where(f=>f.HasExtension("jpg","png","wav","mp3","jpeg","mp4","webm","ogg", "ttf")));
        
        Defines.Add(c => c.CodeGen == CSharpCodeGen.Debug, "DEBUG");

        Defines.Add(c => ((DotsRuntimeCSharpProgramConfiguration) c).EnableUnityCollectionsChecks, "ENABLE_UNITY_COLLECTIONS_CHECKS");

        Defines.Add(
            c => (c as DotsRuntimeCSharpProgramConfiguration)?.ScriptingBackend == ScriptingBackend.TinyIl2cpp,
            "UNITY_DOTSPLAYER_IL2CPP");
        Defines.Add(c => (c as DotsRuntimeCSharpProgramConfiguration)?.ScriptingBackend == ScriptingBackend.Dotnet, "UNITY_DOTSPLAYER_DOTNET");

        ProjectFile.RedirectMSBuildBuildTargetToBee = true;
        ProjectFile.AddCustomLinkRoot(SourcePath, ".");
        ProjectFile.RootNameSpace = "";
        
        DotsRuntimeCSharpProgramCustomizer.RunAllCustomizersOn(this);
    }

    protected virtual NPath DeterminePathForProjectFile()
    {
        return new NPath(FileName).ChangeExtension(".csproj");
    }

    public static bool DoesPackageSourceIndicateUserHasControlOverSource(string packageSource)
    {
        switch (packageSource)
        {
            case "NoPackage":
            case "Local":
            case "Embedded":
                return true;
            default:
                return false;
        }
    }

    internal NativeProgram GetOrMakeNativeProgram()
    {
        if (NativeProgram != null)
            return NativeProgram;
        
        var libname = "lib_"+new NPath(FileName).FileNameWithoutExtension.ToLower().Replace(".","_");
        NativeProgram = new NativeProgram(libname);
        
        NativeProgram.DynamicLinkerSettingsForMac().Add(c => c.WithInstallName(libname + ".dylib"));
        NativeProgram.IncludeDirectories.Add(BuildProgram.BeeRootValue.Combine("cppsupport/include"));

        //lets always add a dummy cpp file, in case this nativeprogram is only used to carry other libraries
        NativeProgram.Sources.Add(BuildProgram.BeeRootValue.Combine("cppsupport/dummy.cpp"));

        NativeProgram.Defines.Add(c => c.Platform is WebGLPlatform, "UNITY_WEBGL=1");
        NativeProgram.Defines.Add(c => c.Platform is WindowsPlatform, "UNITY_WINDOWS=1");
        NativeProgram.Defines.Add(c => c.Platform is MacOSXPlatform, "UNITY_MACOSX=1");
        NativeProgram.Defines.Add(c => c.Platform is LinuxPlatform, "UNITY_LINUX=1");
        NativeProgram.Defines.Add(c => c.Platform is IosPlatform, "UNITY_IOS=1");
        NativeProgram.Defines.Add(c => c.Platform is AndroidPlatform, "UNITY_ANDROID=1");

        // sigh
        NativeProgram.Defines.Add("BUILD_" + SourcePath.FileName.ToUpper().Replace(".", "_") + "=1");

        NativeProgram.Defines.Add(c => c.CodeGen == CodeGen.Debug, "DEBUG=1");
        
        NativeProgram.Defines.Add("BINDGEM_DOTS=1");

        return NativeProgram;
    }

    private static bool ShouldTargetTinyCorlib(CSharpProgramConfiguration config, DotsRuntimeCSharpProgram program)
    {
        return !(program is AsmDefBasedDotsRuntimeCSharpProgram asmdefProgram) || !asmdefProgram.IsTestAssembly;
    }
    
    public override DotNetAssembly SetupSpecificConfiguration(CSharpProgramConfiguration config)
    {
        EnsureNativeProgramLinksToReferences();
        
        var result = base.SetupSpecificConfiguration(config);

        return SetupNativeProgram(config, result);
    }

    protected virtual DotNetAssembly SetupNativeProgram(CSharpProgramConfiguration config, DotNetAssembly result)
    {
        var dotsConfig = (DotsRuntimeCSharpProgramConfiguration) config;

        var npc = dotsConfig.NativeProgramConfiguration;
        if (NativeProgram != null && NativeProgram.Sources.ForAny().Any())
        {
            BuiltNativeProgram setupSpecificConfiguration = NativeProgram.SetupSpecificConfiguration(npc,
                npc.ToolChain.DynamicLibraryFormat ?? npc.ToolChain.StaticLibraryFormat);
            result = result.WithDeployables(setupSpecificConfiguration);
        }

        return result;
    }

    private void EnsureNativeProgramLinksToReferences()
    {
        //todo: find a more elegant way than this..
        if (_doneEnsureNativeProgramLinksToReferences)
            return;
        _doneEnsureNativeProgramLinksToReferences = true;
        
        NativeProgram?.Libraries.Add(npc =>
        {
            var dotsRuntimeCSharpPrograms = References.For(((DotsRuntimeNativeProgramConfiguration) npc).CSharpConfig)
                .OfType<DotsRuntimeCSharpProgram>().ToArray();
            return dotsRuntimeCSharpPrograms.Select(dcp => dcp.NativeProgram).Where(np => np != null)
                .Select(np => new NativeProgramAsLibrary(np) { BuildMode = NativeProgramLibraryBuildMode.Dynamic});
        });
    }
    
    
    class CustomProvideFiles : OneOrMoreFiles
    {
        public NPath SourcePath { get; }
        
        public CustomProvideFiles(NPath sourcePath) => SourcePath = sourcePath;

        public override IEnumerable<NPath> GetFiles()
        {
            var files = SourcePath.Files("*.cs",recurse:true);
            var beeDirs = SourcePath.Directories(true).Where(d => d.FileName == "bee~").ToList();
            var ignoreDirectories = files.Where(f => f.HasExtension("asmdef") && f.Parent != SourcePath).Select(asmdef => asmdef.Parent).Concat(beeDirs).ToList();
            return files.Where(f => f.HasExtension("cs") && !ignoreDirectories.Any(f.IsChildOf));
        }

        public override IEnumerable<XElement> CustomMSBuildElements(NPath projectFileParentPath)
        {
            if (SourcePath != projectFileParentPath && !SourcePath.IsChildOf(projectFileParentPath)) 
                return null;
            
            var relative = SourcePath.RelativeTo(projectFileParentPath).ToString(SlashMode.Native);

            var prefix = relative == "." ? "" : $"{relative}\\";
            var ns = ProjectFile.DefaultNamespace;
            return new[]
            {
                new XElement(ns + "Compile", new XAttribute("Include", $@"{prefix}**\*.cs"),
                    new XAttribute("Exclude", $"{prefix}bee?\\**\\*.*"))
            };

        }
    }
}

public enum ScriptingBackend
{
    TinyIl2cpp,
    Dotnet
}

public sealed class DotsRuntimeCSharpProgramConfiguration : CSharpProgramConfiguration
{
    public DotsRuntimeNativeProgramConfiguration NativeProgramConfiguration { get; }

    public ScriptingBackend ScriptingBackend { get; }

    public Platform Platform => NativeProgramConfiguration.ToolChain.Platform;
    
    public bool MultiThreadedJobs { get; private set; }

    private string _identifier { get; set; }
    
    public DotsRuntimeCSharpProgramConfiguration(CSharpCodeGen csharpCodegen, CodeGen cppCodegen,
        //The stevedore global manifest will override DownloadableCsc.Csc72 artifacts and use Csc73
        ToolChain nativeToolchain, ScriptingBackend scriptingBackend, string identifier, bool enableUnityCollectionsChecks, bool enableManagedDebugging, bool multiThreadedJobs, NativeProgramFormat executableFormat = null) : base(csharpCodegen, DownloadableCsc.Csc72, HostPlatform.IsWindows ? (DebugFormat)DebugFormat.Pdb : DebugFormat.PortablePdb,nativeToolchain.Architecture is x86Architecture ? nativeToolchain.Architecture : null)
    {
        NativeProgramConfiguration = new DotsRuntimeNativeProgramConfiguration(cppCodegen, nativeToolchain, identifier, this, executableFormat:executableFormat);
        _identifier = identifier;
        EnableUnityCollectionsChecks = enableUnityCollectionsChecks;
        MultiThreadedJobs = multiThreadedJobs;
        EnableManagedDebugging = enableManagedDebugging;
        ScriptingBackend = scriptingBackend;
    }


    public override string Identifier => _identifier;
    public bool EnableUnityCollectionsChecks { get; }
    public bool EnableManagedDebugging { get; }

    public DotsRuntimeCSharpProgramConfiguration WithMultiThreadedJobs(bool value) => MultiThreadedJobs == value ? this : With(c=>c.MultiThreadedJobs = value);
    public DotsRuntimeCSharpProgramConfiguration WithIdentifier(string value) => Identifier == value ? this : With(c=>c._identifier = value);

    private DotsRuntimeCSharpProgramConfiguration With(Action<DotsRuntimeCSharpProgramConfiguration> modifyCallback)
    {
        var copy = (DotsRuntimeCSharpProgramConfiguration) MemberwiseClone();
        modifyCallback(copy);
        return copy;
    }
}

public class DotsRuntimeNativeProgramConfiguration : NativeProgramConfiguration
{
    private NativeProgramFormat _executableFormat;
    public DotsRuntimeCSharpProgramConfiguration CSharpConfig { get; }
    public DotsRuntimeNativeProgramConfiguration(CodeGen codeGen, ToolChain toolChain, string identifier, DotsRuntimeCSharpProgramConfiguration cSharpConfig, NativeProgramFormat executableFormat = null) : base(codeGen, toolChain, false)
    {
        Identifier = identifier;
        CSharpConfig = cSharpConfig;
        _executableFormat = executableFormat;
    }

    public NativeProgramFormat ExecutableFormat => _executableFormat ?? base.ToolChain.ExecutableFormat;
    
    public override string Identifier { get; }
}