using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using NUnit.Framework;

namespace NSubstitute.Weaving.Tests
{
    [TestFixture]
    class ResolverTestForMethods : ResolverTestBase
    {
        [Test]
        public void NoFakeUsageTest()
        {
            var target = CreateAssembly("test");
            var fake = CreateFakeAssembly();
            var type = CreateType(target, "Foo", "Bar");
            var int32 = target.MainModule.Import(typeof(int));
            var method = CreateMethodDefinition(type, "Hello", int32, int32);

            var resolver = new Resolver(target, fake);
            Assert.That(resolver.Resolve(target.MainModule, method).FullName, Is.EqualTo("System.Int32 Foo.Bar::Hello(System.Int32)"));
        }

        [Test]
        public void NonGenericMethodInNonGenericTypeTest()
        {
            var target = CreateAssembly("test");
            var fake = CreateFakeAssembly();
            var type = CreateType(target, "Foo", "Bar");
            var datetime = target.MainModule.Import(typeof(DateTime));
            var method = CreateMethodDefinition(type, "Hello", datetime, datetime);

            var resolver = new Resolver(target, fake);
            Assert.That(resolver.Resolve(target.MainModule, method).FullName, Is.EqualTo("Fake.System.DateTime Foo.Bar::Hello(Fake.System.DateTime)"));
        }

        [Test]
        public void GenericOpenMethodInSimpleTypeUsageTest()
        {
            var target = CreateAssembly("test");
            var fake = CreateFakeAssembly();
            var type = CreateType(target, "Foo", "Bar");
            var datetime = target.MainModule.Import(typeof(DateTime));
            var method = CreateMethodDefinition(type, "Hello", datetime, datetime);
            AddGenericMethodParameters(method, 1);

            var resolver = new Resolver(target, fake);
            Assert.That(resolver.Resolve(target.MainModule, method).FullName, Is.EqualTo("Fake.System.DateTime Foo.Bar::Hello(Fake.System.DateTime)"));
        }

        [Test]
        public void GenericClosedMethodInSimpleTypeUsageTest()
        {
            var target = CreateAssembly("test");
            var fake = CreateFakeAssembly();
            var type = CreateType(target, "Foo", "Bar");
            var datetime = target.MainModule.Import(typeof(DateTime));
            var method = CreateMethodDefinition(type, "Hello", datetime, datetime);
            AddGenericMethodParameters(method, 1);
            var methodInstance = new GenericInstanceMethod(method);
            methodInstance.GenericArguments.Add(datetime);

            var resolver = new Resolver(target, fake);
            Assert.That(resolver.Resolve(target.MainModule, methodInstance).FullName, Is.EqualTo("Fake.System.DateTime Foo.Bar::Hello<Fake.System.DateTime>(Fake.System.DateTime)"));
        }

        [Test]
        public void NonGenericMethodInOpenGenericTypeTest()
        {
            var target = CreateAssembly("test");
            var fake = CreateFakeAssembly();
            var type = CreateType(target, "Foo", "Bar`1");
            type.GenericParameters.Add(new GenericParameter("T", type));
            var datetime = target.MainModule.Import(typeof(DateTime));
            var method = CreateMethodDefinition(type, "Hello", datetime, datetime);

            var resolver = new Resolver(target, fake);
            Assert.That(resolver.Resolve(target.MainModule, method).FullName, Is.EqualTo("Fake.System.DateTime Foo.Bar`1::Hello(Fake.System.DateTime)"));
        }

