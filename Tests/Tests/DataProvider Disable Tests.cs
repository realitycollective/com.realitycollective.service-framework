// Copyright (c) Reality Collective. All rights reserved.

using NUnit.Framework;
using RealityToolkit.ServiceFramework.Services;
using RealityToolkit.ServiceFramework.Tests.Interfaces;
using RealityToolkit.ServiceFramework.Tests.Providers;
using RealityToolkit.ServiceFramework.Tests.Services;
using RealityToolkit.ServiceFramework.Tests.Utilities;

namespace RealityToolkit.ServiceFramework.Tests.I_DataProviderDisabling
{
    internal class DataProviderDisableTests
    {
        private ServiceManager testServiceManager;

        #region Disable Running Data Provider

        [Test]
        public void Test_09_01_DataProviderDisable()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            var dataProvider1 = new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1);
            testServiceManager.TryRegisterService<ITestDataProvider1>(dataProvider1);

            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when it was started");
            Assert.IsTrue(dataProvider1.IsEnabled, "Test data provider was in a disabled state when it was started");

            testServiceManager.DisableService<ITestDataProvider1>();

            // Retrieve
            var dataProvidertest1Retrieval = testServiceManager.GetService<ITestDataProvider1>();

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when the data provider was disabled, should still be enabled");
            Assert.IsFalse(dataProvider1.IsEnabled, "Test data provider was in a enabled state when it was disabled");
            Assert.IsFalse(dataProvidertest1Retrieval.IsEnabled, "Test data provider was in a enabled state when it was disabled after retrieval.");
        }

        [Test]
        public void Test_09_02_DataProviderDisableByName()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            var dataProvider1 = new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1);
            testServiceManager.TryRegisterService<ITestDataProvider1>(dataProvider1);

            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when it was started");
            Assert.IsTrue(dataProvider1.IsEnabled, "Test data provider was in a disabled state when it was started");

            testServiceManager.DisableService<ITestDataProvider1>(TestDataProvider1.TestName);

            // Retrieve
            var dataProvidertest1Retrieval = testServiceManager.GetService<ITestDataProvider1>();

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when the data provider was disabled, should still be enabled");
            Assert.IsFalse(dataProvider1.IsEnabled, "Test data provider was in a enabled state when it was disabled");
            Assert.IsFalse(dataProvidertest1Retrieval.IsEnabled, "Test data provider was in a enabled state when it was disabled after retrieval.");
        }

        [Test]
        public void Test_09_03_DataProviderDisableDirect()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            var dataProvider1 = new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1);
            testServiceManager.TryRegisterService<ITestDataProvider1>(dataProvider1);

            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when it was started");
            Assert.IsTrue(dataProvider1.IsEnabled, "Test data provider was in a disabled state when it was started");

            dataProvider1.Disable();

            // Retrieve
            var dataProvidertest1Retrieval = testServiceManager.GetService<ITestDataProvider1>();

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when the data provider was disabled, should still be enabled");
            Assert.IsFalse(dataProvider1.IsEnabled, "Test data provider was in a enabled state when it was disabled");
            Assert.IsFalse(dataProvidertest1Retrieval.IsEnabled, "Test data provider was in a enabled state when it was disabled after retrieval.");
        }

        [Test]
        public void Test_09_04_DataProviderDisabledWithServices()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register service 1 and data provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1));

            // Register service 2 and data provider
            var testService2 = new TestService2();
            testServiceManager.TryRegisterService<ITestService2>(testService2);
            testServiceManager.TryRegisterService<ITestDataProvider2>(new TestDataProvider2(TestDataProvider2.TestName, 0, null, testService2));

            testServiceManager.DisableAllServices();

            // Retrieve
            var dataProvidertest1Retrieval = testServiceManager.GetService<ITestDataProvider1>();
            var dataProvidertest2Retrieval = testServiceManager.GetService<ITestDataProvider2>();

            // Tests
            Assert.IsFalse(testService1.IsEnabled, "Test service 1 was in a enabled state when it was disabled");
            Assert.IsFalse(testService2.IsEnabled, "Test service 2 was in a enabled state when it was disabled");
            Assert.IsFalse(dataProvidertest1Retrieval.IsEnabled, "Test data provider was in a enabled state when it was disabled after retrieval.");
            Assert.IsFalse(dataProvidertest2Retrieval.IsEnabled, "Test data provider was in a enabled state when it was disabled after retrieval.");
        }

        [Test]
        public void Test_09_05_DataProviderDisablePriorToRegistration()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Create Serivce
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);

            // Create disabled data provider
            var dataProvider1 = new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1);
            dataProvider1.Disable();

            Assert.IsFalse(dataProvider1.IsEnabled, "Test data provider was in a enabled state when it was started disabled");

            // Register data provider
            testServiceManager.TryRegisterService<ITestDataProvider1>(dataProvider1);

            Assert.IsFalse(dataProvider1.IsEnabled, "Test data provider was in a enabled state when it was started disabled after registration");

            // Retrieve
            var dataProvidertest1Retrieval = testServiceManager.GetService<ITestDataProvider1>();

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when the data provider was disabled, should still be enabled");
            Assert.IsFalse(dataProvider1.IsEnabled, "Test data provider was in a enabled state when it was started disabled");
            Assert.IsFalse(dataProvidertest1Retrieval.IsEnabled, "Test data provider was in a enabled state when it was started disabled after retrieval.");
        }

        #endregion Disable Running Data Provider
    }
}