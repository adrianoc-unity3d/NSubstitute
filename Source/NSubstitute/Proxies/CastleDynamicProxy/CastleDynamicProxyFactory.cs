using System;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using Castle.DynamicProxy;
using Castle.DynamicProxy.Generators;
using NSubstitute.Core;
using NSubstitute.Exceptions;

namespace NSubstitute.Proxies.CastleDynamicProxy
{
    public class CastleDynamicProxyFactory : IProxyFactory
    {
        readonly ProxyGenerator _proxyGenerator;
        readonly AllMethodsExceptCallRouterCallsHook _allMethodsExceptCallRouterCallsHook;

        public CastleDynamicProxyFactory()
        {
            ConfigureDynamicProxyToAvoidReplicatingProblematicAttributes();

            _proxyGenerator = new ProxyGenerator();
            _allMethodsExceptCallRouterCallsHook = new AllMethodsExceptCallRouterCallsHook();
        }

        public object GenerateProxy(ICallRouter callRouter, Type typeToProxy, Type[] additionalInterfaces, object[] constructorArguments)
        {
            VerifyClassHasNotBeenPassedAsAnAdditionalInterface(additionalInterfaces);

            var interceptor = new CastleForwardingInterceptor(new CastleInvocationMapper(), callRouter);
            var proxyGenerationOptions = GetOptionsToMixinCallRouter(callRouter);
            var proxy = CreateProxyUsingCastleProxyGenerator(typeToProxy, additionalInterfaces, constructorArguments, interceptor, proxyGenerationOptions);
            interceptor.StartIntercepting();
            return proxy;
        }

        private object CreateProxyUsingCastleProxyGenerator(Type typeToProxy, Type[] additionalInterfaces,
                                                            object[] constructorArguments,
                                                            IInterceptor interceptor,
                                                            ProxyGenerationOptions proxyGenerationOptions)
        {
            if (typeToProxy.IsInterface)
            {
                VerifyNoConstructorArgumentsGivenForInterface(constructorArguments);
                return _proxyGenerator.CreateInterfaceProxyWithoutTarget(typeToProxy, additionalInterfaces, proxyGenerationOptions, interceptor);
            }

            if (typeToProxy.IsGenericType && typeToProxy.GetGenericTypeDefinition() == typeof(Substitute.StaticProxy<>))
            {
                if (additionalInterfaces.Any())
                    throw new SubstituteException("Can not substitute interfaces as static");
                if (constructorArguments.Any())
                    throw new SubstituteException("Constructor arguments make no sense for statics");

                // extract the T from StaticProxy<T>
                var actualType = typeToProxy.GetGenericArguments()[0];

                // find our hook and set it
                var staticMocker = CastlePatchedInterceptorRegistry.GetStaticMocker(actualType);
                if (staticMocker == null)
                    throw new SubstituteException("Can not substitute statics for non-patched types");
                staticMocker(interceptor);

                // callers will need an actual object in order to chain further arranging
                return Activator.CreateInstance(typeToProxy);
            }

            var instanceMocker = CastlePatchedInterceptorRegistry.GetInstanceMocker(typeToProxy);

            // requests for additional interfaces cannot be done via patching (not per-instance anyway)
            if (additionalInterfaces.Any() || instanceMocker == null)
            {
                return _proxyGenerator.CreateClassProxy(typeToProxy, additionalInterfaces, proxyGenerationOptions, constructorArguments, interceptor);
            }

            var newInstance = Activator.CreateInstance(typeToProxy, constructorArguments);
            instanceMocker(newInstance, interceptor);
            return newInstance;
        }

        private ProxyGenerationOptions GetOptionsToMixinCallRouter(ICallRouter callRouter)
        {
            var options = new ProxyGenerationOptions(_allMethodsExceptCallRouterCallsHook);
            options.AddMixinInstance(callRouter);
            return options;
        }

        private class AllMethodsExceptCallRouterCallsHook : AllMethodsHook
        {
            public override bool ShouldInterceptMethod(Type type, MethodInfo methodInfo)
            {
                return IsNotCallRouterMethod(methodInfo)
                    && IsNotBaseObjectMethod(methodInfo)
                    && base.ShouldInterceptMethod(type, methodInfo);
            }

            private static bool IsNotCallRouterMethod(MethodInfo methodInfo)
            {
                return methodInfo.DeclaringType != typeof(ICallRouter);
            }

            private static bool IsNotBaseObjectMethod(MethodInfo methodInfo)
            {
                return methodInfo.GetBaseDefinition().DeclaringType != typeof (object);
            }
        }

        private void VerifyNoConstructorArgumentsGivenForInterface(object[] constructorArguments)
        {
            if (constructorArguments != null && constructorArguments.Length > 0)
            {
                throw new SubstituteException("Can not provide constructor arguments when substituting for an interface.");
            }
        }

        private void VerifyClassHasNotBeenPassedAsAnAdditionalInterface(Type[] additionalInterfaces)
        {
            if (additionalInterfaces != null && additionalInterfaces.Any(x => x.IsClass))
            {
                throw new SubstituteException("Can not substitute for multiple classes. To substitute for multiple types only one type can be a concrete class; other types can only be interfaces.");
            }
        }

        private static void ConfigureDynamicProxyToAvoidReplicatingProblematicAttributes()
        {
#pragma warning disable 618
            AttributesToAvoidReplicating.Add<SecurityPermissionAttribute>();
#pragma warning restore 618

            AttributesToAvoidReplicating.Add<System.ServiceModel.ServiceContractAttribute>();

            AttributesToAvoidReplicating.Add<ReflectionPermissionAttribute>();
            AttributesToAvoidReplicating.Add<PermissionSetAttribute>();
            AttributesToAvoidReplicating.Add<System.Runtime.InteropServices.MarshalAsAttribute>();
#if (NET4 || NET45)
            AttributesToAvoidReplicating.Add<System.Runtime.InteropServices.TypeIdentifierAttribute>();
#endif
            AttributesToAvoidReplicating.Add<UIPermissionAttribute>();
        }
    }
}