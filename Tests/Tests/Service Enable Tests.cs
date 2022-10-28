// Copyright (c) Reality Collective. All rights reserved.

using NUnit.Framework;
using RealityCollective.ServiceFramework.Services;
using RealityCollective.ServiceFramework.Tests.Interfaces;
using RealityCollective.ServiceFramework.Tests.Services;
using RealityCollective.ServiceFramework.Tests.Utilities;

namespace RealityCollective.ServiceFramework.Tests.J_ServiceEnabling
{
    internal class ServiceEnableTests
    {
        private ServiceManager testServiceManager;

        #region 10 Enable Service

        [Test]
        public void Test_10_01_ServiceEnable()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register
            var testService1 = new TestService1();
            testService1.Disable();
            testServiceManager.TryRegisterService<ITestService1>(testService1);

            Assert.IsFalse(testService1.IsEnabled, "Test service was in a enabled state when it was disabled");

            testServiceManager.EnableService<ITestService1>();

            // Retrieve
            var testService1Retrieval = testServiceManager.GetService<ITestService1>();

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when it was enabled");
            Assert.IsTrue(testService1Retrieval.IsEnabled, "Test service was in a disabled state when it was enabled after retrieval.");
        }

        [Test]
        public void Test_10_02_ServiceEnableByName()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register
            var testService1 = new TestService1();
            testService1.Disable();
            testServiceManager.TryRegisterService<ITestService1>(testService1);

            Assert.IsFalse(testService1.IsEnabled, "Test service was in a enabled state when it was disabled");

            testServiceManager.EnableService<ITestService1>(TestService1.TestName);

            // Retrieve
            var testService1Retrieval = testServiceManager.GetService<ITestService1>();

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when it was enabled");
            Assert.IsTrue(testService1Retrieval.IsEnabled, "Test service was in a disabled state when it was enabled after retrieval.");
        }

        [Test]
        public void Test_10_03_ServiceEnableDirect()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register
            var testService1 = new TestService1();
            testService1.Disable();
            testServiceManager.TryRegisterService<ITestService1>(testService1);

            Assert.IsFalse(testService1.IsEnabled, "Test service was in a enabled state when it was disabled");

            testService1.Enable();

            // Retrieve
            var testService1Retrieval = testServiceManager.GetService<ITestService1>();

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when it was enabled");
            Assert.IsTrue(testService1Retrieval.IsEnabled, "Test service was in a disabled state when it was enabled after retrieval.");
        }

        [Test]
        public void Test_10_04_EnableAllServices()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            var testService2 = new TestService2();
            testServiceManager.TryRegisterService<ITestService2>(testService2);

            testServiceManager.DisableAllServices();

            Assert.IsFalse(testService1.IsEnabled, "Test service was in a enabled state when it was disabled");
            Assert.IsFalse(testService2.IsEnabled, "Test service was in a enabled state when it was disabled");

            testServiceManager.EnableAllServices();

            // Retrieve
            var testService1Retrieval = testServiceManager.GetService<ITestService1>();
            var testService2Retrieval = testServiceManager.GetService<ITestService2>();

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when it was enabled");
            Assert.IsTrue(testService2.IsEnabled, "Test service was in a disabled state when it was enabled");
            Assert.IsTrue(testService1Retrieval.IsEnabled, "Test service was in a disabled state when it was enabled after retrieval.");
            Assert.IsTrue(testService2Retrieval.IsEnabled, "Test service was in a disabled state when it was enabled after retrieval.");
        }

        #endregion Enable Service
    }
}