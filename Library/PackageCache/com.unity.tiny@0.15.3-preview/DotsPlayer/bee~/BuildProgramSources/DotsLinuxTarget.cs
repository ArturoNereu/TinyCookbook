using Bee.Toolchain.Linux;
using DotsBuildTargets;
using Unity.BuildSystem.NativeProgramSupport;

abstract class DotsLinuxTarget : DotsBuildSystemTarget
{
    protected override ToolChain ToolChain => new LinuxGccToolchain(LinuxGccSdk.Locatorx64.UserDefaultOrDummy);
}

class DotsLinuxDotNetTarget : DotsLinuxTarget
{
    protected override string Identifier => "linux-dotnet";

    protected override ScriptingBackend ScriptingBackend => ScriptingBackend.Dotnet;
}

class DotsLinuxIL2CPPTarget : DotsLinuxTarget
{
    protected override string Identifier => "linux-il2cpp";
}
