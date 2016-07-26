using System;

namespace NSubstitute.Weavers.Tests.Hackweek
{
#   pragma warning disable 67

    public class Calculator
    {
        public int Add(int a, int b) => a + b;
        public string Mode { get; set; }
        public event EventHandler PoweringUp;
        public static float Square(float f) => f * f;
        public int Multiply(int a, int b) => a * b;
    }

#   pragma warning restore 67
}
