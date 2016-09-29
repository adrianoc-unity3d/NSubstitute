using System;

namespace NSubstitute.Weaver.Tests.Hackweek
{
    public class AdvancedMatching
    {
        public string MakeString<T>(T obj) => obj?.ToString();
        public string MakeString<T1, T2>(T1 obj1, T2 obj2) => ((object)obj1 ?? "").ToString() + ((object)obj2 ?? "");
        public TReturn ReturnThingy<T1, T2, TReturn>(T1 obj1, T2 obj2, TReturn rc) => rc;
        public string AddNumbers(int i, float f) => (i + f).ToString();
        public string AddNumbers(int i, float f, double g) => (i + f + g).ToString();
    }

    public class AdvancedMatching<T1, T2>
    {
        public string MakeString<T3>(T3 obj) => typeof(T1).Name + typeof(T2).Name + obj;
    }
}
