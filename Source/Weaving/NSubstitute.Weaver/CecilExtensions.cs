using System.Linq;
using Mono.Cecil;

namespace NSubstitute.Weaving
{
    public static class CecilExtensions
    {
        public static bool IsPublic(this PropertyDefinition property)
        {
            if (property.GetMethod != null && property.GetMethod.IsPublic)
                return true;
            if (property.SetMethod != null && property.SetMethod.IsPublic)
                return true;
            if (property.OtherMethods?.Any(m => m.IsPublic) ?? false)
                return true;

            return false;
        }
    }
}
