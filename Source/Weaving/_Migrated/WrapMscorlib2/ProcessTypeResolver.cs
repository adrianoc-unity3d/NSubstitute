using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace WrapMscorlib2
{
    public class ProcessTypeResolver
    {
        private readonly AssemblyDefinition _assembly;

        public ProcessTypeResolver(AssemblyDefinition assembly)
        {
            _assembly = assembly;
        }

        int InheritanceChainLength(TypeReference type)
        {
            var baseType = type.Resolve().BaseType;
            if (baseType == null)
                return 1;
            return 1 + InheritanceChainLength(baseType);
        }

        public IEnumerable<TypeDefinition> Resolve(IEnumerable<string> typesToCopy)
        {
            var toCopy = new HashSet<string>(typesToCopy);

            var types = new List<TypeDefinition>(_assembly.MainModule.Types.Where(t => toCopy.Contains(t.FullName)));
            types.Sort(((lhs, rhs) =>
            {
                var lhsChain = InheritanceChainLength(lhs);
                var rhsChain = InheritanceChainLength(rhs);
                return lhsChain.CompareTo(rhsChain);
            }));
            return types;
        }
    }
}
