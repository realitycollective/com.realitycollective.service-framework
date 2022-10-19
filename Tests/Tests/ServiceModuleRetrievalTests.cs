// Copyright (c) Reality Collective. All rights reserved.

using NUnit.Framework;
using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Definitions.Platforms;
using RealityCollective.ServiceFramework.Interfaces;
using RealityCollective.ServiceFramework.Services;
using RealityCollective.ServiceFramework.Tests.Interfaces;
using RealityCollective.ServiceFramework.Tests.Profiles;
using RealityCollective.ServiceFramework.Tests.Modules;
using RealityCollective.ServiceFramework.Tests.Services;
using RealityCollective.ServiceFramework.Tests.Utilities;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;

namespace RealityCollective.ServiceFramework.Tests.E_ServiceModuleRetrieval
{
    internal class ServiceModuleRetrievalTests
    {
        private ServiceManager testServiceManager;

        #region Service Module Retrieval

        [Test]
        public void Test_05_01_RetrieveSingleServiceModule()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register Service 1 and service provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            var dataProvider1 = new TestServiceModule1(TestServiceModule1.TestName, 0, null, testService1);
            testServiceManager.TryRegisterService<ITestServiceModule1>(dataProvider1);

            // Retrieve
            var dataProvider1Retrieval = testServiceManager.GetService<ITestServiceModule1>();

