// Copyright (c) Reality Collective. All rights reserved.

using NUnit.Framework;
using RealityToolkit.ServiceFramework.Definitions;
using RealityToolkit.ServiceFramework.Definitions.Platforms;
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
            testServiceManager = new ServiceManager();
            testServiceManager.Initialize();
            //LogAssert.Expect(LogType.Warning, $"There are multiple instances of the ServiceManager in this project, is this expected?");

            var confirm = testServiceManager.ConfirmInitialized();

            var managerGameObject = GameObject.Find(nameof(ServiceManager));
            ServiceManagerInstance instance = managerGameObject.GetComponent<ServiceManagerInstance>();
            Assert.IsNotNull(managerGameObject, "No manager found in the scene");

            //This is supposed to fail but is not :S
            Assert.AreEqual(instance.Manager.ServiceManagerInstanceGuid, testServiceManager.ServiceManagerInstanceGuid, "Service Manager not found");
        }

        [Test]
        public void Test_01_01_InitializeServiceManagerInstance()
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var serviceManagerGameObject = new GameObject("ServiceManager");
            testServiceManager = new ServiceManager(serviceManagerGameObject);

            Assert.IsNotNull(testServiceManager, "Service Manager not created");
            Assert.IsTrue(testServiceManager.IsInitialized, "Manager not Initialised");
        }

        [Test]
        public void Test_01_01_InitializeServiceManagerGameObject()
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var serviceManagerGameObject = new GameObject(nameof(ServiceManager));
            var serviceManagerInstance = serviceManagerGameObject.AddComponent<ServiceManagerInstance>();
            serviceManagerInstance.InitialiseServiceManager();

            var serviceManagerGO = GameObject.Find(nameof(ServiceManager));
            ServiceManagerInstance instance = serviceManagerGO.GetComponent<ServiceManagerInstance>();
            Assert.IsNotNull(serviceManagerGO, "Unable to find ServiceManager GO");
            Assert.IsNotNull(instance, "Service Manager class not found on ServiceManager GO");
            Assert.IsNotNull(instance.Manager, "Service Manager not instantiated on ServiceManagerInstance class");
            Assert.IsTrue(instance.Manager.IsInitialized, "Manager not Initialised");
        }

        [Test]
        public void Test_01_02_TestNoProfileFound()
        {
            testServiceManager = null;

            // Setup
            TestUtilities.CleanupScene();
            TestUtilities.InitializeServiceManager(ref testServiceManager);
            testServiceManager.ConfirmInitialized();
            Assert.IsNotNull(testServiceManager, "Service Manager instance not found");
            Assert.IsTrue(testServiceManager.IsInitialized, "Service Manager was not initialized");

            testServiceManager.ActiveProfile = null;

            // Tests
            Assert.AreEqual(0, testServiceManager.ActiveServices.Count, "Service Manager services were found where none should exist");
            Assert.IsFalse(testServiceManager.HasActiveProfile, "Profile found for the Service Manager where none should exist");
            Assert.IsNull(testServiceManager.ActiveProfile, "Profile found for the Service Manager where none should exist for instance");
            LogAssert.Expect(LogType.Error, $"No {nameof(ServiceProvidersProfile)} found, cannot initialize the {nameof(ServiceManager)}");
        }
        
        [Test]
        public void Test_01_03_CreateServiceManager()
        {
            testServiceManager = null;

            TestUtilities.InitializeServiceManagerScene(ref testServiceManager,false);

            // Tests
            Assert.AreEqual(0, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        #endregion 01 Service Locater

        #region 02 Service Registration - Code

        [Test]
        public void Test_02_01_RegisterService()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            var testService1 = new TestService1();
            var serviceResult = testServiceManager.TryRegisterService<ITestService>(testService1);

            // Tests
            Assert.IsTrue(serviceResult, "Test service was not registered");
            Assert.IsTrue(testService1.ServiceGuid != System.Guid.Empty, "No GUID generated for the test service");
            Assert.AreEqual(activeServiceCount + 1, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_02_02_RegisterSecondService()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            var testService1 = new TestService1();
            var serviceResult = testServiceManager.TryRegisterService<ITestService>(testService1);

            var testService2 = new TestService2();
            var serviceResult2 = testServiceManager.TryRegisterService<ITestService2>(testService2);

            // Tests
            Assert.IsTrue(serviceResult2, "Test service 2 was not registered");
            Assert.IsTrue(testService2.ServiceGuid == System.Guid.Empty, "GUID found for the second test service when none configured");
            Assert.AreEqual(activeServiceCount + 2, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }        

        [Test]
        public void Test_02_03_TryRegisterServiceTwice()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register
            testServiceManager.TryRegisterService<ITestService>(new TestService1());

            // Register again
            var testService2 = testServiceManager.TryRegisterService<ITestService>(new TestService1());
            LogAssert.Expect(LogType.Error, $"There is already a [{nameof(ITestService)}.{TestService1.TestName}] registered!");

            // Tests
            Assert.IsFalse(testService2, "Test service was registered when it should not have been");
            Assert.AreEqual(activeServiceCount + 1, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        #endregion 02 Service Registration - Code

        #region 02 Service Registration - Config

        [Test]
        public void Test_02_04_RegisterServiceConfig()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            ITestService testService1;
            var config = new ServiceConfiguration<ITestService>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult = testServiceManager.TryCreateAndRegisterService<ITestService>(config, out testService1);

            // Tests
            Assert.IsTrue(serviceResult, "Test service was not registered");
            Assert.IsNotNull(testService1, "Test service instance was not returned");
            Assert.IsTrue(testService1.ServiceGuid != System.Guid.Empty, "No GUID generated for the test service");
            Assert.AreEqual(activeServiceCount + 1, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_02_05_RegisterSecondServiceConfigGeneric()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            ITestService testService1;
            var config = new ServiceConfiguration<ITestService>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult = testServiceManager.TryCreateAndRegisterService<ITestService>(config, out testService1);

            ITestService2 testService2;
            var config2 = new ServiceConfiguration<ITestService2>(typeof(TestService2), "Test Service2", 1, AllPlatforms.Platforms, null);
            var serviceResult2 = testServiceManager.TryCreateAndRegisterService<ITestService2>(config2, out testService2);            

            // Tests
            Assert.IsTrue(serviceResult2, "Test service 2 was not registered");
            Assert.IsNotNull(testService2, "Test service 2 instance was not returned");
            Assert.IsTrue(testService2.ServiceGuid == System.Guid.Empty, "GUID found for the second test service when none configured");
            Assert.AreEqual(activeServiceCount + 2, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }


        [Test]
        public void Test_02_05_RegisterSecondServiceConfigNonGeneric()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            ITestService testService1;
            var config = new ServiceConfiguration<ITestService>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult = testServiceManager.TryCreateAndRegisterService(config, out testService1);

            ITestService2 testService2;
            var config2 = new ServiceConfiguration<ITestService2>(typeof(TestService2), "Test Service2", 1, AllPlatforms.Platforms, null);
            var serviceResult2 = testServiceManager.TryCreateAndRegisterService(config2, out testService2);

            // Tests
            Assert.IsTrue(serviceResult2, "Test service 2 was not registered");
            Assert.IsNotNull(testService2, "Test service 2 instance was not returned");
            Assert.IsTrue(testService2.ServiceGuid == System.Guid.Empty, "GUID found for the second test service when none configured");
            Assert.AreEqual(activeServiceCount + 2, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_02_06_TryRegisterServiceTwiceConfig()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            ITestService testService1;
            var config = new ServiceConfiguration<ITestService>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult = testServiceManager.TryCreateAndRegisterService<ITestService>(config, out testService1);

            ITestService testService2;
            var config2 = new ServiceConfiguration<ITestService>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult2 = testServiceManager.TryCreateAndRegisterService<ITestService>(config, out testService2);
            LogAssert.Expect(LogType.Error, "There is already a [ITestService.Test Service 1] registered!");

            // Tests
            Assert.IsFalse(serviceResult2, "Test service was registered when it should not have been");
            Assert.IsNotNull(testService2, "Test service 1 instance was not returned when it should have been");
            Assert.IsTrue(testService2.GetType() == typeof(TestService1), "The wrong type of service was returned");
            Assert.AreEqual(activeServiceCount + 1, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_02_07_RegisterServiceConfigurations()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            var configurations = new ServiceConfiguration<IService>[2];

            configurations[0] = new ServiceConfiguration<IService>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            configurations[1] = new ServiceConfiguration<IService>(typeof(TestService2), TestService2.TestName, 1, AllPlatforms.Platforms, null);

            var result = testServiceManager.TryRegisterServiceConfigurations(configurations);

            // Tests
            Assert.IsTrue(result, "Test services were not registered");
            Assert.AreEqual(activeServiceCount + 2, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        #endregion 02 Service Registration - Config

        #region 03 Data Provider Registration - Code

        [Test]
        public void Test_03_01_RegisterServiceAndDataProvider()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService>(testService1);

            var testDataProvider = new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1);
            var dataProviderResult = testServiceManager.TryRegisterService<ITestDataProvider1>(testDataProvider);

            // Tests
            Assert.IsTrue(dataProviderResult, "Test data provider was not registered");
            Assert.IsTrue(testDataProvider.ServiceGuid != System.Guid.Empty);
            Assert.AreEqual(activeServiceCount + 2, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_02_RegisterMultipleDataProviders()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService>(new TestService1());

            // Register
            var dataProvider1Result = testServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1));
            var dataProvider2Result = testServiceManager.TryRegisterService<ITestDataProvider2>(new TestDataProvider2(TestDataProvider2.TestName, 0, null, testService1));

            // Tests
            Assert.IsTrue(dataProvider1Result, "Data Provider 1 was not registered");
            Assert.IsTrue(dataProvider2Result, "Data Provider 2 was not registered");
            Assert.AreEqual(activeSystemCount + 3, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_03_RegisterDataProviderInMultipleServices()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            // Register Service 1 and data provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService>(new TestService1());
            var dataProvider1Result = testServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1));

            // Register Service 1 and data provider
            var testService2 = new TestService2();
            testServiceManager.TryRegisterService<ITestService2>(new TestService2());
            var dataProvider2Result = testServiceManager.TryRegisterService<ITestDataProvider2>(new TestDataProvider2(TestDataProvider2.TestName, 0, null, testService2));

            // Tests
            Assert.IsTrue(dataProvider1Result, "Data Provider 1 was not registered");
            Assert.IsTrue(dataProvider2Result, "Data Provider 2 was not registered");
            Assert.AreEqual(activeSystemCount + 4, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_04_RegisterDataProviderMultipleTimes()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService>(new TestService1());

            // Register
            var dataProvider1Result = testServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1));
            var dataProvider2Result = testServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1));
            LogAssert.Expect(LogType.Error, "There is already a [ITestDataProvider1.Test Data Provider 1] registered!");

            // Tests
            Assert.IsTrue(dataProvider1Result, "Data Provider 1 was not registered");
            Assert.IsFalse(dataProvider2Result, "Data Provider 2 was registered when it should not have been");
            Assert.AreEqual(activeSystemCount + 2, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }        

        #endregion 03 Data Provider Registration - Code        

        #region 03 Data Provider Registration - Config

        [Test]
        public void Test_03_05_RegisterServiceAndDataProviderConfig()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            ITestService testService1;
            var config = new ServiceConfiguration<ITestService>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult = testServiceManager.TryCreateAndRegisterService<ITestService>(config, out testService1);

            var dataProviderconfig = new ServiceConfiguration<ITestDataProvider1>(typeof(TestDataProvider1), TestDataProvider1.TestName, 1, AllPlatforms.Platforms, null);
            var dataProviderResult = testServiceManager.TryCreateAndRegisterDataProvider<ITestDataProvider1>(dataProviderconfig, testService1);

            // Tests
            Assert.IsTrue(dataProviderResult, "Test data provider was not registered");
            Assert.AreEqual(activeServiceCount + 2, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_06_RegisterMultipleDataProvidersConfigGeneric()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            ITestService testService1;
            var config = new ServiceConfiguration<ITestService>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult = testServiceManager.TryCreateAndRegisterService<ITestService>(config, out testService1);

            var dataProvider1config = new ServiceConfiguration<ITestDataProvider1>(typeof(TestDataProvider1), TestDataProvider1.TestName, 1, AllPlatforms.Platforms, null);
            var dataProvider1Result = testServiceManager.TryCreateAndRegisterDataProvider<ITestDataProvider1>(dataProvider1config, testService1);

            var dataProvider2config = new ServiceConfiguration<ITestDataProvider2>(typeof(TestDataProvider2), TestDataProvider2.TestName, 1, AllPlatforms.Platforms, null);
            var dataProvider2Result = testServiceManager.TryCreateAndRegisterDataProvider<ITestDataProvider2>(dataProvider2config, testService1);

            // Tests
            Assert.IsTrue(dataProvider1Result, "Data Provider 1 was not registered");
            Assert.IsTrue(dataProvider2Result, "Data Provider 2 was not registered");
            Assert.AreEqual(activeSystemCount + 3, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_06_RegisterMultipleDataProvidersConfigNonGeneric()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            ITestService testService1;
            var config = new ServiceConfiguration<ITestService>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult = testServiceManager.TryCreateAndRegisterService(config, out testService1);

            var dataProvider1config = new ServiceConfiguration<ITestDataProvider1>(typeof(TestDataProvider1), TestDataProvider1.TestName, 1, AllPlatforms.Platforms, null);
            var dataProvider1Result = testServiceManager.TryCreateAndRegisterDataProvider(dataProvider1config, testService1);

            var dataProvider2config = new ServiceConfiguration<ITestDataProvider2>(typeof(TestDataProvider2), TestDataProvider2.TestName, 1, AllPlatforms.Platforms, null);
            var dataProvider2Result = testServiceManager.TryCreateAndRegisterDataProvider(dataProvider2config, testService1);

            // Tests
            Assert.IsTrue(dataProvider1Result, "Data Provider 1 was not registered");
            Assert.IsTrue(dataProvider2Result, "Data Provider 2 was not registered");
            Assert.AreEqual(activeSystemCount + 3, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_07_RegisterDataProviderInMultipleServicesConfig()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            // Register Service 1 and data provider
            ITestService testService1;
            var config = new ServiceConfiguration<ITestService>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult = testServiceManager.TryCreateAndRegisterService<ITestService>(config, out testService1);

            var dataProvider1config = new ServiceConfiguration<ITestDataProvider1>(typeof(TestDataProvider1), TestDataProvider1.TestName, 1, AllPlatforms.Platforms, null);
            var dataProvider1Result = testServiceManager.TryCreateAndRegisterDataProvider<ITestDataProvider1>(dataProvider1config, testService1);

            // Register Service 1 and data provider
            ITestService2 testService2;
            var config2 = new ServiceConfiguration<ITestService2>(typeof(TestService2), TestService2.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult2 = testServiceManager.TryCreateAndRegisterService<ITestService2>(config2, out testService2);

            var dataProvider2config = new ServiceConfiguration<ITestDataProvider2>(typeof(TestDataProvider2), TestDataProvider2.TestName, 1, AllPlatforms.Platforms, null);
            var dataProvider2Result = testServiceManager.TryCreateAndRegisterDataProvider<ITestDataProvider2>(dataProvider2config, testService2);

            // Tests
            Assert.IsTrue(dataProvider1Result, "Data Provider 1 was not registered");
            Assert.IsTrue(dataProvider2Result, "Data Provider 2 was not registered");
            Assert.AreEqual(activeSystemCount + 4, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_08_RegisterDataProviderMultipleTimesConfig()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            ITestService testService1;
            var config = new ServiceConfiguration<ITestService>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult = testServiceManager.TryCreateAndRegisterService<ITestService>(config, out testService1);

            var dataProviderconfig = new ServiceConfiguration<ITestDataProvider1>(typeof(TestDataProvider1), TestDataProvider1.TestName, 1, AllPlatforms.Platforms, null);
            var dataProvider1Result = testServiceManager.TryCreateAndRegisterDataProvider<ITestDataProvider1>(dataProviderconfig, testService1);

            var dataProviderconfig2 = new ServiceConfiguration<ITestDataProvider1>(typeof(TestDataProvider1), TestDataProvider1.TestName, 1, AllPlatforms.Platforms, null);
            var dataProvider2Result = testServiceManager.TryCreateAndRegisterDataProvider<ITestDataProvider1>(dataProviderconfig2, testService1);
            LogAssert.Expect(LogType.Error, "There is already a [ITestDataProvider1.Test Data Provider 1] registered!");

            // Tests
            Assert.IsTrue(dataProvider1Result, "Data Provider 1 was not registered");
            Assert.IsFalse(dataProvider2Result, "Data Provider 2 was registered when it should not have been");
            Assert.AreEqual(activeSystemCount + 2, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_08_RegisterServiceConfigurationsWithDataProviders()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            var configurations = new ServiceConfiguration<IService>[2];

            var testService1Profile = new TestService1Profile();
            var dataProvider1Configuration = new ServiceConfiguration<ITestDataProvider1>(typeof(TestDataProvider1), TestDataProvider1.TestName, 1, AllPlatforms.Platforms, null);
            testService1Profile.AddConfiguration(dataProvider1Configuration);

            var testService2Profile = new TestService2Profile();
            var dataProvider2Configuration = new ServiceConfiguration<ITestDataProvider2>(typeof(TestDataProvider2), TestDataProvider2.TestName, 1, AllPlatforms.Platforms, null);
            testService2Profile.AddConfiguration(dataProvider2Configuration);

            configurations[0] = new ServiceConfiguration<IService>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, testService1Profile);
            configurations[1] = new ServiceConfiguration<IService>(typeof(TestService2), TestService2.TestName, 1, AllPlatforms.Platforms, testService2Profile);

            var result = testServiceManager.TryRegisterServiceConfigurations(configurations);

            // Tests
            Assert.IsTrue(result, "Test services were not registered");
            Assert.AreEqual(activeServiceCount + 4, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        #endregion 03 Data Provider Registration - Config

        #region 04 Service Retrieval

        [Test]
        public void Test_04_01_ServiceExists()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService>(testService1);

            // Retrieve
            var testService1Retrieval = testServiceManager.IsServiceRegistered(testService1);
            var testService1RetrievalInterface = testServiceManager.IsServiceRegistered<ITestService>();

            // Tests
            Assert.IsTrue(testService1Retrieval, "Test service was not found");
            Assert.IsTrue(testService1RetrievalInterface, "Test service was not found via interface");
            Assert.AreEqual(activeServiceCount + 1, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        
        [Test]
        public void Test_04_02_ServiceDoesNotExist()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register
            var testService1 = new TestService1();

            // Retrieve
            var testService1Retrieval = testServiceManager.IsServiceRegistered(testService1);
            var testService1RetrievalInterface = testServiceManager.IsServiceRegistered<ITestService>();

            // Tests
            Assert.IsFalse(testService1Retrieval, "Test service was found in registry when it was not added");
            Assert.IsFalse(testService1RetrievalInterface, "Test service was found via interface in registry when it was not added");
            Assert.AreEqual(activeServiceCount, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_04_03_RetrieveService()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService>(testService1);

            // Retrieve
            var testService1Retrieval = testServiceManager.GetService<ITestService>();

            // Tests
            Assert.IsNotNull(testService1Retrieval, "Test service was not found");
            Assert.IsTrue(testService1.ServiceGuid == testService1Retrieval.ServiceGuid, "Service GUID does not match");
            Assert.AreEqual(activeServiceCount + 1, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_04_04_RetrieveSecondService()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register Service 1
            testServiceManager.TryRegisterService<ITestService>(new TestService1());

            // Register Service 2
            var testService2 = new TestService2();
            testServiceManager.TryRegisterService<ITestService2>(testService2);

            // Retrieve
            var testService2Retrieval = testServiceManager.GetService<ITestService2>();

            // Tests
            Assert.IsNotNull(testService2Retrieval, "Test service was not found");
            Assert.IsTrue(testService2.ServiceGuid == testService2Retrieval.ServiceGuid, "Service GUID does not match");
            Assert.AreEqual(activeServiceCount + 2, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_04_05_RetrieveServiceDoesNotExist()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Retrieve
            var testService1 = testServiceManager.GetService<ITestService>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestService)} service.");

            // Tests
            Assert.IsNull(testService1, "Test service was found");
            Assert.AreEqual(activeServiceCount, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_04_06_RetrieveSecondServiceDoesNotExist()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register Service 1
            testServiceManager.TryRegisterService<ITestService>(new TestService1());

            // Retrieve
            var testService2 = testServiceManager.GetService<ITestService2>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestService2)} service.");

            // Tests
            Assert.IsNull(testService2, "Test service was not found");
            Assert.AreEqual(activeServiceCount + 1, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_04_07_GetAllServices()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register Service 1
            testServiceManager.TryRegisterService<ITestService>(new TestService1());

            // Register Service 2
            var testService2 = new TestService2();
            testServiceManager.TryRegisterService<ITestService2>(testService2);

            // Retrieve
            var allServices = testServiceManager.GetAllServices();
            var registeredServicesList = testServiceManager.GetServices<IService>();
            var registeredTestService1 = testServiceManager.GetServices<ITestService>();

            // Tests
            Assert.AreEqual(2, allServices.Count, "More or less services found than was expected from full query");
            Assert.AreEqual(2, registeredServicesList.Count, "More or less services found than was expected from IService query");
            Assert.AreEqual(1, registeredTestService1.Count, "More or less services found than was expected from specific Interface query");
        }

        [Test]
        public void Test_04_08_RetrieveRegisterServiceConfigurations()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            var configurations = new ServiceConfiguration<IService>[2];

            configurations[0] = new ServiceConfiguration<IService>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            configurations[1] = new ServiceConfiguration<IService>(typeof(TestService2), TestService2.TestName, 1, AllPlatforms.Platforms, null);

            var result = testServiceManager.TryRegisterServiceConfigurations(configurations);

            var service1 = testServiceManager.GetService<ITestService>();
            var service2 = testServiceManager.GetService<ITestService2>();


            // Tests
            Assert.IsTrue(result, "Test services were not registered");
            Assert.IsNotNull(service1, "Test Service 1 was not registered");
            Assert.IsNotNull(service2, "Test Service 2 was not registered");
            Assert.AreEqual(activeServiceCount + 2, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        #endregion  04 Service Retrieval

        #region 05 Data Provider Retrieval

        [Test]
        public void Test_05_01_RetrieveSingleDataProvider()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register Service 1 and data provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService>(testService1);
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
            testServiceManager.TryRegisterService<ITestService>(testService1);
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
            testServiceManager.TryRegisterService<ITestService>(testService1);
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
            testServiceManager.TryRegisterService<ITestService>(testService1);
            testServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1));
            testServiceManager.TryRegisterService<ITestDataProvider2>(new TestDataProvider2(TestDataProvider2.TestName, 0, null, testService1));

            // Retrieve all registered IDataProviders from service
            var testService = testServiceManager.GetService<ITestService>();
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
            testServiceManager.TryRegisterService<ITestService>(testService1);
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
            testServiceManager.TryRegisterService<ITestService>(testService1);
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
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            var configurations = new ServiceConfiguration<IService>[2];

            var testService1Profile = new TestService1Profile();
            var dataProvider1Configuration = new ServiceConfiguration<ITestDataProvider1>(typeof(TestDataProvider1), TestDataProvider1.TestName, 1, AllPlatforms.Platforms, null);
            testService1Profile.AddConfiguration(dataProvider1Configuration);

            var testService2Profile = new TestService2Profile();
            var dataProvider2Configuration = new ServiceConfiguration<ITestDataProvider2>(typeof(TestDataProvider2), TestDataProvider2.TestName, 1, AllPlatforms.Platforms, null);
            testService2Profile.AddConfiguration(dataProvider2Configuration);

            configurations[0] = new ServiceConfiguration<IService>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, testService1Profile);
            configurations[1] = new ServiceConfiguration<IService>(typeof(TestService2), TestService2.TestName, 1, AllPlatforms.Platforms, testService2Profile);

            var result = testServiceManager.TryRegisterServiceConfigurations(configurations);

            var dataProvider1Retrieval = testServiceManager.GetService<ITestDataProvider1>();
            var dataProvider2Retrieval = testServiceManager.GetService<ITestDataProvider2>();

            // Tests
            Assert.IsNotNull(dataProvider1Retrieval, "Test data provider 1 not found");
            Assert.IsNotNull(dataProvider2Retrieval, "Test data provider 2 not found");
            Assert.IsTrue(result, "Test services were not registered");
            Assert.AreEqual(activeServiceCount + 4, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        #endregion 05 Data Provider Retrieval

        #region 06 Service unRegistration

        [Test]
        public void Test_06_01_UnregisterSingleService()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register service 1
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService>(testService1);

            var serviceUnregister = testServiceManager.TryUnregisterService<ITestService>(testService1);

            var testService1Unregistered = testServiceManager.GetService<ITestService>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestService)} service.");

            // Tests
            Assert.IsNotNull(testService1, "Test service was not registered");
            Assert.IsTrue(serviceUnregister, "Service was not unregistered correctly");
            Assert.IsNull(testService1Unregistered, "Service was found, it should have been unregistered");
            Assert.AreEqual(activeServiceCount, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_06_02_UnregisterServiceWithDataProvider()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register service 1 and data provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService>(testService1);
            testServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1));

            // Unregister Service
            var serviceUnregister = testServiceManager.TryUnregisterService<ITestService>(testService1);

            // Try and retrieve unregistered Service
            var testService1Unregistered = testServiceManager.GetService<ITestService>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestService)} service.");

            // Try and retrieve unregistered Data Provider
            var testDataProvider1Unregistered = testServiceManager.GetService<ITestDataProvider1>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestDataProvider1)} service.");

            // Tests
            Assert.IsNotNull(testService1, "Test service was not registered");
            Assert.IsTrue(serviceUnregister, "Service was not unregistered correctly");
            Assert.IsNull(testService1Unregistered, "Service was found, it should have been unregistered");
            Assert.IsNull(testDataProvider1Unregistered, "Data Provider was found, it should have been unregistered");
            Assert.AreEqual(activeServiceCount, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }


        [Test]
        public void Test_06_03_UnregisterServiceWithMultipleDataProvider()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register service 1 and data provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService>(testService1);
            testServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1));
            testServiceManager.TryRegisterService<ITestDataProvider2>(new TestDataProvider2(TestDataProvider2.TestName, 0, null, testService1));

            // Unregister Service
            var serviceUnregister = testServiceManager.TryUnregisterService<ITestService>(testService1);

            // Try and retrieve unregistered Service
            var testService1Unregistered = testServiceManager.GetService<ITestService>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestService)} service.");

            // Try and retrieve unregistered Data Provider
            var testDataProvider1Unregistered = testServiceManager.GetService<ITestDataProvider1>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestDataProvider1)} service.");
            var testDataProvider2Unregistered = testServiceManager.GetService<ITestDataProvider2>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestDataProvider2)} service.");


            // Tests
            Assert.IsNotNull(testService1, "Test service was not registered");
            Assert.IsTrue(serviceUnregister, "Service was not unregistered correctly");
            Assert.IsNull(testService1Unregistered, "Service was found, it should have been unregistered");
            Assert.IsNull(testDataProvider1Unregistered, "Data Provider 1 was found, it should have been unregistered");
            Assert.IsNull(testDataProvider2Unregistered, "Data Provider 2 was found, it should have been unregistered");
            Assert.AreEqual(activeServiceCount, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_06_04_UnregisterSingleServiceFromMultiple()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register service 1 and data provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService>(testService1);

            // Register service 2
            testServiceManager.TryRegisterService<ITestService2>(new TestService2());

            // Unregister Service 1
            var serviceUnregister = testServiceManager.TryUnregisterService<ITestService>(testService1);

            // Try and retrieve unregistered Service 1
            var testService1Unregistered = testServiceManager.GetService<ITestService>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestService)} service.");

            // Try and retrieve still registered Service 2
            var testService2 = testServiceManager.GetService<ITestService2>();

            // Tests
            Assert.IsNotNull(testService1, "Test service was not registered");
            Assert.IsNotNull(testService2, "Test service 2 was not registered");
            Assert.IsTrue(serviceUnregister, "Service was not unregistered correctly");
            Assert.IsNull(testService1Unregistered, "Service was found, it should have been unregistered");
            Assert.AreEqual(activeServiceCount + 1, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        //---
        [Test]
        public void Test_06_05_UnregisterSingleServiceByInterface()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register service 1
            testServiceManager.TryRegisterService<ITestService>(new TestService1());

            var serviceUnregister = testServiceManager.TryUnregisterServicesOfType<ITestService>();

            var testService1Unregistered = testServiceManager.GetService<ITestService>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestService)} service.");

            // Tests
            Assert.IsTrue(serviceUnregister, "Service was not unregistered correctly");
            Assert.IsNull(testService1Unregistered, "Service was found, it should have been unregistered");
            Assert.AreEqual(activeServiceCount, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_06_06_UnregisterServiceWithDataProviderByInterface()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register service 1 and data provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService>(testService1);
            testServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1));

            // Unregister Service
            var serviceUnregister = testServiceManager.TryUnregisterServicesOfType<ITestService>();

            // Try and retrieve unregistered Service
            var testService1Unregistered = testServiceManager.GetService<ITestService>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestService)} service.");

            // Try and retrieve unregistered Data Provider
            var testDataProvider1Unregistered = testServiceManager.GetService<ITestDataProvider1>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestDataProvider1)} service.");

            // Tests
            Assert.IsNotNull(testService1, "Test service was not registered");
            Assert.IsTrue(serviceUnregister, "Service was not unregistered correctly");
            Assert.IsNull(testService1Unregistered, "Service was found, it should have been unregistered");
            Assert.IsNull(testDataProvider1Unregistered, "Data Provider was found, it should have been unregistered");
            Assert.AreEqual(activeServiceCount, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }


        [Test]
        public void Test_06_07_UnregisterServiceWithMultipleDataProviderByInterface()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register service 1 and data provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService>(testService1);
            testServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1));
            testServiceManager.TryRegisterService<ITestDataProvider2>(new TestDataProvider2(TestDataProvider2.TestName, 0, null, testService1));

            // Unregister Service
            var serviceUnregister = testServiceManager.TryUnregisterServicesOfType<ITestService>();

            // Try and retrieve unregistered Service
            var testService1Unregistered = testServiceManager.GetService<ITestService>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestService)} service.");

            // Try and retrieve unregistered Data Provider
            var testDataProvider1Unregistered = testServiceManager.GetService<ITestDataProvider1>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestDataProvider1)} service.");
            var testDataProvider2Unregistered = testServiceManager.GetService<ITestDataProvider2>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestDataProvider2)} service.");


            // Tests
            Assert.IsNotNull(testService1, "Test service was not registered");
            Assert.IsTrue(serviceUnregister, "Service was not unregistered correctly");
            Assert.IsNull(testService1Unregistered, "Service was found, it should have been unregistered");
            Assert.IsNull(testDataProvider1Unregistered, "Data Provider 1 was found, it should have been unregistered");
            Assert.IsNull(testDataProvider2Unregistered, "Data Provider 2 was found, it should have been unregistered");
            Assert.AreEqual(activeServiceCount, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_06_08_UnregisterSingleServiceFromMultipleByInterface()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register service 1
            testServiceManager.TryRegisterService<ITestService>(new TestService1());

            // Register service 2
            testServiceManager.TryRegisterService<ITestService2>(new TestService2());

            // Unregister Service 1
            var serviceUnregister = testServiceManager.TryUnregisterServicesOfType<ITestService>();

            // Try and retrieve unregistered Service 1
            var testService1Unregistered = testServiceManager.GetService<ITestService>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestService)} service.");

            // Try and retrieve still registered Service 2
            var testService2 = testServiceManager.GetService<ITestService2>();

            // Tests
            Assert.IsNotNull(testService2, "Test service 2 was not registered");
            Assert.IsTrue(serviceUnregister, "Service was not unregistered correctly");
            Assert.IsNull(testService1Unregistered, "Service was found, it should have been unregistered");
            Assert.AreEqual(activeServiceCount + 1, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }


        #endregion 06 Service unRegistration

        #region 07 Data Provider unRegistration

        [Test]
        public void Test_07_01_UnregisterSingleDataProvider()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register service 1 and data provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService>(testService1);
            testServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1));

            // Retrieve registered data provider
            var dataProvider1 = testServiceManager.GetService<ITestDataProvider1>();

            // Unregister data provider from service
            testService1.UnRegisterDataProvider(dataProvider1);

            // Try and retrieve unregistered data provider
            var testDataProvider1Unregistered = testServiceManager.GetService<ITestDataProvider1>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestDataProvider1)} service.");

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
            var serviceRegistration = testServiceManager.TryRegisterService<ITestService>(testService1);
            var dataprovider1Registration = testServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1));
            var dataprovider2Registration = testServiceManager.TryRegisterService<ITestDataProvider2>(new TestDataProvider2(TestDataProvider2.TestName, 0, null, testService1));

            // Retrieve registered data provider
            var dataProvider1 = testServiceManager.GetService<ITestDataProvider1>();

            // Unregister data provider from service
            testService1.UnRegisterDataProvider(dataProvider1);

            // Try and retrieve unregistered data provider
            var testDataProvider1Unregistered = testServiceManager.GetService<ITestDataProvider1>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestDataProvider1)} service.");

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
            testServiceManager.TryRegisterService<ITestService>(testService1);
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
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestDataProvider1)} service.");

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
            testServiceManager.TryRegisterService<ITestService>(testService1);
            testServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1));

            // Retrieve registered data provider
            var dataProvider1 = testServiceManager.GetService<ITestDataProvider1>();

            // Try and retrieve unregistered data provider direct
            var dataproviderUnregister = testServiceManager.TryUnregisterService<ITestDataProvider1>(dataProvider1);

            // Try and retrieve unregistered data provider
            var testDataProvider1Unregistered = testServiceManager.GetService<ITestDataProvider1>();
            LogAssert.Expect(LogType.Error, $"Unable to find {nameof(ITestDataProvider1)} service.");

            // Tests
            Assert.IsNotNull(dataProvider1, "Test data provider was not registered");
            Assert.IsTrue(dataproviderUnregister, "Service was not unregistered correctly");
            Assert.IsNull(testDataProvider1Unregistered, "Data Provider was found, it should have been unregistered");
            Assert.AreEqual(testService1.DataProviders.Count, 0, "DataProvider Count after being unregistered does not match");
            Assert.AreEqual(activeServiceCount + 1, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        #endregion 07 Data Provider unRegistration

        #region 08 Disable Running Services

        [Test]
        public void Test_08_01_ServiceDisable()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService>(testService1);

            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when it was started");

            testServiceManager.DisableService<ITestService>();

            // Retrieve
            var testService1Retrieval = testServiceManager.GetService<ITestService>();

            // Tests
            Assert.IsFalse(testService1.IsEnabled, "Test service was in a enabled state when it was disabled via interface call");
            Assert.IsFalse(testService1Retrieval.IsEnabled, "Test service was in a enabled state when it was disabled after retrieval.");
        }

        [Test]
        public void Test_08_02_ServiceDisableByName()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService>(testService1);

            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when it was started");

            testServiceManager.DisableService<ITestService>(TestService1.TestName);

            // Retrieve
            var testService1Retrieval = testServiceManager.GetService<ITestService>();

            // Tests
            Assert.IsFalse(testService1.IsEnabled, "Test service was in a enabled state when it was disabled via interface call");
            Assert.IsFalse(testService1Retrieval.IsEnabled, "Test service was in a enabled state when it was disabled after retrieval.");
        }

        [Test]
        public void Test_08_03_ServiceDisableDirect()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService>(testService1);

            Assert.IsTrue(testService1.IsEnabled, "Test service was in a disabled state when it was started");

            testService1.Disable();

            // Retrieve
            var testService1Retrieval = testServiceManager.GetService<ITestService>();

            // Tests
            Assert.IsFalse(testService1.IsEnabled, "Test service was in a enabled state when it was disabled");
            Assert.IsFalse(testService1Retrieval.IsEnabled, "Test service was in a enabled state when it was disabled after retrieval.");
        }

        [Test]
        public void Test_08_04_DisableAllServices()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService>(testService1);
            var testService2 = new TestService2();
            testServiceManager.TryRegisterService<ITestService2>(testService2);

            testServiceManager.DisableAllServices();

            // Retrieve
            var testService1Retrieval = testServiceManager.GetService<ITestService>();
            var testService2Retrieval = testServiceManager.GetService<ITestService2>();

            // Tests
            Assert.IsFalse(testService1.IsEnabled, "Test service was in a enabled state when it was disabled");
            Assert.IsFalse(testService2.IsEnabled, "Test service was in a enabled state when it was disabled");
            Assert.IsFalse(testService1Retrieval.IsEnabled, "Test service was in a enabled state when it was disabled after retrieval.");
            Assert.IsFalse(testService2Retrieval.IsEnabled, "Test service was in a enabled state when it was disabled after retrieval.");
        }

        [Test]
        public void Test_08_05_ServiceDisablePriorToRegistration()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Create disabled
            var testService1 = new TestService1();
            testService1.Disable();

            Assert.IsFalse(testService1.IsEnabled, "Test service was in a enabled state, but was registered disabled");

            // Register
            testServiceManager.TryRegisterService<ITestService>(testService1);

            Assert.IsFalse(testService1.IsEnabled, "Test service was in a enabled state, but was registered disabled after registration");

            // Retrieve
            var testService1Retrieval = testServiceManager.GetService<ITestService>();

            // Tests
            Assert.IsFalse(testService1.IsEnabled, "Test service was in a enabled state when it was disabled");
            Assert.IsFalse(testService1Retrieval.IsEnabled, "Test service was in a enabled state when it was disabled after retrieval.");
        }

        #endregion 08 Disable Running Services

        #region 09 Disable Running Data Provider

        [Test]
        public void Test_09_01_DataProviderDisable()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService>(testService1);
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
            testServiceManager.TryRegisterService<ITestService>(testService1);
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
            testServiceManager.TryRegisterService<ITestService>(testService1);
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
            testServiceManager.TryRegisterService<ITestService>(testService1);
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
            testServiceManager.TryRegisterService<ITestService>(testService1);

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

        #endregion 09 Disable Running Data Provider

        #region 10 Enable Service

        [Test]
        public void Test_10_01_ServiceEnable()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register
            var testService1 = new TestService1();
            testService1.Disable();
            testServiceManager.TryRegisterService<ITestService>(testService1);

            Assert.IsFalse(testService1.IsEnabled, "Test service was in a enabled state when it was disabled");

            testServiceManager.EnableService<ITestService>();

            // Retrieve
            var testService1Retrieval = testServiceManager.GetService<ITestService>();

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
            testServiceManager.TryRegisterService<ITestService>(testService1);

            Assert.IsFalse(testService1.IsEnabled, "Test service was in a enabled state when it was disabled");

            testServiceManager.EnableService<ITestService>(TestService1.TestName);

            // Retrieve
            var testService1Retrieval = testServiceManager.GetService<ITestService>();

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
            testServiceManager.TryRegisterService<ITestService>(testService1);

            Assert.IsFalse(testService1.IsEnabled, "Test service was in a enabled state when it was disabled");

            testService1.Enable();

            // Retrieve
            var testService1Retrieval = testServiceManager.GetService<ITestService>();

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
            testServiceManager.TryRegisterService<ITestService>(testService1);
            var testService2 = new TestService2();
            testServiceManager.TryRegisterService<ITestService2>(testService2);

            testServiceManager.DisableAllServices();

            Assert.IsFalse(testService1.IsEnabled, "Test service was in a enabled state when it was disabled");
            Assert.IsFalse(testService2.IsEnabled, "Test service was in a enabled state when it was disabled");

            testServiceManager.EnableAllServices();

            // Retrieve
            var testService1Retrieval = testServiceManager.GetService<ITestService>();
            var testService2Retrieval = testServiceManager.GetService<ITestService2>();

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
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService>(testService1);

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
            testServiceManager.TryRegisterService<ITestService>(testService1);

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
            testServiceManager.TryRegisterService<ITestService>(testService1);

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
            testServiceManager.TryRegisterService<ITestService>(testService1);
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

            #endregion 11 Enable Data Provider
    }
}