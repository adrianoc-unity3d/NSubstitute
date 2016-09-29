using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Unity.Cecil.Visitor;

namespace NSubstitute.Weaver
{
    class Copier
    {
        const string k_PrefixName = "Fake.";
        const string k_FakeForward = "__fake_forward";
        static MethodDefinition s_FakeForwardConstructor;
        static MethodDefinition s_FakeCloneMethod;

        public static void Copy(AssemblyDefinition source, AssemblyDefinition target, AssemblyDefinition nsubstitute, string[] typesToCopy)
        {
            var processTypeResolver = new ProcessTypeResolver(source);
            var typeDefinitions = processTypeResolver.Resolve(typesToCopy).ToList();

            // Copy in two passes to be able to move references within individual classes to other faked types.
            // Note that this functionality is not currently in use, but could prove useful for a later stage,
            // and rather than extracting the logic now, it is better to design it for this purpose up front.

            foreach (var type in typeDefinitions)
                CopyType(target, type);

            foreach (var type in typeDefinitions)
            {
                var typeDefinition = target.MainModule.Types.Single(t => t.FullName == k_PrefixName + type.FullName);
                CopyTypeMembers(target, type, typeDefinition);
            }

            var visitor = new MockInjectorVisitor(nsubstitute, target.MainModule);
            target.Accept(visitor);
        }

        static void CopyTypeMembers(AssemblyDefinition target, TypeDefinition type, TypeDefinition typeDefinition)
        {
            CopyCustomAttributes(target, type, typeDefinition);

            CreateFakeFieldForward(target, type, typeDefinition);

            CopyMethods(target, type, typeDefinition);
            CopyProperties(target, type, typeDefinition);
        }

        static void CopyProperties(AssemblyDefinition target, TypeDefinition type, TypeDefinition typeDefinition)
        {
            foreach (var property in type.Properties)
            {
                if (!property.IsPublic())
                    continue;

                var propertyDefinition = new PropertyDefinition(property.Name, property.Attributes, ResolveType(target, typeDefinition, property.PropertyType));
                if (property.GetMethod != null)
                    propertyDefinition.GetMethod = ResolveMethod(typeDefinition, property.GetMethod);
                if (property.SetMethod != null)
                    propertyDefinition.SetMethod = ResolveMethod(typeDefinition, property.SetMethod);

                typeDefinition.Properties.Add(propertyDefinition);
            }
        }

        static MethodDefinition ResolveMethod(TypeDefinition typeDefinition, MethodDefinition originalMethod)
        {
            var candidates = typeDefinition.Methods.Where(m => m.Name == originalMethod.Name).ToList();
            if (candidates.Count == 1)
                return candidates[0];

            throw new NotImplementedException();
        }

        static void CreateFakeFieldForward(AssemblyDefinition target, TypeDefinition type, TypeDefinition typeDefinition)
        {
            var field = new FieldDefinition(k_FakeForward, FieldAttributes.Assembly, target.MainModule.Import(type));
            typeDefinition.Fields.Add(field);
        }

        static void CopyType(AssemblyDefinition target, TypeDefinition type)
        {
            // TODO: Support inheritance hierarchies when copying types over (e.g. fake StreamWriter should inherit from fake TextWriter)

            var baseTypeRef = type.BaseType != null ? target.MainModule.Import(type.BaseType) : null;
            var typeDefinition = new TypeDefinition(k_PrefixName + type.Namespace, type.Name, type.Attributes & ~TypeAttributes.HasSecurity, baseTypeRef);

            target.MainModule.Types.Add(typeDefinition);
        }

