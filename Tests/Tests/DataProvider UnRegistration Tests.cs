// Copyright (c) Reality Collective. All rights reserved.

using NUnit.Framework;
using RealityCollective.ServiceFramework.Services;
using RealityCollective.ServiceFramework.Tests.Interfaces;
using RealityCollective.ServiceFramework.Tests.Providers;
using RealityCollective.ServiceFramework.Tests.Services;
using RealityCollective.ServiceFramework.Tests.Utilities;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;

namespace RealityCollective.ServiceFramework.Tests.G_DataProviderUnRegistration
{
    internal class DataProviderUnRegistrationTests
    {
        private ServiceManager testServiceManager;

        #region Data Provider unRegistration

        [Test]
        public void Test_07_01_UnregisterSingleDataProvider()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register service 1 and data provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1));

            // Retrieve registered data provider
            var dataProvider1 = testServiceManager.GetService<ITestDataProvider1>();

            // Unregister data provider from service
            testService1.UnRegisterDataProvider(dataProvider1);

            // Try and retrieve unregistered data provider
            var testDataProvider1Unregistered = testServiceManager.GetService<ITestDataProvider1>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestDataProvider1)} service."));

            // Tests
            Assert.IsNotNull(dataProvider1, "Test data provider was not registered");
            Assert.IsNull(testDataProvider1Unregistered, "Data Provider was found, it should have been unregistered");
            Assert.AreEqual(testService1.DataProviders.Count, 0, "DataProvider Count after being unregistered does not match");
            Assert.AreEqual(activeServiceCount + 1, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_07_02_UnregisterSingleDataProviderFromMultiple()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register service 1 and data providers
            var testService1 = new TestService1();
            var serviceRegistration = testServiceManager.TryRegisterService<ITestService1>(testService1);
            var dataprovider1Registration = testServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1));
            var dataprovider2Registration = testServiceManager.TryRegisterService<ITestDataProvider2>(new TestDataProvider2(TestDataProvider2.TestName, 0, null, testService1));

            // Retrieve registered data provider
            var dataProvider1 = testServiceManager.GetService<ITestDataProvider1>();

            // Unregister data provider from service
            testService1.UnRegisterDataProvider(dataProvider1);

            // Try and retrieve unregistered data provider
            var testDataProvider1Unregistered = testServiceManager.GetService<ITestDataProvider1>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestDataProvider1)} service."));

            // Try and retrieve still registered Service
            var testDataProvider2Unregistered = testServiceManager.GetService<ITestDataProvider2>();

            // Tests
            Assert.IsNotNull(dataProvider1, "Test data provider was not registered");
            Assert.IsNull(testDataProvider1Unregistered, "Data Provider was found, it should have been unregistered");
            Assert.IsNotNull(testDataProvider2Unregistered, "Data Provider was not found, it should still be registered");
            Assert.AreEqual(testService1.DataProviders.Count, 1, "DataProvider Count after being unregistered does not match");
            Assert.AreEqual(activeServiceCount + 2, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_07_03_UnregisterSingleDataProviderFromSecondService()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register service 1 and data providers
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1));

            // Register service 2 and data providers
            var testService2 = new TestService2();
            testServiceManager.TryRegisterService<ITestService2>(testService2);
            testServiceManager.TryRegisterService<ITestDataProvider2>(new TestDataProvider2(TestDataProvider2.TestName, 0, null, testService2));

            // Retrieve registered data provider
            var dataProvider1 = testServiceManager.GetService<ITestDataProvider1>();

            // Unregister data provider from service
            testService1.UnRegisterDataProvider(dataProvider1);

            // Try and retrieve unregistered data provider
            var testDataProvider1Unregistered = testServiceManager.GetService<ITestDataProvider1>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestDataProvider1)} service."));

            // Try and retrieve still registered Service
            var testDataProvider2Unregistered = testServiceManager.GetService<ITestDataProvider2>();

            // Tests
            Assert.IsNotNull(dataProvider1, "Test data provider was not registered");
            Assert.IsNull(testDataProvider1Unregistered, "Data Provider was found, it should have been unregistered");
            Assert.AreEqual(testService2.DataProviders.Count, 1, "DataProvider Count after being unregistered does not match");
            Assert.AreEqual(activeServiceCount + 3, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_07_04_UnregisterDataProviderDirect()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register service 1 and data providers
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1));

            // Retrieve registered data provider
            var dataProvider1 = testServiceManager.GetService<ITestDataProvider1>();

            // Try and retrieve unregistered data provider direct
            var dataproviderUnregister = testServiceManager.TryUnregisterService<ITestDataProvider1>(dataProvider1);

            // Try and retrieve unregistered data provider
            var testDataProvider1Unregistered = testServiceManager.GetService<ITestDataProvider1>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestDataProvider1)} service."));

            // Tests
            Assert.IsNotNull(dataProvider1, "Test data provider was not registered");
            Assert.IsTrue(dataproviderUnregister, "Service was not unregistered correctly");
            Assert.IsNull(testDataProvider1Unregistered, "Data Provider was found, it should have been unregistered");
            Assert.AreEqual(testService1.DataProviders.Count, 0, "DataProvider Count after being unregistered does not match");
            Assert.AreEqual(activeServiceCount + 1, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        #endregion Data Provider unRegistration
    }
}