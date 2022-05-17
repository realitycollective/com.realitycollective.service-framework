// Copyright (c) Reality Collective. All rights reserved.

using NUnit.Framework;
using RealityToolkit.ServiceFramework.Definitions;
using RealityToolkit.ServiceFramework.Definitions.Platforms;
using RealityToolkit.ServiceFramework.Interfaces;
using RealityToolkit.ServiceFramework.Services;
using RealityToolkit.ServiceFramework.Tests.Interfaces;
using RealityToolkit.ServiceFramework.Tests.Profiles;
using RealityToolkit.ServiceFramework.Tests.Providers;
using RealityToolkit.ServiceFramework.Tests.Services;
using RealityToolkit.ServiceFramework.Tests.Utilities;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;

namespace RealityToolkit.ServiceFramework.Tests.E_DataProviderRetrieval
{
    internal class DataProviderRetrievalTests
    {
        private ServiceManager testServiceManager;

        #region Data Provider Retrieval

        [Test]
        public void Test_05_01_RetrieveSingleDataProvider()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register Service 1 and data provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            var dataProvider1 = new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1);
            testServiceManager.TryRegisterService<ITestDataProvider1>(dataProvider1);

            // Retrieve
            var dataProvider1Retrieval = testServiceManager.GetService<ITestDataProvider1>();

