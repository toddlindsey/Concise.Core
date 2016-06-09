using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concise.Core
{
    /// <summary>
    /// When used with <see cref="PropertyInjectionBehavior"/>, properties decorated with this attribute
    /// will be automatically injected from the container.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class InjectAttribute : Attribute
    { }
}
