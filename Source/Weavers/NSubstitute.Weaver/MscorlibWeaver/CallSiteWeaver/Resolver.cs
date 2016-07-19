using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace NSubstitute.Weavers
{
    class Resolver
    {
        readonly AssemblyDefinition m_Target;
        readonly AssemblyDefinition m_FakeAssembly;
        Dictionary<string, TypeDefinition> m_Map;

        public Resolver(AssemblyDefinition target, AssemblyDefinition fakeAssembly)
        {
            m_Target = target;
            m_FakeAssembly = fakeAssembly;
            m_Map = m_FakeAssembly.MainModule.Types.ToDictionary(t => t.FullName.Substring("Fake.".Length), t => t);
        }

        static MethodDefinition TryResolve(MethodReference reference)
        {
            try
            {
                return reference.Resolve();
            }
            catch (AssemblyResolutionException)
            {
                Console.Error.WriteLine($"Error resolving assembly for method {reference}");
                return null;
            }
        }

        public MethodReference Resolve(ModuleDefinition module, MethodReference reference, bool resolveBody = true)
        {
            if (reference.DeclaringType.Scope.Name == m_FakeAssembly.Name.Name)
                return reference;

            var def = TryResolve(reference);

            if (resolveBody)
            {
                if (def == null)
                    goto skipbody;

                if (!def.HasBody)
                    goto skipbody;

                foreach (var instruction in def.Body.Instructions)
                {
                    var methodReference = instruction.Operand as MethodReference;
                    if (methodReference != null && methodReference.Module == m_Target.MainModule)
                        if (methodReference != reference)
                            instruction.Operand = Resolve(module, methodReference, false);

                    var typeReference = instruction.Operand as TypeReference;
                    if (typeReference != null)
                        instruction.Operand = Resolve(module, typeReference);
                }

                foreach (var variable in def.Body.Variables)
                {
                    var typeReference = Resolve(module, variable.VariableType);
                    if (variable.VariableType != typeReference)
                        variable.VariableType = typeReference;
                }
            }
        skipbody:
            var genericInstanceMethod = reference as GenericInstanceMethod;
            if (genericInstanceMethod != null)
            {
                for (var i = 0; i < genericInstanceMethod.GenericArguments.Count; ++i)
                {
                    var genericArgument = Resolve(module, genericInstanceMethod.GenericArguments[i]);
                    genericInstanceMethod.GenericArguments[i] = genericArgument;
                }
            }

            var returnType = Resolve(module, reference.ReturnType);
            if (reference.ReturnType != returnType)
                reference.ReturnType = returnType;
            foreach (var p in reference.Parameters)
            {
                var parameterType = Resolve(module, p.ParameterType);
                if (p.ParameterType != parameterType)
                    p.ParameterType = parameterType;
            }

            reference = Recreate(reference, Resolve(module, reference.DeclaringType));

            return module.Import(reference);
        }

        static MethodReference Recreate(MethodReference reference, TypeReference declaringType)
        {
            try
            {
                if (reference.DeclaringType == declaringType)
                    return reference;

                reference.DeclaringType = declaringType;
                return reference;
            }
            catch (InvalidOperationException)
            {
                var instance = (GenericInstanceMethod)reference;
                var newReference = new GenericInstanceMethod(new MethodReference(instance.Name, instance.ReturnType, declaringType));
                instance.GenericArguments.ToList().ForEach(arg => newReference.GenericArguments.Add(arg));
                instance.GenericParameters.ToList().ForEach(arg => newReference.GenericParameters.Add(arg));
                instance.Parameters.ToList().ForEach(arg => newReference.Parameters.Add(arg));
                return newReference;
            }
        }

        public TypeReference Resolve(ModuleDefinition module, TypeReference type)
        {
            if (type.IsGenericInstance)
            {
                var baseType = type.Resolve();
                var instance = (GenericInstanceType)type;
                var resolvedBaseType = Resolve(module, baseType);
                var resolvedInstance =
                    resolvedBaseType.MakeGenericInstanceType(
                        instance.GenericArguments.Select(arg => Resolve(module, arg)).ToArray());
                return module.Import(resolvedInstance);
            }

            TypeDefinition orig;
            if (!m_Map.TryGetValue(type.FullName, out orig))
                return module.Import(type);

            return module.Import(orig);
        }
    }
}
