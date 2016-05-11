using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Castle.DynamicProxy;

namespace NSubstitute.Proxies.CastleDynamicProxy
{
    public class CastlePatchedInterceptorRegistry
    {
        public static Action<object, IInterceptor> GetInstanceMocker(Type forType)
        {
            var field = GetOrAddTypeFieldCache(forType).InstanceInterceptor;
            if (field == null)
                return null;

            return (instance, interceptor) => field.SetValue(instance, interceptor);
        }

        public static Action<IInterceptor> GetStaticMocker(Type forType)
        {
            var field = GetOrAddTypeFieldCache(forType).StaticInterceptor;
            if (field == null)
                return null;

            return interceptor => field.SetValue(null, interceptor);
        }

    	[MethodImpl(MethodImplOptions.NoInlining)]
        public static object CallMockMethodOrImpl(object mockedInstance, MethodInfo originalMethod, params object[] callArgs)
        {
    		var caller = new StackTrace(1).GetFrame(0).GetMethod();
    	    return CallMockMethodOrImpl(mockedInstance, (MethodInfo)caller, originalMethod, callArgs);
        }

        public static object CallMockMethodOrImpl(object mockedInstance, MethodInfo mockMethod, MethodInfo originalMethod, params object[] callArgs)
        {
            TypeFieldCache typeFieldCache;
            if (s_TypeFieldCache.TryGetValue(mockedInstance.GetType(), out typeFieldCache))
            {
                var interceptor = (IInterceptor)typeFieldCache.InstanceInterceptor.GetValue(mockedInstance);
                if (interceptor != null)
                    return CallInterceptor(interceptor, mockedInstance, mockMethod, originalMethod, callArgs);
            }

            return originalMethod.Invoke(mockedInstance, callArgs);
        }

    	[MethodImpl(MethodImplOptions.NoInlining)]
        public static object CallMockStaticMethodOrImpl(MethodInfo originalMethod, params object[] callArgs)
        {
    		var caller = new StackTrace(1).GetFrame(0).GetMethod();
    	    return CallMockMethodOrImpl((MethodInfo)caller, originalMethod, callArgs);
        }

        public static object CallMockStaticMethodOrImpl(MethodInfo mockMethod, MethodInfo originalMethod, params object[] callArgs)
        {
            TypeFieldCache typeFieldCache;
            if (s_TypeFieldCache.TryGetValue(originalMethod.DeclaringType, out typeFieldCache))
            {
                var interceptor = (IInterceptor)typeFieldCache.StaticInterceptor.GetValue(null);
                if (interceptor != null)
                    return CallInterceptor(interceptor, null, mockMethod, originalMethod, callArgs);
            }

            return originalMethod.Invoke(null, callArgs);
        }

        static object CallInterceptor(IInterceptor interceptor, object mockedInstance, MethodInfo mockMethod, MethodInfo originalMethod, object[] callArgs)
        {
            var invocation = new Invocation
            {
                Arguments = callArgs,
                Method = mockMethod,
                MethodInvocationTarget = originalMethod,
                Proxy = mockedInstance,
            };

            interceptor.Intercept(invocation);

            Array.Copy(invocation.Arguments, callArgs, callArgs.Length);
            return invocation.ReturnValue;
        }

        class Invocation : IInvocation
        {
            // NSubstitute does not currently require these
            public Type[] GenericArguments { get { throw new NotImplementedException(); } }
            public Type TargetType { get { throw new NotImplementedException(); } }
            public object GetArgumentValue(int index) { throw new NotImplementedException(); }
            public MethodInfo GetConcreteMethod() { throw new NotImplementedException(); }
            public MethodInfo GetConcreteMethodInvocationTarget() { throw new NotImplementedException(); }
            public void SetArgumentValue(int index, object value) { throw new NotImplementedException(); }

            // minimum config required for nsub
            public object[] Arguments { get; set; }
            public MethodInfo Method { get; set; }
            public MethodInfo MethodInvocationTarget { get; set; }
            public object Proxy { get; set; }
            public object ReturnValue { get; set; }

            public object InvocationTarget => Proxy;

            public void Proceed()
            {
                ReturnValue = MethodInvocationTarget.Invoke(InvocationTarget, Arguments);
            }
        }

        class TypeFieldCache
        {
            public TypeFieldCache(Type type)
            {
                InstanceInterceptor = type.GetField("__mockInterceptor", BindingFlags.Instance | BindingFlags.NonPublic);
                StaticInterceptor = type.GetField("__mockStaticInterceptor", BindingFlags.Static | BindingFlags.NonPublic);
            }

            public readonly FieldInfo InstanceInterceptor, StaticInterceptor;
        }

        static TypeFieldCache GetOrAddTypeFieldCache(Type type)
        {
            TypeFieldCache typeFieldCache;
            if (s_TypeFieldCache.TryGetValue(type, out typeFieldCache))
                return typeFieldCache;

            typeFieldCache = new TypeFieldCache(type);
            s_TypeFieldCache.Add(type, typeFieldCache);
            return typeFieldCache;
        }

        static Dictionary<Type, TypeFieldCache> s_TypeFieldCache = new Dictionary<Type, TypeFieldCache>();
    }
}
