// Copyright (c) Reality Collective. All rights reserved.

using NUnit.Framework;
using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Definitions.Platforms;
using RealityCollective.ServiceFramework.Interfaces;
using RealityCollective.ServiceFramework.Services;
using RealityCollective.ServiceFramework.Tests.Interfaces;
using RealityCollective.ServiceFramework.Tests.Profiles;
using RealityCollective.ServiceFramework.Tests.Providers;
using RealityCollective.ServiceFramework.Tests.Services;
using RealityCollective.ServiceFramework.Tests.Utilities;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;

namespace RealityCollective.ServiceFramework.Tests.E_ServiceProviderRetrieval
{
    internal class ServiceProviderRetrievalTests
    {
        private ServiceManager testServiceManager;

        #region Service Provider Retrieval

        [Test]
        public void Test_05_01_RetrieveSingleServiceProvider()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register Service 1 and Service Provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            var ServiceProvider1 = new TestServiceProvider1(TestServiceProvider1.TestName, 0, null, testService1);
            testServiceManager.TryRegisterService<ITestServiceProvider1>(ServiceProvider1);

            // Retrieve
            var ServiceProvider1Retrieval = testServiceManager.GetService<ITestServiceProvider1>();

            // Tests
            Assert.IsNotNull(ServiceProvider1, "Test Service Provider not found");
            Assert.AreEqual(ServiceProvider1.ServiceGuid, ServiceProvider1Retrieval.ServiceGuid, "Service GUID does not match");
            Assert.AreEqual(activeServiceCount + 2, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_05_02_RetrieveSecondServiceProvider()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register service 1 and Service Provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestServiceProvider1>(new TestServiceProvider1(TestServiceProvider1.TestName, 0, null, testService1));

            // Register service 1 and Service Provider
            var ServiceProvider2 = new TestServiceProvider2(TestServiceProvider2.TestName, 0, null, testService1);
            testServiceManager.TryRegisterService<ITestServiceProvider2>(ServiceProvider2);

            // Retrieve
            var ServiceProvider2Retrieval = testServiceManager.GetService<ITestServiceProvider2>();

            // Tests
            Assert.IsNotNull(ServiceProvider2, "Test Service Provider not found");
            Assert.AreEqual(ServiceProvider2.ServiceGuid, ServiceProvider2Retrieval.ServiceGuid, "Service GUID does not match");
            Assert.AreEqual(activeServiceCount + 3, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_05_03_RetrieveAllServiceProvider()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            var testService1 = new TestService1();

            // Register service 1 and Service Providers
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestServiceProvider1>(new TestServiceProvider1(TestServiceProvider1.TestName, 0, null, testService1));
            testServiceManager.TryRegisterService<ITestServiceProvider2>(new TestServiceProvider2(TestServiceProvider2.TestName, 0, null, testService1));

            // Retrieve all registered IServiceProviders
            var ServiceProviders = testServiceManager.GetServices<IServiceProvider>();

            // Tests
            Assert.IsNotEmpty(ServiceProviders, "Service Providers were not registered");
            Assert.AreEqual(activeSystemCount + 3, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_05_04_RetrieveAllServiceProvidersForService()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            // Register service 1 and Service Provider2
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestServiceProvider1>(new TestServiceProvider1(TestServiceProvider1.TestName, 0, null, testService1));
            testServiceManager.TryRegisterService<ITestServiceProvider2>(new TestServiceProvider2(TestServiceProvider2.TestName, 0, null, testService1));

            // Retrieve all registered IServiceProviders from service
            var testService = testServiceManager.GetService<ITestService1>();
            var ServiceProviders = testService.ServiceProviders;

            // Tests
            Assert.IsNotEmpty(ServiceProviders, "Service Providers were not registered");
            Assert.AreEqual(ServiceProviders.Count, 2, "Could not locate all Service Providers");
            Assert.AreEqual(activeSystemCount + 3, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_05_05_RetrieveAllRegisteredServiceProvidersFromMultipleServices()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            // Register service 1 and Service Provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestServiceProvider1>(new TestServiceProvider1(TestServiceProvider1.TestName, 0, null, testService1));

            // Register service 2 and Service Provider
            var testService2 = new TestService2();
            testServiceManager.TryRegisterService<ITestService2>(testService2);
            testServiceManager.TryRegisterService<ITestServiceProvider2>(new TestServiceProvider2(TestServiceProvider2.TestName, 0, null, testService2));

            // Retrieve all registered IServiceServiceProvider
            var ServiceProviders = testServiceManager.GetServices<IServiceProvider>();

            // Tests
            Assert.IsNotEmpty(ServiceProviders, "Service Providers were not registered");
            Assert.AreEqual(ServiceProviders.Count, 2, "Could not locate all Service Providers");
            Assert.AreEqual(activeSystemCount + 4, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_05_06_ServiceServiceProviderDoesNotExist()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register service 1 and Service Provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            var testServiceProvider2 = new TestServiceProvider1(TestServiceProvider1.TestName, 0, null, testService1);
            testServiceManager.TryRegisterService<ITestServiceProvider1>(testServiceProvider2);

            // Validate non-existent Service Provider 2
            var isServiceProviderRegistered = testServiceManager.IsServiceRegistered<ITestServiceProvider2>();

            // Tests
            Assert.IsFalse(isServiceProviderRegistered, "Service Provider was found when it was not registered");
        }

        [Test]
        public void Test_05_07_RetrieveRegisterServiceConfigurationsWithServiceProviders()
        {
            // Check logs
            LogAssert.Expect(LogType.Log, new Regex("Test Service 1 is Initialised"));
            LogAssert.Expect(LogType.Log, new Regex("Test Service Provider 1 is Initialised"));
            LogAssert.Expect(LogType.Log, new Regex("Test Service 2 is Initialised"));
            LogAssert.Expect(LogType.Error, new Regex("Unable to find ITestServiceProvider2 service."));

            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            var configurations = new ServiceConfiguration<IService>[2];

            var testService1Profile = (TestService1Profile)ScriptableObject.CreateInstance(typeof(TestService1Profile));
            var ServiceProvider1Configuration = new ServiceConfiguration<ITestServiceProvider1>(typeof(TestServiceProvider1), TestServiceProvider1.TestName, 1, AllPlatforms.Platforms, null);
            testService1Profile.AddConfiguration(ServiceProvider1Configuration);

            var testService2Profile = (TestService2Profile)ScriptableObject.CreateInstance(typeof(TestService2Profile));
            var ServiceProvider2Configuration = new ServiceConfiguration<ITestServiceProvider2>(typeof(TestServiceProvider2), TestServiceProvider2.TestName, 1, AllPlatforms.Platforms, null);
            testService2Profile.AddConfiguration(ServiceProvider2Configuration);

            configurations[0] = new ServiceConfiguration<IService>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, testService1Profile);
            configurations[1] = new ServiceConfiguration<IService>(typeof(TestService2), TestService2.TestName, 1, AllPlatforms.Platforms, testService2Profile);

            var result = testServiceManager.TryRegisterServiceConfigurations(configurations);

            // Both services should be found following configuration registration
            var service1Registration = testServiceManager.GetService<ITestService1>();
            var service2Registration = testServiceManager.GetService<ITestService2>();

            // Service Provider 1 should return because its service allows the registration of Service Providers
            var ServiceProvider1Registration = testServiceManager.GetService<ITestServiceProvider1>();

            // Service Provider 2 should NOT return because its service does NOT allow the registration of Service Providers
            var ServiceProvider2Registration = testServiceManager.GetService<ITestServiceProvider2>();

            // Tests
            Assert.IsTrue(result, "Test services were not registered");
            Assert.IsNotNull(service1Registration, "Test Service 1 should be registered but it was not found");
            Assert.IsNotNull(service2Registration, "Test Service 2 should be registered but it was not found");
            Assert.IsNotNull(ServiceProvider1Registration, "Service Provider 1 should be registered but it was not found");
            Assert.IsNull(ServiceProvider2Registration, "Service Provider 2 should NOT be registered but it was found");
            Assert.AreEqual(1, service1Registration.ServiceProviders.Count, "Test Service 1 Service Provider count did not match, should be 1");
            Assert.AreEqual(1, service2Registration.ServiceProviders.Count, "Test Service 2 Service Provider count did not match, should be 1");
            Assert.AreEqual(activeServiceCount + 3, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        #endregion Service Provider Retrieval
    }
}