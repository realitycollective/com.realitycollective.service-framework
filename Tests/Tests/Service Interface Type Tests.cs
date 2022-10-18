// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using NUnit.Framework;
using RealityCollective.ServiceFramework.Tests.Services;
using RealityCollective.ServiceFramework.Extensions;
using RealityCollective.ServiceFramework.Tests.Interfaces;
using RealityCollective.ServiceFramework.Tests.Providers;

namespace RealityCollective.ServiceFramework.Tests
{
    internal class ServiceInterfaceTypeTests
    {
        [Test]
        public void Test_TestService1_Type()
        {
            var testService1 = new TestService1();
            var interfaceType = testService1.GetType().FindServiceInterfaceType(typeof(ITestService1));

            Assert.AreEqual(typeof(ITestService1), interfaceType);
        }

        [Test]
        public void Test_TestService2_Type()
        {
            var testService2 = new TestService2();
            var interfaceType = testService2.GetType().FindServiceInterfaceType(typeof(ITestService2));

            Assert.AreEqual(typeof(ITestService2), interfaceType);
        }

        [Test]
        public void Test_TestServiceProvider1_Type()
        {
            var testService1 = new TestService1(nameof(TestService1), 0, null);
            var testServiceProvider1 = new TestServiceProvider1(nameof(TestServiceProvider1), 1, null, testService1);
            var interfaceType = testServiceProvider1.GetType().FindServiceInterfaceType(typeof(ITestServiceProvider1));

            Assert.AreEqual(typeof(ITestServiceProvider1), interfaceType);
        }

        [Test]
        public void Test_TestServiceProvider2_Type()
        {
            var testService2 = new TestService2(nameof(TestService2), 0, null);
            var testServiceProvider2 = new TestServiceProvider2(nameof(TestServiceProvider2), 1, null, testService2);
            var interfaceType = testServiceProvider2.GetType().FindServiceInterfaceType(typeof(ITestServiceProvider2));

            Assert.AreEqual(typeof(ITestServiceProvider2), interfaceType);
        }

        [Test]
        public void Test_BaseTestService1ServiceProvider_Type()
        {
            var testService1 = new TestService1(nameof(TestService1), 0, null);
            var baseTestService1ServiceProvider = new BaseTestService1ServiceProvider(nameof(BaseTestService1ServiceProvider), 1, null, testService1);
            var interfaceType = baseTestService1ServiceProvider.GetType().FindServiceInterfaceType(typeof(ITestService1ServiceProvider));

            Assert.AreEqual(typeof(ITestService1ServiceProvider), interfaceType);
        }

        [Test]
        public void Test_TestService1ServiceProviderA_Type()
        {
            var testService1 = new TestService1(nameof(TestService1), 0, null);
            var testService1ServiceProviderA = new TestService1ServiceProviderA(nameof(TestService1ServiceProviderA), 1, null, testService1);
            var interfaceType = testService1ServiceProviderA.GetType().FindServiceInterfaceType(typeof(ITestService1ServiceProviderA));

            Assert.AreEqual(typeof(ITestService1ServiceProviderA), interfaceType);
        }

        [Test]
        public void Test_TestService1ServiceProviderB_Type()
        {
            var testService1 = new TestService1(nameof(TestService1), 0, null);
            var testService1ServiceProviderB = new TestService1ServiceProviderB(nameof(TestService1ServiceProviderB), 1, null, testService1);
            var interfaceType = testService1ServiceProviderB.GetType().FindServiceInterfaceType(typeof(ITestService1ServiceProviderB));

            Assert.AreEqual(typeof(ITestService1ServiceProviderB), interfaceType);
        }
    }
}