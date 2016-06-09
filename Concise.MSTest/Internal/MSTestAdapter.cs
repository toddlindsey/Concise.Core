using Concise.Core;
using Concise.Core.TestFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Concise.MSTest.Internal
{
    internal class MSTestAdapter : ITestFrameworkAdapter
    {
        public bool IsAssertionException(Exception ex)
        {
            Guard.AgainstNull(ex, nameof(ex));
            return ex is UnitTestAssertException;
        }

        public Exception CreateAssertionException(string message)
        {
            Guard.AgainstNullOrEmpty(message, nameof(message));
            return new AssertFailedException(AssertContext.CurrentFullContext + message);
        }

        public Exception CreateInconclusiveException(string message)
        {
            Guard.AgainstNullOrEmpty(message, nameof(message));
            return new AssertInconclusiveException(AssertContext.CurrentFullContext + message);
        }
    }
}
