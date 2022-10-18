// Copyright (c) Reality Collective. All rights reserved.

using NUnit.Framework;
using RealityCollective.ServiceFramework.Services;
using RealityCollective.ServiceFramework.Tests.Interfaces;
using RealityCollective.ServiceFramework.Tests.Providers;
using RealityCollective.ServiceFramework.Tests.Services;
using RealityCollective.ServiceFramework.Tests.Utilities;

namespace RealityCollective.ServiceFramework.Tests.I_ServiceProviderDisabling
{
    internal class ServiceProviderDisableTests
    {
        private ServiceManager testServiceManager;

        #region Disable Running Service Provider

        [Test]
        public void Test_09_01_ServiceProviderDisable()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            var ServiceProvider1 = new TestServiceProvider1(TestServiceProvider1.TestName, 0, null, testService1);
            testServiceManager.TryRegisterService<ITestServiceProvider1>(ServiceProvider1);

            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when it was started");
            Assert.IsTrue(ServiceProvider1.IsEnabled, "Test Service Provider was in a disabled state when it was started");

            testServiceManager.DisableService<ITestServiceProvider1>();

            // Retrieve
            var ServiceProvidertest1Retrieval = testServiceManager.GetService<ITestServiceProvider1>();

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when the Service Provider was disabled, should still be enabled");
            Assert.IsFalse(ServiceProvider1.IsEnabled, "Test Service Provider was in a enabled state when it was disabled");
            Assert.IsFalse(ServiceProvidertest1Retrieval.IsEnabled, "Test Service Provider was in a enabled state when it was disabled after retrieval.");
        }

        [Test]
        public void Test_09_02_ServiceProviderDisableByName()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            var ServiceProvider1 = new TestServiceProvider1(TestServiceProvider1.TestName, 0, null, testService1);
            testServiceManager.TryRegisterService<ITestServiceProvider1>(ServiceProvider1);

            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when it was started");
            Assert.IsTrue(ServiceProvider1.IsEnabled, "Test Service Provider was in a disabled state when it was started");

            testServiceManager.DisableService<ITestServiceProvider1>(TestServiceProvider1.TestName);

            // Retrieve
            var ServiceProvidertest1Retrieval = testServiceManager.GetService<ITestServiceProvider1>();

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when the Service Provider was disabled, should still be enabled");
            Assert.IsFalse(ServiceProvider1.IsEnabled, "Test Service Provider was in a enabled state when it was disabled");
            Assert.IsFalse(ServiceProvidertest1Retrieval.IsEnabled, "Test Service Provider was in a enabled state when it was disabled after retrieval.");
        }

        [Test]
        public void Test_09_03_ServiceProviderDisableDirect()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            var ServiceProvider1 = new TestServiceProvider1(TestServiceProvider1.TestName, 0, null, testService1);
            testServiceManager.TryRegisterService<ITestServiceProvider1>(ServiceProvider1);

            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when it was started");
            Assert.IsTrue(ServiceProvider1.IsEnabled, "Test Service Provider was in a disabled state when it was started");

            ServiceProvider1.Disable();

            // Retrieve
            var ServiceProvidertest1Retrieval = testServiceManager.GetService<ITestServiceProvider1>();

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when the Service Provider was disabled, should still be enabled");
            Assert.IsFalse(ServiceProvider1.IsEnabled, "Test Service Provider was in a enabled state when it was disabled");
            Assert.IsFalse(ServiceProvidertest1Retrieval.IsEnabled, "Test Service Provider was in a enabled state when it was disabled after retrieval.");
        }

        [Test]
        public void Test_09_04_ServiceProviderDisabledWithServices()
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

            // Tests
            Assert.IsFalse(testService1.IsEnabled, "Test service 1 was in a enabled state when it was disabled");
            Assert.IsFalse(testService2.IsEnabled, "Test service 2 was in a enabled state when it was disabled");
            Assert.IsFalse(ServiceProvidertest1Retrieval.IsEnabled, "Test Service Provider was in a enabled state when it was disabled after retrieval.");
            Assert.IsFalse(ServiceProvidertest2Retrieval.IsEnabled, "Test Service Provider was in a enabled state when it was disabled after retrieval.");
        }

        [Test]
        public void Test_09_05_ServiceProviderDisablePriorToRegistration()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Create Serivce
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);

            // Create disabled Service Provider
            var ServiceProvider1 = new TestServiceProvider1(TestServiceProvider1.TestName, 0, null, testService1);
            ServiceProvider1.Disable();

            Assert.IsFalse(ServiceProvider1.IsEnabled, "Test Service Provider was in a enabled state when it was started disabled");

            // Register Service Provider
            testServiceManager.TryRegisterService<ITestServiceProvider1>(ServiceProvider1);

            Assert.IsFalse(ServiceProvider1.IsEnabled, "Test Service Provider was in a enabled state when it was started disabled after registration");

            // Retrieve
            var ServiceProvidertest1Retrieval = testServiceManager.GetService<ITestServiceProvider1>();

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when the Service Provider was disabled, should still be enabled");
            Assert.IsFalse(ServiceProvider1.IsEnabled, "Test Service Provider was in a enabled state when it was started disabled");
            Assert.IsFalse(ServiceProvidertest1Retrieval.IsEnabled, "Test Service Provider was in a enabled state when it was started disabled after retrieval.");
        }

        #endregion Disable Running Service Provider
    }
}