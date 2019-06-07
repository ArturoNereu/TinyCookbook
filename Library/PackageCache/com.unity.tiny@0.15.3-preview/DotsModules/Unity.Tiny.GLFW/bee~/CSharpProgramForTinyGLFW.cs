using JetBrains.Annotations;
using Unity.BuildSystem.NativeProgramSupport;

[UsedImplicitly]
class CustomizerForTinyGLFW : DotsRuntimeCSharpProgramCustomizer
{
    public override void Customize(DotsRuntimeCSharpProgram program)
    {
        if (program.SourcePath.FileName == "Unity.Tiny.GLFW")
        {
            External.GLFWStaticLibrary = External.SetupGLFW();
            program.NativeProgram.Defines.Add(c => c.Platform is WindowsPlatform, "GLEW_BUILD");
            program.NativeProgram.Libraries.Add(new NativeProgramAsLibrary(External.GLFWStaticLibrary){BuildMode = NativeProgramLibraryBuildMode.BagOfObjects});
        }
    }
}