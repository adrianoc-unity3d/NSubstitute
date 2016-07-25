using System;
using Mono.Cecil;
using NSubstitute.Weavers;

// this project exists just to make fody happy for in-solution weaving. note that this project
// outputs to solution root "Weavers" folder for the binaries so fody picks it up in order to
// patch the test project.
//
// https://github.com/Fody/Fody/wiki/InSolutionWeaving

public class ModuleWeaver
{
    // see https://github.com/Fody/Fody/wiki/ModuleWeaver for all requirements and magic properties on this class

    // we're given the module to weave
    public ModuleDefinition ModuleDefinition { get; set; }

    // filled by fody with delegates that will log to msbuild
    public Action<string> LogDebug { get; set; }
    public Action<string> LogInfo { get; set; }
    public Action<string> LogWarning { get; set; }
    public Action<string> LogError { get; set; }

    // called via fody during msbuild
    public void Execute()
    {
        LogWarning(ModuleDefinition.Assembly.FullName);

        MockWeaver.InjectFakes(ModuleDefinition);
    }
}
