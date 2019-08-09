using System;
using Unity.BuildSystem.NativeProgramSupport;

class CustomizerForTinyHtml : DotsRuntimeCSharpProgramCustomizer
{
    public override void Customize(DotsRuntimeCSharpProgram program)
    {
        if (program.FileName == "Unity.Tiny.HTML.dll")
        {
            Il2Cpp.AddLibIl2CppAsLibraryFor(program.NativeProgram);
        }
    }
}