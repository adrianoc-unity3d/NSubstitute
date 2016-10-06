using System;
using System.Runtime.CompilerServices;

namespace NSubstitute.Weaver.Tests
{
    public class ClassWithNoDefaultCtor
    {
        public ClassWithNoDefaultCtor(string i) { }
        public ClassWithNoDefaultCtor(string i1, string i2) { }
    }

    public class ClassWithCtorICall
    {
        public ClassWithCtorICall()
	    {
		    DoICall();
	    }
    
        [MethodImpl((MethodImplOptions)0x1000)]
        static extern void DoICall();
    }

    public class ClassWithCtorThrow
    {
        public ClassWithCtorThrow()
	    {
		    throw new InvalidOperationException();
	    }
    }
}
