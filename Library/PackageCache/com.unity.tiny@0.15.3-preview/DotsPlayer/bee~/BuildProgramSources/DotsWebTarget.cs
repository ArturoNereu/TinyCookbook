using Bee.NativeProgramSupport.Building;
using Bee.Toolchain.Emscripten;
using System;
using DotsBuildTargets;
using Unity.BuildSystem.NativeProgramSupport;

abstract class DotsWebTarget : DotsBuildSystemTarget
{
	protected abstract bool UseWasm { get; }

	protected override NativeProgramFormat GetExecutableFormatForConfig(DotsConfiguration config)
	{
		var format = new EmscriptenExecutableFormat(ToolChain, "html");

		switch (config)
		{
			case DotsConfiguration.Debug:
				return format.WithLinkerSetting<EmscriptenDynamicLinker>(d => TinyEmscripten.ConfigureEmscriptenLinkerFor(d, "debug"));

			case DotsConfiguration.Develop:
				return format.WithLinkerSetting<EmscriptenDynamicLinker>(d => TinyEmscripten.ConfigureEmscriptenLinkerFor(d, "develop"));

			case DotsConfiguration.Release:
				return format.WithLinkerSetting<EmscriptenDynamicLinker>(d => TinyEmscripten.ConfigureEmscriptenLinkerFor(d, "release"));

			default:
				throw new NotImplementedException("Unknown config: " + config);
		}
	}
}

class DotsAsmJSTarget : DotsWebTarget
{
	protected override bool UseWasm => false;

	protected override string Identifier => "asmjs";

	protected override ToolChain ToolChain => TinyEmscripten.ToolChain_AsmJS;
}

class DotsWasmTarget : DotsWebTarget
{
	protected override bool UseWasm => true;

	protected override string Identifier => "wasm";

	protected override ToolChain ToolChain => TinyEmscripten.ToolChain_Wasm;
}
