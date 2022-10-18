// Copyright (c) Reality Collective. All rights reserved.

using NUnit.Framework;
using RealityCollective.ServiceFramework.Services;
using RealityCollective.ServiceFramework.Tests.Interfaces;
using RealityCollective.ServiceFramework.Tests.Providers;
using RealityCollective.ServiceFramework.Tests.Services;
using RealityCollective.ServiceFramework.Tests.Utilities;

namespace RealityCollective.ServiceFramework.Tests.K_ServiceProviderEnabling
{
    internal class ServiceProviderEnableTests
    {
        private ServiceManager testServiceManager;

        #region Enable Service Provider

        [Test]
        public void Test_11_01_ServiceProviderEnable()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);

            var ServiceProvider1 = new TestServiceProvider1(TestServiceProvider1.TestName, 0, null, testService1);
            testServiceManager.TryRegisterService<ITestServiceProvider1>(ServiceProvider1);
            ServiceProvider1.Disable();

            Assert.IsFalse(ServiceProvider1.IsEnabled, "Test Service Provider was in a enabled state when it was disabled");

            // Retrieve
            var ServiceProvidertest1Retrieval = testServiceManager.GetService<ITestServiceProvider1>();

            testServiceManager.EnableService<ITestServiceProvider1>();

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when the Service Provider was disabled, should still be enabled");
            Assert.IsTrue(ServiceProvider1.IsEnabled, "Test Service Provider was in a disabled state when it was enabled");
            Assert.IsTrue(ServiceProvidertest1Retrieval.IsEnabled, "Test Service Provider was in a disabled state when it was enabled after retrieval.");
        }

        [Test]
        public void Test_11_02_ServiceProviderEnableByName()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);

            var ServiceProvider1 = new TestServiceProvider1(TestServiceProvider1.TestName, 0, null, testService1);
            testServiceManager.TryRegisterService<ITestServiceProvider1>(ServiceProvider1);
            ServiceProvider1.Disable();

            Assert.IsFalse(ServiceProvider1.IsEnabled, "Test Service Provider was in a enabled state when it was disabled");

            // Retrieve
            var ServiceProvidertest1Retrieval = testServiceManager.GetService<ITestServiceProvider1>();

            testServiceManager.EnableService<ITestServiceProvider1>(TestServiceProvider1.TestName);

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when the Service Provider was disabled, should still be enabled");
            Assert.IsTrue(ServiceProvider1.IsEnabled, "Test Service Provider was in a disabled state when it was enabled");
            Assert.IsTrue(ServiceProvidertest1Retrieval.IsEnabled, "Test Service Provider was in a disabled state when it was enabled after retrieval.");
        }

        [Test]
        public void Test_11_03_ServiceProviderEnableDirect()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);

            var ServiceProvider1 = new TestServiceProvider1(TestServiceProvider1.TestName, 0, null, testService1);
            testServiceManager.TryRegisterService<ITestServiceProvider1>(ServiceProvider1);
            ServiceProvider1.Disable();

            Assert.IsFalse(ServiceProvider1.IsEnabled, "Test Service Provider was in a enabled state when it was disabled");

            // Retrieve
            var ServiceProvidertest1Retrieval = testServiceManager.GetService<ITestServiceProvider1>();

            ServiceProvidertest1Retrieval.Enable();

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when the Service Provider was disabled, should still be enabled");
            Assert.IsTrue(ServiceProvider1.IsEnabled, "Test Service Provider was in a disabled state when it was enabled");
            Assert.IsTrue(ServiceProvidertest1Retrieval.IsEnabled, "Test Service Provider was in a disabled state when it was enabled after retrieval.");
        }

        [Test]
        public void Test_11_04_ServiceProviderEnabledWithServices()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register service 1 and Service Provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestServiceProvider1>(new TestServiceProvider1(TestServiceProvider1.TestName, 0, null, testService1));

            // Register service 2 and Service Provider
            var testService2 = new TestService2();
            testServiceManager.TryRegisterService<ITestService2>(testService2);
            testServiceManager.TryRegisterService<ITestServiceProvider2>(new TestServiceProvider2(TestServiceProvider2.TestName, 0, null, testService2));

            testServiceManager.DisableAllServices();

            // Retrieve
            var ServiceProvidertest1Retrieval = testServiceManager.GetService<ITestServiceProvider1>();
            var ServiceProvidertest2Retrieval = testServiceManager.GetService<ITestServiceProvider2>();

            Assert.IsFalse(ServiceProvidertest1Retrieval.IsEnabled, "Test Service Provider was in a enabled state when it was disabled");
            Assert.IsFalse(ServiceProvidertest1Retrieval.IsEnabled, "Test Service Provider was in a enabled state when it was disabled");

            testServiceManager.EnableAllServices();

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service 1 was in a disabled state when it was enabled");
            Assert.IsTrue(testService2.IsEnabled, "Test service 2 was in a disabled state when it was enabled");
            Assert.IsTrue(ServiceProvidertest1Retrieval.IsEnabled, "Test Service Provider was in a disabled state when it was enabled after retrieval.");
            Assert.IsTrue(ServiceProvidertest2Retrieval.IsEnabled, "Test Service Provider was in a disabled state when it was enabled after retrieval.");
        }

        #endregion Enable Service Provider
    }
}