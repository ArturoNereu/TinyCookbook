using Bee.NativeProgramSupport.Building;
using System.Collections.Generic;
using Unity.BuildSystem.CSharpSupport;
using Unity.BuildSystem.NativeProgramSupport;

namespace DotsBuildTargets
{
    enum DotsConfiguration
    {
        Debug,
        Develop,
        Release,
    }

    abstract class DotsBuildSystemTarget
    {
        public virtual IEnumerable<DotsRuntimeCSharpProgramConfiguration> GetConfigs()
        {
            if (!ToolChain.CanBuild)
                yield break;

            yield return new DotsRuntimeCSharpProgramConfiguration(
                csharpCodegen: CSharpCodeGen.Debug,
                cppCodegen: CodeGen.Debug,
                nativeToolchain: ToolChain,
                scriptingBackend: ScriptingBackend,
                enableUnityCollectionsChecks: true,
                enableManagedDebugging: false,
                identifier: $"{Identifier}-debug",
                multiThreadedJobs: CanRunMultiThreadedJobs,
                executableFormat: GetExecutableFormatForConfig(DotsConfiguration.Debug));

            yield return new DotsRuntimeCSharpProgramConfiguration(
                csharpCodegen: CSharpCodeGen.Debug,
                cppCodegen: CodeGen.Release,
                nativeToolchain: ToolChain,
                scriptingBackend: ScriptingBackend,
                enableUnityCollectionsChecks: true,
                enableManagedDebugging: true,
                identifier: $"{Identifier}-mdb",
                multiThreadedJobs: CanRunMultiThreadedJobs,
                executableFormat: GetExecutableFormatForConfig(DotsConfiguration.Debug));

            yield return new DotsRuntimeCSharpProgramConfiguration(
                csharpCodegen: CSharpCodeGen.Release,
                cppCodegen: CodeGen.Release,
                nativeToolchain: ToolChain,
                scriptingBackend: ScriptingBackend,
                enableUnityCollectionsChecks: true,
                enableManagedDebugging: false,
                identifier: $"{Identifier}-develop",
                multiThreadedJobs: CanRunMultiThreadedJobs,
                executableFormat: GetExecutableFormatForConfig(DotsConfiguration.Develop));

            yield return new DotsRuntimeCSharpProgramConfiguration(
                csharpCodegen: CSharpCodeGen.Release,
                cppCodegen: CodeGen.Release,
                nativeToolchain: ToolChain,
                scriptingBackend: ScriptingBackend,
                enableUnityCollectionsChecks: false,
                enableManagedDebugging: false,
                identifier: $"{Identifier}-release",
                multiThreadedJobs: CanRunMultiThreadedJobs,
                executableFormat: GetExecutableFormatForConfig(DotsConfiguration.Release));
        }

        protected virtual bool CanRunMultiThreadedJobs => false; // Disabling by default; Eventually: ScriptingBackend == ScriptingBackend.Dotnet;

        protected abstract string Identifier { get; }
        protected abstract ToolChain ToolChain { get; }
        protected virtual ScriptingBackend ScriptingBackend => ScriptingBackend.TinyIl2cpp;
        protected virtual NativeProgramFormat GetExecutableFormatForConfig(DotsConfiguration config) => null;
    }
}