        static void CopyMethods(AssemblyDefinition target, TypeDefinition type, TypeDefinition typeDefinition)
        {
            if (!type.IsAbstract)
            {
                s_FakeForwardConstructor = CreateFakeForwardConstructor(target, type, typeDefinition);
                s_FakeCloneMethod = null;
            }
            else
            {
                s_FakeForwardConstructor = null;
                s_FakeCloneMethod = CreateFakeCloneMethod(target, type, typeDefinition);
            }

            foreach (var method in type.Methods)
            {
                if (!method.IsPublic)
                    continue;

                //var fakeField = CreateFakeField(target, type, typeDefinition, method);
                //FieldDefinition fakeField = null;

                var methodDefinition = new MethodDefinition(method.Name, method.Attributes & ~MethodAttributes.HasSecurity, ResolveType(target, typeDefinition, method.ReturnType));
                foreach (var parameter in method.Parameters)
                {
                    var parameterDefinition = new ParameterDefinition(parameter.Name, parameter.Attributes,
                            ResolveType(target, typeDefinition, parameter.ParameterType));
                    methodDefinition.Parameters.Add(parameterDefinition);
                }

                if (method.Name == ".ctor")
                {
                    FillConstructor(target, type, method, typeDefinition, methodDefinition);
                    typeDefinition.Methods.Add(methodDefinition);
                }
                else
                {
                    MethodDefinition implMethod = CreateMainImplementationForwardingMethod(target, method, typeDefinition, methodDefinition);
                    //CreateMainFakeMethodContents(target, type, method, typeDefinition, methodDefinition, fakeField, implMethod);
                }

                //typeDefinition.Methods.Add(methodDefinition);
            }
        }

        static MethodDefinition CreateMainImplementationForwardingMethod(AssemblyDefinition target, MethodDefinition method, TypeDefinition typeDefinition, MethodDefinition methodDefinition)
        {
            var implMethod = new MethodDefinition(method.Name /* + "__Impl"*/, method.Attributes & ~MethodAttributes.HasSecurity, methodDefinition.ReturnType);
            typeDefinition.Methods.Add(implMethod);

            #region Check and call local delegate field
            foreach (var param in methodDefinition.Parameters)
                implMethod.Parameters.Add(new ParameterDefinition(param.Name, param.Attributes, param.ParameterType));

            implMethod.Body = new MethodBody(implMethod);

            if (implMethod.ReturnType.FullName == typeDefinition.FullName && s_FakeForwardConstructor == null)
                implMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));

