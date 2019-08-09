using System;
using Bee.Core;
using Bee.Stevedore;
using Bee.Toolchain.VisualStudio;
using JetBrains.Annotations;
using Unity.BuildSystem.NativeProgramSupport;

[UsedImplicitly]
class CustomizerForZeroJobs : DotsRuntimeCSharpProgramCustomizer
{
    public override void Customize(DotsRuntimeCSharpProgram program)
    {
        if (program.SourcePath.FileName == "Unity.ZeroJobs")
        {
            program.NativeProgram.Libraries.Add(c => c.Platform is LinuxPlatform, new SystemLibrary("rt"));
            program.SupportFiles.Add(
                c => ((DotsRuntimeCSharpProgramConfiguration) c).MultiThreadedJobs,
                c => new[] { NativeJobsPrebuiltLibrary.For((DotsRuntimeCSharpProgramConfiguration)c) });
        }
    }

    static class NativeJobsPrebuiltLibrary
    {
        public static PrecompiledLibrary For(DotsRuntimeCSharpProgramConfiguration config)
        {
            switch (config.Platform)
            {
                case WindowsPlatform _:
                    var arch = config.Architecture.DisplayName;
                    var winArtifacts = new StevedoreArtifact("nativejobs-win");
                    Backend.Current.Register(winArtifacts);
                    return new MsvcDynamicLibrary($@"{winArtifacts.Path}\lib\windows\{arch}\release\nativejobs.dll");
                case MacOSXPlatform _:
                    var osxArtifacts = new StevedoreArtifact("nativejobs-osx");
                    Backend.Current.Register(osxArtifacts);
                    return new DynamicLibrary($@"{osxArtifacts.Path}\lib\osx\x86_64\release\libnativejobs.dylib");
                default:
                    throw new NotSupportedException($"{nameof(DotsRuntimeCSharpProgramConfiguration.MultiThreadedJobs)} is enabled for platform {config.Platform}, but there is no NativeJobs prebuilt library for that");
            }
        }
    }


}


