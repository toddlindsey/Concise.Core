using Concise.Core.Performance;
using Concise.Core.CommonExtensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Concise.Core.TestFramework;

namespace Concise.Core.Steps
{
    /// <summary>
    /// Interface for adding and executing BDD topSteps.
    /// Consumers should instantiate an instance of this class during test initialization, and call <see cref="Dispose"/>
    /// on test cleanup, but should not actually use this interface, instead relying on <see cref="BddStringExtensions"/>
    /// </summary>
    public interface ITestStepContext : IDisposable
    {
        /// <summary>
        /// Get or set the action to take when a step takes longer than the designated "maximum duration"
        /// Default is <see cref="StepPerformanceFailureAction.FailTest"/>
        /// </summary>
        StepPerformanceFailureAction PerformanceFailAction { get; set; }

        /// <summary>
        /// Track and immediately execute the specified <see cref="TestStep"/>.
        /// </summary>
        void AddAndExecute(TestStep step);
    }

    /// <inheritdoc/>
    public class TestStepContext : ITestStepContext
    {
        private List<TestStep> topSteps = new List<TestStep>();
        private bool stepResultsReported = false;

        private const string CallContextKey = "TestStepContext";
        private const string CurrentStepKey = "BddCurrentStep";

        private ITestFrameworkAdapter adapter;

        public TestStepContext()
        {
            // Oh good grief - it turns out SimpleInjector.Verify() creates an instance of *every* registration,
            // yet fails to call Dispose() on IDisposables -- quite annoying.  I don't want to completely get rid of the Verify(), 
            // so we'll just allow this constructor to reset the TestStepContext instead of throwing an exception
            // if( TestStepContext.Current != null)
            //    throw new InvalidOperationException("Invalid attempt to create a new TestStepContext when one was already established on this call context.  (did you fail to Dispose() the last one?)");

            TestStepContext.Current = this;
            this.PerformanceFailAction = StepPerformanceFailureAction.FailTest;

            this.adapter = (ITestFrameworkAdapter) Bootstrapper.InternalLocator.GetService(typeof(ITestFrameworkAdapter));
        }

        /// <summary>
        /// Return the current/active test context
        /// </summary>
        public static TestStepContext Current
        {
            get { return CallContext.GetData(CallContextKey) as TestStepContext; }
            private set { CallContext.SetData(CallContextKey, value); }
        }

        /// <summary>
        /// Get or set the currently running BDD Step
        /// </summary>
        public static TestStep CurrentStep
        {
            get { return CallContext.GetData(CurrentStepKey) as TestStep; }
            set { CallContext.SetData(CurrentStepKey, value); }
        }

        /// <inheritdoc/>
        public StepPerformanceFailureAction PerformanceFailAction { get; set; }

        /// <inheritdoc/>
        public void AddAndExecute(TestStep step)
        {
            TestStep parentStep = TestStepContext.CurrentStep;

            List<TestStep> stepList = parentStep == null ? this.topSteps : parentStep.Children;
            lock (stepList)
                stepList.Add(step);

            TimeSpan duration = TimeSpan.Zero;
            try
            {
                TestStepContext.CurrentStep = step;
                Collect.TimeOf(step.Action, out duration);
                step.Duration = duration;
                step.FunctionalPassed = true;
            }
            catch (Exception ex)
            {
                step.FunctionalPassed = false;
                step.Exception = ex;
                step.Duration = duration;
                if (step.FailFast)
                {
                    // If we are in a continue on fail context on an *ancestor* BDD step, we do NOT want to 
                    // render the BDD steps at this time - just throw the exception
                    // Also true if there are any parent steps
                    if (step.InContinueOnFailContext || step.Parent != null)
                        throw;
                    else
                        this.RenderStepResultsAndFail();
                }
            }
            finally
            {
                TestStepContext.CurrentStep = parentStep;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            CallContext.FreeNamedDataSlot(CallContextKey);

            lock (this.topSteps)
            {
                if (this.topSteps.Any() && !stepResultsReported)
                {
                    string output = this.ComposeTestResults();

                    // If *any* step in the given tree failed functionally, return false
                    Func<TestStep, bool> stepTreePassed = null;
                    stepTreePassed = (TestStep step) =>
                    {
                        return step.FunctionalPassed && step.Children.All(c => stepTreePassed(c));
                    };

                    // If *any* step in the tree failed on performance, return false
                    Func<TestStep, bool> stepTreePerformancePassed = null;
                    stepTreePerformancePassed = (TestStep step) =>
                    {
                        return step.PerformancePassed && step.Children.All(c => stepTreePerformancePassed(c));
                    };

                    bool passedFuntionally = this.topSteps.All(step => stepTreePassed(step));

                    if (passedFuntionally)
                    {
                        Console.WriteLine(output);

                        bool performancePassed = this.topSteps.All(step => stepTreePerformancePassed(step));
                        if (!performancePassed)
                        {
                            if (this.PerformanceFailAction == StepPerformanceFailureAction.FailTest)
                                Fail.With(output);
                            else if (this.PerformanceFailAction == StepPerformanceFailureAction.AssertInconclusive)
                                Fail.Inconclusive(output);
                            else
                                throw new InvalidOperationException("Unrecognized action: " + this.PerformanceFailAction.ToString());
                        }

                    }
                    else
                        Fail.With(output);
                }
            }
        }

        /// <summary>
        /// Return a string of the test results across ALL topSteps
        /// </summary>
        protected string ComposeTestResults()
        {
            var builder = new StringBuilder();
            builder.AppendLine();
            builder.AppendLine();
            this.RenderStepResults(builder, this.topSteps, 0);
            return builder.ToString();
        }

        /// <summary>
        /// Render all provided steps, and all of their children (recursively)
        /// </summary>
        /// <param name="builder">String builder to write output to</param>
        /// <param name="steps">The steps to render</param>
        /// <param name="level">The current step level (starting with 0, incrementing for recursive calls)</param>
        private void RenderStepResults(StringBuilder builder, IEnumerable<TestStep> steps, int level)
        {
            foreach (var step in steps)
            {
                builder.AppendLine(
                    (step.FunctionalPassed ? (step.PerformancePassed ? "PASS" : "PERF") : "FAIL") + ">  ".Repeat(level + 1) +
                    step.Description +
                    " (" +
                    step.Duration.TotalSeconds.ToString("F3", CultureInfo.InvariantCulture) + "s" +
                    ")");

                // If there is an exception that isn't already reported by a child step, report it
                if (step.Exception != null && !step.Children.Any(child => child.Exception == step.Exception))
                {
                    builder.AppendLine();

                    Exception ex = step.Exception;

                    // TargetInvocationExceptions are just noise, just render the inner exception.
                    if (ex is TargetInvocationException)
                        ex = ex.InnerException;

                    if (adapter.IsAssertionException(ex))
                    {
                        builder.AppendLine(ex.Message);
                        builder.AppendLine();
                        builder.AppendLine(ex.StackTrace.ToString());
                    }
                    else
                        builder.AppendLine(ex.ToString());

                    builder.AppendLine();
                }

                if (step.Children.Any())
                    this.RenderStepResults(builder, step.Children, level + 1);
            }
        }

        /// <summary>
        /// Will call <see cref="ComposeTestResults"/> to compose the test results, and will then 
        /// throw an <see cref="AssertFailedException"/> (MSTest assertion failure) with those results.
        /// </summary>
        protected void RenderStepResultsAndFail()
        {
            string output = this.ComposeTestResults();
            stepResultsReported = true;
            Fail.With(output);
        }
    }
}