            if (!method.IsStatic)
            {
                implMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0)); // this
                if (typeDefinition.IsValueType)
                    implMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldflda,
                            typeDefinition.Fields.Single(f => f.Name == k_FakeForward))); // this.__fake_forward
                else
                    implMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld,
                            FakeForwardField(typeDefinition)));
            }

            foreach (var param in implMethod.Parameters)
            {
                implMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg, param));
                if (param.ParameterType.FullName == typeDefinition.FullName)
                    implMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, FakeForwardField(typeDefinition)));
            }
            if (method.IsVirtual && !typeDefinition.IsValueType)
                implMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt, target.MainModule.Import(method)));
            else
                implMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Call, target.MainModule.Import(method))); // this.__fake__forward.<Method>(arguments)
            if (implMethod.ReturnType.FullName == typeDefinition.FullName)
            {
                implMethod.Body.Instructions.Add(s_FakeForwardConstructor != null
                    ? Instruction.Create(OpCodes.Newobj, s_FakeForwardConstructor)
                    : Instruction.Create(OpCodes.Callvirt, s_FakeCloneMethod));
                implMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
                return implMethod;
            }

            implMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            #endregion

            return implMethod;
        }

        // TODO: Finish the fake clone method, which is needed for inheritance hierarchies
        // When dealing with abstract or super types that create a modified version of a type, it is necessary for the wrapper to be able to
        // construct a new instance of the fake wrapped around the new type, but since we don't have access to enough information at the call-site,
        // we introduce a new clone method explicitly for this purpose.
        // For a typical use case, see TextWriter.Synchronized.
        static MethodDefinition CreateFakeCloneMethod(AssemblyDefinition target, TypeDefinition type, TypeDefinition typeDefinition)
        {
            var methodDefinition = new MethodDefinition("__FakeClone", MethodAttributes.Family | MethodAttributes.Virtual, typeDefinition);
            methodDefinition.Parameters.Add(new ParameterDefinition("fake", ParameterAttributes.None, target.MainModule.Import(type)));

            //if (typeDefinition.IsAbstract)
            methodDefinition.Attributes |= MethodAttributes.Abstract;

            typeDefinition.Methods.Add(methodDefinition);

            return methodDefinition;
        }

        static FieldDefinition CreateFakeField(AssemblyDefinition target, TypeDefinition type, TypeDefinition typeDefinition, MethodDefinition method)
        {
            if (method.Name == ".ctor")
                return null;

            if (method.Parameters.Any(p => p.ParameterType.IsByReference)) // TODO: Figure out byref
                return null;

            TypeReference baseType;
            if (method.ReturnType.FullName == "System.Void")
            {
                if (method.Parameters.Count == 0)
                    baseType = type.Module.GetType("System.Action");
                else
                    baseType = type.Module.GetType($"System.Action`{method.Parameters.Count}");
            }
            else
                baseType = type.Module.GetType($"System.Func`{method.Parameters.Count + 1}");
            baseType = target.MainModule.Import(baseType);

            if (method.ReturnType.FullName == "System.Void")
            {
                if (method.Parameters.Count > 0)
                    baseType = baseType.MakeGenericInstanceType(
                            method.Parameters.Select<ParameterDefinition, TypeReference>(p => ResolveType(target, typeDefinition, p.ParameterType)).ToArray());
            }
            else
                baseType = baseType.MakeGenericInstanceType(
                        method.Parameters.Select<ParameterDefinition, TypeReference>(p => ResolveType(target, typeDefinition, p.ParameterType))
                        .Concat(new[] {ResolveType(target, typeDefinition, method.ReturnType)})
                        .ToArray());

            var attributes = FieldAttributes.Public;
            if (method.IsStatic)
                attributes |= FieldAttributes.Static;

            var disambiguator = string.Join("_", method.Parameters.Select(p => p.ParameterType.Name).ToArray());
            var fieldName = method.Name + "Fake" + (disambiguator == "" ? "" : "_" + disambiguator);
            if (method.IsStatic)
                fieldName += "_static";
            var fieldDefinition = new FieldDefinition(fieldName, attributes, baseType);
            typeDefinition.Fields.Add(fieldDefinition);
            return fieldDefinition;
        }

        static MethodDefinition CreateFakeForwardConstructor(AssemblyDefinition target, TypeDefinition type, TypeDefinition typeDefinition)
        {
            var method = new MethodDefinition(".ctor", MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.RTSpecialName | MethodAttributes.SpecialName, target.MainModule.Import(type.Module.GetType("System.Void")));
            var parameterDefinition = new ParameterDefinition("forward", ParameterAttributes.In, target.MainModule.Import(type));
            method.Parameters.Add(parameterDefinition);

            method.Body = new MethodBody(method);
            AddBaseTypeCtorCall(target, typeDefinition, method);
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0)); // this
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg, parameterDefinition)); // forward
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Stfld, (FieldReference)FakeForwardField(typeDefinition))); // this.__fake_forward = forward
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

            typeDefinition.Methods.Add(method);
            return method;
        }

        static FieldDefinition FakeForwardField(TypeDefinition typeDefinition)
        {
            return typeDefinition.Fields.Single(f => f.Name == k_FakeForward);
        }

        static void CreateMainFakeMethodContents(AssemblyDefinition target, TypeDefinition type, MethodDefinition method, TypeDefinition typeDefinition, MethodDefinition methodDefinition, FieldDefinition fakeField, MethodDefinition implMethod)
        {
            methodDefinition.Body = new MethodBody(methodDefinition);

            var nop = Instruction.Create(OpCodes.Nop);
            var ret = Instruction.Create(OpCodes.Ret);

            if (fakeField != null)
            {
                AddFakeFieldCallback(target, method, methodDefinition, fakeField, nop, ret);
            }

            methodDefinition.Body.Instructions.Add(nop);

            if (!methodDefinition.IsStatic)
                methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));

            foreach (var param in methodDefinition.Parameters)
                methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg, param));
            methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Call, implMethod));

            methodDefinition.Body.Instructions.Add(ret);
        }

        static void AddFakeFieldCallback(AssemblyDefinition target, MethodDefinition method,
            MethodDefinition methodDefinition, FieldDefinition fakeField, Instruction nop, Instruction ret)
        {
            if (method.IsStatic)
            {
                methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Ldsfld, fakeField));
                methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));
                methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Cgt_Un));
                methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Brfalse_S, nop));
                methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Ldsfld, fakeField));
                foreach (var param in methodDefinition.Parameters)
                    methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg, param));
                methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt,
                        (MethodReference)ResolveGenericInvoke(target, fakeField)));
                methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Br_S, ret));
            }
            else
            {
                methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, fakeField));
                methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Ldnull));
                methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Cgt_Un));
                methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Brfalse_S, nop));
                methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Ldfld, fakeField));
                foreach (var param in methodDefinition.Parameters)
                    methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg, param));
                methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Callvirt,
                        (MethodReference)ResolveGenericInvoke(target, fakeField)));
                methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Br_S, ret));
            }
        }

        static MethodReference ResolveGenericInvoke(AssemblyDefinition target, FieldDefinition fakeField)
        {
            if (!fakeField.FieldType.IsGenericInstance)
                return target.MainModule.Import(fakeField.FieldType.Resolve().Methods.Single(m => m.Name == "Invoke"));

            var genericType = (GenericInstanceType)fakeField.FieldType;
            var openType = genericType.Resolve();
            var openInvoke = target.MainModule.Import(openType.Methods.Single(m => m.IsPublic && m.Name == "Invoke"));

            var realInvoke = new MethodReference(openInvoke.Name, openInvoke.ReturnType,
                    openInvoke.DeclaringType.MakeGenericInstanceType(genericType.GenericArguments.ToArray()))
            {
                HasThis = openInvoke.HasThis,
                ExplicitThis = openInvoke.ExplicitThis,
                CallingConvention = openInvoke.CallingConvention
            };

            foreach (var parameter in openInvoke.Parameters)
                realInvoke.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
            foreach (var genericParameter in openInvoke.GenericParameters)
                realInvoke.GenericParameters.Add(new GenericParameter(genericParameter.Name, realInvoke));

            return realInvoke;
        }

        static void FillConstructor(AssemblyDefinition target, TypeDefinition type, MethodDefinition method, TypeDefinition typeDefinition, MethodDefinition methodDefinition)
        {
            methodDefinition.Body = new MethodBody(methodDefinition);

            AddBaseTypeCtorCall(target, typeDefinition, methodDefinition);

            methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));

            foreach (var param in methodDefinition.Parameters)
                methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg, param));

            methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Newobj, target.MainModule.Import(method)));
            methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Stfld, typeDefinition.Fields.Single(f => f.Name == k_FakeForward)));
            methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }

        static void AddBaseTypeCtorCall(AssemblyDefinition target, TypeDefinition typeDefinition,
            MethodDefinition methodDefinition)
        {
            if (typeDefinition.BaseType != null && !typeDefinition.IsValueType)
            {
                methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
                var baseTypeCtor =
                    target.MainModule.Import(
                        typeDefinition.BaseType.Resolve()
                        .Methods.Single(m => m.IsConstructor && m.Parameters.Count == 0));
                methodDefinition.Body.Instructions.Add(Instruction.Create(OpCodes.Call, baseTypeCtor));
            }
        }

        static TypeReference ResolveType(AssemblyDefinition target, TypeDefinition typeDefinition, TypeReference type)
        {
            //var typeDef = target.MainModule.Types.FirstOrDefault(t => t.FullName == prefixName + type.FullName);
            var typeDef = k_PrefixName + type.FullName == typeDefinition.FullName;
            if (typeDef)
                return typeDefinition;

            return target.MainModule.Import(type);
        }

        static void CopyCustomAttributes(AssemblyDefinition target, TypeDefinition type, TypeDefinition typeDefinition)
        {
            foreach (var customAttribute in type.CustomAttributes)
            {
                if (!customAttribute.AttributeType.Resolve().IsPublic)
                    continue;

                var attribute = new CustomAttribute(target.MainModule.Import(customAttribute.Constructor.Resolve()));
                foreach (var arg in customAttribute.ConstructorArguments)
                    attribute.ConstructorArguments.Add(new CustomAttributeArgument(
                            target.MainModule.Import(arg.Type.Resolve()), arg.Value));

                typeDefinition.CustomAttributes.Add(attribute);
            }
        }
    }
}
