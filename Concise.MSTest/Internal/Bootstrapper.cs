using Concise.Core.IoC;
using Concise.Core.TestFramework;
using Concise.MSTest.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concise.Core.MSTest.Internal
{
    /// <summary>
    /// Internal bootstrapper for Concise.MSTest
    /// </summary>
    internal static class Bootstrapper
    {
        public static void Register(IContainer container)
        {
            container.RegisterTransient<ITestFrameworkAdapter, MSTestAdapter>();
        }
    }
}
