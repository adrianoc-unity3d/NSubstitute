using Mono.Cecil;

// this project exists just to make fody happy for in-solution weaving. note that this project
// outputs to solution root "Weavers" folder for the binaries so fody picks it up in order to
// patch the test project.
//
// https://github.com/Fody/Fody/wiki/InSolutionWeaving

public class ModuleWeaver
{
    public ModuleDefinition ModuleDefinition { get; set; }

    public void Execute()
    {
        var typeDefinition = new TypeDefinition(
            GetType().Assembly.GetName().Name,
            "TypeInjectedBy" + GetType().Name, TypeAttributes.Public, ModuleDefinition.Import(typeof(object)));
        ModuleDefinition.Types.Add(typeDefinition);
    }
}
