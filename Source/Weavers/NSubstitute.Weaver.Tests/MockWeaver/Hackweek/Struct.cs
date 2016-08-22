using System;

namespace NSubstitute.Weavers.Tests.Hackweek
{
	public struct SimpleStruct
	{
		public int m_Value;
		public int m_AnotherPropertyValue;

		public void SetValue (int v)
		{
			m_Value = v;
		}

		public int GetValue ()
		{
			return m_Value;
		}

		public int valueProperty { get; set; }

		public int anotherValueProperty
		{
			get { return m_AnotherPropertyValue; }
			set { m_AnotherPropertyValue = value; }
		}
	}
}
