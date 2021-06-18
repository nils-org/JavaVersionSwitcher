using System;
using System.Linq;
using SimpleInjector;
using Spectre.Console.Cli;

namespace JavaVersionSwitcher
{
    /// <summary>
    /// Shallow wrapper around SimpleInjector
    /// </summary>
    internal class SimpleInjectorRegistrar : ITypeRegistrar
    {
        private readonly Container _container;
        private readonly Type[] _knownMultiRegistrations;

        public SimpleInjectorRegistrar(Container container)
        {
            _container = container;
            _knownMultiRegistrations = new[]
            {
                typeof(ICommand),
                typeof(ICommand<>)
            };
        }

        public void Register(Type service, Type implementation)
        {
            if (_knownMultiRegistrations.Contains(service))
            {
                _container.Collection.Append(service, implementation, Lifestyle.Singleton);
                return;
            }
            
            _container.Register(service, implementation, Lifestyle.Singleton);
        }

        public void RegisterInstance(Type service, object implementation)
        {
            _container.RegisterInstance(service, implementation);
        }

        public void RegisterLazy(Type service, Func<object> factory)
        {
            _container.Register(service, factory, Lifestyle.Singleton);
        }

        public ITypeResolver Build()
        {
            _container.Verify();
            return new SimpleInjectorTypeResolver(_container);
        }

        private class SimpleInjectorTypeResolver : ITypeResolver
        {
            private readonly Container _container;

            public SimpleInjectorTypeResolver(Container container)
            {
                _container = container;
            }

            public object Resolve(Type type)
            {
                if (type == null)
                {
                    throw new ArgumentNullException(nameof(type));
                }

                return _container.GetInstance(type);
            }
        }
    }
}