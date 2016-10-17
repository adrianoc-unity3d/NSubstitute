using System;
using System.Runtime.CompilerServices;

// ReSharper disable MemberInitializerValueIgnored
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedParameter.Local

namespace NSubstitute.Weaver.Tests
{
    class ClassWithDefaultCtor
    {
        public ClassWithDefaultCtor()
        {
            Value = 123;
        }

        public int Value = 234;

        void Dummy() {}
    }

    class ClassWithNoDefaultCtor
    {
        public ClassWithNoDefaultCtor(string i) {}
        public ClassWithNoDefaultCtor(string i1, string i2) {}

        void Dummy() {}
    }

    class ClassWithNoDefaultCtorNoMethods
    {
        public ClassWithNoDefaultCtorNoMethods(string i) {}
        public ClassWithNoDefaultCtorNoMethods(string i1, string i2) {}
    }

    class ClassWithCtorICall
    {
        public ClassWithCtorICall()
        {
            DoICall();
        }

        [MethodImpl((MethodImplOptions) 0x1000)]
        static extern void DoICall();
    }

    class ClassWithCtorThrow
    {
        public ClassWithCtorThrow()
        {
            throw new InvalidOperationException();
        }

        void Dummy() {}
    }

    class EmptyClass
    {
    }
}
