using System;

namespace Unity.Cecil.Visitor.Tests
{
	interface IEmptyInterface
	{
		void InterfaceMethod();
	}

	public class Fixture : IEmptyInterface
	{
		public int Field;
		public event EventHandler evt { add {}  remove {} }

		public void InterfaceMethod()
		{

		}

		public int AMethod(int arg, float arg2)
		{
			return 0;
		}

		public class Nested : IEmptyInterface
		{
			public int Field;

			public void InterfaceMethod()
			{
				
			}

			public int BMethod(int arg, float arg2)
			{
				return 0;
			}

			public int CMethod(object arg, float[] arg2)
			{
				return 0;
			}
		}
	}

	public class Generic<T>
	{
		
	}

	public class ComposedTypes
	{
		public int[] IntField;
		public Generic<int> GenericField;
	}
}