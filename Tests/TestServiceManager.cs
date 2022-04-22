// Copyright (c) Reality Collective. All rights reserved.

using NUnit.Framework;
using RealityToolkit.ServiceFramework.Definitions;
using RealityToolkit.ServiceFramework.Interfaces;
using RealityToolkit.ServiceFramework.Services;
using RealityToolkit.ServiceFramework.Tests.Interfaces;
using RealityToolkit.ServiceFramework.Tests.Providers;
using RealityToolkit.ServiceFramework.Tests.Services;
using RealityToolkit.ServiceFramework.Tests.Utilities;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

namespace RealityToolkit.ServiceFramework.Tests
{
    public class TestServiceManager
    {
        private ServiceManager testServiceManager;

        #region 01 Service Locater

        [Test]
        public void Test_01_01_InitializeServiceManager()
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var serviceManager = new ServiceManager();
            serviceManager.Initialize();
            ServiceManager.Instance.ConfirmInitialized();
            
            //var gameObject = GameObject.Find(nameof(ServiceManager));
            Assert.AreEqual(serviceManager, ServiceManager.Instance, "Service Manager not found");
        }
        
        [Test]
        public void Test_01_02_TestNoProfileFound()
        {
            testServiceManager = null;

            // Setup
            TestUtilities.CleanupScene();
            TestUtilities.InitializeServiceManager(ref testServiceManager);
            ServiceManager.Instance.ConfirmInitialized();
            Assert.IsNotNull(ServiceManager.Instance, "Service Manager instance not found");
            Assert.IsTrue(ServiceManager.Instance.IsInitialized, "Service Manager was not initialized");

            ServiceManager.Instance.ActiveProfile = null;

            // Tests
            Assert.AreEqual(0, ServiceManager.Instance.ActiveServices.Count, "Service Manager services were found where none should exist");
            Assert.IsFalse(ServiceManager.Instance.HasActiveProfile, "Profile found for the Service Manager where none should exist");
            Assert.IsNull(ServiceManager.Instance.ActiveProfile, "Profile found for the Service Manager where none should exist for instance");
            LogAssert.Expect(LogType.Error, $"No {nameof(ServiceManagerRootProfile)} found, cannot initialize the {nameof(ServiceManager)}");
        }
        
        [Test]
        public void Test_01_03_CreateServiceManager()
        {
            testServiceManager = null;

            TestUtilities.InitializeServiceManagerScene(testServiceManager,false);

            // Tests
            Assert.AreEqual(0, ServiceManager.Instance.ActiveServices.Count, "More or less services found than was expected");
        }

        #endregion 01 Service Locater

        #region 02 Service Registration

        [Test]
        public void Test_02_01_RegisterService()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeServiceCount = ServiceManager.Instance.ActiveServices.Count;

            var testService1 = new TestService1();
            var serviceResult = ServiceManager.Instance.TryRegisterService<ITestService>(testService1);

