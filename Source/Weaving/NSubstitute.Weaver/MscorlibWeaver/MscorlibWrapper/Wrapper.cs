using Mono.Cecil;

namespace NSubstitute.Weaving
{
    public static class Wrapper
    {
        static string[] s_TypesToCopy = { "System.Text.StringBuilder", "System.DateTime", "System.IO.File", "System.IO.Path", "System.Console", "System.Threading.Thread" };

        public static AssemblyDefinition Wrap(string mscorlibPath, string nsubstitutePath)
        {
            var mscorlib = AssemblyDefinition.ReadAssembly(mscorlibPath);
            var fakelib =
                AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition("mscorlib.fake", mscorlib.Name.Version),
                    mscorlib.MainModule.Name, mscorlib.MainModule.Kind);
            var nsubstitute = AssemblyDefinition.ReadAssembly(nsubstitutePath);

            Copier.Copy(mscorlib, fakelib, nsubstitute, s_TypesToCopy);
            return fakelib;
        }
    }
}
