using System.Diagnostics;
using System.IO;
using Unity.Editor.Extensions;
using Unity.Editor.Tools;

namespace Unity.Editor.Build
{
    public abstract class BuildTarget
    {
        public override string ToString()
        {
            return GetDisplayName();
        }

        public abstract string GetDisplayName();
        public abstract string GetUnityPlatformName();
        public abstract string GetExecutableExtension();
        public virtual string GetBeeTargetName() { return GetDisplayName(); }
        public abstract bool Run(FileInfo buildTarget);
    }

    public abstract class DesktopBuildTarget : BuildTarget
    {
        protected string GetBuildTargetName()
        {
#if UNITY_EDITOR_WIN
            return "Windows";
#elif UNITY_EDITOR_OSX
            return "macOS";
#else
            return "Linux";
#endif
        }

        public override string GetUnityPlatformName()
        {
#if UNITY_EDITOR_WIN
            return "WindowsStandalone64";
#elif UNITY_EDITOR_OSX
            return "macOSStandalone";
#else
            return "LinuxStandalone64";
#endif
        }

        public override string GetExecutableExtension()
        {
            return ".exe";
        }

        public override bool Run(FileInfo buildTarget)
        {
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = buildTarget.FullName;
#if !UNITY_EDITOR_WIN
            startInfo.Arguments = startInfo.FileName.DoubleQuoted();
            startInfo.FileName = Application.MonoDirectory.GetFile("mono").FullName;
#endif
            startInfo.WorkingDirectory = buildTarget.Directory.FullName;
            startInfo.CreateNoWindow = true;
            var process = Process.Start(startInfo);
            return process != null;
        }
    }

    public class DesktopDotNetBuildTarget : DesktopBuildTarget
    {
        public override string GetDisplayName()
        {
            return GetBuildTargetName() + " DotNet";
        }

        public override string GetBeeTargetName()
        {
            return GetBuildTargetName() + "-DotNet";
        }
    }

    public class DesktopIL2CPPBuildTarget : DesktopBuildTarget
    {
        public override string GetDisplayName()
        {
            return GetBuildTargetName() + " IL2CPP";
        }

        public override string GetBeeTargetName()
        {
            return GetBuildTargetName() + "-IL2CPP";
        }
    }

    public abstract class WebBuildTarget : BuildTarget
    {
        protected string GetBuildTargetName()
        {
            return "Web";
        }

        public override string GetUnityPlatformName()
        {
            return "WebGL";
        }

        public override string GetExecutableExtension()
        {
            return ".html";
        }

        public override bool Run(FileInfo buildTarget)
        {
            return HTTPServer.Instance.HostAndOpen(
                buildTarget.Directory.FullName,
                buildTarget.Name,
                19050);
        }
    }

    public class AsmJSBuildTarget : WebBuildTarget
    {
        public override string GetDisplayName()
        {
            return GetBuildTargetName() + " (AsmJS)";
        }

        public override string GetBeeTargetName()
        {
            return "asmjs";
        }
    }

    public class WasmBuildTarget : WebBuildTarget
    {
        public override string GetDisplayName()
        {
            return GetBuildTargetName() + " (Wasm)";
        }

        public override string GetBeeTargetName()
        {
            return "wasm";
        }
    }
}
