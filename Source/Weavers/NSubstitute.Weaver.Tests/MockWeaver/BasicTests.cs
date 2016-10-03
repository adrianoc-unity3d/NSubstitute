using NSubstitute.Exceptions;
using NUnit.Framework;
using Shouldly;

namespace NSubstitute.Weaver.Tests
{
    [TestFixture]
    class BasicTests
    {
        [Test]
        public void Can_Mock_Class()
        {
            Substitute.For<ClassWithNoDefaultCtor>();
        }

        [Test]
        public void Mocking_Class_With_Constructor_Params_Should_Throw()
        {
            Should.Throw<SubstituteException>(() => Substitute.For<ClassWithNoDefaultCtor>(null));
            Should.Throw<SubstituteException>(() => Substitute.For<ClassWithNoDefaultCtor>("test"));
            Should.Throw<SubstituteException>(() => Substitute.For<ClassWithNoDefaultCtor>(null, null));
        }

        [Test]
        public void Can_Mock_Class_Containing_ICall_In_Ctor()
        {
            Substitute.For<ClassWithCtorICall>();
        }

        [Test]
        public void Can_Mock_Class_Containing_Throw_In_Ctor()
        {
            Substitute.For<ClassWithCtorThrow>();
        }
    }
}
