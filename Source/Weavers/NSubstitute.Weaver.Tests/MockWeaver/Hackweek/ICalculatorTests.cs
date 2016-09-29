using System;
using NSubstitute;
using NUnit.Framework;

namespace NSubstitute.Weaver.Tests.Hackweek
{
    // to support: privates, concretes, structs, partial mock statics
    // non-goals: fields

    [TestFixture]
    public class ICalculatorTests
    {
        [Test]
        public static void InstanceHooksOk()
        {
            var icalculator = Substitute.For<ICalculator>();
            icalculator.Add(1, 2).Returns(4);
            Assert.That(icalculator.Add(1, 2), Is.EqualTo(4));
        }

        [Test]
        public static void PropertyHooksOk()
        {
            var icalculator = Substitute.For<ICalculator>();

            icalculator.Mode.Returns("DEC");
            Assert.That(icalculator.Mode, Is.EqualTo("DEC"));
            icalculator.Mode = "HEX";
            Assert.That(icalculator.Mode, Is.EqualTo("HEX"));

            icalculator.Mode.Returns("HEX", "DEC", "BIN");
            Assert.That(icalculator.Mode, Is.EqualTo("HEX"));
            Assert.That(icalculator.Mode, Is.EqualTo("DEC"));
            Assert.That(icalculator.Mode, Is.EqualTo("BIN"));
        }

        [Test]
        public static void TestTheTutorialThings()
        {
            var icalculator = Substitute.For<ICalculator>();

            icalculator.Add(1, 2);
            icalculator.Received().Add(1, 2);
            icalculator.DidNotReceive().Add(5, 7);

            icalculator.Add(10, -5);
            icalculator.Received().Add(10, Arg.Any<int>());
            icalculator.Received().Add(10, Arg.Is<int>(x => x < 0));

            icalculator
            .Add(Arg.Any<int>(), Arg.Any<int>())
            .Returns(x => (int)x[0] + (int)x[1]);
            Assert.That(icalculator.Add(5, 10), Is.EqualTo(15));

            bool eventWasRaised = false;
            icalculator.PoweringUp += (sender, args) => eventWasRaised = true;

            icalculator.PoweringUp += Raise.Event();
            Assert.That(eventWasRaised);

            var argumentUsed = 0;
            icalculator.Multiply(Arg.Any<int>(), Arg.Do<int>(x => argumentUsed = x));

            icalculator.Multiply(123, 42);

            Assert.AreEqual(42, argumentUsed);
        }

        [Test]
        public static void SeeHowTheyHandleGenerics()
        {
            var icalculator = Substitute.For<ICalculator>();

            icalculator.MakeString(10).Returns("ten");
            icalculator.MakeString("5").Returns("five");

            Assert.AreEqual("five", icalculator.MakeString("5"));
            Assert.AreEqual("ten", icalculator.MakeString(10));

            icalculator.MakeString2<int>().Returns("int");
            icalculator.MakeString2<string>().Returns("string");

            Assert.AreEqual("int", icalculator.MakeString2<int>());
            Assert.AreEqual("string", icalculator.MakeString2<string>());
        }
    }
}
