using System;
using NSubstitute.Exceptions;
using NUnit.Framework;

namespace NSubstitute.Weaver.Tests.Hackweek
{
    [TestFixture]
    public class CalculatorTests
    {
        [Test]
        public static void InstanceHooksOk()
        {
            var calculator = Substitute.For<Calculator>();
            calculator.Add(1, 2).Returns(4);
            Assert.That(calculator.Add(1, 2), Is.EqualTo(4));
        }

        [Test]
        public static void StaticHooksOk()
        {
            using (var cstatic = Substitute.ForStatic<Calculator>())
            {
                Calculator.Square(1.5f).Returns(10f);
                Assert.That(Calculator.Square(1.5f), Is.EqualTo(10f));
                cstatic.Received().Static(() => Calculator.Square(1.5f));
            }
        }

        [Test]
        public static void PropertyHooksOk()
        {
            var calculator = Substitute.For<Calculator>();

            calculator.Mode.Returns("DEC");
            Assert.That(calculator.Mode, Is.EqualTo("DEC"));
            calculator.Mode = "HEX";
            Assert.That(calculator.Mode, Is.EqualTo("HEX"));

            calculator.Mode.Returns("HEX", "DEC", "BIN");
            Assert.That(calculator.Mode, Is.EqualTo("HEX"));
            Assert.That(calculator.Mode, Is.EqualTo("DEC"));
            Assert.That(calculator.Mode, Is.EqualTo("BIN"));
        }

        [Test]
        public static void TestTheTutorialThings()
        {
            var calculator = Substitute.For<Calculator>();

            calculator.Add(1, 2);
            calculator.Received().Add(1, 2);
            calculator.DidNotReceive().Add(5, 7);

            calculator.Add(10, -5);
            calculator.Received().Add(10, Arg.Any<int>());
            calculator.Received().Add(10, Arg.Is<int>(x => x < 0));

            calculator
            .Add(Arg.Any<int>(), Arg.Any<int>())
            .Returns(x => (int)x[0] + (int)x[1]);
            Assert.That(calculator.Add(5, 10), Is.EqualTo(15));

            bool eventWasRaised = false;
            calculator.PoweringUp += (sender, args) => eventWasRaised = true;

            calculator.PoweringUp += Raise.Event();
            Assert.That(eventWasRaised);

            var argumentUsed = 0;
            calculator.Multiply(Arg.Any<int>(), Arg.Do<int>(x => argumentUsed = x));

            calculator.Multiply(123, 42);

            Assert.AreEqual(42, argumentUsed);
        }

        [Test]
        public static void CanPartialMockConcreteClass()
        {
            var calculator = Substitute.ForPartsOf<Calculator>();
            Assert.AreEqual(3, calculator.Add(1, 2));
            calculator.Multiply(3, 4).Returns(10);
            Assert.AreEqual(10, calculator.Multiply(3, 4));
        }

        /*
        [Test]
        public static void CanPartialMockStatics()
        {
            // have to implement Substitute.ForPartsOfStatic first
        }*/

        [Test]
        public static void StaticReroutesNotUsedAcrossDisposals()
        {
            using (var scalc1 = Substitute.ForStatic<Calculator>())
            {
                Calculator.Square(Arg.Any<float>()).Returns(10);
                Assert.That(Calculator.Square(3), Is.EqualTo(10));
                Calculator.Square(4);
                scalc1.Received().Static(() => Calculator.Square(4));
                scalc1.Received().Static(() => Calculator.Square(Arg.Any<float>()));

                Assert.That(Calculator.Square(3), Is.EqualTo(10));
            }

            Assert.That(Calculator.Square(3), Is.EqualTo(9), "should be passthru again");

            using (var scalc2 = Substitute.ForStatic<Calculator>())
            {
                Assert.That(Calculator.Square(3), Is.EqualTo(0), "should be default again");
                scalc2.Received().Static(() => Calculator.Square(3));
            }

            Assert.That(Calculator.Square(4), Is.EqualTo(16), "passthru once more");
        }

        [Test]
        public static void DoubleSubstitutionWithoutDisposeShouldThrow()
        {
            using (Substitute.ForStatic<Calculator>())
                Assert.Throws<SubstituteException>(() => Substitute.ForStatic<Calculator>());
        }

        [Test]
        public static void DoubleSubstitutionWithDisposeShouldNotThrow()
        {
            Substitute.ForStatic<Calculator>().Dispose();
            Assert.DoesNotThrow(() => Substitute.ForStatic<Calculator>().Dispose());
        }
    }
}
