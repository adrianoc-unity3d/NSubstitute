using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using NUnit.Framework;

namespace NSubstitute.Weavers.Tests
{
    [TestFixture]
    class ResolverTestForTypes : ResolverTestBase
    {
        [Test]
        public void NoResolveTest()
        {
            var target = CreateAssembly("target");
            var fake = CreateFakeAssembly();
            var t = CreateType(target, "Foo", "Bar");

            var resolver = new Resolver(target, fake);
            Assert.That(resolver.Resolve(target.MainModule, t), Is.EqualTo(t));
        }

        [Test]
        public void DirectUsageTest()
        {
            var target = CreateAssembly("target");
            var fake = CreateFakeAssembly();
            var datetime = fake.MainModule.Types.Single(t => t.Name == "DateTime");
            var realDateTime = target.MainModule.Import(typeof(DateTime));

            var resolver = new Resolver(target, fake);
            Assert.That(resolver.Resolve(target.MainModule, realDateTime).FullName, Is.EqualTo(datetime.FullName));
        }

        [Test]
        public void GenericArgumentTest()
        {
            var target = CreateAssembly("target");
            var fake = CreateFakeAssembly();
            var realDateTime = target.MainModule.Import(typeof(DateTime));
            var type = CreateType(target, "Foo", "Bar`1");
            type.GenericParameters.Add(new GenericParameter("T", type));
            var instantiatedType = type.MakeGenericInstanceType(realDateTime);

            var resolver = new Resolver(target, fake);
            Assert.That(resolver.Resolve(target.MainModule, instantiatedType).FullName, Is.EqualTo("Foo.Bar`1<Fake.System.DateTime>"));
        }

        [Test]
        public void InceptionGenericArgumentTest()
        {
            var target = CreateAssembly("target");
            var fake = CreateFakeAssembly();
            var realDateTime = target.MainModule.Import(typeof(DateTime));
            var type = CreateType(target, "Foo", "Bar`1");
            type.GenericParameters.Add(new GenericParameter("T", type));
            var instantiatedType = type.MakeGenericInstanceType(realDateTime);
            var inceptionInstantiatedType = type.MakeGenericInstanceType(instantiatedType);

            var resolver = new Resolver(target, fake);
            Assert.That(resolver.Resolve(target.MainModule, inceptionInstantiatedType).FullName, Is.EqualTo("Foo.Bar`1<Foo.Bar`1<Fake.System.DateTime>>"));
        }
    }
}