            // Tests
            Assert.IsNotNull(dataProvider1, "Test service provider not found");
            Assert.AreEqual(dataProvider1.ServiceGuid, dataProvider1Retrieval.ServiceGuid, "Service GUID does not match");
            Assert.AreEqual(activeServiceCount + 2, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_05_02_RetrieveSecondServiceModule()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register service 1 and service provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestServiceModule1>(new TestServiceModule1(TestServiceModule1.TestName, 0, null, testService1));

            // Register service 1 and service provider
            var dataProvider2 = new TestServiceModule2(TestServiceModule2.TestName, 0, null, testService1);
            testServiceManager.TryRegisterService<ITestServiceModule2>(dataProvider2);

            // Retrieve
            var dataProvider2Retrieval = testServiceManager.GetService<ITestServiceModule2>();

            // Tests
            Assert.IsNotNull(dataProvider2, "Test service provider not found");
            Assert.AreEqual(dataProvider2.ServiceGuid, dataProvider2Retrieval.ServiceGuid, "Service GUID does not match");
            Assert.AreEqual(activeServiceCount + 3, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_05_03_RetrieveAllServiceModule()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            var testService1 = new TestService1();

            // Register service 1 and service providers
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestServiceModule1>(new TestServiceModule1(TestServiceModule1.TestName, 0, null, testService1));
            testServiceManager.TryRegisterService<ITestServiceModule2>(new TestServiceModule2(TestServiceModule2.TestName, 0, null, testService1));

            // Retrieve all registered IServiceModules
            var ServiceModules = testServiceManager.GetServices<IServiceModule>();

            // Tests
            Assert.IsNotEmpty(ServiceModules, "Service Modules were not registered");
            Assert.AreEqual(activeSystemCount + 3, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_05_04_RetrieveAllServiceModulesForService()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            // Register service 1 and service provider2
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestServiceModule1>(new TestServiceModule1(TestServiceModule1.TestName, 0, null, testService1));
            testServiceManager.TryRegisterService<ITestServiceModule2>(new TestServiceModule2(TestServiceModule2.TestName, 0, null, testService1));

            // Retrieve all registered IServiceModules from service
            var testService = testServiceManager.GetService<ITestService1>();
            var ServiceModules = testService.ServiceModules;

            // Tests
            Assert.IsNotEmpty(ServiceModules, "Service Modules were not registered");
            Assert.AreEqual(ServiceModules.Count, 2, "Could not locate all service providers");
            Assert.AreEqual(activeSystemCount + 3, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_05_05_RetrieveAllRegisteredServiceModulesFromMultipleServices()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            // Register service 1 and service provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestServiceModule1>(new TestServiceModule1(TestServiceModule1.TestName, 0, null, testService1));

            // Register service 2 and service provider
            var testService2 = new TestService2();
            testServiceManager.TryRegisterService<ITestService2>(testService2);
            testServiceManager.TryRegisterService<ITestServiceModule2>(new TestServiceModule2(TestServiceModule2.TestName, 0, null, testService2));

            // Retrieve all registered IServiceServiceModule
            var ServiceModules = testServiceManager.GetServices<IServiceModule>();

            // Tests
            Assert.IsNotEmpty(ServiceModules, "Service Modules were not registered");
            Assert.AreEqual(ServiceModules.Count, 2, "Could not locate all service providers");
            Assert.AreEqual(activeSystemCount + 4, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_05_06_ServiceServiceModuleDoesNotExist()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register service 1 and service provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            var testServiceModule2 = new TestServiceModule1(TestServiceModule1.TestName, 0, null, testService1);
            testServiceManager.TryRegisterService<ITestServiceModule1>(testServiceModule2);

            // Validate non-existent service provider 2
            var isServiceModuleRegistered = testServiceManager.IsServiceRegistered<ITestServiceModule2>();

            // Tests
            Assert.IsFalse(isServiceModuleRegistered, "Service Module was found when it was not registered");
        }

        [Test]
        public void Test_05_07_RetrieveRegisterServiceConfigurationsWithServiceModules()
        {
            // Check logs
            LogAssert.Expect(LogType.Log, new Regex("Test Service 1 is Initialised"));
            LogAssert.Expect(LogType.Log, new Regex("Test Service Module 1 is Initialised"));
            LogAssert.Expect(LogType.Log, new Regex("Test Service 2 is Initialised"));
            LogAssert.Expect(LogType.Error, new Regex("Unable to find ITestServiceModule2 service."));

            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            var configurations = new ServiceConfiguration<IService>[2];

            var testService1Profile = (TestService1Profile)ScriptableObject.CreateInstance(typeof(TestService1Profile));
            var dataProvider1Configuration = new ServiceConfiguration<ITestServiceModule1>(typeof(TestServiceModule1), TestServiceModule1.TestName, 1, AllPlatforms.Platforms, null);
            testService1Profile.AddConfiguration(dataProvider1Configuration);

            var testService2Profile = (TestService2Profile)ScriptableObject.CreateInstance(typeof(TestService2Profile));
            var dataProvider2Configuration = new ServiceConfiguration<ITestServiceModule2>(typeof(TestServiceModule2), TestServiceModule2.TestName, 1, AllPlatforms.Platforms, null);
            testService2Profile.AddConfiguration(dataProvider2Configuration);

            configurations[0] = new ServiceConfiguration<IService>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, testService1Profile);
            configurations[1] = new ServiceConfiguration<IService>(typeof(TestService2), TestService2.TestName, 1, AllPlatforms.Platforms, testService2Profile);

            var result = testServiceManager.TryRegisterServiceConfigurations(configurations);

            // Both services should be found following configuration registration
            var service1Registration = testServiceManager.GetService<ITestService1>();
            var service2Registration = testServiceManager.GetService<ITestService2>();

            // Service Module 1 should return because its service allows the registration of Service Modules
            var dataProvider1Registration = testServiceManager.GetService<ITestServiceModule1>();

            // Service Module 2 should NOT return because its service does NOT allow the registration of Service Modules
            var dataProvider2Registration = testServiceManager.GetService<ITestServiceModule2>();

            // Tests
            Assert.IsTrue(result, "Test services were not registered");
            Assert.IsNotNull(service1Registration, "Test Service 1 should be registered but it was not found");
            Assert.IsNotNull(service2Registration, "Test Service 2 should be registered but it was not found");
            Assert.IsNotNull(dataProvider1Registration, "Service Module 1 should be registered but it was not found");
            Assert.IsNull(dataProvider2Registration, "Service Module 2 should NOT be registered but it was found");
            Assert.AreEqual(1, service1Registration.ServiceModules.Count, "Test Service 1 Service Module count did not match, should be 1");
            Assert.AreEqual(1, service2Registration.ServiceModules.Count, "Test Service 2 Service Module count did not match, should be 1");
            Assert.AreEqual(activeServiceCount + 3, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        #endregion Service Module Retrieval
    }
}