        [Test]
        public void NonGenericMethodInClosedGenericTypeTest()
        {
            var target = CreateAssembly("test");
            var fake = CreateFakeAssembly();
            var type = CreateType(target, "Foo", "Bar`1");
            type.GenericParameters.Add(new GenericParameter("T", type));
            var datetime = target.MainModule.Import(typeof(DateTime));
            var typeInstance = type.MakeGenericInstanceType(datetime);
            var methodDefinition = CreateMethodDefinition(type, "Hello", datetime, datetime);
            var method = new MethodReference(methodDefinition.Name, methodDefinition.ReturnType, typeInstance);
            methodDefinition.Parameters.ToList().ForEach(p => method.Parameters.Add(p));

            var resolver = new Resolver(target, fake);
            Assert.That(resolver.Resolve(target.MainModule, method).FullName, Is.EqualTo("Fake.System.DateTime Foo.Bar`1<Fake.System.DateTime>::Hello(Fake.System.DateTime)"));
        }

        [Test]
        public void NonGenericMethodInClosedGenericTypeWithTypeParameterReferencesTest()
        {
            var target = CreateAssembly("test");
            var fake = CreateFakeAssembly();
            var type = CreateType(target, "Foo", "Bar`1");
            var genericParameter = new GenericParameter("T", type);
            type.GenericParameters.Add(genericParameter);
            var datetime = target.MainModule.Import(typeof(DateTime));
            var typeInstance = type.MakeGenericInstanceType(datetime);
            var methodDefinition = CreateMethodDefinition(type, "Hello", genericParameter, genericParameter);
            var method = new MethodReference(methodDefinition.Name, typeInstance.GenericArguments[0], typeInstance);
            methodDefinition.Parameters.ToList().ForEach(p => method.Parameters.Add(new ParameterDefinition(p.Name, p.Attributes, typeInstance.GenericArguments[0])));

            var resolver = new Resolver(target, fake);
            Assert.That(resolver.Resolve(target.MainModule, method).FullName, Is.EqualTo("Fake.System.DateTime Foo.Bar`1<Fake.System.DateTime>::Hello(Fake.System.DateTime)"));
        }

        [Test]
        public void OpenGenericMethodInNonGenericTypeTest()
        {
            var target = CreateAssembly("test");
            var fake = CreateFakeAssembly();
            var type = CreateType(target, "Foo", "Bar");
            var datetime = target.MainModule.Import(typeof(DateTime));
            var method = CreateMethodDefinition(type, "Hello", datetime, datetime);
            AddGenericMethodParameters(method, 1);

            var resolver = new Resolver(target, fake);
            Assert.That(resolver.Resolve(target.MainModule, method).FullName, Is.EqualTo("Fake.System.DateTime Foo.Bar::Hello(Fake.System.DateTime)"));
        }

        [Test]
        public void OpenGenericMethodInOpenTypeTest()
        {
            var target = CreateAssembly("test");
            var fake = CreateFakeAssembly();
            var type = CreateType(target, "Foo", "Bar`1");
            type.GenericParameters.Add(new GenericParameter("T", type));
            var datetime = target.MainModule.Import(typeof(DateTime));
            var method = CreateMethodDefinition(type, "Hello", datetime, datetime);
            AddGenericMethodParameters(method, 1);

            var resolver = new Resolver(target, fake);
            Assert.That(resolver.Resolve(target.MainModule, method).FullName, Is.EqualTo("Fake.System.DateTime Foo.Bar`1::Hello(Fake.System.DateTime)"));
        }

        [Test]
        public void OpenGenericMethodInClosedGenericTypeTest()
        {
            var target = CreateAssembly("test");
            var fake = CreateFakeAssembly();
            var type = CreateType(target, "Foo", "Bar`1");
            type.GenericParameters.Add(new GenericParameter("T", type));
            var datetime = target.MainModule.Import(typeof(DateTime));
            var typeInstance = type.MakeGenericInstanceType(datetime);
            var methodDefinition = CreateMethodDefinition(type, "Hello", datetime, datetime);
            AddGenericMethodParameters(methodDefinition, 1);
            var method = new MethodReference(methodDefinition.Name, methodDefinition.ReturnType, typeInstance);
            methodDefinition.Parameters.ToList().ForEach(p => method.Parameters.Add(p));
            methodDefinition.GenericParameters.ToList().ForEach(p => method.GenericParameters.Add(p));

            var resolver = new Resolver(target, fake);
            Assert.That(resolver.Resolve(target.MainModule, method).FullName, Is.EqualTo("Fake.System.DateTime Foo.Bar`1<Fake.System.DateTime>::Hello(Fake.System.DateTime)"));
        }

