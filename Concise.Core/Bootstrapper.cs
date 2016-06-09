using Concise.Core.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concise.Core
{
    public static class Bootstrapper
    {
        public static IContainer Register()
        {
            var container = new ConciseContainer();
            Register(container);
            return container;
        }

        public static void RegisterWithSimpleInjector(object simpleInjectorContainer)
        {
            Guard.AgainstNull(simpleInjectorContainer, nameof(simpleInjectorContainer));
        }

        private static void Register(IContainer container)
        {

            Bootstrapper.InternalLocator = container.Provider;
        }

        /// <summary>
        /// Internal property for use by TestBase classes only
        /// </summary>
        internal static IServiceProvider InternalLocator { get; private set; }
    }
}
