using System;
using SimpleInjector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using Concise.Core.IoC;
using System.Web;
using Concise.Core.IoC.SimpleInjector;

namespace Concise.Core.UnitTests.IoC.SimpleInjector
{
    [TestClass]
    public class SimpleInjectorAdapterTests
    {
        [TestMethod]
        public void SimpleInjectorAdapter_Create_WithValidContainer_Succeeds()
        {
            SimpleInjectorAdapter adapter = null;
            Action act = () => adapter = SimpleInjectorAdapter.Create(new Container());

            act.ShouldNotThrow();
            adapter.Should().NotBeNull();
        }

        [TestMethod]
        public void SimpleInjectorAdapter_Create_WithNonSimpleInjectorObject_Fails()
        {
            SimpleInjectorAdapter adapter = null;
            Action act = () => adapter = SimpleInjectorAdapter.Create(new object());

            act.ShouldThrow<ArgumentException>()
                .And.Message.Should().Contain("not of type");
        }

        private interface IMyInterface { };

        private class MyClass : IMyInterface { };

        [TestMethod]
        public void SimpleInjectorAdapter_RegisterSingleton_Always_Works()
        {
            var container = new Container();
            var adapter = SimpleInjectorAdapter.Create(container);

            adapter.RegisterSingleton<IMyInterface, MyClass>();

            object result1 = container.GetInstance<IMyInterface>();
            result1.Should().NotBeNull();

            object result2 = container.GetInstance<IMyInterface>();
            result2.Should().NotBeNull();

            result1.Should().BeOfType<MyClass>();
            result1.Should().Be(result2, "registraton was for a singleton, not a transient");
        }

        [TestMethod]
        public void SimpleInjectorAdapter_Register_Always_Works()
        {
            var container = new Container();
            var adapter = SimpleInjectorAdapter.Create(container);

            adapter.RegisterTransient<IMyInterface, MyClass>();

            object result1 = container.GetInstance<IMyInterface>();
            result1.Should().NotBeNull();

            object result2 = container.GetInstance<IMyInterface>();
            result2.Should().NotBeNull();

            result1.Should().BeOfType<MyClass>();
            result1.Should().NotBe(result2, "registraton was for a transient, not a singleton");
        }

        [TestMethod]
        public void SimpleInjectorAdapter_Provider_Always_Works()
        {
            var container = new Container();
            var adapter = SimpleInjectorAdapter.Create(container);

            adapter.RegisterSingleton<IMyInterface, MyClass>();

            object result = adapter.Provider.GetService(typeof(IMyInterface));

            result.Should().NotBeNull();
            result.Should().BeOfType<MyClass>();
        }
    }
}
