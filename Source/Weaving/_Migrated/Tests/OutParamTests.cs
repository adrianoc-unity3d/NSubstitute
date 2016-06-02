using System;
using NSubstitute;
using NSubstitute.Proxies.CastleDynamicProxy;
using NUnit.Framework;

// to support: privates, concretes, structs, partial mock statics
// non-goals: fields

[TestFixture]
public class OutParamTests
{
	[Test]
	public static void OutParametersProperlySet()
	{
		// arrange
		var value = 0;
		var itest = Substitute.For<OutParam>();
		itest
			.Foo(10, out value)
			.Returns(x =>
			{
				x[1] = 5;
				return 8;
			});

		// act
		var result = itest.Foo(10, out value);

		// assert
		Assert.That(result, Is.EqualTo(8));
		Assert.That(value, Is.EqualTo(5));
	}

	[Test]
	public static void OutParametersProperlySetWithoutArranging()
	{
		var itest = Substitute.ForPartsOf<OutParam>();
		var value = 0;
		var result = itest.Foo(10, out value);

		Assert.That(value, Is.EqualTo(123));
		Assert.That(result, Is.EqualTo(456));
	}
}
