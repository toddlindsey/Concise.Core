using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Concise.Core.IoC.SimpleInjector
{
    public class SimpleInjectorAdapter : IContainer
    {
        public static SimpleInjectorAdapter Create(object simpleInjectorContainer)
        {
            Guard.AgainstNull(simpleInjectorContainer, nameof(simpleInjectorContainer));

            return new SimpleInjectorAdapter(simpleInjectorContainer);
        }

        private static readonly Version MinimumAssemblyVersion = Version.Parse("2.6.0.0");
        private Version loadedVersion;
        private bool version3OrHigher;
        private dynamic container;

        private SimpleInjectorAdapter(object simpleInjectorContainer)
        {
            // Ensure a supported version of SimpleInjector is loaded
            Assembly simpleInjectorAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .SingleOrDefault(a => a.GetName().Name == "SimpleInjector");

            if (simpleInjectorAssembly == null)
                throw new ArgumentException("A recognized version of SimpleInjector has not been loaded");

            this.loadedVersion = simpleInjectorAssembly.GetName().Version;

            if (loadedVersion < MinimumAssemblyVersion)
                throw new ArgumentException($"The version of SimpleInjector loaded is {simpleInjectorAssembly.GetName().Version}, but the minimum supported is {MinimumAssemblyVersion}");

            const string expectedTypeName = "SimpleInjector.Container";
            if (simpleInjectorContainer.GetType().FullName != expectedTypeName)
                throw new ArgumentException($"The argument is not of type {expectedTypeName}", nameof(simpleInjectorContainer));

            this.version3OrHigher = this.loadedVersion >= Version.Parse("3.0.0.0");

            this.container = simpleInjectorContainer;
        }

        public IServiceProvider Provider
        {
            get { return (IServiceProvider) this.container; }
        }

        public void RegisterSingleton<TService, TImplementation>()
        {
            if (this.version3OrHigher)
                this.container.RegisterSingleton(typeof(TService), typeof(TImplementation));
            else
                this.container.RegisterSingle(typeof(TService), typeof(TImplementation));
        }

        public void RegisterTransient<TService, TImplementation>()
        {
            this.container.Register(typeof(TService), typeof(TImplementation));
        }
    }
}
