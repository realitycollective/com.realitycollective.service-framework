// Copyright (c) Reality Collective. All rights reserved.

using NUnit.Framework;
using RealityCollective.ServiceFramework.Services;
using RealityCollective.ServiceFramework.Tests.Interfaces;
using RealityCollective.ServiceFramework.Tests.Providers;
using RealityCollective.ServiceFramework.Tests.Services;
using RealityCollective.ServiceFramework.Tests.Utilities;

namespace RealityCollective.ServiceFramework.Tests.K_DataProviderEnabling
{
    internal class DataProviderEnableTests
    {
        private ServiceManager testServiceManager;

        #region Enable Data Provider

        [Test]
        public void Test_11_01_DataProviderEnable()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);

            var dataProvider1 = new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1);
            testServiceManager.TryRegisterService<ITestDataProvider1>(dataProvider1);
            dataProvider1.Disable();

            Assert.IsFalse(dataProvider1.IsEnabled, "Test data provider was in a enabled state when it was disabled");

            // Retrieve
            var dataProvidertest1Retrieval = testServiceManager.GetService<ITestDataProvider1>();

            testServiceManager.EnableService<ITestDataProvider1>();

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when the data provider was disabled, should still be enabled");
            Assert.IsTrue(dataProvider1.IsEnabled, "Test data provider was in a disabled state when it was enabled");
            Assert.IsTrue(dataProvidertest1Retrieval.IsEnabled, "Test data provider was in a disabled state when it was enabled after retrieval.");
        }

        [Test]
        public void Test_11_02_DataProviderEnableByName()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);

            var dataProvider1 = new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1);
            testServiceManager.TryRegisterService<ITestDataProvider1>(dataProvider1);
            dataProvider1.Disable();

            Assert.IsFalse(dataProvider1.IsEnabled, "Test data provider was in a enabled state when it was disabled");

            // Retrieve
            var dataProvidertest1Retrieval = testServiceManager.GetService<ITestDataProvider1>();

            testServiceManager.EnableService<ITestDataProvider1>(TestDataProvider1.TestName);

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when the data provider was disabled, should still be enabled");
            Assert.IsTrue(dataProvider1.IsEnabled, "Test data provider was in a disabled state when it was enabled");
            Assert.IsTrue(dataProvidertest1Retrieval.IsEnabled, "Test data provider was in a disabled state when it was enabled after retrieval.");
        }

        [Test]
        public void Test_11_03_DataProviderEnableDirect()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);

            var dataProvider1 = new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1);
            testServiceManager.TryRegisterService<ITestDataProvider1>(dataProvider1);
            dataProvider1.Disable();

            Assert.IsFalse(dataProvider1.IsEnabled, "Test data provider was in a enabled state when it was disabled");

            // Retrieve
            var dataProvidertest1Retrieval = testServiceManager.GetService<ITestDataProvider1>();

            dataProvidertest1Retrieval.Enable();

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when the data provider was disabled, should still be enabled");
            Assert.IsTrue(dataProvider1.IsEnabled, "Test data provider was in a disabled state when it was enabled");
            Assert.IsTrue(dataProvidertest1Retrieval.IsEnabled, "Test data provider was in a disabled state when it was enabled after retrieval.");
        }

        [Test]
        public void Test_11_04_DataProviderEnabledWithServices()
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

            Assert.IsFalse(dataProvidertest1Retrieval.IsEnabled, "Test data provider was in a enabled state when it was disabled");
            Assert.IsFalse(dataProvidertest1Retrieval.IsEnabled, "Test data provider was in a enabled state when it was disabled");

            testServiceManager.EnableAllServices();

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service 1 was in a disabled state when it was enabled");
            Assert.IsTrue(testService2.IsEnabled, "Test service 2 was in a disabled state when it was enabled");
            Assert.IsTrue(dataProvidertest1Retrieval.IsEnabled, "Test data provider was in a disabled state when it was enabled after retrieval.");
            Assert.IsTrue(dataProvidertest2Retrieval.IsEnabled, "Test data provider was in a disabled state when it was enabled after retrieval.");
        }

        #endregion Enable Data Provider
    }
}