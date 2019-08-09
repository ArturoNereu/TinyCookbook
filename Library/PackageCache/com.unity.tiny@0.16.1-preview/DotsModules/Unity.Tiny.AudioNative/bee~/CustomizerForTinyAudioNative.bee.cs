using JetBrains.Annotations;
using Unity.BuildSystem.NativeProgramSupport;

[UsedImplicitly]
class CustomizerForTinyAudioNative : DotsRuntimeCSharpProgramCustomizer
{
    public override void Customize(DotsRuntimeCSharpProgram program)
    {
        if (program.SourcePath.FileName == "Unity.Tiny.AudioNative")
        { 
            program.NativeProgram.Libraries.Add(c=>c.Platform is LinuxPlatform, new SystemLibrary("dl"), new SystemLibrary("rt"));
        }
    }
}