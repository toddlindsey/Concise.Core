using FluentAssertions.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concise.Core.FluentAssertions
{
    public class FailWithContextStrategy : IAssertionStrategy
    {
        public IEnumerable<string> DiscardFailures()
        {
            return new string[] { };
        }

        public IEnumerable<string> FailureMessages
        {
            get { return new string[] { }; }
        }

        public void HandleFailure(string message)
        {
            Fail.With(message);
        }

        public void ThrowIfAny(IDictionary<string, object> context)
        {
        }
    }
}
