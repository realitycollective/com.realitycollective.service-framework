// Copyright (c) Reality Collective. All rights reserved.

using NUnit.Framework;
using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Definitions.Platforms;
using RealityCollective.ServiceFramework.Interfaces;
using RealityCollective.ServiceFramework.Services;
using RealityCollective.ServiceFramework.Tests.Interfaces;
using RealityCollective.ServiceFramework.Tests.Profiles;
using RealityCollective.ServiceFramework.Tests.Modules;
using RealityCollective.ServiceFramework.Tests.Services;
using RealityCollective.ServiceFramework.Tests.Utilities;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;

namespace RealityCollective.ServiceFramework.Tests.C_ServiceModuleRegistration
{
    internal class ServiceModuleRegistrationTests
    {
        private ServiceManager testServiceManager;

        #region Data Provider Registration - Code

        [Test]
        public void Test_03_01_RegisterServiceAndServiceModule()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);

            var testServiceModule = new TestServiceModule1(TestServiceModule1.TestName, 0, null, testService1);
            var dataProviderResult = testServiceManager.TryRegisterService<ITestServiceModule1>(testServiceModule);

            // Tests
            Assert.IsTrue(dataProviderResult, "Test data provider was not registered");
            Assert.IsTrue(testServiceModule.ServiceGuid != System.Guid.Empty);
            Assert.AreEqual(activeServiceCount + 2, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_02_RegisterMultipleServiceModules()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(new TestService1());

            // Register
            var dataProvider1Result = testServiceManager.TryRegisterService<ITestServiceModule1>(new TestServiceModule1(TestServiceModule1.TestName, 0, null, testService1));
            var dataProvider2Result = testServiceManager.TryRegisterService<ITestServiceModule2>(new TestServiceModule2(TestServiceModule2.TestName, 0, null, testService1));

            // Tests
            Assert.IsTrue(dataProvider1Result, "Data Provider 1 was not registered");
            Assert.IsTrue(dataProvider2Result, "Data Provider 2 was not registered");
            Assert.AreEqual(activeSystemCount + 3, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_03_RegisterServiceModuleInMultipleServices()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            // Register Service 1 and data provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(new TestService1());
            var dataProvider1Result = testServiceManager.TryRegisterService<ITestServiceModule1>(new TestServiceModule1(TestServiceModule1.TestName, 0, null, testService1));

            // Register Service 1 and data provider
            var testService2 = new TestService2();
            testServiceManager.TryRegisterService<ITestService2>(new TestService2());
            var dataProvider2Result = testServiceManager.TryRegisterService<ITestServiceModule2>(new TestServiceModule2(TestServiceModule2.TestName, 0, null, testService2));

            // Tests
            Assert.IsTrue(dataProvider1Result, "Data Provider 1 was not registered");
            Assert.IsTrue(dataProvider2Result, "Data Provider 2 was not registered");
            Assert.AreEqual(activeSystemCount + 4, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_04_RegisterServiceModuleMultipleTimes()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(new TestService1());

            // Register
            var dataProvider1Result = testServiceManager.TryRegisterService<ITestServiceModule1>(new TestServiceModule1(TestServiceModule1.TestName, 0, null, testService1));
            var dataProvider2Result = testServiceManager.TryRegisterService<ITestServiceModule1>(new TestServiceModule1(TestServiceModule1.TestName, 0, null, testService1));
            LogAssert.Expect(LogType.Error, new Regex("There is already a ITestServiceModule1.Test Data Provider 1 registered!"));

            // Tests
            Assert.IsTrue(dataProvider1Result, "Data Provider 1 was not registered");
            Assert.IsFalse(dataProvider2Result, "Data Provider 2 was registered when it should not have been");
            Assert.AreEqual(activeSystemCount + 2, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        #endregion Data Provider Registration - Code        

        #region Data Provider Registration - Config

        [Test]
        public void Test_03_05_RegisterServiceAndServiceModuleConfig()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            ITestService1 testService1;
            var config = new ServiceConfiguration<ITestService1>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult = testServiceManager.TryCreateAndRegisterService<ITestService1>(config, out testService1);

            var dataProviderconfig = new ServiceConfiguration<ITestServiceModule1>(typeof(TestServiceModule1), TestServiceModule1.TestName, 1, AllPlatforms.Platforms, null);
            var dataProviderResult = testServiceManager.TryCreateAndRegisterServiceModule<ITestServiceModule1>(dataProviderconfig, testService1);

            // Tests
            Assert.IsTrue(dataProviderResult, "Test data provider was not registered");
            Assert.AreEqual(activeServiceCount + 2, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_06_RegisterMultipleServiceModulesConfigGeneric()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            ITestService1 testService1;
            var config = new ServiceConfiguration<ITestService1>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult = testServiceManager.TryCreateAndRegisterService<ITestService1>(config, out testService1);

            var dataProvider1config = new ServiceConfiguration<ITestServiceModule1>(typeof(TestServiceModule1), TestServiceModule1.TestName, 1, AllPlatforms.Platforms, null);
            var dataProvider1Result = testServiceManager.TryCreateAndRegisterServiceModule<ITestServiceModule1>(dataProvider1config, testService1);

            var dataProvider2config = new ServiceConfiguration<ITestServiceModule2>(typeof(TestServiceModule2), TestServiceModule2.TestName, 1, AllPlatforms.Platforms, null);
            var dataProvider2Result = testServiceManager.TryCreateAndRegisterServiceModule<ITestServiceModule2>(dataProvider2config, testService1);

            // Tests
            Assert.IsTrue(dataProvider1Result, "Data Provider 1 was not registered");
            Assert.IsTrue(dataProvider2Result, "Data Provider 2 was not registered");
            Assert.AreEqual(activeSystemCount + 3, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_07_RegisterMultipleServiceModulesConfigNonGeneric()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            ITestService1 testService1;
            var config = new ServiceConfiguration<ITestService1>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult = testServiceManager.TryCreateAndRegisterService(config, out testService1);

            var dataProvider1config = new ServiceConfiguration<ITestServiceModule1>(typeof(TestServiceModule1), TestServiceModule1.TestName, 1, AllPlatforms.Platforms, null);
            var dataProvider1Result = testServiceManager.TryCreateAndRegisterServiceModule(dataProvider1config, testService1);

            var dataProvider2config = new ServiceConfiguration<ITestServiceModule2>(typeof(TestServiceModule2), TestServiceModule2.TestName, 1, AllPlatforms.Platforms, null);
            var dataProvider2Result = testServiceManager.TryCreateAndRegisterServiceModule(dataProvider2config, testService1);

            // Tests
            Assert.IsTrue(dataProvider1Result, "Data Provider 1 was not registered");
            Assert.IsTrue(dataProvider2Result, "Data Provider 2 was not registered");
            Assert.AreEqual(activeSystemCount + 3, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_08_RegisterServiceModuleInMultipleServicesConfig()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            // Register Service 1 and data provider 1
            ITestService1 testService1;
            var config = new ServiceConfiguration<ITestService1>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult = testServiceManager.TryCreateAndRegisterService<ITestService1>(config, out testService1);

            var dataProvider1config = new ServiceConfiguration<ITestServiceModule1>(typeof(TestServiceModule1), TestServiceModule1.TestName, 1, AllPlatforms.Platforms, null);
            var dataProvider1Result = testServiceManager.TryCreateAndRegisterServiceModule<ITestServiceModule1>(dataProvider1config, testService1);

            // Register Service 2 and data provider 2
            ITestService2 testService2;
            var config2 = new ServiceConfiguration<ITestService2>(typeof(TestService2), TestService2.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult2 = testServiceManager.TryCreateAndRegisterService<ITestService2>(config2, out testService2);

            var dataProvider2config = new ServiceConfiguration<ITestServiceModule2>(typeof(TestServiceModule2), TestServiceModule2.TestName, 1, AllPlatforms.Platforms, null);
            var dataProvider2Result = testServiceManager.TryCreateAndRegisterServiceModule<ITestServiceModule2>(dataProvider2config, testService2);

            // Tests
            Assert.IsTrue(dataProvider1Result, "Data Provider 1 was not registered");
            Assert.IsTrue(dataProvider2Result, "Data Provider 2 was not registered");
            Assert.AreEqual(activeSystemCount + 3, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_09_RegisterServiceModuleMultipleTimesConfig()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            // Register Service 1
            ITestService1 testService1;
            var config = new ServiceConfiguration<ITestService1>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult = testServiceManager.TryCreateAndRegisterService<ITestService1>(config, out testService1);

            // Register Data Provider 1
            var dataProviderconfig = new ServiceConfiguration<ITestServiceModule1>(typeof(TestServiceModule1), TestServiceModule1.TestName, 1, AllPlatforms.Platforms, null);
            var dataProvider1Result = testServiceManager.TryCreateAndRegisterServiceModule<ITestServiceModule1>(dataProviderconfig, testService1);

            // Register Data Provider 1 a second time
            var dataProviderconfig2 = new ServiceConfiguration<ITestServiceModule1>(typeof(TestServiceModule1), TestServiceModule1.TestName, 1, AllPlatforms.Platforms, null);
            var dataProvider2Result = testServiceManager.TryCreateAndRegisterServiceModule<ITestServiceModule1>(dataProviderconfig2, testService1);
            LogAssert.Expect(LogType.Error, new Regex("There is already a ITestServiceModule1.Test Data Provider 1 registered!"));

            // Tests
            Assert.IsTrue(dataProvider1Result, "Data Provider 1 was not registered");
            Assert.IsFalse(dataProvider2Result, "Data Provider 2 was registered when it should not have been");
            Assert.AreEqual(activeSystemCount + 2, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_10_RegisterServiceConfigurationsWithServiceModules()
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
        }

        #endregion Data Provider Registration - Config
    }
}