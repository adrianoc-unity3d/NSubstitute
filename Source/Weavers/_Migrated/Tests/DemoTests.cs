using System;
using NUnit.Framework;

using UnityEngine; // for REALZ
using NSubstitute; // hugs
using Shouldly; // kisses

class Missile									// system under test
{
	readonly Collider _collider;
	readonly DateTime _validAtTime;

	public Missile(Collider collider, DateTime validAtTime)
	{
		_collider = collider;
		_validAtTime = validAtTime;
	}

	public bool DidValidate { get; private set; }

	public void Validate()						// function we're testing
	{
		var now = DateTime.Now;					// mscorlib.dll !!
		if (now.Ticks >= _validAtTime.Ticks)
		{
			if (!_collider.enabled)				// UnityEngine.dll !!
			{
				var message = Console.ReadLine(); // << WAT
				throw new Exception(message);
			}

			DidValidate = true;
		}
	}
}

public class DemoTests
{
	[Test]
	public static void MissileChecksForCollider()
	{
		// ARRANGE

		using (Substitute.ForStatic<DateTime>())
		using (Substitute.ForStatic(typeof(Console)))
		{
			var nowTime = new DateTime(1955, 11, 12);
			var futureTime = new DateTime(1985, 10, 26);

			DateTime.Now.Returns(nowTime);

			var collider = Substitute.For<Collider>();
			collider.enabled.Returns(true, false, true);

			var okMissile = new Missile(collider, nowTime);
			var badMissile = new Missile(collider, nowTime);
			var notReadyMissile = new Missile(collider, futureTime);

			Console.ReadLine().Returns("wat");

			// ACT

			Should.NotThrow(() => okMissile.Validate());
			Should.NotThrow(() => notReadyMissile.Validate());

			Should
				.Throw<Exception>(() => badMissile.Validate())
				.Message.ShouldBe("wat");

			// ASSERT

			okMissile.DidValidate.ShouldBeTrue();
			badMissile.DidValidate.ShouldBeFalse();
			notReadyMissile.DidValidate.ShouldBeFalse();
		}
	}
}
