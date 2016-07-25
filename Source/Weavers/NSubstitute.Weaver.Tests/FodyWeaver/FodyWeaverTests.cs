using System;
using System.Reflection;
using NUnit.Framework;
using Shouldly;

namespace NSubstitute.Weavers.Tests
{
    [TestFixture]
    public class FodyWeaverTests
    {
        [Test]
        public void Fody_Is_Patching_Test_Assembly()
        {
            Assembly.GetExecutingAssembly().GetType("NSubstitute.Weavers.Tests.InjectedTypeForTest").ShouldNotBeNull();
        }
    }
}
