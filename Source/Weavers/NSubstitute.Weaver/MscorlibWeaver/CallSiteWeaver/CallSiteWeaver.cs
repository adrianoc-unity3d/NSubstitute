using System;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace NSubstitute.Weaver
{
    public class CallSiteWeaver
    {
        Resolver m_Resolver;

        public void Weave(AssemblyDefinition fakeAssembly, AssemblyDefinition assembly, string targetFile)
        {
//            var fakeAssembly = AssemblyDefinition.ReadAssembly(fakeAssemblyPath);
//            var assembly = AssemblyDefinition.ReadAssembly(assemblyPath);
            m_Resolver = new Resolver(assembly, fakeAssembly);

            foreach (var module in assembly.Modules)
            {
                UpdateTypes(module.Types, module);
            }

            if (!string.IsNullOrEmpty(targetFile))
                assembly.Write(targetFile);
        }

        public void UpdateTypes(Collection<TypeDefinition> types, ModuleDefinition module)
        {
            foreach (var type in types)
            {
                UpdateMethods(type.Methods, module);
                UpdateProperties(type, module);
                UpdateFields(type, module);
                UpdateEvents(type.Events, module);
                UpdateTypes(type.NestedTypes, module);

                for (var i = 0; i < type.Interfaces.Count; ++i)
                    type.Interfaces[i] = m_Resolver.Resolve(module, type.Interfaces[i]);
            }
        }

        void UpdateEvents(Collection<EventDefinition> events, ModuleDefinition module)
        {
            foreach (var ev in events)
            {
                if (ev.AddMethod != null)
                    UpdateMethod(module, ev.AddMethod);
                if (ev.RemoveMethod != null)
                    UpdateMethod(module, ev.RemoveMethod);
                if (ev.InvokeMethod != null)
                    UpdateMethod(module, ev.InvokeMethod);
                if (ev.OtherMethods != null)
                    UpdateMethods(ev.OtherMethods, module);
            }
        }

        void UpdateFields(TypeDefinition type, ModuleDefinition module)
        {
            foreach (var field in type.Fields)
                field.FieldType = m_Resolver.Resolve(module, field.FieldType);
        }

        void UpdateProperties(TypeDefinition type, ModuleDefinition module)
        {
            foreach (var property in type.Properties)
            {
                if (property.GetMethod != null)
                    UpdateMethod(module, property.GetMethod);
                if (property.SetMethod != null)
                    UpdateMethod(module, property.SetMethod);
                if (property.HasOtherMethods)
                    foreach (var m in property.OtherMethods)
                        UpdateMethod(module, m);
                property.PropertyType = m_Resolver.Resolve(module, property.PropertyType);
            }
        }

        void UpdateMethods(Collection<MethodDefinition> methods, ModuleDefinition module)
        {
            foreach (var method in methods)
                UpdateMethod(module, method);
        }

        void UpdateMethod(ModuleDefinition module, MethodDefinition method)
        {
            m_Resolver.Resolve(module, method);
        }
    }
}
