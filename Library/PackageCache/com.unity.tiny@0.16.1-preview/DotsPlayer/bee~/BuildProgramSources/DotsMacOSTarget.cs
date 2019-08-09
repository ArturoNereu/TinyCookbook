using DotsBuildTargets;
using Unity.BuildSystem.MacSDKSupport;
using Unity.BuildSystem.NativeProgramSupport;

abstract class DotsMacOSTarget : DotsBuildSystemTarget
{
    protected override ToolChain ToolChain => new MacToolchain(MacSdk.Locatorx64.UserDefaultOrDummy);
}

class DotsMacOSDotNetTarget : DotsMacOSTarget
{
    protected override string Identifier => "macos-dotnet";

    protected override ScriptingBackend ScriptingBackend => ScriptingBackend.Dotnet;
}

class DotsMacOSIL2CPPTarget : DotsMacOSTarget
{
    protected override string Identifier => "macos-il2cpp";
}
