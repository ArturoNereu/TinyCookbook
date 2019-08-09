using System.Linq;
using Bee.DotNet;
using NiceIO;
using Unity.BuildSystem.CSharpSupport;

public class AsmDefBasedDotsRuntimeCSharpProgram : DotsRuntimeCSharpProgram
{
    public DotsRuntimeCSharpProgram[] ReferencedPrograms { get; }
    public AsmDefDescription AsmDefDescription { get; }

    public AsmDefBasedDotsRuntimeCSharpProgram(AsmDefDescription asmDefDescription)
        : base(asmDefDescription.Directory,
            deferConstruction:true
            )
    {
        AsmDefDescription = asmDefDescription;
        ReferencedPrograms = AsmDefDescription.References.Select(BuildProgram.GetOrMakeDotsRuntimeCSharpProgramFor).ToArray();

        var referencesEntryPoint = ReferencedPrograms.Any(r => r.FileName.EndsWith(".exe"));

        var isExe = asmDefDescription.DefineConstraints.Contains("UNITY_DOTS_ENTRYPOINT")
                    || (asmDefDescription.Path.Parent.Files("*.project").Any() && !referencesEntryPoint)
                    || asmDefDescription.OptionalUnityReferences.Contains("TestAssemblies");
        Construct(asmDefDescription.Name, isExe);

        ProjectFile.AdditionalFiles.Add(asmDefDescription.Path);

        IncludePlatforms = AsmDefDescription.IncludePlatforms;
        ExcludePlatforms = AsmDefDescription.ExcludePlatforms;
        Unsafe = AsmDefDescription.Unsafe;
        References.Add(config =>
        {
            if (config is DotsRuntimeCSharpProgramConfiguration dotsConfig)
                return ReferencedPrograms.Where(rp =>
                    rp.IsSupportedOn(dotsConfig.NativeProgramConfiguration.ToolChain.Platform));

            //this codepath will be hit for the bindgem invocation
            return ReferencedPrograms;
        });

        if (BuildProgram.ZeroJobs != null)
            References.Add(BuildProgram.ZeroJobs);
        if (BuildProgram.UnityLowLevel != null)
            References.Add(BuildProgram.UnityLowLevel);

        if (IsTestAssembly)
        {
            References.Add(BuildProgram.NUnitFramework);
            var nunitLiteMain = BuildProgram.BeeRoot.Combine("CSharpSupport/NUnitLiteMain.cs");
            Sources.Add(nunitLiteMain);
            ProjectFile.AddCustomLinkRoot(nunitLiteMain.Parent, "TestRunner");
            References.Add(BuildProgram.NUnitLite);
            References.Add(BuildProgram.GetOrMakeDotsRuntimeCSharpProgramFor(BuildProgramConfigFile.AsmDefDescriptionFor("Unity.Entities")));
        }

        BindGem.ConfigureNativeProgramFor(this);

    }

    protected override NPath DeterminePathForProjectFile() =>
        DoesPackageSourceIndicateUserHasControlOverSource(AsmDefDescription.PackageSource) 
            ? AsmDefDescription.Path.Parent.Combine(AsmDefDescription.Name + ".gen.csproj") 
            : base.DeterminePathForProjectFile();

    public bool IsTestAssembly => AsmDefDescription.OptionalUnityReferences.Contains("TestAssemblies");

    protected override DotNetAssembly SetupNativeProgram(CSharpProgramConfiguration config, DotNetAssembly result)
    {
        BindGem.Instance().SetupInvocation(result, (DotsRuntimeCSharpProgramConfiguration)config);
        return base.SetupNativeProgram(config, result);
    }
}