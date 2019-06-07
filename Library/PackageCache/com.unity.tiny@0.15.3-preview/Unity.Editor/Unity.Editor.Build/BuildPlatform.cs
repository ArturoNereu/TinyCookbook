using System.Diagnostics;
using System.IO;
using Unity.Editor.Extensions;
using Unity.Editor.Tools;

namespace Unity.Editor.Build
{
    public abstract class Platform
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

    public abstract class DesktopPlatform : Platform
    {
        protected string GetPlatformName()
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

    public class DesktopDotNetPlatform : DesktopPlatform
    {
        public override string GetDisplayName()
        {
            return GetPlatformName() + " DotNet";
        }

        public override string GetBeeTargetName()
        {
            return GetPlatformName() + "-DotNet";
        }
    }

    public class DesktopIL2CPPPlatform : DesktopPlatform
    {
        public override string GetDisplayName()
        {
            return GetPlatformName() + " IL2CPP";
        }

        public override string GetBeeTargetName()
        {
            return GetPlatformName() + "-IL2CPP";
        }
    }

    public abstract class WebPlatform : Platform
    {
        protected string GetPlatformName()
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

    public class AsmJSPlatform : WebPlatform
    {
        public override string GetDisplayName()
        {
            return GetPlatformName() + " (AsmJS)";
        }

        public override string GetBeeTargetName()
        {
            return "asmjs";
        }
    }

    public class WasmPlatform : WebPlatform
    {
        public override string GetDisplayName()
        {
            return GetPlatformName() + " (Wasm)";
        }

        public override string GetBeeTargetName()
        {
            return "wasm";
        }
    }
}
