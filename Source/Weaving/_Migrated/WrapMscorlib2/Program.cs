using System;
using Mono.Cecil;

namespace WrapMscorlib2
{
    class Program
    {
        static int Main(string[] args)
        {
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

            return 0;
        }

    }
}
