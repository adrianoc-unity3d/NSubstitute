using System;
using System.Threading;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

// to support: privates, concretes, structs, partial mock statics
// non-goals: fields

[TestFixture]
public class MscorlibTests
{
	[Test]
	public static void DateTimeRerouting()
	{
		using (Substitute.ForStatic<DateTime>())
		{
			DateTime.Now.Returns(new DateTime(2010, 1, 1));

			DateTime.Now.Year.ShouldBe(2010);
		}
	}

	[Test]
	public static void RandomRerouting()
	{
		// nothing special here - Random is all virtuals so no patching necessary

		var random = Substitute.For<Random>();
		random.Next().Returns(10);

		random.Next().ShouldBe(10);
	}

	[Test]
	public static void ConsoleReadLineRerouting()
	{
		using (Substitute.ForStatic(typeof(Console)))
		{
			Console.ReadLine().Returns("xyzzy");

			Console.ReadLine().ShouldBe("xyzzy");
		}
	}

	[Test]
	public static void ThreadSleepRerouting()
	{
		using (Substitute.ForStatic(typeof(Thread)))
		{
			Thread.Sleep(TimeSpan.FromDays(1)); // finishes instantly
		}
	}

	[Test]
	public static void MultipleStaticsAtOnce()
	{
		using (var sdatetime = Substitute.ForStatic<DateTime>())
		using (var sconsole = Substitute.ForStatic(typeof(Console)))
		using (var sthread = Substitute.ForStatic<Thread>())
		{

		}
	}

#if NO
	[Test]
	public static void DateTimeRerouting()
	{
		var dt = new FDateTime(1, 2, 3);
		FDateTime.get_NowFake_static = () =>
		{
			var hook = CastlePatchedInterceptorRegistry.TryGetStaticHook(typeof(FDateTime), 1);
			if (hook != null)
			{
				var results = hook(new object[0]);

				var retvalue = results[0];
				if (!(retvalue is PassthroughPlaceholder))
				{
					return (FDateTime)retvalue;
				}
			}

			return new FDateTime(DateTime.Now.Ticks);
		};

		Assert.AreEqual(1, dt.Year);

		var dtstatic = Substitute.ForStatic<FDateTime>();
		FDateTime.Now.Returns(new FDateTime(2, 3, 4));
		Assert.AreEqual(2, FDateTime.Now.Year);
	}

	[Test]
	public static void StringBuilderRerouting()
	{
		var sb = Substitute.ForPartsOf<FStringBuilder>();

		sb.get_LengthFake = () =>
		{
			var hook = CastlePatchedInterceptorRegistry.TryGetHook(sb);
			if (hook != null)
			{
				var results = hook(new object[0]);
				var retvalue = results[0];
				if (!(retvalue is PassthroughPlaceholder))
					return (int)retvalue;
			}

			var oldHook = sb.get_LengthFake;
			sb.get_LengthFake = null;
			var rv = sb.Length;
			sb.get_LengthFake = oldHook;
			return rv;
		};
		sb.ToStringFake = () =>
		{
			var hook = CastlePatchedInterceptorRegistry.TryGetHook(sb);
			if (hook != null)
			{
				var results = hook(new object[0]);
				var retvalue = results[0];
				if (!(retvalue is PassthroughPlaceholder))
					return (string)retvalue;
			}

			var oldHook = sb.ToStringFake;
			sb.ToStringFake = null;
			var rv = sb.ToString();
			sb.ToStringFake = oldHook;
			return rv;
		};

		sb.Append("abc");

		sb.ToString().Returns("test");
		Assert.AreEqual("test", sb.ToString());

		//sb.Length.Returns(123);

		Assert.AreEqual(3, sb.Length);
	}
#endif
}
