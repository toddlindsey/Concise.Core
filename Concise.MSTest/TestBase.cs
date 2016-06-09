using Concise.Core;
using Concise.Core.Steps;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Concise.MSTest
{
    /// <summary>
    /// A recommended base class for all MSTests.  This class enables scenarios otherwise lacking in MSTest:  
    /// 1) Property Injection.  Just decorate properties with [Inject] as you would in other types.
    /// 2) Class-level initialization and cleanup support on *instance* methods instead of static methods, enabling access to injected behaviors.
    /// 3) Enables use of the BDD test syntax (refer to BddTestContextTests for examples)
    /// 4) Enables use of <see cref="ICurrentTestOperations"/>
    /// </summary>
    public abstract class TestBase<TClass> where TClass : TestBase<TClass>
    {
        private static object classInitLock = new object();
        protected static TestBase<TClass> cleanupClass = null;

        [Inject]
        protected ITestStepContext TestStepContext { get; set; }

        public TestBase()
        {
            if (Concise.Core.Bootstrapper.InternalLocator == null)
                throw new InvalidOperationException("The Pat.QA assembly has not been bootstrapped");

            // Perform injection on all [Inject] properties on this class
            this.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(property => property.GetCustomAttribute<InjectAttribute>() != null)
                .ToList()
                .ForEach(property => property.SetValue(this, Concise.Core.Bootstrapper.InternalLocator.GetService(property.PropertyType)));
        }

        /// <summary>
        /// Test initializer
        /// </summary>
        [TestInitialize]
        public virtual void TestInitialize()
        {
            lock (classInitLock)
            {
                if (cleanupClass == null)
                {
                    this.ClassInitialize();
                    cleanupClass = this;
                }
            }
        }

        /// <summary>
        /// Reserved for future use.
        /// </summary>
        [TestCleanup]
        public virtual void TestCleanup()
        {
            this.TestStepContext.Dispose();
            this.TestStepContext = null;
        }

        /// <summary>
        /// Test classes can override this method to provide a class initializer with access to instance/injected types.
        /// </summary>
        public virtual void ClassInitialize()
        {
        }

        /// <summary>
        /// Test classes can override this method to provide a class cleanup routine with access to instance/injected types.
        /// **NOTE: The derived (non-abstract) test class MUST ALSO create a static cleanup method as follows (this method cannot exist in a base class).
        ///         This is required by how MSTest works, and hopefully very few class cleanup routines will be required.
        ///
        ///         [ClassCleanup]
        ///         public static void StaticClassCleanup()
        ///         {
        ///            if (cleanupClass != null)
        ///                cleanupClass.ClassCleanup();
        ///         }
        ///
        /// </summary>
        public virtual void ClassCleanup()
        { }

        private TestContext testContextInstance;

        /// <summary>
        /// Gets or sets the test context which provides information about and functionality for the current test run.
        /// </summary>
        public virtual TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
    }
}
