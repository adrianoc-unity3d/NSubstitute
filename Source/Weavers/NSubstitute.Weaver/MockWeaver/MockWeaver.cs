using System;
using System.IO;
using Mono.Cecil;
using Unity.Cecil.Visitor;

namespace NSubstitute.Weavers
{
    public static class MockWeaver
    {
        public static void InjectFakes(string assemblyToPatchPath, string nsubstituteAssemblyPath)
        {
            using (var sourceAssemblyFile = File.OpenRead(assemblyToPatchPath))
            {
                var outputAssemblyPath = Path.Combine(Path.Combine(Path.GetDirectoryName(assemblyToPatchPath), "Patched"), Path.GetFileName(assemblyToPatchPath));
                Directory.CreateDirectory(Path.GetDirectoryName(outputAssemblyPath));

                InjectFakes(sourceAssemblyFile, outputAssemblyPath, nsubstituteAssemblyPath, Path.GetDirectoryName(assemblyToPatchPath));
            }
        }

        public static void InjectFakes(Stream assemblyToPatchFile, string outputAssemblyPath, string nsubstituteAssemblyPath, string assemblySearchPath = null)
        {
            var readerParams = new ReaderParameters();
            if (assemblySearchPath != null)
            {
                var resolver = new DefaultAssemblyResolver();
                resolver.AddSearchDirectory(assemblySearchPath);
                readerParams.AssemblyResolver = resolver;
            }

            var assemblyToPatch = AssemblyDefinition.ReadAssembly(assemblyToPatchFile, readerParams);
            assemblyToPatch.Accept(new MockInjectorVisitor(AssemblyDefinition.ReadAssembly(nsubstituteAssemblyPath), assemblyToPatch.MainModule));
            assemblyToPatch.Write(outputAssemblyPath, new WriterParameters { WriteSymbols = true});
        }

        internal static void InjectFakes(ModuleDefinition assemblyToPatch)
        {
            //assemblyToPatch.Accept(new MockInjectorVisitor(AssemblyDefinition.ReadAssembly(nsubstituteAssemblyPath), assemblyToPatch));

            // this is copied from a sample, but let's leave it in here to check basic fody injection mechanics are set up right (there's a test for it elsewhere)
            var typeDefinition = new TypeDefinition("NSubstitute.Weavers.Tests", "InjectedTypeForTest", TypeAttributes.NotPublic, assemblyToPatch.Import(typeof(object)));

            assemblyToPatch.Types.Add(typeDefinition);
        }
    }
}