        [Test]
        public void OpenNonGenericMethodInClosedGenericTypeTest()
        {
            var target = CreateAssembly("test");
            var fake = CreateFakeAssembly();
            var type = CreateType(target, "Foo", "Bar`1");
            type.GenericParameters.Add(new GenericParameter("T", type));
            var datetime = target.MainModule.Import(typeof(DateTime));
            var typeInstance = type.MakeGenericInstanceType(datetime);
            var methodDefinition = CreateMethodDefinition(type, "Hello", datetime, datetime);
            AddGenericMethodParameters(methodDefinition, 1);
            var method = new MethodReference(methodDefinition.Name, methodDefinition.ReturnType, typeInstance);
            methodDefinition.Parameters.ToList().ForEach(p => method.Parameters.Add(p));
            methodDefinition.GenericParameters.ToList().ForEach(p => method.GenericParameters.Add(p));

            var resolver = new Resolver(target, fake);
            Assert.That(resolver.Resolve(target.MainModule, method).FullName, Is.EqualTo("Fake.System.DateTime Foo.Bar`1<Fake.System.DateTime>::Hello(Fake.System.DateTime)"));
        }

        [Test]
        public void OpenGenericMethodInClosedGenericTypeWithTypeParameterReferencesTest()
        {
            var target = CreateAssembly("test");
            var fake = CreateFakeAssembly();
            var type = CreateType(target, "Foo", "Bar`1");
            var genericParameter = new GenericParameter("T", type);
            type.GenericParameters.Add(genericParameter);
            var datetime = target.MainModule.Import(typeof(DateTime));
            var typeInstance = type.MakeGenericInstanceType(datetime);
            var methodDefinition = CreateMethodDefinition(type, "Hello", genericParameter, genericParameter);
            var method = new MethodReference(methodDefinition.Name, typeInstance.GenericArguments[0], typeInstance);
            methodDefinition.Parameters.ToList().ForEach(p => method.Parameters.Add(new ParameterDefinition(p.Name, p.Attributes, typeInstance.GenericArguments[0])));
            methodDefinition.GenericParameters.ToList().ForEach(p => method.GenericParameters.Add(p));

            var resolver = new Resolver(target, fake);
            Assert.That(resolver.Resolve(target.MainModule, method).FullName, Is.EqualTo("Fake.System.DateTime Foo.Bar`1<Fake.System.DateTime>::Hello(Fake.System.DateTime)"));
        }

        [Test]
        public void ClosedGenericMethodInNonGenericTypeTest()
        {
            var target = CreateAssembly("test");
            var fake = CreateFakeAssembly();
            var type = CreateType(target, "Foo", "Bar");
            var datetime = target.MainModule.Import(typeof(DateTime));
            var method = CreateMethodDefinition(type, "Hello", datetime, datetime);
            AddGenericMethodParameters(method, 1);
            var methodInstance = new GenericInstanceMethod(method);
            methodInstance.GenericArguments.Add(datetime);

            var resolver = new Resolver(target, fake);
            Assert.That(resolver.Resolve(target.MainModule, methodInstance).FullName, Is.EqualTo("Fake.System.DateTime Foo.Bar::Hello<Fake.System.DateTime>(Fake.System.DateTime)"));
        }

