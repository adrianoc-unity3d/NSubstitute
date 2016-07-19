using System;
using NSubstitute.Weavers;

namespace NSubstitute.Weaver.App
{
    public class Program
    {
        public static int Main(string[] args)
        {
            MockWeaver.InjectFakes(args[0], args[1]);

#if NO
            // TODO: do the usual command line parsing. also we need to merge the prolog and call site patchers.

            // call site patcher

            var patcher = new CallSiteWeaver();
            var fakeAssembly = AssemblyDefinition.ReadAssembly(args[0]);
            var assembly = AssemblyDefinition.ReadAssembly(args[1]);

            patcher.Weave(fakeAssembly, assembly, args[2]);

            // mscorlib wrapper

            if (args.Length != 2)
            {
                Console.Error.WriteLine("Usage: {0} <mscorlib path> <nsubstitute path> <target path>");
                return 1;
            }

            var mscorlibPath = args[0];
            var nsubstitutePath = args[1];
            var targetPath = args[2];

            var assembly = Wrapper.Wrap(mscorlibPath, nsubstitutePath);

            assembly.Write(targetPath);
#endif

            return 0;
        }
    }
}
