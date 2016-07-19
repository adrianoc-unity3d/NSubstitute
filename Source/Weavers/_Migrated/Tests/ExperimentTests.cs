using System;
using NSubstitute;
using NUnit.Framework;

// to support: privates, concretes, structs, partial mock statics
// non-goals: fields

[TestFixture]
public class ExperimentTests
{
	[Test]
	public static void PartialInterfaceExperiment()
	{
		// this works without patching required, because we're working with an interface
		var test = Substitute.ForPartsOf<IExperiment, Experiment>();
		test.A.Returns("AA");
		Assert.AreEqual("AA", test.A);
		Assert.AreEqual("b", test.B);
	}

	[Test]
	public static void PartialConcreteExperiment()
	{
		// this only works when we've patched and hooked the concrete type
		var test = Substitute.ForPartsOf<Experiment>();
		test.A.Returns("AA");
		Assert.AreEqual("AA", test.A);
		Assert.AreEqual("b", test.B);
	}

	public class NSubExperiment
	{
		public virtual int A()
		{
			return B() + 5;
		}

		public virtual int B()
		{
			return 1;
		}
	}

	[Test]
	public static void SeeHowTheyHandleNestedMockedMethodCalls()
	{
		var mock = Substitute.ForPartsOf<NSubExperiment>();
		Assert.AreEqual(6, mock.A());
		Assert.AreEqual(1, mock.B());

		mock.B().Returns(10);
		Assert.AreEqual(15, mock.A()); // note that A's internal impl ends up calling the mocked B, not the original B impl
		Assert.AreEqual(10, mock.B());
	}
}
