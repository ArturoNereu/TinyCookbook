using JetBrains.Annotations;
using Unity.BuildSystem.NativeProgramSupport;

[UsedImplicitly]
class CustomizerForZeroJobs : DotsRuntimeCSharpProgramCustomizer
{
    public override void Customize(DotsRuntimeCSharpProgram program)
    {
        if (program.SourcePath.FileName == "ZeroJobs")
        { 
            program.NativeProgram.Libraries.Add(c=>c.Platform is LinuxPlatform, new SystemLibrary("rt"));
        }
    }
}