        [Test]
        public void ClosedGenericMethodInOpenTypeTest()
        {
            var target = CreateAssembly("test");
            var fake = CreateFakeAssembly();
            var type = CreateType(target, "Foo", "Bar`1");
            type.GenericParameters.Add(new GenericParameter("T", type));
            var datetime = target.MainModule.Import(typeof(DateTime));
            var method = CreateMethodDefinition(type, "Hello", datetime, datetime);
            AddGenericMethodParameters(method, 1);
            var methodInstance = new GenericInstanceMethod(method);
            methodInstance.GenericArguments.Add(datetime);

            var resolver = new Resolver(target, fake);
            Assert.That(resolver.Resolve(target.MainModule, methodInstance).FullName, Is.EqualTo("Fake.System.DateTime Foo.Bar`1::Hello<Fake.System.DateTime>(Fake.System.DateTime)"));
        }

        [Test]
        public void ClosedGenericMethodInClosedGenericTypeTest()
        {
            var target = CreateAssembly("test");
            var fake = CreateFakeAssembly();
            var type = CreateType(target, "Foo", "Bar`1");
            type.GenericParameters.Add(new GenericParameter("T", type));
            var datetime = target.MainModule.Import(typeof(DateTime));
            var typeInstance = type.MakeGenericInstanceType(datetime);
            var methodDefinition = CreateMethodDefinition(type, "Hello", datetime, datetime);
            AddGenericMethodParameters(methodDefinition, 1);
            var method = new MethodReference(methodDefinition.Name, methodDefinition.ReturnType, typeInstance);
            methodDefinition.Parameters.ToList().ForEach(p => method.Parameters.Add(p));
            methodDefinition.GenericParameters.ToList().ForEach(p => method.GenericParameters.Add(p));
            var methodInstance = new GenericInstanceMethod(method);
            methodInstance.GenericArguments.Add(datetime);

            var resolver = new Resolver(target, fake);
            Assert.That(resolver.Resolve(target.MainModule, methodInstance).FullName, Is.EqualTo("Fake.System.DateTime Foo.Bar`1<Fake.System.DateTime>::Hello<Fake.System.DateTime>(Fake.System.DateTime)"));
        }

        [Test]
        public void ClosedNonGenericMethodInClosedGenericTypeTest()
        {
            var target = CreateAssembly("test");
            var fake = CreateFakeAssembly();
            var type = CreateType(target, "Foo", "Bar`1");
            type.GenericParameters.Add(new GenericParameter("T", type));
            var datetime = target.MainModule.Import(typeof(DateTime));
            var typeInstance = type.MakeGenericInstanceType(datetime);
            var methodDefinition = CreateMethodDefinition(type, "Hello", datetime, datetime);
            AddGenericMethodParameters(methodDefinition, 1);
            var method = new MethodReference(methodDefinition.Name, methodDefinition.ReturnType, typeInstance);
            methodDefinition.Parameters.ToList().ForEach(p => method.Parameters.Add(p));
            methodDefinition.GenericParameters.ToList().ForEach(p => method.GenericParameters.Add(p));
            var methodInstance = new GenericInstanceMethod(method);
            methodInstance.GenericArguments.Add(datetime);

            var resolver = new Resolver(target, fake);
            Assert.That(resolver.Resolve(target.MainModule, methodInstance).FullName, Is.EqualTo("Fake.System.DateTime Foo.Bar`1<Fake.System.DateTime>::Hello<Fake.System.DateTime>(Fake.System.DateTime)"));
        }

        [Test]
        public void ClosedGenericMethodInClosedGenericTypeWithTypeParameterReferencesTest()
        {
            var target = CreateAssembly("test");
            var fake = CreateFakeAssembly();
            var type = CreateType(target, "Foo", "Bar`1");
            var genericParameter = new GenericParameter("T", type);
            type.GenericParameters.Add(genericParameter);
            var datetime = target.MainModule.Import(typeof(DateTime));
            var typeInstance = type.MakeGenericInstanceType(datetime);
            var methodDefinition = CreateMethodDefinition(type, "Hello", genericParameter, genericParameter);
            var method = new MethodReference(methodDefinition.Name, typeInstance.GenericArguments[0], typeInstance);
            methodDefinition.Parameters.ToList().ForEach(p => method.Parameters.Add(new ParameterDefinition(p.Name, p.Attributes, typeInstance.GenericArguments[0])));
            methodDefinition.GenericParameters.ToList().ForEach(p => method.GenericParameters.Add(p));
            var methodInstance = new GenericInstanceMethod(method);
            methodInstance.GenericArguments.Add(datetime);

            var resolver = new Resolver(target, fake);
            Assert.That(resolver.Resolve(target.MainModule, methodInstance).FullName, Is.EqualTo("Fake.System.DateTime Foo.Bar`1<Fake.System.DateTime>::Hello<Fake.System.DateTime>(Fake.System.DateTime)"));
        }

