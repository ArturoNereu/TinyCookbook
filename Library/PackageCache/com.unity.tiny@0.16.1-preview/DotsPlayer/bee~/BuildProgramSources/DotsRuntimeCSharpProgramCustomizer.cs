using System;
using System.Linq;

internal abstract class DotsRuntimeCSharpProgramCustomizer
{
    private static DotsRuntimeCSharpProgramCustomizer[] All = MakeAllCustomizers();

    static DotsRuntimeCSharpProgramCustomizer[] MakeAllCustomizers()
    {
        return typeof(BuildProgram).Assembly
            .GetTypes()
            .Where(t => typeof(DotsRuntimeCSharpProgramCustomizer).IsAssignableFrom(t))
            .Where(t=>!t.IsAbstract)
            .OrderBy(t => t.Name)
            .Select(Activator.CreateInstance)
            .Cast<DotsRuntimeCSharpProgramCustomizer>()
            .ToArray();
    }

    public static void RunAllCustomizersOn(DotsRuntimeCSharpProgram program)
    {
        foreach(var customizer in All)
            customizer.Customize(program);
    }

    public abstract void Customize(DotsRuntimeCSharpProgram program);
}