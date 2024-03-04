// Copyright (c) Reality Collective. All rights reserved.

using NUnit.Framework;
using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Definitions.Platforms;
using RealityCollective.ServiceFramework.Interfaces;
using RealityCollective.ServiceFramework.Services;
using RealityCollective.ServiceFramework.Tests.Interfaces;
using RealityCollective.ServiceFramework.Tests.Modules;
using RealityCollective.ServiceFramework.Tests.Profiles;
using RealityCollective.ServiceFramework.Tests.Services;
using RealityCollective.ServiceFramework.Tests.Utilities;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;

namespace RealityCollective.ServiceFramework.Tests.F_ServiceUnRegistration
{
    internal class ServiceUnRegistrationTests
    {
        private ServiceManager testServiceManager;

        #region Service unRegistration

        [Test]
        public void Test_06_01_UnregisterSingleService()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register service 1
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);

            var serviceUnregister = testServiceManager.TryUnregisterService<ITestService1>(testService1);

            var testService1Unregistered = testServiceManager.GetService<ITestService1>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestService1)} service."));

            // Tests
            Assert.IsNotNull(testService1, "Test service was not registered");
            Assert.IsTrue(serviceUnregister, "Service was not unregistered correctly");
            Assert.IsNull(testService1Unregistered, "Service was found, it should have been unregistered");
            Assert.AreEqual(activeServiceCount, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_06_02_UnregisterServiceWithServiceModule()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register service 1 and service provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestServiceModule1>(new TestServiceModule1(TestServiceModule1.TestName, 0, null, testService1));

            // Unregister Service
            var serviceUnregister = testServiceManager.TryUnregisterService<ITestService1>(testService1);

            // Try and retrieve unregistered Service
            var testService1Unregistered = testServiceManager.GetService<ITestService1>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestService1)} service."));

            // Try and retrieve unregistered Service Module
            var testServiceModule1Unregistered = testServiceManager.GetService<ITestServiceModule1>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestServiceModule1)} service."));

            // Tests
            Assert.IsNotNull(testService1, "Test service was not registered");
            Assert.IsTrue(serviceUnregister, "Service was not unregistered correctly");
            Assert.IsNull(testService1Unregistered, "Service was found, it should have been unregistered");
            Assert.IsNull(testServiceModule1Unregistered, "Service Module was found, it should have been unregistered");
            Assert.AreEqual(activeServiceCount, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_06_03_UnregisterServiceWithMultipleServiceModule()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register service 1 and service provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestServiceModule1>(new TestServiceModule1(TestServiceModule1.TestName, 0, null, testService1));
            testServiceManager.TryRegisterService<ITestServiceModule2>(new TestServiceModule2(TestServiceModule2.TestName, 0, null, testService1));

            // Unregister Service
            var serviceUnregister = testServiceManager.TryUnregisterService<ITestService1>(testService1);

            // Try and retrieve unregistered Service
            var testService1Unregistered = testServiceManager.GetService<ITestService1>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestService1)} service."));

            // Try and retrieve unregistered Service Module
            var testServiceModule1Unregistered = testServiceManager.GetService<ITestServiceModule1>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestServiceModule1)} service."));
            var testServiceModule2Unregistered = testServiceManager.GetService<ITestServiceModule2>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestServiceModule2)} service."));


            // Tests
            Assert.IsNotNull(testService1, "Test service was not registered");
            Assert.IsTrue(serviceUnregister, "Service was not unregistered correctly");
            Assert.IsNull(testService1Unregistered, "Service was found, it should have been unregistered");
            Assert.IsNull(testServiceModule1Unregistered, "Service Module 1 was found, it should have been unregistered");
            Assert.IsNull(testServiceModule2Unregistered, "Service Module 2 was found, it should have been unregistered");
            Assert.AreEqual(activeServiceCount, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_06_04_UnregisterSingleServiceFromMultiple()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register service 1 and service provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);

            // Register service 2
            testServiceManager.TryRegisterService<ITestService2>(new TestService2());

            // Unregister Service 1
            var serviceUnregister = testServiceManager.TryUnregisterService<ITestService1>(testService1);

            // Try and retrieve unregistered Service 1
            var testService1Unregistered = testServiceManager.GetService<ITestService1>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestService1)} service."));

            // Try and retrieve still registered Service 2
            var testService2 = testServiceManager.GetService<ITestService2>();

            // Tests
            Assert.IsNotNull(testService1, "Test service was not registered");
            Assert.IsNotNull(testService2, "Test service 2 was not registered");
            Assert.IsTrue(serviceUnregister, "Service was not unregistered correctly");
            Assert.IsNull(testService1Unregistered, "Service was found, it should have been unregistered");
            Assert.AreEqual(activeServiceCount + 1, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_06_05_UnregisterSingleServiceByInterface()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register service 1
            testServiceManager.TryRegisterService<ITestService1>(new TestService1());

            var serviceUnregister = testServiceManager.TryUnregisterServicesOfType<ITestService1>();

            var testService1Unregistered = testServiceManager.GetService<ITestService1>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestService1)} service."));

            // Tests
            Assert.IsTrue(serviceUnregister, "Service was not unregistered correctly");
            Assert.IsNull(testService1Unregistered, "Service was found, it should have been unregistered");
            Assert.AreEqual(activeServiceCount, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_06_06_UnregisterServiceWithServiceModuleByInterface()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register service 1 and service provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestServiceModule1>(new TestServiceModule1(TestServiceModule1.TestName, 0, null, testService1));

            // Unregister Service
            var serviceUnregister = testServiceManager.TryUnregisterServicesOfType<ITestService1>();

            // Try and retrieve unregistered Service
            var testService1Unregistered = testServiceManager.GetService<ITestService1>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestService1)} service."));

            // Try and retrieve unregistered Service Module
            var testServiceModule1Unregistered = testServiceManager.GetService<ITestServiceModule1>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestServiceModule1)} service."));

            // Tests
            Assert.IsNotNull(testService1, "Test service was not registered");
            Assert.IsTrue(serviceUnregister, "Service was not unregistered correctly");
            Assert.IsNull(testService1Unregistered, "Service was found, it should have been unregistered");
            Assert.IsNull(testServiceModule1Unregistered, "Service Module was found, it should have been unregistered");
            Assert.AreEqual(activeServiceCount, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_06_07_UnregisterServiceWithMultipleServiceModuleByInterface()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register service 1 and service provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestServiceModule1>(new TestServiceModule1(TestServiceModule1.TestName, 0, null, testService1));
            testServiceManager.TryRegisterService<ITestServiceModule2>(new TestServiceModule2(TestServiceModule2.TestName, 0, null, testService1));

            // Unregister Service
            var serviceUnregister = testServiceManager.TryUnregisterServicesOfType<ITestService1>();

            // Try and retrieve unregistered Service
            var testService1Unregistered = testServiceManager.GetService<ITestService1>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestService1)} service."));

            // Try and retrieve unregistered Service Module
            var testServiceModule1Unregistered = testServiceManager.GetService<ITestServiceModule1>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestServiceModule1)} service."));
            var testServiceModule2Unregistered = testServiceManager.GetService<ITestServiceModule2>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestServiceModule2)} service."));


            // Tests
            Assert.IsNotNull(testService1, "Test service was not registered");
            Assert.IsTrue(serviceUnregister, "Service was not unregistered correctly");
            Assert.IsNull(testService1Unregistered, "Service was found, it should have been unregistered");
            Assert.IsNull(testServiceModule1Unregistered, "Service Module 1 was found, it should have been unregistered");
            Assert.IsNull(testServiceModule2Unregistered, "Service Module 2 was found, it should have been unregistered");
            Assert.AreEqual(activeServiceCount, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_06_08_UnregisterSingleServiceFromMultipleByInterface()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register service 1
            testServiceManager.TryRegisterService<ITestService1>(new TestService1());

            // Register service 2
            testServiceManager.TryRegisterService<ITestService2>(new TestService2());

            // Unregister Service 1
            var serviceUnregister = testServiceManager.TryUnregisterServicesOfType<ITestService1>();

            // Try and retrieve unregistered Service 1
            var testService1Unregistered = testServiceManager.GetService<ITestService1>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestService1)} service."));

            // Try and retrieve still registered Service 2
            var testService2 = testServiceManager.GetService<ITestService2>();

            // Tests
            Assert.IsNotNull(testService2, "Test service 2 was not registered");
            Assert.IsTrue(serviceUnregister, "Service was not unregistered correctly");
            Assert.IsNull(testService1Unregistered, "Service was found, it should have been unregistered");
            Assert.AreEqual(activeServiceCount + 1, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_06_09_UnRegisterServiceConfigurations()
        {
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

            // Tests
            Assert.IsTrue(result, "Test services were not registered");
            Assert.AreEqual(activeServiceCount + 3, testServiceManager.ActiveServices.Count, "More or less services found than was expected");

            var unregisterResult = testServiceManager.TryUnRegisterServiceConfigurations(configurations);
            Assert.IsTrue(result, "Test services were successfully unregistered");
            Assert.AreEqual(0, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }
        #endregion Service unRegistration
    }
}