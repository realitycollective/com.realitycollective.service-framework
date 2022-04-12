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
        #region Service Locater

        [Test]
        public void Test_01_01_InitializeServiceManager()
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            ServiceManager.ConfirmInitialized();
            
            var gameObject = GameObject.Find(nameof(ServiceManager));
            Assert.AreEqual(nameof(ServiceManager), gameObject.name, "Service Manager not found");
        }
        
        [Test]
        public void Test_01_02_TestNoProfileFound()
        {
            // Setup
            TestUtilities.CleanupScene();
            Assert.IsFalse(ServiceManager.IsInitialized, "Service Manager initialized when it should not be");
            ServiceManager.ConfirmInitialized();
            Assert.IsNotNull(ServiceManager.Instance, "Service Manager instance not found");
            Assert.IsTrue(ServiceManager.IsInitialized, "Service Manager was not initialized");

            ServiceManager.Instance.ActiveProfile = null;

            // Tests
            Assert.AreEqual(0, ServiceManager.ActiveServices.Count, "Service Manager services were found where none should exist");
            Assert.IsFalse(ServiceManager.HasActiveProfile, "Profile found for the Service Manager where none should exist");
            Assert.IsNull(ServiceManager.Instance.ActiveProfile, "Profile found for the Service Manager where none should exist for instance");
            LogAssert.Expect(LogType.Error, $"No {nameof(ServiceManagerRootProfile)} found, cannot initialize the {nameof(ServiceManager)}");
        }
        
        [Test]
        public void Test_01_03_CreateServiceManager()
        {
            TestUtilities.InitializeServiceManagerScene(false);

            // Tests
            Assert.AreEqual(0, ServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        #endregion Service Locater

        #region Service Registration

        [Test]
        public void Test_02_01_RegisterService()
        {
            TestUtilities.InitializeServiceManagerScene(false);

            var activeServiceCount = ServiceManager.ActiveServices.Count;

            var testService1 = new TestService1();

            // Register
            var serviceResult = ServiceManager.TryRegisterService<ITestService>(testService1);

            // Tests
            Assert.IsTrue(serviceResult, "Test service was not registered");
            Assert.IsTrue(testService1.ServiceGuid != System.Guid.Empty, "No GUID generated for the test service");
            Assert.IsTrue(activeServiceCount + 1 == ServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }


        [Test]
        public void Test_02_02_TryRegisterServiceTwice()
        {
            TestUtilities.InitializeServiceManagerScene(false);

            var activeServiceCount = ServiceManager.ActiveServices.Count;

            // Register
            ServiceManager.TryRegisterService<ITestService>(new TestService1());

            // Register
            var testService2 = ServiceManager.TryRegisterService<ITestService>(new TestService1());
            LogAssert.Expect(LogType.Error, $"There is already a [{nameof(ITestService)}.{TestService1.TestName}] registered!");

            // Tests
            Assert.IsFalse(testService2, "Test service was registered when it should not have been");
            Assert.IsTrue(activeServiceCount + 1 == ServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        #endregion Service Registration

        #region Data Provider Registration

        [Test]
        public void Test_03_01_RegisterServiceAndDataProvider()
        {
            TestUtilities.InitializeServiceManagerScene(false);

            var activeServiceCount = ServiceManager.ActiveServices.Count;

            var testService1 = new TestService1();

            ServiceManager.TryRegisterService<ITestService>(testService1);

            var testDataProvider = new TestDataProvider1(testService1);

            var dataProviderResult = ServiceManager.TryRegisterService<ITestDataProvider1>(testDataProvider);

            // Tests
            Assert.IsTrue(dataProviderResult, "Test data provider was not registered");
            Assert.IsTrue(testDataProvider.ServiceGuid != System.Guid.Empty);
            Assert.IsTrue(activeServiceCount + 2 == ServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_02_RegisterMultipleDataProviders()
        {
            TestUtilities.InitializeServiceManagerScene(false);

            var activeSystemCount = ServiceManager.ActiveServices.Count;

            // Register
            ServiceManager.TryRegisterService<ITestService>(new TestService1());

            // Retrieve
            var testService1 = ServiceManager.GetService<ITestService>();

            // Register
            var dataProvider1Result = ServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(testService1));
            var dataProvider2Result = ServiceManager.TryRegisterService<ITestDataProvider2>(new TestDataProvider2(testService1));

            // Tests
            Assert.IsTrue(dataProvider1Result, "Data Provider 1 was not registered");
            Assert.IsTrue(dataProvider2Result, "Data Provider 2 was not registered");
            Assert.IsTrue(activeSystemCount + 3 == ServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        #endregion Data Provider Registration

        #region Service Retrieval

        [Test]
        public void Test_04_01_RetrieveService()
        {
            TestUtilities.InitializeServiceManagerScene(false);

            var activeServiceCount = ServiceManager.ActiveServices.Count;

            // Register
            ServiceManager.TryRegisterService<ITestService>(new TestService1());

            // Retrieve
            var testService1 = ServiceManager.GetService<ITestService>();

            // Tests
            Assert.IsNotNull(testService1, "Test service was not found");
            Assert.IsTrue(activeServiceCount + 1 == ServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_04_02_RetrieveSecondService()
        {
            TestUtilities.InitializeServiceManagerScene(false);

            var activeServiceCount = ServiceManager.ActiveServices.Count;

            // Register Service 1
            ServiceManager.TryRegisterService<ITestService>(new TestService1());

            // Register Service 2
            ServiceManager.TryRegisterService<ITestService2>(new TestService2());

            // Retrieve
            var testService2 = ServiceManager.GetService<ITestService2>();

            // Tests
            Assert.IsNotNull(testService2, "Test service was not found");
            Assert.IsTrue(activeServiceCount + 2 == ServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_04_03_RetrieveServiceDoesNotExist()
        {
            TestUtilities.InitializeServiceManagerScene(false);

            var activeServiceCount = ServiceManager.ActiveServices.Count;

            // Retrieve
            var testService1 = ServiceManager.GetService<ITestService>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestService)} service.");

            // Tests
            Assert.IsNull(testService1, "Test service was found");
            Assert.IsTrue(activeServiceCount == ServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_04_04_RetrieveSecondServiceDoesNotExist()
        {
            TestUtilities.InitializeServiceManagerScene(false);

            var activeServiceCount = ServiceManager.ActiveServices.Count;

            // Register Service 1
            ServiceManager.TryRegisterService<ITestService>(new TestService1());

            // Retrieve
            var testService2 = ServiceManager.GetService<ITestService2>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestService2)} service.");

            // Tests
            Assert.IsNull(testService2, "Test service was not found");
            Assert.IsTrue(activeServiceCount + 1 == ServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        #endregion  Service Retrieval

        #region Data Provider Retrieval

        [Test]
        public void Test_05_01_RetrieveSingleDataProvider()
        {
            TestUtilities.InitializeServiceManagerScene(false);

            var activeServiceCount = ServiceManager.ActiveServices.Count;

            var testService1 = new TestService1();
            // Register
            ServiceManager.TryRegisterService<ITestService>(testService1);

            // Register
            ServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(testService1));

            // Retrieve
            var dataProvider1 = ServiceManager.GetService<ITestDataProvider1>();

            // Tests
            Assert.IsNotNull(dataProvider1, "Test data provider not found");
            Assert.IsTrue(activeServiceCount + 2 == ServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_05_02_RetrieveSecondDataProvider()
        {
            TestUtilities.InitializeServiceManagerScene(false);

            var activeSystemCount = ServiceManager.ActiveServices.Count;

            var testService1 = new TestService1();

            // Register
            ServiceManager.TryRegisterService<ITestService>(testService1);

            // Register
            ServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(testService1));
            ServiceManager.TryRegisterService<ITestDataProvider2>(new TestDataProvider2(testService1));

            // Retrieve all registered IDataProviders
            var dataProviders = ServiceManager.GetServices<IServiceDataProvider>();

            // Tests
            Assert.IsNotEmpty(dataProviders, "Data Providers were not registered");
            Assert.IsTrue(activeSystemCount + 3 == ServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_05_03_RetrieveAllDataProvidersForService()
        {
            TestUtilities.InitializeServiceManagerScene(false);

            var activeSystemCount = ServiceManager.ActiveServices.Count;

            var testService1 = new TestService1();

            // Register
            ServiceManager.TryRegisterService<ITestService>(testService1);

            // Register
            ServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(testService1));
            ServiceManager.TryRegisterService<ITestDataProvider2>(new TestDataProvider2(testService1));

            // Retrieve all registered IDataProviders

            var testService = ServiceManager.GetService<ITestService>();
            var dataProviders = testService.DataProviders;

            // Tests
            Assert.IsNotEmpty(dataProviders, "Data Providers were not registered");
            Assert.IsTrue(dataProviders.Count == 2, "Could not locate all data providers");
            Assert.IsTrue(activeSystemCount + 3 == ServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_05_04_RetrieveAllRegisteredDataProviders()
        {
            TestUtilities.InitializeServiceManagerScene(false);

            var activeSystemCount = ServiceManager.ActiveServices.Count;

            var testService1 = new TestService1();

            // Register
            ServiceManager.TryRegisterService<ITestService>(testService1);

            // Register
            ServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(testService1));

            var testService2 = new TestService2();

            // Register
            ServiceManager.TryRegisterService<ITestService>(testService2);

            // Register
            ServiceManager.TryRegisterService<ITestDataProvider2>(new TestDataProvider2(testService2));

            // Retrieve all registered IServiceDataProvider
            var dataProviders = ServiceManager.GetServices<IServiceDataProvider>();

            // Tests
            Assert.IsNotEmpty(dataProviders, "Data Providers were not registered");
            Assert.IsTrue(dataProviders.Count == 2, "Could not locate all data providers");
            Assert.IsTrue(activeSystemCount + 4 == ServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_05_05_ServiceDataProviderDoesNotExist()
        {
            TestUtilities.InitializeServiceManagerScene(false);

            // Register
            ServiceManager.TryRegisterService<ITestService>(new TestService1());

            var testService1 = ServiceManager.GetService<ITestService>();

            // Add test data provider 1
            ServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(testService1));

            // Validate non-existent data provider 2
            var isServiceRegistered = ServiceManager.IsServiceRegistered<ITestDataProvider2>();

            // Tests
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestDataProvider2)} service.");
            Assert.IsFalse(isServiceRegistered, "Service was found when it was meant to be unregistered");
        }

        #endregion Data Provider Retrieval

        // Test to validate ALL data providers are not destroyed when a service is destroyed
        // Test to check a services data providers are accurate when a provider is destroyed independently 

        // Test Service Removal
        // Test Data Provider Removal

        #region Service unRegistration

        [Test]
        public void Test_06_01_UnregisterSingleService()
        {
            TestUtilities.InitializeServiceManagerScene(false);

            var activeServiceCount = ServiceManager.ActiveServices.Count;

            ServiceManager.TryRegisterService<ITestService>(new TestService1());

            var testService1 = ServiceManager.GetService<ITestService>();

            var serviceUnregistration = ServiceManager.TryUnregisterService<ITestService>(testService1);

            var testService1Unregistered = ServiceManager.GetService<ITestService>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestService)} service.");

            // Tests
            Assert.IsNotNull(testService1, "Test service was not registered");
            Assert.IsTrue(serviceUnregistration, "Service was not unregistered correctly");
            Assert.IsNull(testService1Unregistered, "Service was found, it should have been unregistered");
            Assert.IsTrue(activeServiceCount == ServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_06_02_UnregisterServiceWithDataProvider()
        {
            TestUtilities.InitializeServiceManagerScene(false);

            var activeServiceCount = ServiceManager.ActiveServices.Count;

            ServiceManager.TryRegisterService<ITestService>(new TestService1());

            var testService1 = ServiceManager.GetService<ITestService>();

            ServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(testService1));

            var dataProvider1 = ServiceManager.GetService<ITestDataProvider1>();

            var serviceUnregistration = ServiceManager.TryUnregisterService<ITestService>(testService1);

            var testService1Unregistered = ServiceManager.GetService<ITestService>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestService)} service.");

            var testDataProvider1Unregistered = ServiceManager.GetService<ITestDataProvider1>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestDataProvider1)} service.");

            // Tests
            Assert.IsNotNull(testService1, "Test service was not registered");
            Assert.IsNotNull(dataProvider1, "Test data provider was not registered");
            Assert.IsTrue(serviceUnregistration, "Service was not unregistered correctly");
            Assert.IsNull(testService1Unregistered, "Service was found, it should have been unregistered");
            Assert.IsNull(testDataProvider1Unregistered, "Data Provider was found, it should have been unregistered");
            Assert.IsTrue(activeServiceCount == ServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_06_03_UnregisterSingleServiceFromMultiple()
        {
            TestUtilities.InitializeServiceManagerScene(false);

            var activeServiceCount = ServiceManager.ActiveServices.Count;

            ServiceManager.TryRegisterService<ITestService>(new TestService1());
            ServiceManager.TryRegisterService<ITestService2>(new TestService2());

            var testService1 = ServiceManager.GetService<ITestService>();

            var serviceUnregistration = ServiceManager.TryUnregisterService<ITestService>(testService1);

            var testService1Unregistered = ServiceManager.GetService<ITestService>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestService)} service.");

            var testService2 = ServiceManager.GetService<ITestService2>();

            // Tests
            Assert.IsNotNull(testService1, "Test service was not registered");
            Assert.IsNotNull(testService2, "Test service 2 was not registered");
            Assert.IsTrue(serviceUnregistration, "Service was not unregistered correctly");
            Assert.IsNull(testService1Unregistered, "Service was found, it should have been unregistered");
            Assert.IsTrue(activeServiceCount + 1 == ServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        #endregion Service unRegistration

        #region Data Provider unRegistration

        #endregion Data Provider unRegistration

        #region Disable Running Services

        #endregion Disable Running Services

        #region Disable Running Data Provider

        #endregion Disable Running Data Provider

        #region Enable previously disabled registered Service

        #endregion Enable previously disabled registered Service

        #region Enable previously disabled Data Provider

        #endregion Enable previously disabled Data Provider










        [Test]
        public void Test_xx04_03xx_UnregisterServiceAndServiceDataProvider()
        {
            TestUtilities.InitializeServiceManagerScene(false);

            var activeSystemCount = ServiceManager.ActiveServices.Count;

            // Register
            ServiceManager.TryRegisterService<ITestService>(new TestService1());

            // Retrieve
            var testService1 = ServiceManager.GetService<ITestService>();

            // Register
            ServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(testService1));

            // Retrieve
            var dataProvider1 = ServiceManager.GetService<ITestDataProvider1>();

            // Tests
            Assert.IsNotNull(testService1, "Test service was not found");
            Assert.IsNotNull(dataProvider1, "Test data provider not found");
            Assert.IsTrue(activeSystemCount + 2 == ServiceManager.ActiveServices.Count, "More or less services found than was expected");

            // Unregister
            var successService = ServiceManager.TryUnregisterServicesOfType<ITestService>();

            var successDataProvider = ServiceManager.TryUnregisterServicesOfType<ITestDataProvider1>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestDataProvider1)} service.");

            // Validate non-existent service
            var isServiceRegistered = ServiceManager.IsServiceRegistered<ITestService>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestService)} service.");

            var isDataProviderRegistered = ServiceManager.IsServiceRegistered<ITestDataProvider1>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestDataProvider1)} service.");

            // Tests
            Assert.IsTrue(successService, "Service was not unregistered successfully");
            Assert.IsFalse(successDataProvider,"Data provider was not unregistered successfully");
            Assert.IsFalse(isServiceRegistered, "Service was found when it was meant to be unregistered");
            Assert.IsFalse(isDataProviderRegistered, "Data Provider was found when it was meant to be unregistered");
            Assert.IsTrue(activeSystemCount == ServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }
        

        
        [Test]
        public void Test_xx04_05_UnregisterServiceDataProviders()
        {
            TestUtilities.InitializeServiceManagerScene(false);

            var activeSystemCount = ServiceManager.ActiveServices.Count;

            // Register
            ServiceManager.TryRegisterService<ITestService>(new TestService1());

            // Retrieve
            var testService1 = ServiceManager.GetService<ITestService>();

            // Validate
            Assert.IsNotNull(testService1, "Test service was not found service was not found");

            // Register
            ServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(testService1));
            ServiceManager.TryRegisterService<ITestDataProvider2>(new TestDataProvider2(testService1));

            // Retrieve all data providers
            var dataProviders = ServiceManager.GetServices<IServiceDataProvider>();

            // Tests
            Assert.IsTrue(dataProviders.Count == 2, "More or less data providers found than was expected");
            Assert.IsTrue(activeSystemCount + 3 == ServiceManager.ActiveServices.Count, "More or less services found than was expected");

            // Retrieve services
            var extensionService1 = ServiceManager.GetService<ITestDataProvider1>();
            var extensionService2 = ServiceManager.GetService<ITestDataProvider2>();

            // Validate
            Assert.IsNotNull(extensionService1, "Extension service 1 not found");
            Assert.IsNotNull(extensionService2, "Extension service 2 not found");

            // Unregister
            var successService = ServiceManager.TryUnregisterServicesOfType<ITestService>();
            var successDataProvider1 = ServiceManager.TryUnregisterServicesOfType<ITestDataProvider1>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestDataProvider1)} service.");

            var successDataProvider2 = ServiceManager.TryUnregisterServicesOfType<ITestDataProvider2>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestDataProvider2)} service.");


            // Tests
            Assert.IsTrue(successService, "Service was not unregistered successfully");
            Assert.IsFalse(successDataProvider1, "Data Provider 1 was not unregistered successfully");
            Assert.IsFalse(successDataProvider2, "Data Provider 2 was not unregistered successfully");

            // Validate non-existent service
            var isServiceRegistered = ServiceManager.IsServiceRegistered<ITestService>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestService)} service.");

            var isService1Registered = ServiceManager.IsServiceRegistered<ITestDataProvider1>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestDataProvider1)} service.");
            
            var isService2Registered = ServiceManager.IsServiceRegistered<ITestDataProvider2>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestDataProvider2)} service.");

            // Tests
            Assert.IsFalse(isServiceRegistered, "Service was found when it was meant to be unregistered");
            Assert.IsFalse(isService1Registered, "Data Provider 1 was found when it was meant to be unregistered");
            Assert.IsFalse(isService2Registered, "Data Provider 2 was found when it was meant to be unregistered");
            Assert.IsTrue(activeSystemCount == ServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }
        


        #region Service Retrieval Tests


        [Test]
        public void Test_xx06_01_TryGetDataProvider()
        {
            TestUtilities.InitializeServiceManagerScene(false);
            var activeSystemCount = ServiceManager.ActiveServices.Count;

            ServiceManager.TryRegisterService<ITestService>(new TestService1());
            var testService1 = ServiceManager.GetService<ITestService>();

            // Register
            ServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(testService1));

            // Retrieve
            var result = ServiceManager.TryGetService<ITestDataProvider1>(out var extensionService1);

            // Tests
            Assert.IsTrue(result, "Registered Service not found");
            Assert.IsNotNull(extensionService1, "Extension Service not found");
            Assert.IsTrue(activeSystemCount + 2 == ServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }


        [Test]
        public void Test_xx06_04_TryRetrieveMultipleDataProviders()
        {
            TestUtilities.InitializeServiceManagerScene(false);
            var initialSystemCount = ServiceManager.ActiveServices.Count;
            var expectedServicesToRegister = 3; // Registering a Service and two Data Providers

            ServiceManager.TryRegisterService<ITestService>(new TestService1());
            var testService1 = ServiceManager.GetService<ITestService>();

            // Register
            ServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(testService1));
            ServiceManager.TryRegisterService<ITestDataProvider2>(new TestDataProvider2(testService1));

            // Retrieve
            var resultTrue1 = ServiceManager.TryGetService<ITestDataProvider1>(TestDataProvider1.TestName, out var extensionService1);
            var resultTrue2 = ServiceManager.TryGetService<ITestDataProvider2>(TestDataProvider2.TestName, out var extensionService2);

            // Tests
            Assert.IsTrue(initialSystemCount + expectedServicesToRegister == ServiceManager.ActiveServices.Count, $"Active systems count mismatch, expected {initialSystemCount + expectedServicesToRegister} but found {ServiceManager.ActiveServices.Count}");
            Assert.IsTrue(resultTrue1, "Test Data Provider 1 found");
            Assert.IsTrue(resultTrue2, "Test Data Provider 2 found");
            Assert.IsNotNull(extensionService1, "Test Data Provider 1 service found");
            Assert.IsNotNull(extensionService2, "Test Data Provider 2 service found");
        }
        #endregion Service Retrieval Tests

        #region Service Enable/Disable Tests

        [Test]
        public void Test_07_01_EnableServices()
        {
            TestUtilities.InitializeServiceManagerScene(false);

            ServiceManager.TryRegisterService<ITestService>(new TestService1());
            var testService1 = ServiceManager.GetService<ITestService>();
            
            // Add test 1 services
            ServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(testService1));
            var provider1 = ServiceManager.GetService<ITestDataProvider1>();
            // Add test 2 services
            ServiceManager.TryRegisterService<ITestDataProvider2>(new TestDataProvider2(testService1));
            var provider2 = ServiceManager.GetService<ITestDataProvider2>();
            
            Assert.IsNotNull(testService1, "Test service was not reigstered");
            Assert.IsTrue(testService1.IsEnabled, "Test service found but was not enabled");
            Assert.IsNotNull(provider1, "Test provider 1 was not reigstered");
            Assert.IsNotNull(provider2, "Test provider 2 was not reigstered");
        }

        [Test]
        public void Test_07_02_DisableServices()
        {
            TestUtilities.InitializeServiceManagerScene(false);

            ServiceManager.TryRegisterService<ITestService>(new TestService1());
            var testService1 = ServiceManager.GetService<ITestService>();
            
            // Add test 1 services
            ServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(testService1));
            var provider1 = ServiceManager.GetService<ITestDataProvider1>();
            
            // Disable registered services
            testService1.Disable();

            Assert.IsNotNull(testService1, "Test service was not reigstered");
            Assert.IsFalse(testService1.IsEnabled, "Test service found but enabled after being disabled");
            Assert.IsNotNull(provider1, "Test provider 1 was not reigstered");
        }

        #endregion Service Enable/Disable Tests
    }
}