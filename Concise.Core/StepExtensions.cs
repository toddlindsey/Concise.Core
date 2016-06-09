using Concise.Core.Steps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concise.Core
{
    /// <summary>
    /// Extension methods to <see cref="string"/> for the purpose of defining BDD steps
    /// </summary>
    public static class BddStringExtensions
    {
        private const string NoContextMessage =
            "Invalid attempt to define a BDD step without first creating a TestStepContext.";

        /// <summary>
        /// Same as <see cref="x(string,Action,TimeSpan)"/>, but where max duration is <see cref="TimeSpan.MaxValue"/>.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "x", Justification = "Lowercase to not compete with the BDD step string itself")]
        public static void x(this string stepDescription, Action action)
        {
            stepDescription.x(action, TimeSpan.MaxValue);
        }

        /// <summary>
        /// Create a fail-fast step definition, meaning a failure in this step will immediately fail the entire test.
        /// </summary>
        /// <param name="stepDescription">The plain-english description of this step</param>
        /// <param name="action">The action to perform</param>
        public static void x(this string stepDescription, Action action, TimeSpan maxDuration)
        {
            if (TestStepContext.Current == null)
                throw new InvalidOperationException(NoContextMessage);

            var step = new TestStep(stepDescription, action, TestStepContext.CurrentStep, maxDuration, true);
            TestStepContext.Current.AddAndExecute(step);
        }

        /// <summary>
        /// Create a step definition that, if it fails, will still allow the next defined step to execute.
        /// </summary>
        /// <param name="stepDescription">The plain-english description of this step</param>
        /// <param name="action">The action to perform</param>
        public static void continueOnFail(this string stepDescription, Action action, TimeSpan maxDuration)
        {
            // If there is no current BddTestContext, just create one that will never be disposed.
            // This allows code to use step definitions even if there is no active test running with a BddTestContext at that time.
            // NOTE: the BddTestContext constructor will register the object with CallContext so it will not be garbage collected
            // unless Dispose() is explicitly called.
            if (TestStepContext.Current == null)
                new TestStepContext();

            var step = new TestStep(stepDescription, action, TestStepContext.CurrentStep, maxDuration, false);
            TestStepContext.Current.AddAndExecute(step);
        }

        /// <summary>
        /// Same as <see cref="continueOnFail(string,Action,TimeSpan)"/>, but where max duration is <see cref="TimeSpan.MaxValue"/>.
        /// </summary>
        public static void continueOnFail(this string stepDescription, Action action)
        {
            stepDescription.continueOnFail(action, TimeSpan.MaxValue);
        }
    }
}
