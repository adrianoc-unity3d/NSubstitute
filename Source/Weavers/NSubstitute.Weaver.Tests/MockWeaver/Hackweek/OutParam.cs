using System;

namespace NSubstitute.Weavers.Tests.Hackweek
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
