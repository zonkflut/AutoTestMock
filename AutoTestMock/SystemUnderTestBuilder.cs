using System;
using System.Linq;
using System.Reflection;
using Castle.Core;
using Castle.MicroKernel;
using Castle.MicroKernel.Context;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Rhino.Mocks;

namespace AutoTestMock
{
    /// <summary>
    /// Implements an Auto-mocking Container for test fixtures.
    /// </summary>
    /// <remarks>
    /// With Thanks to Mark Seemann http://blog.ploeh.dk/2013/03/11/auto-mocking-container/.
    /// </remarks>
    public class SystemUnderTestBuilder : ITestObjectBuilder
    {
        private IWindsorContainer container;
        
        public ITestObjectBuilder FromAssemblyContaining<T>()
        {
            container = new WindsorContainer().Install(new AssemblyInstaller(typeof(T).Assembly));
            return this;
        }

        public ITestObjectBuilder FromAssemblies(params Assembly[] assemblies)
        {
            container = new WindsorContainer().Install(new AssemblyInstaller(assemblies));
            return this;
        }

        public ITestObjectBuilder FromAssembliesContainingTypes(params Type[] types)
        {
            container = new WindsorContainer().Install(new AssemblyInstaller(types.Select(t => t.Assembly).ToArray()));
            return this;
        }

        T ITestObjectBuilder.CreateSUT<T>()
        {
            return container.Resolve<T>();
        }

        T ITestObjectBuilder.Dependancy<T>()
        {
            return container.Resolve<T>();
        }

        private class AssemblyInstaller : IWindsorInstaller
        {
            private readonly Assembly[] assemblies;

            internal AssemblyInstaller(params Assembly[] assemblies)
            {
                this.assemblies = assemblies;
            }

            public void Install(IWindsorContainer container, IConfigurationStore store)
            {
                container.Kernel.Resolver.AddSubResolver(new RhinoMocksResolver(container.Kernel));
                foreach (var assembly in assemblies)
                {
                    container.Register(Classes.FromAssembly(assembly)
                    .Pick()
                    .WithServiceSelf()
                    .LifestyleTransient());
                }
            }
        }

        private class RhinoMocksResolver : ISubDependencyResolver
        {
            private readonly IKernel kernel;

            public RhinoMocksResolver(IKernel kernel)
            {
                this.kernel = kernel;
            }

            public bool CanResolve(
                CreationContext context,
                ISubDependencyResolver contextHandlerResolver,
                ComponentModel model,
                DependencyModel dependency)
            {
                return dependency.TargetType.IsInterface;
            }

            public object Resolve(
                CreationContext context,
                ISubDependencyResolver contextHandlerResolver,
                ComponentModel model,
                DependencyModel dependency)
            {
                var obj = MockRepository.GenerateMock(dependency.TargetType, new Type[0]);
                this.kernel.Register(Component.For(dependency.TargetType)
                    .Instance(obj)
                    .LifestyleTransient());
                return obj;
            }
        }
    }
}
