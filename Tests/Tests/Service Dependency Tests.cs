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

namespace RealityCollective.ServiceFramework.Tests.N_ServiceDependency
{
    internal class ServiceDependencyTests
    {
        private ServiceManager testServiceManager;

        #region Service Retrieval

        [Test]
        public void Test_10_01_ParentServiceRegistration()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            var config = new ServiceConfiguration<ITestService1>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult = testServiceManager.TryCreateAndRegisterService<ITestService1>(config, out ITestService1 testService1);

            // Tests
            Assert.IsTrue(serviceResult, "Test service was not registered");
            Assert.IsNotNull(testService1, "Test service instance was not returned");
            Assert.IsTrue(testService1.ServiceGuid != System.Guid.Empty, "No GUID generated for the test service");
            Assert.AreEqual(activeServiceCount + 1, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_10_02_DependentServiceRegistration()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            var config = new ServiceConfiguration<ITestService1>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            testServiceManager.TryCreateAndRegisterService<ITestService1>(config, out ITestService1 testService1);

            var config2 = new ServiceConfiguration<ITestDependencyService1>(typeof(DependencyTestService1), "Dependency Service", 1, AllPlatforms.Platforms, null);
            var serviceResult2 = testServiceManager.TryCreateAndRegisterService<ITestDependencyService1>(config2, out ITestDependencyService1 testService2);

            // Tests
            Assert.IsTrue(serviceResult2, "Test service 2 was not registered");
            Assert.IsNotNull(testService2, "Test service 2 instance was not returned");
            Assert.IsTrue(testService2.ServiceGuid == System.Guid.Empty, "GUID found for the second test service when none configured");
            Assert.AreEqual(activeServiceCount + 2, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
            Assert.IsNotNull((testService2 as DependencyTestService1)?.testService1, "Dependent service was not injected with parent service");
        }

        [Test]
        public void Test_10_03_DependentServiceRegistrationMissingDependency()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            var config2 = new ServiceConfiguration<ITestDependencyService1>(typeof(DependencyTestService1), "Dependency Service", 1, AllPlatforms.Platforms, null);
            var serviceResult2 = testServiceManager.TryCreateAndRegisterService<ITestDependencyService1>(config2, out ITestDependencyService1 testService2);
            LogAssert.Expect(LogType.Error, new Regex("Failed to find an ITestService1 service to inject into testService1!"));
            LogAssert.Expect(LogType.Error, new Regex("Failed to register the DependencyTestService1 service due to missing dependencies. Ensure all dependencies are registered prior to registering this service."));

            // Tests
            Assert.IsFalse(serviceResult2, "Test service 2 was registered, when it should not have been");
            Assert.IsNull(testService2, "Test service 2 instance was returned");
            Assert.AreEqual(activeServiceCount, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
            Assert.IsNull((testService2 as DependencyTestService1)?.testService1, "Dependent service was not injected with parent service");
        }

                [Test]
        public void Test_10_04_MultipleDependentServiceRegistration()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            var config = new ServiceConfiguration<ITestService1>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            testServiceManager.TryCreateAndRegisterService<ITestService1>(config, out ITestService1 testService1);

            var config2 = new ServiceConfiguration<ITestDependencyService1>(typeof(DependencyTestService1), "Dependency Service", 1, AllPlatforms.Platforms, null);
            testServiceManager.TryCreateAndRegisterService<ITestDependencyService1>(config2, out ITestDependencyService1 testService2);

            var config3 = new ServiceConfiguration<ITestDependencyService2>(typeof(DependencyTestService2), "Dependency Service", 1, AllPlatforms.Platforms, null);
            var serviceResult3 = testServiceManager.TryCreateAndRegisterService<ITestDependencyService2>(config3, out ITestDependencyService2 testService3);

            // Tests
            Assert.IsTrue(serviceResult3, "Test service 3 was not registered");
            Assert.IsNotNull(testService3, "Test service 3 instance was not returned");
            Assert.IsTrue(testService3.ServiceGuid == System.Guid.Empty, "GUID found for the second test service when none configured");
            Assert.AreEqual(activeServiceCount + 3, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
            Assert.IsNotNull((testService3 as DependencyTestService2)?.testService1, "Dependent service was not injected with parent service");
            Assert.IsNotNull((testService3 as DependencyTestService2)?.testService2, "Dependent service was not injected with parent service");
        }

        #endregion Service Retrieval
    }
}