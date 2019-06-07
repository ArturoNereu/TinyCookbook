using Bee.Toolchain.Xcode;
using JetBrains.Annotations;
using Unity.BuildSystem.NativeProgramSupport;
using static Unity.BuildSystem.NativeProgramSupport.NativeProgramConfiguration;


[UsedImplicitly]
class CustomizerForTinyImage2Diostb : DotsRuntimeCSharpProgramCustomizer
{
    public override void Customize(DotsRuntimeCSharpProgram program)
    {
        if (program.SourcePath.FileName == "Unity.Tiny.Image2DIOSTB")
        {
            program.NativeProgram.Libraries.Add(IsWindows, new SystemLibrary("opengl32.lib"));
            program.NativeProgram.Libraries.Add(c => c.Platform is MacOSXPlatform, new SystemFramework("OpenGL"));
            program.NativeProgram.Libraries.Add(IsLinux, new SystemLibrary("GL"));
        }
    }
}