using System;

namespace NSubstitute.Weaver.Tests.Hackweek
{
    public class OutParam
    {
        public int Foo(int j, out int i)
        {
            i = 123;
            return 456;
        }
    }
}