            // Tests
            Assert.IsNotNull(dataProvider1, "Test data provider not found");
            Assert.AreEqual(dataProvider1.ServiceGuid, dataProvider1Retrieval.ServiceGuid, "Service GUID does not match");
            Assert.AreEqual(activeServiceCount + 2, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_05_02_RetrieveSecondDataProvider()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register service 1 and data provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1));

            // Register service 1 and data provider
            var dataProvider2 = new TestDataProvider2(TestDataProvider2.TestName, 0, null, testService1);
            testServiceManager.TryRegisterService<ITestDataProvider2>(dataProvider2);

            // Retrieve
            var dataProvider2Retrieval = testServiceManager.GetService<ITestDataProvider2>();

            // Tests
            Assert.IsNotNull(dataProvider2, "Test data provider not found");
            Assert.AreEqual(dataProvider2.ServiceGuid, dataProvider2Retrieval.ServiceGuid, "Service GUID does not match");
            Assert.AreEqual(activeServiceCount + 3, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_05_03_RetrieveAllDataProvider()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            var testService1 = new TestService1();

            // Register service 1 and data providers
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1));
            testServiceManager.TryRegisterService<ITestDataProvider2>(new TestDataProvider2(TestDataProvider2.TestName, 0, null, testService1));

            // Retrieve all registered IDataProviders
            var dataProviders = testServiceManager.GetServices<IServiceDataProvider>();

            // Tests
            Assert.IsNotEmpty(dataProviders, "Data Providers were not registered");
            Assert.AreEqual(activeSystemCount + 3, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_05_04_RetrieveAllDataProvidersForService()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            // Register service 1 and data provider2
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1));
            testServiceManager.TryRegisterService<ITestDataProvider2>(new TestDataProvider2(TestDataProvider2.TestName, 0, null, testService1));

            // Retrieve all registered IDataProviders from service
            var testService = testServiceManager.GetService<ITestService1>();
            var dataProviders = testService.DataProviders;

            // Tests
            Assert.IsNotEmpty(dataProviders, "Data Providers were not registered");
            Assert.AreEqual(dataProviders.Count, 2, "Could not locate all data providers");
            Assert.AreEqual(activeSystemCount + 3, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_05_05_RetrieveAllRegisteredDataProvidersFromMultipleServices()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            // Register service 1 and data provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1));

            // Register service 2 and data provider
            var testService2 = new TestService2();
            testServiceManager.TryRegisterService<ITestService2>(testService2);
            testServiceManager.TryRegisterService<ITestDataProvider2>(new TestDataProvider2(TestDataProvider2.TestName, 0, null, testService2));

            // Retrieve all registered IServiceDataProvider
            var dataProviders = testServiceManager.GetServices<IServiceDataProvider>();

            // Tests
            Assert.IsNotEmpty(dataProviders, "Data Providers were not registered");
            Assert.AreEqual(dataProviders.Count, 2, "Could not locate all data providers");
            Assert.AreEqual(activeSystemCount + 4, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_05_06_ServiceDataProviderDoesNotExist()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register service 1 and data provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            var testDataProvider2 = new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1);
            testServiceManager.TryRegisterService<ITestDataProvider1>(testDataProvider2);

            // Validate non-existent data provider 2
            var isDataProviderRegistered = testServiceManager.IsServiceRegistered<ITestDataProvider2>();

            // Tests
            Assert.IsFalse(isDataProviderRegistered, "Data Provider was found when it was not registered");
        }

        [Test]
        public void Test_05_07_RetrieveRegisterServiceConfigurationsWithDataProviders()
        {
            // Check logs
            LogAssert.Expect(LogType.Log, new Regex("Test Service 1 is Initialised"));
            LogAssert.Expect(LogType.Log, new Regex("Test Data Provider 1 is Initialised"));
            LogAssert.Expect(LogType.Log, new Regex("Test Service 2 is Initialised"));
            LogAssert.Expect(LogType.Error, new Regex("Unable to find ITestDataProvider2 service."));

            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            var configurations = new ServiceConfiguration<IService>[2];

            var testService1Profile = (TestService1Profile)ScriptableObject.CreateInstance(typeof(TestService1Profile));
            var dataProvider1Configuration = new ServiceConfiguration<ITestDataProvider1>(typeof(TestDataProvider1), TestDataProvider1.TestName, 1, AllPlatforms.Platforms, null);
            testService1Profile.AddConfiguration(dataProvider1Configuration);

            var testService2Profile = (TestService2Profile)ScriptableObject.CreateInstance(typeof(TestService2Profile));
            var dataProvider2Configuration = new ServiceConfiguration<ITestDataProvider2>(typeof(TestDataProvider2), TestDataProvider2.TestName, 1, AllPlatforms.Platforms, null);
            testService2Profile.AddConfiguration(dataProvider2Configuration);

            configurations[0] = new ServiceConfiguration<IService>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, testService1Profile);
            configurations[1] = new ServiceConfiguration<IService>(typeof(TestService2), TestService2.TestName, 1, AllPlatforms.Platforms, testService2Profile);

            var result = testServiceManager.TryRegisterServiceConfigurations(configurations);

            // Both services should be found following configuration registration
            var service1Registration = testServiceManager.GetService<ITestService1>();
            var service2Registration = testServiceManager.GetService<ITestService2>();

            // Data Provider 1 should return because its service allows the registration of Data Providers
            var dataProvider1Registration = testServiceManager.GetService<ITestDataProvider1>();

            // Data Provider 2 should NOT return because its service does NOT allow the registration of Data Providers
            var dataProvider2Registration = testServiceManager.GetService<ITestDataProvider2>();

            // Tests
            Assert.IsTrue(result, "Test services were not registered");
            Assert.IsNotNull(service1Registration, "Test Service 1 should be registered but it was not found");
            Assert.IsNotNull(service2Registration, "Test Service 2 should be registered but it was not found");
            Assert.IsNotNull(dataProvider1Registration, "Data Provider 1 should be registered but it was not found");
            Assert.IsNull(dataProvider2Registration, "Data Provider 2 should NOT be registered but it was found");
            Assert.AreEqual(1, service1Registration.DataProviders.Count, "Test Service 1 Data Provider count did not match, should be 1");
            Assert.AreEqual(1, service2Registration.DataProviders.Count, "Test Service 2 Data Provider count did not match, should be 1");
            Assert.AreEqual(activeServiceCount + 3, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        #endregion Data Provider Retrieval
    }
}