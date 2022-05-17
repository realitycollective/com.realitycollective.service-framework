// Copyright (c) Reality Collective. All rights reserved.

using NUnit.Framework;
using RealityToolkit.ServiceFramework.Definitions;
using RealityToolkit.ServiceFramework.Definitions.Platforms;
using RealityToolkit.ServiceFramework.Interfaces;
using RealityToolkit.ServiceFramework.Services;
using RealityToolkit.ServiceFramework.Tests.Interfaces;
using RealityToolkit.ServiceFramework.Tests.Services;
using RealityToolkit.ServiceFramework.Tests.Utilities;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;

namespace RealityToolkit.ServiceFramework.Tests.D_ServiceRetrieval
{
    internal class ServiceRetrievalTests
    {
        private ServiceManager testServiceManager;

        #region Service Retrieval

        [Test]
        public void Test_04_01_ServiceExists()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);

            // Retrieve
            var testService1Retrieval = testServiceManager.IsServiceRegistered(testService1);
            var testService1RetrievalInterface = testServiceManager.IsServiceRegistered<ITestService1>();

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
            var testService1RetrievalInterface = testServiceManager.IsServiceRegistered<ITestService1>();

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
            testServiceManager.TryRegisterService<ITestService1>(testService1);

            // Retrieve
            var testService1Retrieval = testServiceManager.GetService<ITestService1>();

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
            testServiceManager.TryRegisterService<ITestService1>(new TestService1());

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
            var testService1 = testServiceManager.GetService<ITestService1>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestService1)} service."));

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
            testServiceManager.TryRegisterService<ITestService1>(new TestService1());

            // Retrieve
            var testService2 = testServiceManager.GetService<ITestService2>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestService2)} service."));

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
            testServiceManager.TryRegisterService<ITestService1>(new TestService1());

            // Register Service 2
            var testService2 = new TestService2();
            testServiceManager.TryRegisterService<ITestService2>(testService2);

            // Retrieve
            var allServices = testServiceManager.GetAllServices();
            var registeredServicesList = testServiceManager.GetServices<IService>();
            var registeredTestService1 = testServiceManager.GetServices<ITestService1>();

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

            var service1 = testServiceManager.GetService<ITestService1>();
            var service2 = testServiceManager.GetService<ITestService2>();


            // Tests
            Assert.IsTrue(result, "Test services were not registered");
            Assert.IsNotNull(service1, "Test Service 1 was not registered");
            Assert.IsNotNull(service2, "Test Service 2 was not registered");
            Assert.AreEqual(activeServiceCount + 2, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        #endregion Service Retrieval
    }
}