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
        public void Test_TestDataProvider1_Type()
        {
            var testService1 = new TestService1(nameof(TestService1), 0, null);
            var testDataProvider1 = new TestDataProvider1(nameof(TestDataProvider1), 1, null, testService1);
            var interfaceType = testDataProvider1.GetType().FindServiceInterfaceType(typeof(ITestDataProvider1));

            Assert.AreEqual(typeof(ITestDataProvider1), interfaceType);
        }

        [Test]
        public void Test_TestDataProvider2_Type()
        {
            var testService2 = new TestService2(nameof(TestService2), 0, null);
            var testDataProvider2 = new TestDataProvider2(nameof(TestDataProvider2), 1, null, testService2);
            var interfaceType = testDataProvider2.GetType().FindServiceInterfaceType(typeof(ITestDataProvider2));

            Assert.AreEqual(typeof(ITestDataProvider2), interfaceType);
        }

        [Test]
        public void Test_BaseTestService1DataProvider_Type()
        {
            var testService1 = new TestService1(nameof(TestService1), 0, null);
            var baseTestService1DataProvider = new BaseTestService1DataProvider(nameof(BaseTestService1DataProvider), 1, null, testService1);
            var interfaceType = baseTestService1DataProvider.GetType().FindServiceInterfaceType(typeof(ITestService1DataProvider));

            Assert.AreEqual(typeof(ITestService1DataProvider), interfaceType);
        }

        [Test]
        public void Test_TestService1DataProviderA_Type()
        {
            var testService1 = new TestService1(nameof(TestService1), 0, null);
            var testService1DataProviderA = new TestService1DataProviderA(nameof(TestService1DataProviderA), 1, null, testService1);
            var interfaceType = testService1DataProviderA.GetType().FindServiceInterfaceType(typeof(ITestService1DataProviderA));

            Assert.AreEqual(typeof(ITestService1DataProviderA), interfaceType);
        }

        [Test]
        public void Test_TestService1DataProviderB_Type()
        {
            var testService1 = new TestService1(nameof(TestService1), 0, null);
            var testService1DataProviderB = new TestService1DataProviderB(nameof(TestService1DataProviderB), 1, null, testService1);
            var interfaceType = testService1DataProviderB.GetType().FindServiceInterfaceType(typeof(ITestService1DataProviderB));

            Assert.AreEqual(typeof(ITestService1DataProviderB), interfaceType);
        }
    }
}