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
        public void Test_06_02_UnregisterServiceWithServiceProvider()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register service 1 and Service Provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestServiceProvider1>(new TestServiceProvider1(TestServiceProvider1.TestName, 0, null, testService1));

            // Unregister Service
            var serviceUnregister = testServiceManager.TryUnregisterService<ITestService1>(testService1);

            // Try and retrieve unregistered Service
            var testService1Unregistered = testServiceManager.GetService<ITestService1>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestService1)} service."));

            // Try and retrieve unregistered Service Provider
            var testServiceProvider1Unregistered = testServiceManager.GetService<ITestServiceProvider1>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestServiceProvider1)} service."));

            // Tests
            Assert.IsNotNull(testService1, "Test service was not registered");
            Assert.IsTrue(serviceUnregister, "Service was not unregistered correctly");
            Assert.IsNull(testService1Unregistered, "Service was found, it should have been unregistered");
            Assert.IsNull(testServiceProvider1Unregistered, "Service Provider was found, it should have been unregistered");
            Assert.AreEqual(activeServiceCount, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_06_03_UnregisterServiceWithMultipleServiceProvider()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register service 1 and Service Provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestServiceProvider1>(new TestServiceProvider1(TestServiceProvider1.TestName, 0, null, testService1));
            testServiceManager.TryRegisterService<ITestServiceProvider2>(new TestServiceProvider2(TestServiceProvider2.TestName, 0, null, testService1));

            // Unregister Service
            var serviceUnregister = testServiceManager.TryUnregisterService<ITestService1>(testService1);

            // Try and retrieve unregistered Service
            var testService1Unregistered = testServiceManager.GetService<ITestService1>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestService1)} service."));

            // Try and retrieve unregistered Service Provider
            var testServiceProvider1Unregistered = testServiceManager.GetService<ITestServiceProvider1>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestServiceProvider1)} service."));
            var testServiceProvider2Unregistered = testServiceManager.GetService<ITestServiceProvider2>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestServiceProvider2)} service."));


            // Tests
            Assert.IsNotNull(testService1, "Test service was not registered");
            Assert.IsTrue(serviceUnregister, "Service was not unregistered correctly");
            Assert.IsNull(testService1Unregistered, "Service was found, it should have been unregistered");
            Assert.IsNull(testServiceProvider1Unregistered, "Service Provider 1 was found, it should have been unregistered");
            Assert.IsNull(testServiceProvider2Unregistered, "Service Provider 2 was found, it should have been unregistered");
            Assert.AreEqual(activeServiceCount, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_06_04_UnregisterSingleServiceFromMultiple()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register service 1 and Service Provider
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
        public void Test_06_06_UnregisterServiceWithServiceProviderByInterface()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register service 1 and Service Provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestServiceProvider1>(new TestServiceProvider1(TestServiceProvider1.TestName, 0, null, testService1));

            // Unregister Service
            var serviceUnregister = testServiceManager.TryUnregisterServicesOfType<ITestService1>();

            // Try and retrieve unregistered Service
            var testService1Unregistered = testServiceManager.GetService<ITestService1>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestService1)} service."));

            // Try and retrieve unregistered Service Provider
            var testServiceProvider1Unregistered = testServiceManager.GetService<ITestServiceProvider1>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestServiceProvider1)} service."));

            // Tests
            Assert.IsNotNull(testService1, "Test service was not registered");
            Assert.IsTrue(serviceUnregister, "Service was not unregistered correctly");
            Assert.IsNull(testService1Unregistered, "Service was found, it should have been unregistered");
            Assert.IsNull(testServiceProvider1Unregistered, "Service Provider was found, it should have been unregistered");
            Assert.AreEqual(activeServiceCount, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_06_07_UnregisterServiceWithMultipleServiceProviderByInterface()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register service 1 and Service Provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestServiceProvider1>(new TestServiceProvider1(TestServiceProvider1.TestName, 0, null, testService1));
            testServiceManager.TryRegisterService<ITestServiceProvider2>(new TestServiceProvider2(TestServiceProvider2.TestName, 0, null, testService1));

            // Unregister Service
            var serviceUnregister = testServiceManager.TryUnregisterServicesOfType<ITestService1>();

            // Try and retrieve unregistered Service
            var testService1Unregistered = testServiceManager.GetService<ITestService1>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestService1)} service."));

            // Try and retrieve unregistered Service Provider
            var testServiceProvider1Unregistered = testServiceManager.GetService<ITestServiceProvider1>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestServiceProvider1)} service."));
            var testServiceProvider2Unregistered = testServiceManager.GetService<ITestServiceProvider2>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestServiceProvider2)} service."));


            // Tests
            Assert.IsNotNull(testService1, "Test service was not registered");
            Assert.IsTrue(serviceUnregister, "Service was not unregistered correctly");
            Assert.IsNull(testService1Unregistered, "Service was found, it should have been unregistered");
            Assert.IsNull(testServiceProvider1Unregistered, "Service Provider 1 was found, it should have been unregistered");
            Assert.IsNull(testServiceProvider2Unregistered, "Service Provider 2 was found, it should have been unregistered");
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

        #endregion Service unRegistration
    }
}