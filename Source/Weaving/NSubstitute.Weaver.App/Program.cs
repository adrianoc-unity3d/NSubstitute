using System;
using NSubstitute.Weaving;

namespace NSubstitute.Weaver.App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            PrologPatcher.InjectFakes(args[0], args[1]);
        }
    }
}
