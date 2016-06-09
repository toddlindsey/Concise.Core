using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concise.Core.TestFramework
{
    public interface ITestFrameworkAdapter
    {
        bool IsAssertionException(Exception ex);

        Exception CreateAssertionException(string message);

        Exception CreateInconclusiveException(string message);
    }
}
