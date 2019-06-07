using System;
using Unity.BuildSystem.NativeProgramSupport;

class CustomizerForTinyHtml : DotsRuntimeCSharpProgramCustomizer
{
    public override void Customize(DotsRuntimeCSharpProgram program)
    {
        if (program.FileName == "Unity.Tiny.HTML.dll")
        {
            program.NativeProgram.Libraries.Add(new NativeProgramAsLibrary(Il2Cpp.LibIL2Cpp));
        }
    }
}