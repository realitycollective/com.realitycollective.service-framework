// Copyright (c) Reality Collective. All rights reserved.

using NUnit.Framework;
using RealityCollective.ServiceFramework.Services;
using RealityCollective.ServiceFramework.Tests.Interfaces;
using RealityCollective.ServiceFramework.Tests.Modules;
using RealityCollective.ServiceFramework.Tests.Services;
using RealityCollective.ServiceFramework.Tests.Utilities;

namespace RealityCollective.ServiceFramework.Tests.I_ServiceModuleDisabling
{
    internal class ServiceModuleDisableTests
    {
        private ServiceManager testServiceManager;

        #region Disable Running Data Provider

        [Test]
        public void Test_09_01_ServiceModuleDisable()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            var dataProvider1 = new TestServiceModule1(TestServiceModule1.TestName, 0, null, testService1);
            testServiceManager.TryRegisterService<ITestServiceModule1>(dataProvider1);

            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when it was started");
            Assert.IsTrue(dataProvider1.IsEnabled, "Test data provider was in a disabled state when it was started");

            testServiceManager.DisableService<ITestServiceModule1>();

            // Retrieve
            var dataProvidertest1Retrieval = testServiceManager.GetService<ITestServiceModule1>();

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when the data provider was disabled, should still be enabled");
            Assert.IsFalse(dataProvider1.IsEnabled, "Test data provider was in a enabled state when it was disabled");
            Assert.IsFalse(dataProvidertest1Retrieval.IsEnabled, "Test data provider was in a enabled state when it was disabled after retrieval.");
        }

        [Test]
        public void Test_09_02_ServiceModuleDisableByName()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            var dataProvider1 = new TestServiceModule1(TestServiceModule1.TestName, 0, null, testService1);
            testServiceManager.TryRegisterService<ITestServiceModule1>(dataProvider1);

            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when it was started");
            Assert.IsTrue(dataProvider1.IsEnabled, "Test data provider was in a disabled state when it was started");

            testServiceManager.DisableService<ITestServiceModule1>(TestServiceModule1.TestName);

            // Retrieve
            var dataProvidertest1Retrieval = testServiceManager.GetService<ITestServiceModule1>();

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when the data provider was disabled, should still be enabled");
            Assert.IsFalse(dataProvider1.IsEnabled, "Test data provider was in a enabled state when it was disabled");
            Assert.IsFalse(dataProvidertest1Retrieval.IsEnabled, "Test data provider was in a enabled state when it was disabled after retrieval.");
        }

        [Test]
        public void Test_09_03_ServiceModuleDisableDirect()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            var dataProvider1 = new TestServiceModule1(TestServiceModule1.TestName, 0, null, testService1);
            testServiceManager.TryRegisterService<ITestServiceModule1>(dataProvider1);

            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when it was started");
            Assert.IsTrue(dataProvider1.IsEnabled, "Test data provider was in a disabled state when it was started");

            dataProvider1.Disable();

            // Retrieve
            var dataProvidertest1Retrieval = testServiceManager.GetService<ITestServiceModule1>();

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when the data provider was disabled, should still be enabled");
            Assert.IsFalse(dataProvider1.IsEnabled, "Test data provider was in a enabled state when it was disabled");
            Assert.IsFalse(dataProvidertest1Retrieval.IsEnabled, "Test data provider was in a enabled state when it was disabled after retrieval.");
        }

        [Test]
        public void Test_09_04_ServiceModuleDisabledWithServices()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register service 1 and data provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestServiceModule1>(new TestServiceModule1(TestServiceModule1.TestName, 0, null, testService1));

            // Register service 2 and data provider
            var testService2 = new TestService2();
            testServiceManager.TryRegisterService<ITestService2>(testService2);
            testServiceManager.TryRegisterService<ITestServiceModule2>(new TestServiceModule2(TestServiceModule2.TestName, 0, null, testService2));

            testServiceManager.DisableAllServices();

            // Retrieve
            var dataProvidertest1Retrieval = testServiceManager.GetService<ITestServiceModule1>();
            var dataProvidertest2Retrieval = testServiceManager.GetService<ITestServiceModule2>();

            // Tests
            Assert.IsFalse(testService1.IsEnabled, "Test service 1 was in a enabled state when it was disabled");
            Assert.IsFalse(testService2.IsEnabled, "Test service 2 was in a enabled state when it was disabled");
            Assert.IsFalse(dataProvidertest1Retrieval.IsEnabled, "Test data provider was in a enabled state when it was disabled after retrieval.");
            Assert.IsFalse(dataProvidertest2Retrieval.IsEnabled, "Test data provider was in a enabled state when it was disabled after retrieval.");
        }

        [Test]
        public void Test_09_05_ServiceModuleDisablePriorToRegistration()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Create Serivce
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);

            // Create disabled data provider
            var dataProvider1 = new TestServiceModule1(TestServiceModule1.TestName, 0, null, testService1);
            dataProvider1.Disable();

            Assert.IsFalse(dataProvider1.IsEnabled, "Test data provider was in a enabled state when it was started disabled");

            // Register data provider
            testServiceManager.TryRegisterService<ITestServiceModule1>(dataProvider1);

            Assert.IsFalse(dataProvider1.IsEnabled, "Test data provider was in a enabled state when it was started disabled after registration");

            // Retrieve
            var dataProvidertest1Retrieval = testServiceManager.GetService<ITestServiceModule1>();

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when the data provider was disabled, should still be enabled");
            Assert.IsFalse(dataProvider1.IsEnabled, "Test data provider was in a enabled state when it was started disabled");
            Assert.IsFalse(dataProvidertest1Retrieval.IsEnabled, "Test data provider was in a enabled state when it was started disabled after retrieval.");
        }

        #endregion Disable Running Data Provider
    }
}