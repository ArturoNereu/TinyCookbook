using Bee.Toolchain.Windows;
using DotsBuildTargets;
using Unity.BuildSystem.NativeProgramSupport;

abstract class DotsWindowsTarget : DotsBuildSystemTarget
{
    protected override ToolChain ToolChain => new WindowsToolchain(WindowsSdk.Locatorx86.UserDefaultOrDummy);
}

class DotsWindowsDotNetTarget : DotsWindowsTarget
{
    protected override string Identifier => "windows-dotnet";

    protected override ScriptingBackend ScriptingBackend => ScriptingBackend.Dotnet;
}

class DotsWindowsIL2CPPTarget : DotsWindowsTarget
{
    protected override string Identifier => "windows-il2cpp";
}
