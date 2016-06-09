using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concise.Core.IoC
{
    public interface IContainer
    {
        IServiceProvider Provider { get; }

        void RegisterSingleton<TService, TImplementation>();

        void RegisterTransient<TService, TImplementation>();
    }
}
