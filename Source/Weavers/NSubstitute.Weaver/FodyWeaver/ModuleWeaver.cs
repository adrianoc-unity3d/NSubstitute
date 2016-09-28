using System;
using System.IO;
using Mono.Cecil;
using Unity.Cecil.Visitor;
using TypeAttributes = Mono.Cecil.TypeAttributes;

namespace NSubstitute.Weavers.Fody
{
    public class ModuleWeaver
    {
        // see https://github.com/Fody/Fody/wiki/ModuleWeaver for all requirements and magic properties on this class

        // we're given the module to weave and the xml element for the FodyWeavers.xml spec
        public ModuleDefinition ModuleDefinition { get; set; }

        // will contain the full directory path of the current weaver
        public string AddinDirectoryPath { get; set; }

        // filled by fody with delegates that will log to msbuild
        public Action<string> LogDebug { get; set; }
        public Action<string> LogInfo { get; set; }
        public Action<string> LogWarning { get; set; }
        public Action<string> LogError { get; set; }

        // may be filled by an NSubstitute wrapper task, if any (otherwise we autodetect location)
        public string NSubstituteAssemblyPath { get; set; }

        // called via fody during msbuild
        public void Execute()
        {
            var nsubstituteAssemblyPath = NSubstituteAssemblyPath ?? Path.Combine(AddinDirectoryPath, "NSubstitute.dll");

            if (!File.Exists(nsubstituteAssemblyPath))
            {
                throw new FileNotFoundException($"Unable to weave without a valid NSubstitute assembly (given {nsubstituteAssemblyPath ?? "<none>"})");
            }

            LogDebug("Using NSubstitute assembly: " + nsubstituteAssemblyPath);

            ModuleDefinition.Accept(new MockInjectorVisitor(AssemblyDefinition.ReadAssembly(nsubstituteAssemblyPath), ModuleDefinition));

            // this is copied from a sample, but let's leave it in here to check basic fody injection mechanics are set up right (there's a test for it elsewhere)
            var typeDefinition = new TypeDefinition("NSubstitute.Weavers.Tests", "InjectedTypeForTest", TypeAttributes.NotPublic, ModuleDefinition.Import(typeof(object)));
            ModuleDefinition.Types.Add(typeDefinition);
        }
    }
}
