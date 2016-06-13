using System;
using Castle.DynamicProxy;
using NSubstitute.Core;

namespace NSubstitute.Proxies.CastleDynamicProxy
{
    public class CastleInvocationMapper
    {
        readonly static CallFactory CallFactory = new CallFactory();

        public virtual ICall Map(IInvocation castleInvocation)
        {
            Func<object> baseMethod = null;
            if (castleInvocation.InvocationTarget != null &&
                /*castleInvocation.MethodInvocationTarget.IsVirtual &&*/        // $$$ HACKED - this was disabled to permit NSubstitute to patch concretes and statics - do it better
                !castleInvocation.MethodInvocationTarget.IsAbstract /*&&
                !castleInvocation.MethodInvocationTarget.IsFinal*/)
            {
                Func<object> baseResult = () => { castleInvocation.Proceed(); return castleInvocation.ReturnValue; };
                var result = new Lazy<object>(baseResult);
                baseMethod = () => result.Value;
            }

            return CallFactory.Create(castleInvocation.Method, castleInvocation.Arguments, castleInvocation.Proxy, baseMethod);
        }
    }
}
