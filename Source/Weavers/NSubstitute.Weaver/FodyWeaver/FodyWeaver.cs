using System;
using Mono.Cecil;
using Unity.Cecil.Visitor;

namespace NSubstitute.Weavers
{
    public class FodyWeaver
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
            const string nsubstituteAssemblyPath = @"C:\Proj\_external\NSubstitute\Output\Debug\NET35\NSubstitute\NSubstitute.dll";

            ModuleDefinition.Accept(new MockInjectorVisitor(AssemblyDefinition.ReadAssembly(nsubstituteAssemblyPath), ModuleDefinition));

            // this is copied from a sample, but let's leave it in here to check basic fody injection mechanics are set up right (there's a test for it elsewhere)
            var typeDefinition = new TypeDefinition("NSubstitute.Weavers.Tests", "InjectedTypeForTest", TypeAttributes.NotPublic, ModuleDefinition.Import(typeof(object)));
            ModuleDefinition.Types.Add(typeDefinition);
        }
    }
}
