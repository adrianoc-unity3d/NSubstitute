using System;
using NSubstitute.Exceptions;
using NUnit.Framework;

namespace NSubstitute.Weavers.Tests.Hackweek
{
    [TestFixture]
    public class StructTests
    {
        [Test]
        public static void TestStructSetValueMethodCall()
        {
            var ts = new SimpleStruct();
            ts.SetValue(10);
            Assert.AreEqual(10, ts.m_Value);
        }

        [Test]
        public static void TestStructSetFieldDirect()
        {
            var ts = new SimpleStruct();
            ts.m_Value = 10;
            Assert.AreEqual(10, ts.m_Value);
        }

        [Test]
        public static void TestStructSetFieldDirect_Verify_WithGetMethod()
        {
            var ts = new SimpleStruct();
            ts.m_Value = 10;
            Assert.AreEqual(10, ts.GetValue());
        }

        [Test]
        public static void TestStructSetGetProperty()
        {
            var ts = new SimpleStruct();
            ts.valueProperty = 10;
            Assert.AreEqual(10, ts.valueProperty);
        }

        [Test]
        public static void TestStructSetGetAnotherProperty()
        {
            var ts = new SimpleStruct();
            ts.anotherValueProperty = 10;
            Assert.AreEqual(10, ts.anotherValueProperty);
            Assert.AreEqual(10, ts.m_AnotherPropertyValue);
        }
    }
}
