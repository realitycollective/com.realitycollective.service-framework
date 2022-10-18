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

namespace RealityCollective.ServiceFramework.Tests.G_ServiceProviderUnRegistration
{
    internal class ServiceProviderUnRegistrationTests
    {
        private ServiceManager testServiceManager;

        #region Service Provider unRegistration

        [Test]
        public void Test_07_01_UnregisterSingleServiceProvider()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register service 1 and Service Provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestServiceProvider1>(new TestServiceProvider1(TestServiceProvider1.TestName, 0, null, testService1));

            // Retrieve registered Service Provider
            var ServiceProvider1 = testServiceManager.GetService<ITestServiceProvider1>();

            // Unregister Service Provider from service
            testService1.UnRegisterServiceProvider(ServiceProvider1);

            // Try and retrieve unregistered Service Provider
            var testServiceProvider1Unregistered = testServiceManager.GetService<ITestServiceProvider1>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestServiceProvider1)} service."));

            // Tests
            Assert.IsNotNull(ServiceProvider1, "Test Service Provider was not registered");
            Assert.IsNull(testServiceProvider1Unregistered, "Service Provider was found, it should have been unregistered");
            Assert.AreEqual(testService1.ServiceProviders.Count, 0, "ServiceProvider Count after being unregistered does not match");
            Assert.AreEqual(activeServiceCount + 1, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_07_02_UnregisterSingleServiceProviderFromMultiple()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register service 1 and Service Providers
            var testService1 = new TestService1();
            var serviceRegistration = testServiceManager.TryRegisterService<ITestService1>(testService1);
            var ServiceProvider1Registration = testServiceManager.TryRegisterService<ITestServiceProvider1>(new TestServiceProvider1(TestServiceProvider1.TestName, 0, null, testService1));
            var ServiceProvider2Registration = testServiceManager.TryRegisterService<ITestServiceProvider2>(new TestServiceProvider2(TestServiceProvider2.TestName, 0, null, testService1));

            // Retrieve registered Service Provider
            var ServiceProvider1 = testServiceManager.GetService<ITestServiceProvider1>();

            // Unregister Service Provider from service
            testService1.UnRegisterServiceProvider(ServiceProvider1);

            // Try and retrieve unregistered Service Provider
            var testServiceProvider1Unregistered = testServiceManager.GetService<ITestServiceProvider1>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestServiceProvider1)} service."));

            // Try and retrieve still registered Service
            var testServiceProvider2Unregistered = testServiceManager.GetService<ITestServiceProvider2>();

            // Tests
            Assert.IsNotNull(ServiceProvider1, "Test Service Provider was not registered");
            Assert.IsNull(testServiceProvider1Unregistered, "Service Provider was found, it should have been unregistered");
            Assert.IsNotNull(testServiceProvider2Unregistered, "Service Provider was not found, it should still be registered");
            Assert.AreEqual(testService1.ServiceProviders.Count, 1, "ServiceProvider Count after being unregistered does not match");
            Assert.AreEqual(activeServiceCount + 2, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_07_03_UnregisterSingleServiceProviderFromSecondService()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register service 1 and Service Providers
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestServiceProvider1>(new TestServiceProvider1(TestServiceProvider1.TestName, 0, null, testService1));

            // Register service 2 and Service Providers
            var testService2 = new TestService2();
            testServiceManager.TryRegisterService<ITestService2>(testService2);
            testServiceManager.TryRegisterService<ITestServiceProvider2>(new TestServiceProvider2(TestServiceProvider2.TestName, 0, null, testService2));

            // Retrieve registered Service Provider
            var ServiceProvider1 = testServiceManager.GetService<ITestServiceProvider1>();

            // Unregister Service Provider from service
            testService1.UnRegisterServiceProvider(ServiceProvider1);

            // Try and retrieve unregistered Service Provider
            var testServiceProvider1Unregistered = testServiceManager.GetService<ITestServiceProvider1>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestServiceProvider1)} service."));

            // Try and retrieve still registered Service
            var testServiceProvider2Unregistered = testServiceManager.GetService<ITestServiceProvider2>();

            // Tests
            Assert.IsNotNull(ServiceProvider1, "Test Service Provider was not registered");
            Assert.IsNull(testServiceProvider1Unregistered, "Service Provider was found, it should have been unregistered");
            Assert.AreEqual(testService2.ServiceProviders.Count, 1, "ServiceProvider Count after being unregistered does not match");
            Assert.AreEqual(activeServiceCount + 3, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_07_04_UnregisterServiceProviderDirect()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register service 1 and Service Providers
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestServiceProvider1>(new TestServiceProvider1(TestServiceProvider1.TestName, 0, null, testService1));

            // Retrieve registered Service Provider
            var ServiceProvider1 = testServiceManager.GetService<ITestServiceProvider1>();

            // Try and retrieve unregistered Service Provider direct
            var ServiceProviderUnregister = testServiceManager.TryUnregisterService<ITestServiceProvider1>(ServiceProvider1);

            // Try and retrieve unregistered Service Provider
            var testServiceProvider1Unregistered = testServiceManager.GetService<ITestServiceProvider1>();
            LogAssert.Expect(LogType.Error, new Regex($"Unable to find {nameof(ITestServiceProvider1)} service."));

            // Tests
            Assert.IsNotNull(ServiceProvider1, "Test Service Provider was not registered");
            Assert.IsTrue(ServiceProviderUnregister, "Service was not unregistered correctly");
            Assert.IsNull(testServiceProvider1Unregistered, "Service Provider was found, it should have been unregistered");
            Assert.AreEqual(testService1.ServiceProviders.Count, 0, "ServiceProvider Count after being unregistered does not match");
            Assert.AreEqual(activeServiceCount + 1, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        #endregion Service Provider unRegistration
    }
}