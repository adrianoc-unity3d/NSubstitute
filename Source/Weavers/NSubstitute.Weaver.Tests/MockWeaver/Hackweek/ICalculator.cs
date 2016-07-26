using System;

namespace NSubstitute.Weavers.Tests.Hackweek
{
    public interface ICalculator
    {
        int Add(int a, int b);
        string Mode { get; set; }
        event EventHandler PoweringUp;
        int Multiply(int a, int b);

        string MakeString<T>(T obj);
        string MakeString2<T>();
    }
}
