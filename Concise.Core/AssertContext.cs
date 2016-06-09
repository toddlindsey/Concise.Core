using Concise.Core.FluentAssertions;
using Concise.Core.CommonExtensions;
using FluentAssertions.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Concise.Core
{
    /// <summary>
    /// Defines a scope in which, if an assertion is done using the FluentAssertions framework,
    /// the assertion will be reported as occuring within a named scope - and the scopes can exist in a hierarchy.
    /// </summary>
    /// <remarks>
    /// For example, this code:
    /// 
    /// using(new AssertContext("accounting module"))
    /// {
    ///     using(new AssertContext("profit and loss statement")
    ///     {
    ///         90.00M.Should().Be(100.00M, "that is total of the transactions entered");
    ///     }
    /// }
    /// 
    /// Will result in the following assertion:
    /// "Within account module > profit and loss statement: 
    /// 90.00 was found but expected to be 100.00, because that is the total of the transactions entered"
    /// </remarks>
    public class AssertContext : IDisposable
    {
        private string name;
        private AssertContext parent;
        private AssertionScope scope;
        private const string ContextKey = "TestContextKey";

        /// <summary>
        /// Starts a new assertion context.  Always place inside a using block.
        /// </summary>
        /// <param name="name">Plain-english description of this context</param>
        public AssertContext(string name)
        {
            this.name = name;
            this.parent = CallContext.GetData(ContextKey) as AssertContext;
            CallContext.SetData(ContextKey, this);

            this.scope = new AssertionScope(name);

            // TEMPORARY approach for the POC.  We may need to request/submit a change on GitHub to get around this.
            typeof(AssertionScope)
                .GetField("assertionStrategy", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(this.scope, new FailWithContextStrategy());
        }

        /// <summary>
        /// Return the current assertion context
        /// </summary>
        public static AssertContext Current
        {
            get { return CallContext.GetData(ContextKey) as AssertContext; }
        }

        /// <summary>
        /// Return a string containing the current full context (full meaning it includes all parents)
        /// </summary>
        public static string CurrentFullContext
        {
            get { return AssertContext.Current != null ? AssertContext.Current.FullContext : string.Empty; }
        }

        /// <summary>
        /// Plain-english name for this context
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// Return a list of the individual context strings making up the full context, where the 
        /// first item is for the top-most parent.
        /// </summary>
        public IList<string> FullContextList
        {
            get
            {
                IList<string> fullContext;
                if (this.parent != null)
                    fullContext = this.parent.FullContextList;
                else
                    fullContext = new List<string>();

                if (this.name.Exists())
                    fullContext.Add(this.name);

                return fullContext;
            }
        }

        /// <summary>
        /// Return a string containing the full context from this object up (full meaning it includes all parents)
        /// </summary>
        public string FullContext
        {
            get { return "Within " + string.Join("> ", this.FullContextList) + ": "; }
        }

        /// <summary>
        /// The parent <see cref="AssertContext"/>, or null if no parent.
        /// </summary>
        public AssertContext Parent
        {
            get { return this.parent; }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (this.parent != null)
                CallContext.SetData(ContextKey, this.parent);
            else
                CallContext.FreeNamedDataSlot(ContextKey);

            this.scope.Dispose();
            this.scope = null;
        }
    }
}
