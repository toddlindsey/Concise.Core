using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concise.Core.Steps
{
    /// <summary>
    /// Failure options for performance failures.  Configured via <see cref="IStepTestContext"/>.
    /// </summary>
    public enum StepPerformanceFailureAction
    {
        /// <summary>
        /// Fail the test on performance failures
        /// </summary>
        FailTest,

        /// <summary>
        /// Flag the test result as "inconclusive" on performance failures
        /// </summary>
        AssertInconclusive
    }
}
