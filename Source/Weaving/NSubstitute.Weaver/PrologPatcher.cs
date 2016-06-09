using System;
using System.IO;
using Mono.Cecil;
using Unity.Cecil.Visitor;

namespace NSubstitute.Weaving
{
    public static class PrologPatcher
    {
        public static void InjectFakes(string assemblyToPatch, string registryAssemblyPath)
        {
            using (var assembly = File.OpenRead(assemblyToPatch))
            {
                var targetPath = Path.Combine(Path.GetDirectoryName(assemblyToPatch), "Patched");
                if (!Directory.Exists(targetPath))
                    Directory.CreateDirectory(targetPath);

                var target = Path.Combine(targetPath, Path.GetFileName(assemblyToPatch));
                InjectFakes(assembly, target, registryAssemblyPath);
            }
        }

        public static void InjectFakes(Stream intoAssembly, string targetAssemblyPath, string mockRegistryAssemblyPath)
        {
            var assembly = AssemblyDefinition.ReadAssembly(intoAssembly);

            assembly.Accept(new PrologInjectorVisitor(AssemblyDefinition.ReadAssembly(mockRegistryAssemblyPath), assembly.MainModule));

            assembly.Write(targetAssemblyPath, new WriterParameters { WriteSymbols = true});
        }
    }
}