        [Test]
        public void LocalVariablesAndBodyReferencesInToNonGenericTypeTest()
        {
            var target = CreateAssembly("test");
            var fake = CreateFakeAssembly();
            var datetime = target.MainModule.Import(typeof(DateTime));

            var nonGenericType = CreateType(target, "Foo", "Type1");
            var nonGenericTypeNonGenericMethod = CreateMethodDefinition(nonGenericType, "Method1", datetime, datetime);
            var nonGenericTypeGenericMethod = CreateMethodDefinition(nonGenericType, "Method2", datetime, datetime);
            AddGenericMethodParameters(nonGenericTypeGenericMethod, 1);
            nonGenericTypeGenericMethod.ReturnType = nonGenericTypeGenericMethod.GenericParameters[0];
            nonGenericTypeGenericMethod.Parameters[0].ParameterType = nonGenericTypeGenericMethod.GenericParameters[0];

            var testType = CreateType(target, "Bar", "Test");
            var method = CreateMethodDefinition(testType, "Hello", datetime, datetime);
            method.Body = new MethodBody(method);
            method.Body.Variables.Add(new VariableDefinition("v0", nonGenericType));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldtoken, nonGenericType));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldtoken, nonGenericTypeNonGenericMethod));
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldtoken, nonGenericTypeGenericMethod));
            var closedMethod = new GenericInstanceMethod(nonGenericTypeGenericMethod);
            closedMethod.GenericArguments.Add(datetime);
            method.Body.Instructions.Add(Instruction.Create(OpCodes.Ldtoken, closedMethod));

            var resolver = new Resolver(target, fake);
            var actualMethodRef = resolver.Resolve(target.MainModule, method);
            Assert.That(actualMethodRef.FullName, Is.EqualTo("Fake.System.DateTime Bar.Test::Hello(Fake.System.DateTime)"));
            Assert.That(method.Body.Variables[0].VariableType, Is.EqualTo(nonGenericType));
            Assert.That(method.Body.Instructions[0].Operand, Is.EqualTo(nonGenericType));
            Assert.That(method.Body.Instructions[1].Operand.ToString(), Is.EqualTo("Fake.System.DateTime Foo.Type1::Method1(Fake.System.DateTime)"));
            Assert.That(method.Body.Instructions[2].Operand.ToString(), Is.EqualTo("R0 Foo.Type1::Method2(R0)"));
            Assert.That(method.Body.Instructions[3].Operand.ToString(), Is.EqualTo("R0 Foo.Type1::Method2<Fake.System.DateTime>(R0)"));
        }

        static void AddGenericMethodParameters(MethodDefinition method, int arguments)
        {
            for (var i = 0; i < arguments; ++i)
                method.GenericParameters.Add(new GenericParameter("R" + i, method));
        }

        static MethodDefinition CreateMethodDefinition(TypeDefinition type, string name, TypeReference returnType,
            params TypeReference[] arguments)
        {
            var md = new MethodDefinition(name, MethodAttributes.Public, returnType);
            for (var i = 0; i < arguments.Length; ++i)
                md.Parameters.Add(new ParameterDefinition("p" + i, ParameterAttributes.None, arguments[i]));
            type.Methods.Add(md);
            return md;
        }
    }
}
