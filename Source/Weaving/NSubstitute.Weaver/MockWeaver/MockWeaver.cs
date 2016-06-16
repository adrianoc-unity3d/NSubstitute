using System;
using System.IO;
using Mono.Cecil;
using Unity.Cecil.Visitor;

namespace NSubstitute.Weaving
{
    public static class MockWeaver
    {
        public static void InjectFakes(string assemblyToPatch, string registryAssemblyPath)
        {
            using (var assembly = File.OpenRead(assemblyToPatch))
            {
                var targetPath = Path.Combine(Path.GetDirectoryName(assemblyToPatch), "Patched");
                if (!Directory.Exists(targetPath))
                    Directory.CreateDirectory(targetPath);

                var target = Path.Combine(targetPath, Path.GetFileName(assemblyToPatch));
                InjectFakes(assembly, target, registryAssemblyPath, Path.GetDirectoryName(assemblyToPatch));
            }
        }

        public static void InjectFakes(Stream intoAssembly, string targetAssemblyPath, string mockRegistryAssemblyPath, string assemblySeachPath = null)
        {
			var readerParams = new ReaderParameters();
	        if (assemblySeachPath != null)
	        {
		        var resolver = new DefaultAssemblyResolver();
				resolver.AddSearchDirectory(assemblySeachPath);
		        readerParams.AssemblyResolver = resolver;
	        }
	        var assembly = AssemblyDefinition.ReadAssembly(intoAssembly, readerParams);

            assembly.Accept(new MockInjectorVisitor(AssemblyDefinition.ReadAssembly(mockRegistryAssemblyPath), assembly.MainModule));

            assembly.Write(targetAssemblyPath, new WriterParameters { WriteSymbols = true});
        }
    }
}
