// Copyright (c) Reality Collective. All rights reserved.

using NUnit.Framework;
using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Definitions.Platforms;
using RealityCollective.ServiceFramework.Interfaces;
using RealityCollective.ServiceFramework.Services;
using RealityCollective.ServiceFramework.Tests.Interfaces;
using RealityCollective.ServiceFramework.Tests.Services;
using RealityCollective.ServiceFramework.Tests.Utilities;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;

namespace RealityCollective.ServiceFramework.Tests.B_ServiceRegistration
{
    internal class ServiceRegistrationTests : MonoBehaviour
    {
        private ServiceManager testServiceManager;

        #region Service Registration - Code

        [Test]
        public void Test_02_01_RegisterService()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            var testService1 = new TestService1();
            var serviceResult = testServiceManager.TryRegisterService<ITestService1>(testService1);

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
            var serviceResult = testServiceManager.TryRegisterService<ITestService1>(testService1);

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
            LogAssert.Expect(LogType.Error, new Regex("There is already a ITestService1.Test Service 1 registered!"));

            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            // Register
            testServiceManager.TryRegisterService<ITestService1>(new TestService1());

            // Register again
            var testService2 = testServiceManager.TryRegisterService<ITestService1>(new TestService1());

            // Tests
            Assert.IsFalse(testService2, "Test service was registered when it should not have been");
            Assert.AreEqual(activeServiceCount + 1, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        #endregion Service Registration - Code

        #region Service Registration - Config

        [Test]
        public void Test_02_04_RegisterServiceConfig()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            ITestService1 testService1;
            var config = new ServiceConfiguration<ITestService1>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult = testServiceManager.TryCreateAndRegisterService<ITestService1>(config, out testService1);

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

            ITestService1 testService1;
            var config = new ServiceConfiguration<ITestService1>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult = testServiceManager.TryCreateAndRegisterService<ITestService1>(config, out testService1);

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
        public void Test_02_06_RegisterSecondServiceConfigNonGeneric()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            ITestService1 testService1;
            var config = new ServiceConfiguration<ITestService1>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
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
        public void Test_02_07_TryRegisterServiceTwiceConfig()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            ITestService1 testService1;
            var config = new ServiceConfiguration<ITestService1>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult = testServiceManager.TryCreateAndRegisterService<ITestService1>(config, out testService1);

            ITestService1 testService2;
            var config2 = new ServiceConfiguration<ITestService1>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult2 = testServiceManager.TryCreateAndRegisterService<ITestService1>(config, out testService2);
            LogAssert.Expect(LogType.Error, new Regex("There is already a ITestService1.Test Service 1 registered!"));

            // Tests
            Assert.IsFalse(serviceResult2, "Test service was registered when it should not have been");
            Assert.IsNotNull(testService2, "Test service 1 instance was not returned when it should have been");
            Assert.IsTrue(testService2.GetType() == typeof(TestService1), "The wrong type of service was returned");
            Assert.AreEqual(activeServiceCount + 1, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_02_08_RegisterServiceConfigurations()
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

        #endregion Service Registration - Config
    }
}