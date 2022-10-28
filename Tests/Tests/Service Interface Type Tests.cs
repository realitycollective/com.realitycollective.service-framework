// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using NUnit.Framework;
using RealityCollective.ServiceFramework.Tests.Services;
using RealityCollective.ServiceFramework.Extensions;
using RealityCollective.ServiceFramework.Tests.Interfaces;
using RealityCollective.ServiceFramework.Tests.Modules;

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
        public void Test_TestServiceModule1_Type()
        {
            var testService1 = new TestService1(nameof(TestService1), 0, null);
            var testServiceModule1 = new TestServiceModule1(nameof(TestServiceModule1), 1, null, testService1);
            var interfaceType = testServiceModule1.GetType().FindServiceInterfaceType(typeof(ITestServiceModule1));

            Assert.AreEqual(typeof(ITestServiceModule1), interfaceType);
        }

        [Test]
        public void Test_TestServiceModule2_Type()
        {
            var testService2 = new TestService2(nameof(TestService2), 0, null);
            var testServiceModule2 = new TestServiceModule2(nameof(TestServiceModule2), 1, null, testService2);
            var interfaceType = testServiceModule2.GetType().FindServiceInterfaceType(typeof(ITestServiceModule2));

            Assert.AreEqual(typeof(ITestServiceModule2), interfaceType);
        }

        [Test]
        public void Test_BaseTestService1ServiceModule_Type()
        {
            var testService1 = new TestService1(nameof(TestService1), 0, null);
            var baseTestService1ServiceModule = new BaseTestService1ServiceModule(nameof(BaseTestService1ServiceModule), 1, null, testService1);
            var interfaceType = baseTestService1ServiceModule.GetType().FindServiceInterfaceType(typeof(ITestService1ServiceModule));

            Assert.AreEqual(typeof(ITestService1ServiceModule), interfaceType);
        }

        [Test]
        public void Test_TestService1ServiceModuleA_Type()
        {
            var testService1 = new TestService1(nameof(TestService1), 0, null);
            var testService1ServiceModuleA = new TestService1ServiceModuleA(nameof(TestService1ServiceModuleA), 1, null, testService1);
            var interfaceType = testService1ServiceModuleA.GetType().FindServiceInterfaceType(typeof(ITestService1ServiceModuleA));

            Assert.AreEqual(typeof(ITestService1ServiceModuleA), interfaceType);
        }

        [Test]
        public void Test_TestService1ServiceModuleB_Type()
        {
            var testService1 = new TestService1(nameof(TestService1), 0, null);
            var testService1ServiceModuleB = new TestService1ServiceModuleB(nameof(TestService1ServiceModuleB), 1, null, testService1);
            var interfaceType = testService1ServiceModuleB.GetType().FindServiceInterfaceType(typeof(ITestService1ServiceModuleB));

            Assert.AreEqual(typeof(ITestService1ServiceModuleB), interfaceType);
        }
    }
}