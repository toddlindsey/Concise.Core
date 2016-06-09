using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concise.Core
{
    public static class ConciseFactory
    {
        internal static IServiceProvider Provider;

        public static TService Get<TService>() where TService : class
        {
            return null as TService;
        }
    }
}
