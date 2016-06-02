using System;
using Mono.Cecil;

namespace CallSitePatcher.Tests
{
    internal class ResolverTestBase
    {
        protected static AssemblyDefinition CreateAssembly(string name)
        {
            return AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition(name, new Version(2, 3)),
                "<Module>", ModuleKind.Dll);
        }

        protected static TypeDefinition CreateType(AssemblyDefinition assembly, string @namespace, string name)
        {
            var t = new TypeDefinition(@namespace, name, TypeAttributes.Public | TypeAttributes.Class);
            assembly.MainModule.Types.Add(t);
            return t;
        }

        protected static AssemblyDefinition CreateFakeAssembly()
        {
            var assembly = CreateAssembly("mscorlib.fake");
            CreateType(assembly, "Fake.System", "DateTime");
            return assembly;
        }
    }
}