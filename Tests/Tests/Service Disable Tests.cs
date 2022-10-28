// Copyright (c) Reality Collective. All rights reserved.

using NUnit.Framework;
using RealityCollective.ServiceFramework.Services;
using RealityCollective.ServiceFramework.Tests.Interfaces;
using RealityCollective.ServiceFramework.Tests.Services;
using RealityCollective.ServiceFramework.Tests.Utilities;

namespace RealityCollective.ServiceFramework.Tests.H_ServiceDisabling
{
    internal class ServiceDisableTests
    {
        private ServiceManager testServiceManager;

        #region Disable Running Services

        [Test]
        public void Test_08_01_ServiceDisable()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);

            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when it was started");

            testServiceManager.DisableService<ITestService1>();

            // Retrieve
            var testService1Retrieval = testServiceManager.GetService<ITestService1>();

            // Tests
            Assert.IsFalse(testService1.IsEnabled, "Test service was in a enabled state when it was disabled via interface call");
            Assert.IsFalse(testService1Retrieval.IsEnabled, "Test service was in a enabled state when it was disabled after retrieval.");
        }

        [Test]
        public void Test_08_02_ServiceDisableByName()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);

            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when it was started");

            testServiceManager.DisableService<ITestService1>(TestService1.TestName);

            // Retrieve
            var testService1Retrieval = testServiceManager.GetService<ITestService1>();

            // Tests
            Assert.IsFalse(testService1.IsEnabled, "Test service was in a enabled state when it was disabled via interface call");
            Assert.IsFalse(testService1Retrieval.IsEnabled, "Test service was in a enabled state when it was disabled after retrieval.");
        }

        [Test]
        public void Test_08_03_ServiceDisableDirect()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);

            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when it was started");

            testService1.Disable();

            // Retrieve
            var testService1Retrieval = testServiceManager.GetService<ITestService1>();

            // Tests
            Assert.IsFalse(testService1.IsEnabled, "Test service was in a enabled state when it was disabled");
            Assert.IsFalse(testService1Retrieval.IsEnabled, "Test service was in a enabled state when it was disabled after retrieval.");
        }

        [Test]
        public void Test_08_04_DisableAllServices()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            var testService2 = new TestService2();
            testServiceManager.TryRegisterService<ITestService2>(testService2);

            testServiceManager.DisableAllServices();

            // Retrieve
            var testService1Retrieval = testServiceManager.GetService<ITestService1>();
            var testService2Retrieval = testServiceManager.GetService<ITestService2>();

            // Tests
            Assert.IsFalse(testService1.IsEnabled, "Test service was in a enabled state when it was disabled");
            Assert.IsFalse(testService2.IsEnabled, "Test service was in a enabled state when it was disabled");
            Assert.IsFalse(testService1Retrieval.IsEnabled, "Test service was in a enabled state when it was disabled after retrieval.");
            Assert.IsFalse(testService2Retrieval.IsEnabled, "Test service was in a enabled state when it was disabled after retrieval.");
        }

        [Test]
        public void Test_08_05_ServiceDisablePriorToRegistration()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Create disabled
            var testService1 = new TestService1();
            testService1.Disable();

            Assert.IsFalse(testService1.IsEnabled, "Test service was in a enabled state, but was registered disabled");

            // Register
            testServiceManager.TryRegisterService<ITestService1>(testService1);

            Assert.IsFalse(testService1.IsEnabled, "Test service was in a enabled state, but was registered disabled after registration");

            // Retrieve
            var testService1Retrieval = testServiceManager.GetService<ITestService1>();

            // Tests
            Assert.IsFalse(testService1.IsEnabled, "Test service was in a enabled state when it was disabled");
            Assert.IsFalse(testService1Retrieval.IsEnabled, "Test service was in a enabled state when it was disabled after retrieval.");
        }

        #endregion Disable Running Services
    }
}