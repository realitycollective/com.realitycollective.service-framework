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

namespace RealityCollective.ServiceFramework.Tests.C_ServiceProviderRegistration
{
    internal class ServiceProviderRegistrationTests
    {
        private ServiceManager testServiceManager;

        #region Service Provider Registration - Code

        [Test]
        public void Test_03_01_RegisterServiceAndServiceProvider()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);

            var testServiceProvider = new TestServiceProvider1(TestServiceProvider1.TestName, 0, null, testService1);
            var ServiceProviderResult = testServiceManager.TryRegisterService<ITestServiceProvider1>(testServiceProvider);

            // Tests
            Assert.IsTrue(ServiceProviderResult, "Test Service Provider was not registered");
            Assert.IsTrue(testServiceProvider.ServiceGuid != System.Guid.Empty);
            Assert.AreEqual(activeServiceCount + 2, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_02_RegisterMultipleServiceProviders()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(new TestService1());

            // Register
            var ServiceProvider1Result = testServiceManager.TryRegisterService<ITestServiceProvider1>(new TestServiceProvider1(TestServiceProvider1.TestName, 0, null, testService1));
            var ServiceProvider2Result = testServiceManager.TryRegisterService<ITestServiceProvider2>(new TestServiceProvider2(TestServiceProvider2.TestName, 0, null, testService1));

            // Tests
            Assert.IsTrue(ServiceProvider1Result, "Service Provider 1 was not registered");
            Assert.IsTrue(ServiceProvider2Result, "Service Provider 2 was not registered");
            Assert.AreEqual(activeSystemCount + 3, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_03_RegisterServiceProviderInMultipleServices()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            // Register Service 1 and Service Provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(new TestService1());
            var ServiceProvider1Result = testServiceManager.TryRegisterService<ITestServiceProvider1>(new TestServiceProvider1(TestServiceProvider1.TestName, 0, null, testService1));

            // Register Service 1 and Service Provider
            var testService2 = new TestService2();
            testServiceManager.TryRegisterService<ITestService2>(new TestService2());
            var ServiceProvider2Result = testServiceManager.TryRegisterService<ITestServiceProvider2>(new TestServiceProvider2(TestServiceProvider2.TestName, 0, null, testService2));

            // Tests
            Assert.IsTrue(ServiceProvider1Result, "Service Provider 1 was not registered");
            Assert.IsTrue(ServiceProvider2Result, "Service Provider 2 was not registered");
            Assert.AreEqual(activeSystemCount + 4, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_04_RegisterServiceProviderMultipleTimes()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(new TestService1());

            // Register
            var ServiceProvider1Result = testServiceManager.TryRegisterService<ITestServiceProvider1>(new TestServiceProvider1(TestServiceProvider1.TestName, 0, null, testService1));
            var ServiceProvider2Result = testServiceManager.TryRegisterService<ITestServiceProvider1>(new TestServiceProvider1(TestServiceProvider1.TestName, 0, null, testService1));
            LogAssert.Expect(LogType.Error, new Regex("There is already a ITestServiceProvider1.Test Service Provider 1 registered!"));

            // Tests
            Assert.IsTrue(ServiceProvider1Result, "Service Provider 1 was not registered");
            Assert.IsFalse(ServiceProvider2Result, "Service Provider 2 was registered when it should not have been");
            Assert.AreEqual(activeSystemCount + 2, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        #endregion Service Provider Registration - Code        

        #region Service Provider Registration - Config

        [Test]
        public void Test_03_05_RegisterServiceAndServiceProviderConfig()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            ITestService1 testService1;
            var config = new ServiceConfiguration<ITestService1>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult = testServiceManager.TryCreateAndRegisterService<ITestService1>(config, out testService1);

            var ServiceProviderconfig = new ServiceConfiguration<ITestServiceProvider1>(typeof(TestServiceProvider1), TestServiceProvider1.TestName, 1, AllPlatforms.Platforms, null);
            var ServiceProviderResult = testServiceManager.TryCreateAndRegisterServiceProvider<ITestServiceProvider1>(ServiceProviderconfig, testService1);

            // Tests
            Assert.IsTrue(ServiceProviderResult, "Test Service Provider was not registered");
            Assert.AreEqual(activeServiceCount + 2, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_06_RegisterMultipleServiceProvidersConfigGeneric()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            ITestService1 testService1;
            var config = new ServiceConfiguration<ITestService1>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult = testServiceManager.TryCreateAndRegisterService<ITestService1>(config, out testService1);

            var ServiceProvider1config = new ServiceConfiguration<ITestServiceProvider1>(typeof(TestServiceProvider1), TestServiceProvider1.TestName, 1, AllPlatforms.Platforms, null);
            var ServiceProvider1Result = testServiceManager.TryCreateAndRegisterServiceProvider<ITestServiceProvider1>(ServiceProvider1config, testService1);

            var ServiceProvider2config = new ServiceConfiguration<ITestServiceProvider2>(typeof(TestServiceProvider2), TestServiceProvider2.TestName, 1, AllPlatforms.Platforms, null);
            var ServiceProvider2Result = testServiceManager.TryCreateAndRegisterServiceProvider<ITestServiceProvider2>(ServiceProvider2config, testService1);

            // Tests
            Assert.IsTrue(ServiceProvider1Result, "Service Provider 1 was not registered");
            Assert.IsTrue(ServiceProvider2Result, "Service Provider 2 was not registered");
            Assert.AreEqual(activeSystemCount + 3, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_07_RegisterMultipleServiceProvidersConfigNonGeneric()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            ITestService1 testService1;
            var config = new ServiceConfiguration<ITestService1>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult = testServiceManager.TryCreateAndRegisterService(config, out testService1);

            var ServiceProvider1config = new ServiceConfiguration<ITestServiceProvider1>(typeof(TestServiceProvider1), TestServiceProvider1.TestName, 1, AllPlatforms.Platforms, null);
            var ServiceProvider1Result = testServiceManager.TryCreateAndRegisterServiceProvider(ServiceProvider1config, testService1);

            var ServiceProvider2config = new ServiceConfiguration<ITestServiceProvider2>(typeof(TestServiceProvider2), TestServiceProvider2.TestName, 1, AllPlatforms.Platforms, null);
            var ServiceProvider2Result = testServiceManager.TryCreateAndRegisterServiceProvider(ServiceProvider2config, testService1);

            // Tests
            Assert.IsTrue(ServiceProvider1Result, "Service Provider 1 was not registered");
            Assert.IsTrue(ServiceProvider2Result, "Service Provider 2 was not registered");
            Assert.AreEqual(activeSystemCount + 3, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_08_RegisterServiceProviderInMultipleServicesConfig()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            // Register Service 1 and Service Provider 1
            ITestService1 testService1;
            var config = new ServiceConfiguration<ITestService1>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult = testServiceManager.TryCreateAndRegisterService<ITestService1>(config, out testService1);

            var ServiceProvider1config = new ServiceConfiguration<ITestServiceProvider1>(typeof(TestServiceProvider1), TestServiceProvider1.TestName, 1, AllPlatforms.Platforms, null);
            var ServiceProvider1Result = testServiceManager.TryCreateAndRegisterServiceProvider<ITestServiceProvider1>(ServiceProvider1config, testService1);

            // Register Service 2 and Service Provider 2
            ITestService2 testService2;
            var config2 = new ServiceConfiguration<ITestService2>(typeof(TestService2), TestService2.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult2 = testServiceManager.TryCreateAndRegisterService<ITestService2>(config2, out testService2);

            var ServiceProvider2config = new ServiceConfiguration<ITestServiceProvider2>(typeof(TestServiceProvider2), TestServiceProvider2.TestName, 1, AllPlatforms.Platforms, null);
            var ServiceProvider2Result = testServiceManager.TryCreateAndRegisterServiceProvider<ITestServiceProvider2>(ServiceProvider2config, testService2);

            // Tests
            Assert.IsTrue(ServiceProvider1Result, "Service Provider 1 was not registered");
            Assert.IsTrue(ServiceProvider2Result, "Service Provider 2 was not registered");
            Assert.AreEqual(activeSystemCount + 3, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_09_RegisterServiceProviderMultipleTimesConfig()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            // Register Service 1
            ITestService1 testService1;
            var config = new ServiceConfiguration<ITestService1>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult = testServiceManager.TryCreateAndRegisterService<ITestService1>(config, out testService1);

            // Register Service Provider 1
            var ServiceProviderconfig = new ServiceConfiguration<ITestServiceProvider1>(typeof(TestServiceProvider1), TestServiceProvider1.TestName, 1, AllPlatforms.Platforms, null);
            var ServiceProvider1Result = testServiceManager.TryCreateAndRegisterServiceProvider<ITestServiceProvider1>(ServiceProviderconfig, testService1);

            // Register Service Provider 1 a second time
            var ServiceProviderconfig2 = new ServiceConfiguration<ITestServiceProvider1>(typeof(TestServiceProvider1), TestServiceProvider1.TestName, 1, AllPlatforms.Platforms, null);
            var ServiceProvider2Result = testServiceManager.TryCreateAndRegisterServiceProvider<ITestServiceProvider1>(ServiceProviderconfig2, testService1);
            LogAssert.Expect(LogType.Error, new Regex("There is already a ITestServiceProvider1.Test Service Provider 1 registered!"));

            // Tests
            Assert.IsTrue(ServiceProvider1Result, "Service Provider 1 was not registered");
            Assert.IsFalse(ServiceProvider2Result, "Service Provider 2 was registered when it should not have been");
            Assert.AreEqual(activeSystemCount + 2, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_10_RegisterServiceConfigurationsWithServiceProviders()
        {
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

            // Tests
            Assert.IsTrue(result, "Test services were not registered");
            Assert.AreEqual(activeServiceCount + 3, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        #endregion Service Provider Registration - Config
    }
}