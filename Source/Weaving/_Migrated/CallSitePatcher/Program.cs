using CallSitePatcher.Library;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace CallSitePatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            var patcher = new Patcher();
            var fakeAssembly = AssemblyDefinition.ReadAssembly(args[0]);
            var assembly = AssemblyDefinition.ReadAssembly(args[1]);

            patcher.Patch(fakeAssembly, assembly, args[2]);
        }
    }
}