            // Tests
            Assert.IsTrue(serviceResult, "Test service was not registered");
            Assert.IsTrue(testService1.ServiceGuid != System.Guid.Empty, "No GUID generated for the test service");
            Assert.AreEqual(activeServiceCount + 1, ServiceManager.Instance.ActiveServices.Count, "More or less services found than was expected");
        }


        [Test]
        public void Test_02_02_TryRegisterServiceTwice()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeServiceCount = ServiceManager.Instance.ActiveServices.Count;

            // Register
            ServiceManager.Instance.TryRegisterService<ITestService>(new TestService1());

            // Register again
            var testService2 = ServiceManager.Instance.TryRegisterService<ITestService>(new TestService1());
            LogAssert.Expect(LogType.Error, $"There is already a [{nameof(ITestService)}.{TestService1.TestName}] registered!");

            // Tests
            Assert.IsFalse(testService2, "Test service was registered when it should not have been");
            Assert.AreEqual(activeServiceCount + 1, ServiceManager.Instance.ActiveServices.Count, "More or less services found than was expected");
        }

        #endregion 02 Service Registration

        #region 03 Data Provider Registration

        [Test]
        public void Test_03_01_RegisterServiceAndDataProvider()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeServiceCount = ServiceManager.Instance.ActiveServices.Count;

            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);

            var testDataProvider = new TestDataProvider1(testService1);
            var dataProviderResult = ServiceManager.Instance.TryRegisterService<ITestDataProvider1>(testDataProvider);

            // Tests
            Assert.IsTrue(dataProviderResult, "Test data provider was not registered");
            Assert.IsTrue(testDataProvider.ServiceGuid != System.Guid.Empty);
            Assert.AreEqual(activeServiceCount + 2, ServiceManager.Instance.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_02_RegisterDataProviderMultipleTimes()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeSystemCount = ServiceManager.Instance.ActiveServices.Count;

            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(new TestService1());

            // Register
            var dataProvider1Result = ServiceManager.Instance.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(testService1));
            var dataProvider2Result = ServiceManager.Instance.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(testService1));
            LogAssert.Expect(LogType.Error, "There is already a [ITestDataProvider1.Test Data Provider 1] registered!");

            // Tests
            Assert.IsTrue(dataProvider1Result, "Data Provider 1 was not registered");
            Assert.IsFalse(dataProvider2Result, "Data Provider 2 was not registered");
            Assert.AreEqual(activeSystemCount + 2, ServiceManager.Instance.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_03_RegisterMultipleDataProviders()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeSystemCount = ServiceManager.Instance.ActiveServices.Count;

            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(new TestService1());

            // Register
            var dataProvider1Result = ServiceManager.Instance.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(testService1));
            var dataProvider2Result = ServiceManager.Instance.TryRegisterService<ITestDataProvider2>(new TestDataProvider2(testService1));

            // Tests
            Assert.IsTrue(dataProvider1Result, "Data Provider 1 was not registered");
            Assert.IsTrue(dataProvider2Result, "Data Provider 2 was not registered");
            Assert.AreEqual(activeSystemCount + 3, ServiceManager.Instance.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_04_RegisterDataProviderInMultipleServices()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeSystemCount = ServiceManager.Instance.ActiveServices.Count;

            // Register Service 1 and data provider
            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(new TestService1());
            var dataProvider1Result = ServiceManager.Instance.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(testService1));

            // Register Service 1 and data provider
            var testService2 = new TestService2();
            ServiceManager.Instance.TryRegisterService<ITestService2>(new TestService2());
            var dataProvider2Result = ServiceManager.Instance.TryRegisterService<ITestDataProvider2>(new TestDataProvider2(testService2));

            // Tests
            Assert.IsTrue(dataProvider1Result, "Data Provider 1 was not registered");
            Assert.IsTrue(dataProvider2Result, "Data Provider 2 was not registered");
            Assert.AreEqual(activeSystemCount + 4, ServiceManager.Instance.ActiveServices.Count, "More or less services found than was expected");
        }

        #endregion 03 Data Provider Registration

        #region 04 Service Retrieval

        [Test]
        public void Test_04_01_ServiceExists()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeServiceCount = ServiceManager.Instance.ActiveServices.Count;

            // Register
            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);

            // Retrieve
            var testService1Retrieval = ServiceManager.Instance.IsServiceRegistered(testService1);
            var testService1RetrievalInterface = ServiceManager.Instance.IsServiceRegistered<ITestService>();

            // Tests
            Assert.IsTrue(testService1Retrieval, "Test service was not found");
            Assert.IsTrue(testService1RetrievalInterface, "Test service was not found via interface");
            Assert.AreEqual(activeServiceCount + 1, ServiceManager.Instance.ActiveServices.Count, "More or less services found than was expected");
        }

        
        [Test]
        public void Test_04_02_ServiceDoesNotExist()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeServiceCount = ServiceManager.Instance.ActiveServices.Count;

            // Register
            var testService1 = new TestService1();

            // Retrieve
            var testService1Retrieval = ServiceManager.Instance.IsServiceRegistered(testService1);
            var testService1RetrievalInterface = ServiceManager.Instance.IsServiceRegistered<ITestService>();

            // Tests
            Assert.IsFalse(testService1Retrieval, "Test service was found in registry when it was not added");
            Assert.IsFalse(testService1RetrievalInterface, "Test service was found via interface in registry when it was not added");
            Assert.AreEqual(activeServiceCount, ServiceManager.Instance.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_04_03_RetrieveService()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeServiceCount = ServiceManager.Instance.ActiveServices.Count;

            // Register
            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);

            // Retrieve
            var testService1Retrieval = ServiceManager.Instance.GetService<ITestService>();

            // Tests
            Assert.IsNotNull(testService1Retrieval, "Test service was not found");
            Assert.IsTrue(testService1.ServiceGuid == testService1Retrieval.ServiceGuid, "Service GUID does not match");
            Assert.AreEqual(activeServiceCount + 1, ServiceManager.Instance.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_04_04_RetrieveSecondService()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeServiceCount = ServiceManager.Instance.ActiveServices.Count;

            // Register Service 1
            ServiceManager.Instance.TryRegisterService<ITestService>(new TestService1());

            // Register Service 2
            var testService2 = new TestService2();
            ServiceManager.Instance.TryRegisterService<ITestService2>(testService2);

            // Retrieve
            var testService2Retrieval = ServiceManager.Instance.GetService<ITestService2>();

            // Tests
            Assert.IsNotNull(testService2Retrieval, "Test service was not found");
            Assert.IsTrue(testService2.ServiceGuid == testService2Retrieval.ServiceGuid, "Service GUID does not match");
            Assert.AreEqual(activeServiceCount + 2, ServiceManager.Instance.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_04_05_RetrieveServiceDoesNotExist()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeServiceCount = ServiceManager.Instance.ActiveServices.Count;

            // Retrieve
            var testService1 = ServiceManager.Instance.GetService<ITestService>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestService)} service.");

            // Tests
            Assert.IsNull(testService1, "Test service was found");
            Assert.AreEqual(activeServiceCount, ServiceManager.Instance.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_04_06_RetrieveSecondServiceDoesNotExist()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeServiceCount = ServiceManager.Instance.ActiveServices.Count;

            // Register Service 1
            ServiceManager.Instance.TryRegisterService<ITestService>(new TestService1());

            // Retrieve
            var testService2 = ServiceManager.Instance.GetService<ITestService2>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestService2)} service.");

            // Tests
            Assert.IsNull(testService2, "Test service was not found");
            Assert.AreEqual(activeServiceCount + 1, ServiceManager.Instance.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_04_07_GetAllServices()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeServiceCount = ServiceManager.Instance.ActiveServices.Count;

            // Register Service 1
            ServiceManager.Instance.TryRegisterService<ITestService>(new TestService1());

            // Register Service 2
            var testService2 = new TestService2();
            ServiceManager.Instance.TryRegisterService<ITestService2>(testService2);

            // Retrieve
            var allServices = ServiceManager.Instance.GetAllServices();
            var registeredServicesList = ServiceManager.Instance.GetServices<IService>();
            var registeredTestService1 = ServiceManager.Instance.GetServices<ITestService>();

            // Tests
            Assert.AreEqual(2, allServices.Count, "More or less services found than was expected from full query");
            Assert.AreEqual(2, registeredServicesList.Count, "More or less services found than was expected from IService query");
            Assert.AreEqual(1, registeredTestService1.Count, "More or less services found than was expected from specific Interface query");
        }

        #endregion  04 Service Retrieval

        #region 05 Data Provider Retrieval

        [Test]
        public void Test_05_01_RetrieveSingleDataProvider()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeServiceCount = ServiceManager.Instance.ActiveServices.Count;

            // Register Service 1 and data provider
            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);
            var dataProvider1 = new TestDataProvider1(testService1);
            ServiceManager.Instance.TryRegisterService<ITestDataProvider1>(dataProvider1);

            // Retrieve
            var dataProvider1Retrieval = ServiceManager.Instance.GetService<ITestDataProvider1>();

            // Tests
            Assert.IsNotNull(dataProvider1, "Test data provider not found");
            Assert.AreEqual(dataProvider1.ServiceGuid, dataProvider1Retrieval.ServiceGuid, "Service GUID does not match");
            Assert.AreEqual(activeServiceCount + 2, ServiceManager.Instance.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_05_02_RetrieveSecondDataProvider()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeServiceCount = ServiceManager.Instance.ActiveServices.Count;

            // Register service 1 and data provider
            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);
            ServiceManager.Instance.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(testService1));

            // Register service 1 and data provider
            var dataProvider2 = new TestDataProvider2(testService1);
            ServiceManager.Instance.TryRegisterService<ITestDataProvider2>(dataProvider2);

            // Retrieve
            var dataProvider2Retrieval = ServiceManager.Instance.GetService<ITestDataProvider2>();

            // Tests
            Assert.IsNotNull(dataProvider2, "Test data provider not found");
            Assert.AreEqual(dataProvider2.ServiceGuid, dataProvider2Retrieval.ServiceGuid, "Service GUID does not match");
            Assert.AreEqual(activeServiceCount + 3, ServiceManager.Instance.ActiveServices.Count, "More or less services found than was expected");
        }        

        [Test]
        public void Test_05_03_RetrieveAllDataProvider()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeSystemCount = ServiceManager.Instance.ActiveServices.Count;

            var testService1 = new TestService1();

            // Register service 1 and data providers
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);
            ServiceManager.Instance.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(testService1));
            ServiceManager.Instance.TryRegisterService<ITestDataProvider2>(new TestDataProvider2(testService1));

            // Retrieve all registered IDataProviders
            var dataProviders = ServiceManager.Instance.GetServices<IServiceDataProvider>();

            // Tests
            Assert.IsNotEmpty(dataProviders, "Data Providers were not registered");
            Assert.AreEqual(activeSystemCount + 3, ServiceManager.Instance.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_05_04_RetrieveAllDataProvidersForService()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeSystemCount = ServiceManager.Instance.ActiveServices.Count;

            // Register service 1 and data provider2
            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);
            ServiceManager.Instance.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(testService1));
            ServiceManager.Instance.TryRegisterService<ITestDataProvider2>(new TestDataProvider2(testService1));

            // Retrieve all registered IDataProviders from service
            var testService = ServiceManager.Instance.GetService<ITestService>();
            var dataProviders = testService.DataProviders;

            // Tests
            Assert.IsNotEmpty(dataProviders, "Data Providers were not registered");
            Assert.AreEqual(dataProviders.Count, 2, "Could not locate all data providers");
            Assert.AreEqual(activeSystemCount + 3, ServiceManager.Instance.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_05_05_RetrieveAllRegisteredDataProvidersFromMultipleServices()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeSystemCount = ServiceManager.Instance.ActiveServices.Count;

            // Register service 1 and data provider
            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);
            ServiceManager.Instance.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(testService1));

            // Register service 2 and data provider
            var testService2 = new TestService2();
            ServiceManager.Instance.TryRegisterService<ITestService2>(testService2);
            ServiceManager.Instance.TryRegisterService<ITestDataProvider2>(new TestDataProvider2(testService2));

            // Retrieve all registered IServiceDataProvider
            var dataProviders = ServiceManager.Instance.GetServices<IServiceDataProvider>();

            // Tests
            Assert.IsNotEmpty(dataProviders, "Data Providers were not registered");
            Assert.AreEqual(dataProviders.Count, 2, "Could not locate all data providers");
            Assert.AreEqual(activeSystemCount + 4, ServiceManager.Instance.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_05_06_ServiceDataProviderDoesNotExist()
        {
            TestUtilities.InitializeServiceManagerScene();

            // Register service 1 and data provider
            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);
            var testDataProvider2 = new TestDataProvider1(testService1);
            ServiceManager.Instance.TryRegisterService<ITestDataProvider1>(testDataProvider2);

            // Validate non-existent data provider 2
            var isDataProviderRegistered = ServiceManager.Instance.IsServiceRegistered<ITestDataProvider2>();

            // Tests
            Assert.IsFalse(isDataProviderRegistered, "Data Provider was found when it was not registered");
        }

        #endregion 05 Data Provider Retrieval

        #region 06 Service unRegistration

        [Test]
        public void Test_06_01_UnregisterSingleService()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeServiceCount = ServiceManager.Instance.ActiveServices.Count;

            // Register service 1
            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);

            var serviceUnregister = ServiceManager.Instance.TryUnregisterService<ITestService>(testService1);

            var testService1Unregistered = ServiceManager.Instance.GetService<ITestService>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestService)} service.");

            // Tests
            Assert.IsNotNull(testService1, "Test service was not registered");
            Assert.IsTrue(serviceUnregister, "Service was not unregistered correctly");
            Assert.IsNull(testService1Unregistered, "Service was found, it should have been unregistered");
            Assert.AreEqual(activeServiceCount, ServiceManager.Instance.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_06_02_UnregisterServiceWithDataProvider()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeServiceCount = ServiceManager.Instance.ActiveServices.Count;

            // Register service 1 and data provider
            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);
            ServiceManager.Instance.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(testService1));

            // Unregister Service
            var serviceUnregister = ServiceManager.Instance.TryUnregisterService<ITestService>(testService1);

            // Try and retrieve unregistered Service
            var testService1Unregistered = ServiceManager.Instance.GetService<ITestService>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestService)} service.");

            // Try and retrieve unregistered Data Provider
            var testDataProvider1Unregistered = ServiceManager.Instance.GetService<ITestDataProvider1>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestDataProvider1)} service.");

            // Tests
            Assert.IsNotNull(testService1, "Test service was not registered");
            Assert.IsTrue(serviceUnregister, "Service was not unregistered correctly");
            Assert.IsNull(testService1Unregistered, "Service was found, it should have been unregistered");
            Assert.IsNull(testDataProvider1Unregistered, "Data Provider was found, it should have been unregistered");
            Assert.AreEqual(activeServiceCount, ServiceManager.Instance.ActiveServices.Count, "More or less services found than was expected");
        }


        [Test]
        public void Test_06_03_UnregisterServiceWithMultipleDataProvider()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeServiceCount = ServiceManager.Instance.ActiveServices.Count;

            // Register service 1 and data provider
            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);
            ServiceManager.Instance.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(testService1));
            ServiceManager.Instance.TryRegisterService<ITestDataProvider2>(new TestDataProvider2(testService1));

            // Unregister Service
            var serviceUnregister = ServiceManager.Instance.TryUnregisterService<ITestService>(testService1);

            // Try and retrieve unregistered Service
            var testService1Unregistered = ServiceManager.Instance.GetService<ITestService>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestService)} service.");

            // Try and retrieve unregistered Data Provider
            var testDataProvider1Unregistered = ServiceManager.Instance.GetService<ITestDataProvider1>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestDataProvider1)} service.");
            var testDataProvider2Unregistered = ServiceManager.Instance.GetService<ITestDataProvider2>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestDataProvider2)} service.");


            // Tests
            Assert.IsNotNull(testService1, "Test service was not registered");
            Assert.IsTrue(serviceUnregister, "Service was not unregistered correctly");
            Assert.IsNull(testService1Unregistered, "Service was found, it should have been unregistered");
            Assert.IsNull(testDataProvider1Unregistered, "Data Provider 1 was found, it should have been unregistered");
            Assert.IsNull(testDataProvider2Unregistered, "Data Provider 2 was found, it should have been unregistered");
            Assert.AreEqual(activeServiceCount, ServiceManager.Instance.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_06_04_UnregisterSingleServiceFromMultiple()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeServiceCount = ServiceManager.Instance.ActiveServices.Count;

            // Register service 1 and data provider
            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);

            // Register service 2
            ServiceManager.Instance.TryRegisterService<ITestService2>(new TestService2());

            // Unregister Service 1
            var serviceUnregister = ServiceManager.Instance.TryUnregisterService<ITestService>(testService1);

            // Try and retrieve unregistered Service 1
            var testService1Unregistered = ServiceManager.Instance.GetService<ITestService>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestService)} service.");

            // Try and retrieve still registered Service 2
            var testService2 = ServiceManager.Instance.GetService<ITestService2>();

            // Tests
            Assert.IsNotNull(testService1, "Test service was not registered");
            Assert.IsNotNull(testService2, "Test service 2 was not registered");
            Assert.IsTrue(serviceUnregister, "Service was not unregistered correctly");
            Assert.IsNull(testService1Unregistered, "Service was found, it should have been unregistered");
            Assert.AreEqual(activeServiceCount + 1, ServiceManager.Instance.ActiveServices.Count, "More or less services found than was expected");
        }

        //---
        [Test]
        public void Test_06_05_UnregisterSingleServiceByInterface()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeServiceCount = ServiceManager.Instance.ActiveServices.Count;

            // Register service 1
            ServiceManager.Instance.TryRegisterService<ITestService>(new TestService1());

            var serviceUnregister = ServiceManager.Instance.TryUnregisterServicesOfType<ITestService>();

            var testService1Unregistered = ServiceManager.Instance.GetService<ITestService>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestService)} service.");

            // Tests
            Assert.IsTrue(serviceUnregister, "Service was not unregistered correctly");
            Assert.IsNull(testService1Unregistered, "Service was found, it should have been unregistered");
            Assert.AreEqual(activeServiceCount, ServiceManager.Instance.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_06_06_UnregisterServiceWithDataProviderByInterface()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeServiceCount = ServiceManager.Instance.ActiveServices.Count;

            // Register service 1 and data provider
            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);
            ServiceManager.Instance.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(testService1));

            // Unregister Service
            var serviceUnregister = ServiceManager.Instance.TryUnregisterServicesOfType<ITestService>();

            // Try and retrieve unregistered Service
            var testService1Unregistered = ServiceManager.Instance.GetService<ITestService>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestService)} service.");

            // Try and retrieve unregistered Data Provider
            var testDataProvider1Unregistered = ServiceManager.Instance.GetService<ITestDataProvider1>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestDataProvider1)} service.");

            // Tests
            Assert.IsNotNull(testService1, "Test service was not registered");
            Assert.IsTrue(serviceUnregister, "Service was not unregistered correctly");
            Assert.IsNull(testService1Unregistered, "Service was found, it should have been unregistered");
            Assert.IsNull(testDataProvider1Unregistered, "Data Provider was found, it should have been unregistered");
            Assert.AreEqual(activeServiceCount, ServiceManager.Instance.ActiveServices.Count, "More or less services found than was expected");
        }


        [Test]
        public void Test_06_07_UnregisterServiceWithMultipleDataProviderByInterface()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeServiceCount = ServiceManager.Instance.ActiveServices.Count;

            // Register service 1 and data provider
            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);
            ServiceManager.Instance.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(testService1));
            ServiceManager.Instance.TryRegisterService<ITestDataProvider2>(new TestDataProvider2(testService1));

            // Unregister Service
            var serviceUnregister = ServiceManager.Instance.TryUnregisterServicesOfType<ITestService>();

            // Try and retrieve unregistered Service
            var testService1Unregistered = ServiceManager.Instance.GetService<ITestService>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestService)} service.");

            // Try and retrieve unregistered Data Provider
            var testDataProvider1Unregistered = ServiceManager.Instance.GetService<ITestDataProvider1>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestDataProvider1)} service.");
            var testDataProvider2Unregistered = ServiceManager.Instance.GetService<ITestDataProvider2>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestDataProvider2)} service.");


            // Tests
            Assert.IsNotNull(testService1, "Test service was not registered");
            Assert.IsTrue(serviceUnregister, "Service was not unregistered correctly");
            Assert.IsNull(testService1Unregistered, "Service was found, it should have been unregistered");
            Assert.IsNull(testDataProvider1Unregistered, "Data Provider 1 was found, it should have been unregistered");
            Assert.IsNull(testDataProvider2Unregistered, "Data Provider 2 was found, it should have been unregistered");
            Assert.AreEqual(activeServiceCount, ServiceManager.Instance.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_06_08_UnregisterSingleServiceFromMultipleByInterface()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeServiceCount = ServiceManager.Instance.ActiveServices.Count;

            // Register service 1
            ServiceManager.Instance.TryRegisterService<ITestService>(new TestService1());

            // Register service 2
            ServiceManager.Instance.TryRegisterService<ITestService2>(new TestService2());

            // Unregister Service 1
            var serviceUnregister = ServiceManager.Instance.TryUnregisterServicesOfType<ITestService>();

            // Try and retrieve unregistered Service 1
            var testService1Unregistered = ServiceManager.Instance.GetService<ITestService>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestService)} service.");

            // Try and retrieve still registered Service 2
            var testService2 = ServiceManager.Instance.GetService<ITestService2>();

            // Tests
            Assert.IsNotNull(testService2, "Test service 2 was not registered");
            Assert.IsTrue(serviceUnregister, "Service was not unregistered correctly");
            Assert.IsNull(testService1Unregistered, "Service was found, it should have been unregistered");
            Assert.AreEqual(activeServiceCount + 1, ServiceManager.Instance.ActiveServices.Count, "More or less services found than was expected");
        }


        #endregion 06 Service unRegistration

        #region 07 Data Provider unRegistration

        [Test]
        public void Test_07_01_UnregisterSingleDataProvider()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeServiceCount = ServiceManager.Instance.ActiveServices.Count;

            // Register service 1 and data provider
            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);
            ServiceManager.Instance.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(testService1));

            // Retrieve registered data provider
            var dataProvider1 = ServiceManager.Instance.GetService<ITestDataProvider1>();

            // Unregister data provider from service
            testService1.UnRegisterDataProvider(dataProvider1);

            // Try and retrieve unregistered data provider
            var testDataProvider1Unregistered = ServiceManager.Instance.GetService<ITestDataProvider1>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestDataProvider1)} service.");

            // Tests
            Assert.IsNotNull(dataProvider1, "Test data provider was not registered");
            Assert.IsNull(testDataProvider1Unregistered, "Data Provider was found, it should have been unregistered");
            Assert.AreEqual(testService1.DataProviders.Count, 0, "DataProvider Count after being unregistered does not match");
            Assert.AreEqual(activeServiceCount + 1, ServiceManager.Instance.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_07_02_UnregisterSingleDataProviderFromMultiple()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeServiceCount = ServiceManager.Instance.ActiveServices.Count;

            // Register service 1 and data providers
            var testService1 = new TestService1();
            var serviceRegistration = ServiceManager.Instance.TryRegisterService<ITestService>(testService1);
            var dataprovider1Registration = ServiceManager.Instance.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(testService1));
            var dataprovider2Registration = ServiceManager.Instance.TryRegisterService<ITestDataProvider2>(new TestDataProvider2(testService1));

            // Retrieve registered data provider
            var dataProvider1 = ServiceManager.Instance.GetService<ITestDataProvider1>();

            // Unregister data provider from service
            testService1.UnRegisterDataProvider(dataProvider1);

            // Try and retrieve unregistered data provider
            var testDataProvider1Unregistered = ServiceManager.Instance.GetService<ITestDataProvider1>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestDataProvider1)} service.");

            // Try and retrieve still registered Service
            var testDataProvider2Unregistered = ServiceManager.Instance.GetService<ITestDataProvider2>();

            // Tests
            Assert.IsNotNull(dataProvider1, "Test data provider was not registered");
            Assert.IsNull(testDataProvider1Unregistered, "Data Provider was found, it should have been unregistered");
            Assert.IsNotNull(testDataProvider2Unregistered, "Data Provider was not found, it should still be registered");
            Assert.AreEqual(testService1.DataProviders.Count, 1, "DataProvider Count after being unregistered does not match");
            Assert.AreEqual(activeServiceCount + 2, ServiceManager.Instance.ActiveServices.Count, "More or less services found than was expected");
        }        

        [Test]
        public void Test_07_03_UnregisterSingleDataProviderFromSecondService()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeServiceCount = ServiceManager.Instance.ActiveServices.Count;

            // Register service 1 and data providers
            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);
            ServiceManager.Instance.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(testService1));

            // Register service 2 and data providers
            var testService2 = new TestService2();
            ServiceManager.Instance.TryRegisterService<ITestService2>(testService2);
            ServiceManager.Instance.TryRegisterService<ITestDataProvider2>(new TestDataProvider2(testService2));            

            // Retrieve registered data provider
            var dataProvider1 = ServiceManager.Instance.GetService<ITestDataProvider1>();

            // Unregister data provider from service
            testService1.UnRegisterDataProvider(dataProvider1);

            // Try and retrieve unregistered data provider
            var testDataProvider1Unregistered = ServiceManager.Instance.GetService<ITestDataProvider1>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestDataProvider1)} service.");

            // Try and retrieve still registered Service
            var testDataProvider2Unregistered = ServiceManager.Instance.GetService<ITestDataProvider2>();            

            // Tests
            Assert.IsNotNull(dataProvider1, "Test data provider was not registered");
            Assert.IsNull(testDataProvider1Unregistered, "Data Provider was found, it should have been unregistered");
            Assert.AreEqual(testService2.DataProviders.Count, 1, "DataProvider Count after being unregistered does not match");
            Assert.AreEqual(activeServiceCount + 3, ServiceManager.Instance.ActiveServices.Count, "More or less services found than was expected");
        }  

        [Test]
        public void Test_07_04_UnregisterDataProviderDirect()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeServiceCount = ServiceManager.Instance.ActiveServices.Count;

            // Register service 1 and data providers
            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);
            ServiceManager.Instance.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(testService1));

            // Retrieve registered data provider
            var dataProvider1 = ServiceManager.Instance.GetService<ITestDataProvider1>();

            // Try and retrieve unregistered data provider direct
            var dataproviderUnregister = ServiceManager.Instance.TryUnregisterService<ITestDataProvider1>(dataProvider1);

            // Try and retrieve unregistered data provider
            var testDataProvider1Unregistered = ServiceManager.Instance.GetService<ITestDataProvider1>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestDataProvider1)} service.");

            // Tests
            Assert.IsNotNull(dataProvider1, "Test data provider was not registered");
            Assert.IsTrue(dataproviderUnregister, "Service was not unregistered correctly");
            Assert.IsNull(testDataProvider1Unregistered, "Data Provider was found, it should have been unregistered");
            Assert.AreEqual(testService1.DataProviders.Count, 0, "DataProvider Count after being unregistered does not match");
            Assert.AreEqual(activeServiceCount + 1, ServiceManager.Instance.ActiveServices.Count, "More or less services found than was expected");
        }

        #endregion 07 Data Provider unRegistration

        #region 08 Disable Running Services

        [Test]
        public void Test_08_01_ServiceDisable()
        {
            TestUtilities.InitializeServiceManagerScene();

            // Register
            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);

            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when it was started");

            ServiceManager.Instance.DisableService<ITestService>();

            // Retrieve
            var testService1Retrieval = ServiceManager.Instance.GetService<ITestService>();

            // Tests
            Assert.IsFalse(testService1.IsEnabled, "Test service was in a enabled state when it was disabled via interface call");
            Assert.IsFalse(testService1Retrieval.IsEnabled, "Test service was in a enabled state when it was disabled after retrieval.");
        }

        [Test]
        public void Test_08_02_ServiceDisableByName()
        {
            TestUtilities.InitializeServiceManagerScene();

            // Register
            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);

            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when it was started");

            ServiceManager.Instance.DisableService<ITestService>(TestService1.TestName);

            // Retrieve
            var testService1Retrieval = ServiceManager.Instance.GetService<ITestService>();

            // Tests
            Assert.IsFalse(testService1.IsEnabled, "Test service was in a enabled state when it was disabled via interface call");
            Assert.IsFalse(testService1Retrieval.IsEnabled, "Test service was in a enabled state when it was disabled after retrieval.");
        }

        [Test]
        public void Test_08_03_ServiceDisableDirect()
        {
            TestUtilities.InitializeServiceManagerScene();

            // Register
            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);

            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when it was started");

            testService1.Disable();

            // Retrieve
            var testService1Retrieval = ServiceManager.Instance.GetService<ITestService>();

            // Tests
            Assert.IsFalse(testService1.IsEnabled, "Test service was in a enabled state when it was disabled");
            Assert.IsFalse(testService1Retrieval.IsEnabled, "Test service was in a enabled state when it was disabled after retrieval.");
        }

        [Test]
        public void Test_08_04_DisableAllServices()
        {
            TestUtilities.InitializeServiceManagerScene();

            // Register
            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);
            var testService2 = new TestService2();
            ServiceManager.Instance.TryRegisterService<ITestService2>(testService2);

            ServiceManager.Instance.DisableAllServices();

            // Retrieve
            var testService1Retrieval = ServiceManager.Instance.GetService<ITestService>();
            var testService2Retrieval = ServiceManager.Instance.GetService<ITestService2>();

            // Tests
            Assert.IsFalse(testService1.IsEnabled, "Test service was in a enabled state when it was disabled");
            Assert.IsFalse(testService2.IsEnabled, "Test service was in a enabled state when it was disabled");
            Assert.IsFalse(testService1Retrieval.IsEnabled, "Test service was in a enabled state when it was disabled after retrieval.");
            Assert.IsFalse(testService2Retrieval.IsEnabled, "Test service was in a enabled state when it was disabled after retrieval.");
        }

        [Test]
        public void Test_08_05_ServiceDisablePriorToRegistration()
        {
            TestUtilities.InitializeServiceManagerScene();

            // Create disabled
            var testService1 = new TestService1();
            testService1.Disable();

            Assert.IsFalse(testService1.IsEnabled, "Test service was in a enabled state, but was registered disabled");

            // Register
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);

            Assert.IsFalse(testService1.IsEnabled, "Test service was in a enabled state, but was registered disabled after registration");

            // Retrieve
            var testService1Retrieval = ServiceManager.Instance.GetService<ITestService>();

            // Tests
            Assert.IsFalse(testService1.IsEnabled, "Test service was in a enabled state when it was disabled");
            Assert.IsFalse(testService1Retrieval.IsEnabled, "Test service was in a enabled state when it was disabled after retrieval.");
        }

        #endregion 08 Disable Running Services

        #region 09 Disable Running Data Provider

        [Test]
        public void Test_09_01_DataProviderDisable()
        {
            TestUtilities.InitializeServiceManagerScene();

            // Register
            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);
            var dataProvider1 = new TestDataProvider1(testService1);
            ServiceManager.Instance.TryRegisterService<ITestDataProvider1>(dataProvider1);

            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when it was started");
            Assert.IsTrue(dataProvider1.IsEnabled, "Test data provider was in a disabled state when it was started");

            ServiceManager.Instance.DisableService<ITestDataProvider1>();

            // Retrieve
            var dataProvidertest1Retrieval = ServiceManager.Instance.GetService<ITestDataProvider1>();

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when the data provider was disabled, should still be enabled");
            Assert.IsFalse(dataProvider1.IsEnabled, "Test data provider was in a enabled state when it was disabled");
            Assert.IsFalse(dataProvidertest1Retrieval.IsEnabled, "Test data provider was in a enabled state when it was disabled after retrieval.");
        }

        [Test]
        public void Test_09_02_DataProviderDisableByName()
        {
            TestUtilities.InitializeServiceManagerScene();

            // Register
            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);
            var dataProvider1 = new TestDataProvider1(testService1);
            ServiceManager.Instance.TryRegisterService<ITestDataProvider1>(dataProvider1);

            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when it was started");
            Assert.IsTrue(dataProvider1.IsEnabled, "Test data provider was in a disabled state when it was started");

            ServiceManager.Instance.DisableService<ITestDataProvider1>(TestDataProvider1.TestName);

            // Retrieve
            var dataProvidertest1Retrieval = ServiceManager.Instance.GetService<ITestDataProvider1>();

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when the data provider was disabled, should still be enabled");
            Assert.IsFalse(dataProvider1.IsEnabled, "Test data provider was in a enabled state when it was disabled");
            Assert.IsFalse(dataProvidertest1Retrieval.IsEnabled, "Test data provider was in a enabled state when it was disabled after retrieval.");
        }

        [Test]
        public void Test_09_03_DataProviderDisableDirect()
        {
            TestUtilities.InitializeServiceManagerScene();

            // Register
            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);
            var dataProvider1 = new TestDataProvider1(testService1);
            ServiceManager.Instance.TryRegisterService<ITestDataProvider1>(dataProvider1);

            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when it was started");
            Assert.IsTrue(dataProvider1.IsEnabled, "Test data provider was in a disabled state when it was started");

            dataProvider1.Disable();

            // Retrieve
            var dataProvidertest1Retrieval = ServiceManager.Instance.GetService<ITestDataProvider1>();

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when the data provider was disabled, should still be enabled");
            Assert.IsFalse(dataProvider1.IsEnabled, "Test data provider was in a enabled state when it was disabled");
            Assert.IsFalse(dataProvidertest1Retrieval.IsEnabled, "Test data provider was in a enabled state when it was disabled after retrieval.");
        }

        [Test]
        public void Test_09_04_DataProviderDisabledWithServices()
        {
            TestUtilities.InitializeServiceManagerScene();

            // Register service 1 and data provider
            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);
            ServiceManager.Instance.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(testService1));

            // Register service 2 and data provider
            var testService2 = new TestService2();
            ServiceManager.Instance.TryRegisterService<ITestService2>(testService2);
            ServiceManager.Instance.TryRegisterService<ITestDataProvider2>(new TestDataProvider2(testService2));

            ServiceManager.Instance.DisableAllServices();

            // Retrieve
            var dataProvidertest1Retrieval = ServiceManager.Instance.GetService<ITestDataProvider1>();
            var dataProvidertest2Retrieval = ServiceManager.Instance.GetService<ITestDataProvider2>();

            // Tests
            Assert.IsFalse(testService1.IsEnabled, "Test service 1 was in a enabled state when it was disabled");
            Assert.IsFalse(testService2.IsEnabled, "Test service 2 was in a enabled state when it was disabled");
            Assert.IsFalse(dataProvidertest1Retrieval.IsEnabled, "Test data provider was in a enabled state when it was disabled after retrieval.");
            Assert.IsFalse(dataProvidertest2Retrieval.IsEnabled, "Test data provider was in a enabled state when it was disabled after retrieval.");
        }

        [Test]
        public void Test_09_05_DataProviderDisablePriorToRegistration()
        {
            TestUtilities.InitializeServiceManagerScene();

            // Create Serivce
            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);

            // Create disabled data provider
            var dataProvider1 = new TestDataProvider1(testService1);
            dataProvider1.Disable();

            Assert.IsFalse(dataProvider1.IsEnabled, "Test data provider was in a enabled state when it was started disabled");

            // Register data provider
            ServiceManager.Instance.TryRegisterService<ITestDataProvider1>(dataProvider1);

            Assert.IsFalse(dataProvider1.IsEnabled, "Test data provider was in a enabled state when it was started disabled after registration");

            // Retrieve
            var dataProvidertest1Retrieval = ServiceManager.Instance.GetService<ITestDataProvider1>();

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when the data provider was disabled, should still be enabled");
            Assert.IsFalse(dataProvider1.IsEnabled, "Test data provider was in a enabled state when it was started disabled");
            Assert.IsFalse(dataProvidertest1Retrieval.IsEnabled, "Test data provider was in a enabled state when it was started disabled after retrieval.");
        }

        #endregion 09 Disable Running Data Provider

        #region 10 Enable Service

        [Test]
        public void Test_10_01_ServiceEnable()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeServiceCount = ServiceManager.Instance.ActiveServices.Count;

            // Register
            var testService1 = new TestService1();
            testService1.Disable();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);

            Assert.IsFalse(testService1.IsEnabled, "Test service was in a enabled state when it was disabled");

            ServiceManager.Instance.EnableService<ITestService>();

            // Retrieve
            var testService1Retrieval = ServiceManager.Instance.GetService<ITestService>();

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when it was enabled");
            Assert.IsTrue(testService1Retrieval.IsEnabled, "Test service was in a disabled state when it was enabled after retrieval.");
        }

        [Test]
        public void Test_10_02_ServiceEnableByName()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeServiceCount = ServiceManager.Instance.ActiveServices.Count;

            // Register
            var testService1 = new TestService1();
            testService1.Disable();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);

            Assert.IsFalse(testService1.IsEnabled, "Test service was in a enabled state when it was disabled");

            ServiceManager.Instance.EnableService<ITestService>(TestService1.TestName);

            // Retrieve
            var testService1Retrieval = ServiceManager.Instance.GetService<ITestService>();

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when it was enabled");
            Assert.IsTrue(testService1Retrieval.IsEnabled, "Test service was in a disabled state when it was enabled after retrieval.");
        }

        [Test]
        public void Test_10_03_ServiceEnableDirect()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeServiceCount = ServiceManager.Instance.ActiveServices.Count;

            // Register
            var testService1 = new TestService1();
            testService1.Disable();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);

            Assert.IsFalse(testService1.IsEnabled, "Test service was in a enabled state when it was disabled");

            testService1.Enable();

            // Retrieve
            var testService1Retrieval = ServiceManager.Instance.GetService<ITestService>();

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when it was enabled");
            Assert.IsTrue(testService1Retrieval.IsEnabled, "Test service was in a disabled state when it was enabled after retrieval.");
        }

        [Test]
        public void Test_10_04_EnableAllServices()
        {
            TestUtilities.InitializeServiceManagerScene();

            var activeServiceCount = ServiceManager.Instance.ActiveServices.Count;

            // Register
            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);
            var testService2 = new TestService2();
            ServiceManager.Instance.TryRegisterService<ITestService2>(testService2);

            ServiceManager.Instance.DisableAllServices();

            Assert.IsFalse(testService1.IsEnabled, "Test service was in a enabled state when it was disabled");
            Assert.IsFalse(testService2.IsEnabled, "Test service was in a enabled state when it was disabled");

            ServiceManager.Instance.EnableAllServices();

            // Retrieve
            var testService1Retrieval = ServiceManager.Instance.GetService<ITestService>();
            var testService2Retrieval = ServiceManager.Instance.GetService<ITestService2>();

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when it was enabled");
            Assert.IsTrue(testService2.IsEnabled, "Test service was in a disabled state when it was enabled");
            Assert.IsTrue(testService1Retrieval.IsEnabled, "Test service was in a disabled state when it was enabled after retrieval.");
            Assert.IsTrue(testService2Retrieval.IsEnabled, "Test service was in a disabled state when it was enabled after retrieval.");
        }

        #endregion 10 Enable Service

        #region 11 Enable Data Provider

        [Test]
        public void Test_11_01_DataProviderEnable()
        {
            TestUtilities.InitializeServiceManagerScene();

            // Register
            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);

            var dataProvider1 = new TestDataProvider1(testService1);
            ServiceManager.Instance.TryRegisterService<ITestDataProvider1>(dataProvider1);
            dataProvider1.Disable();

            Assert.IsFalse(dataProvider1.IsEnabled, "Test data provider was in a enabled state when it was disabled");

            // Retrieve
            var dataProvidertest1Retrieval = ServiceManager.Instance.GetService<ITestDataProvider1>();

            ServiceManager.Instance.EnableService<ITestDataProvider1>();

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when the data provider was disabled, should still be enabled");
            Assert.IsTrue(dataProvider1.IsEnabled, "Test data provider was in a disabled state when it was enabled");
            Assert.IsTrue(dataProvidertest1Retrieval.IsEnabled, "Test data provider was in a disabled state when it was enabled after retrieval.");
        }

        [Test]
        public void Test_11_02_DataProviderEnableByName()
        {
            TestUtilities.InitializeServiceManagerScene();

            // Register
            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);

            var dataProvider1 = new TestDataProvider1(testService1);
            ServiceManager.Instance.TryRegisterService<ITestDataProvider1>(dataProvider1);
            dataProvider1.Disable();

            Assert.IsFalse(dataProvider1.IsEnabled, "Test data provider was in a enabled state when it was disabled");

            // Retrieve
            var dataProvidertest1Retrieval = ServiceManager.Instance.GetService<ITestDataProvider1>();

            ServiceManager.Instance.EnableService<ITestDataProvider1>(TestDataProvider1.TestName);

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when the data provider was disabled, should still be enabled");
            Assert.IsTrue(dataProvider1.IsEnabled, "Test data provider was in a disabled state when it was enabled");
            Assert.IsTrue(dataProvidertest1Retrieval.IsEnabled, "Test data provider was in a disabled state when it was enabled after retrieval.");
        }

        [Test]
        public void Test_11_03_DataProviderEnableDirect()
        {
            TestUtilities.InitializeServiceManagerScene();

            // Register
            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);

            var dataProvider1 = new TestDataProvider1(testService1);
            ServiceManager.Instance.TryRegisterService<ITestDataProvider1>(dataProvider1);
            dataProvider1.Disable();

            Assert.IsFalse(dataProvider1.IsEnabled, "Test data provider was in a enabled state when it was disabled");

            // Retrieve
            var dataProvidertest1Retrieval = ServiceManager.Instance.GetService<ITestDataProvider1>();

            dataProvidertest1Retrieval.Enable();

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when the data provider was disabled, should still be enabled");
            Assert.IsTrue(dataProvider1.IsEnabled, "Test data provider was in a disabled state when it was enabled");
            Assert.IsTrue(dataProvidertest1Retrieval.IsEnabled, "Test data provider was in a disabled state when it was enabled after retrieval.");
        }

        [Test]
        public void Test_11_04_DataProviderEnabledWithServices()
        {
            TestUtilities.InitializeServiceManagerScene();

            // Register service 1 and data provider
            var testService1 = new TestService1();
            ServiceManager.Instance.TryRegisterService<ITestService>(testService1);
            ServiceManager.Instance.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(testService1));

            // Register service 2 and data provider
            var testService2 = new TestService2();
            ServiceManager.Instance.TryRegisterService<ITestService2>(testService2);
            ServiceManager.Instance.TryRegisterService<ITestDataProvider2>(new TestDataProvider2(testService2));

            ServiceManager.Instance.DisableAllServices();

            // Retrieve
            var dataProvidertest1Retrieval = ServiceManager.Instance.GetService<ITestDataProvider1>();
            var dataProvidertest2Retrieval = ServiceManager.Instance.GetService<ITestDataProvider2>();

            Assert.IsFalse(dataProvidertest1Retrieval.IsEnabled, "Test data provider was in a enabled state when it was disabled");
            Assert.IsFalse(dataProvidertest1Retrieval.IsEnabled, "Test data provider was in a enabled state when it was disabled");

            ServiceManager.Instance.EnableAllServices();

            // Tests
            Assert.IsTrue(testService1.IsEnabled, "Test service 1 was in a disabled state when it was enabled");
            Assert.IsTrue(testService2.IsEnabled, "Test service 2 was in a disabled state when it was enabled");
            Assert.IsTrue(dataProvidertest1Retrieval.IsEnabled, "Test data provider was in a disabled state when it was enabled after retrieval.");
            Assert.IsTrue(dataProvidertest2Retrieval.IsEnabled, "Test data provider was in a disabled state when it was enabled after retrieval.");
        }

            #endregion 11 Enable Data Provider
    }
}