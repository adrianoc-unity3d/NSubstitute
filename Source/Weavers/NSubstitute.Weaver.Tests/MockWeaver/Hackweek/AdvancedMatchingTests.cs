using System;
using NUnit.Framework;

namespace NSubstitute.Weavers.Tests.Hackweek
{
    [TestFixture]
    public class AdvancedMatchingTests
    {
        [Test]
        public static void CanMockSimpleGeneric()
        {
            var am = Substitute.For<AdvancedMatching>();

            am.MakeString(10).Returns("ten");
            Assert.AreEqual("ten", am.MakeString(10));

            am.MakeString(1.2, "hi").Returns("yo");
            Assert.AreEqual("yo", am.MakeString(1.2, "hi"));

            am.AddNumbers(10, 20).Returns("thirty");
            Assert.AreEqual("thirty", am.AddNumbers(10, 20));

            am.AddNumbers(100, 50, 15).Returns("something");
            Assert.AreEqual("something", am.AddNumbers(100, 50, 15));
        }

        [Test]
        public static void CanMockGenericClass()
        {
            var am = Substitute.For<AdvancedMatching<float, char>>();

            am.MakeString(123).Returns("stuff");
            Assert.AreEqual("stuff", am.MakeString(123));
        }
    }
}
