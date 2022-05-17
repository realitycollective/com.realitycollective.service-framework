// Copyright (c) Reality Collective. All rights reserved.

using NUnit.Framework;
using RealityToolkit.ServiceFramework.Definitions;
using RealityToolkit.ServiceFramework.Definitions.Platforms;
using RealityToolkit.ServiceFramework.Interfaces;
using RealityToolkit.ServiceFramework.Services;
using RealityToolkit.ServiceFramework.Tests.Interfaces;
using RealityToolkit.ServiceFramework.Tests.Profiles;
using RealityToolkit.ServiceFramework.Tests.Providers;
using RealityToolkit.ServiceFramework.Tests.Services;
using RealityToolkit.ServiceFramework.Tests.Utilities;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;

namespace RealityToolkit.ServiceFramework.Tests.C_DataProviderRegistration
{
    internal class DataProviderRegistrationTests
    {
        private ServiceManager testServiceManager;

        #region Data Provider Registration - Code

        [Test]
        public void Test_03_01_RegisterServiceAndDataProvider()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService1);

            var testDataProvider = new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1);
            var dataProviderResult = testServiceManager.TryRegisterService<ITestDataProvider1>(testDataProvider);

            // Tests
            Assert.IsTrue(dataProviderResult, "Test data provider was not registered");
            Assert.IsTrue(testDataProvider.ServiceGuid != System.Guid.Empty);
            Assert.AreEqual(activeServiceCount + 2, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_02_RegisterMultipleDataProviders()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(new TestService1());

            // Register
            var dataProvider1Result = testServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1));
            var dataProvider2Result = testServiceManager.TryRegisterService<ITestDataProvider2>(new TestDataProvider2(TestDataProvider2.TestName, 0, null, testService1));

            // Tests
            Assert.IsTrue(dataProvider1Result, "Data Provider 1 was not registered");
            Assert.IsTrue(dataProvider2Result, "Data Provider 2 was not registered");
            Assert.AreEqual(activeSystemCount + 3, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_03_RegisterDataProviderInMultipleServices()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            // Register Service 1 and data provider
            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(new TestService1());
            var dataProvider1Result = testServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1));

            // Register Service 1 and data provider
            var testService2 = new TestService2();
            testServiceManager.TryRegisterService<ITestService2>(new TestService2());
            var dataProvider2Result = testServiceManager.TryRegisterService<ITestDataProvider2>(new TestDataProvider2(TestDataProvider2.TestName, 0, null, testService2));

            // Tests
            Assert.IsTrue(dataProvider1Result, "Data Provider 1 was not registered");
            Assert.IsTrue(dataProvider2Result, "Data Provider 2 was not registered");
            Assert.AreEqual(activeSystemCount + 4, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_04_RegisterDataProviderMultipleTimes()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            var testService1 = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(new TestService1());

            // Register
            var dataProvider1Result = testServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1));
            var dataProvider2Result = testServiceManager.TryRegisterService<ITestDataProvider1>(new TestDataProvider1(TestDataProvider1.TestName, 0, null, testService1));
            LogAssert.Expect(LogType.Error, new Regex("There is already a ITestDataProvider1.Test Data Provider 1 registered!"));

            // Tests
            Assert.IsTrue(dataProvider1Result, "Data Provider 1 was not registered");
            Assert.IsFalse(dataProvider2Result, "Data Provider 2 was registered when it should not have been");
            Assert.AreEqual(activeSystemCount + 2, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        #endregion Data Provider Registration - Code        

        #region Data Provider Registration - Config

        [Test]
        public void Test_03_05_RegisterServiceAndDataProviderConfig()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            ITestService1 testService1;
            var config = new ServiceConfiguration<ITestService1>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult = testServiceManager.TryCreateAndRegisterService<ITestService1>(config, out testService1);

            var dataProviderconfig = new ServiceConfiguration<ITestDataProvider1>(typeof(TestDataProvider1), TestDataProvider1.TestName, 1, AllPlatforms.Platforms, null);
            var dataProviderResult = testServiceManager.TryCreateAndRegisterDataProvider<ITestDataProvider1>(dataProviderconfig, testService1);

            // Tests
            Assert.IsTrue(dataProviderResult, "Test data provider was not registered");
            Assert.AreEqual(activeServiceCount + 2, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_06_RegisterMultipleDataProvidersConfigGeneric()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            ITestService1 testService1;
            var config = new ServiceConfiguration<ITestService1>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult = testServiceManager.TryCreateAndRegisterService<ITestService1>(config, out testService1);

            var dataProvider1config = new ServiceConfiguration<ITestDataProvider1>(typeof(TestDataProvider1), TestDataProvider1.TestName, 1, AllPlatforms.Platforms, null);
            var dataProvider1Result = testServiceManager.TryCreateAndRegisterDataProvider<ITestDataProvider1>(dataProvider1config, testService1);

            var dataProvider2config = new ServiceConfiguration<ITestDataProvider2>(typeof(TestDataProvider2), TestDataProvider2.TestName, 1, AllPlatforms.Platforms, null);
            var dataProvider2Result = testServiceManager.TryCreateAndRegisterDataProvider<ITestDataProvider2>(dataProvider2config, testService1);

            // Tests
            Assert.IsTrue(dataProvider1Result, "Data Provider 1 was not registered");
            Assert.IsTrue(dataProvider2Result, "Data Provider 2 was not registered");
            Assert.AreEqual(activeSystemCount + 3, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_07_RegisterMultipleDataProvidersConfigNonGeneric()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            ITestService1 testService1;
            var config = new ServiceConfiguration<ITestService1>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult = testServiceManager.TryCreateAndRegisterService(config, out testService1);

            var dataProvider1config = new ServiceConfiguration<ITestDataProvider1>(typeof(TestDataProvider1), TestDataProvider1.TestName, 1, AllPlatforms.Platforms, null);
            var dataProvider1Result = testServiceManager.TryCreateAndRegisterDataProvider(dataProvider1config, testService1);

            var dataProvider2config = new ServiceConfiguration<ITestDataProvider2>(typeof(TestDataProvider2), TestDataProvider2.TestName, 1, AllPlatforms.Platforms, null);
            var dataProvider2Result = testServiceManager.TryCreateAndRegisterDataProvider(dataProvider2config, testService1);

            // Tests
            Assert.IsTrue(dataProvider1Result, "Data Provider 1 was not registered");
            Assert.IsTrue(dataProvider2Result, "Data Provider 2 was not registered");
            Assert.AreEqual(activeSystemCount + 3, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_08_RegisterDataProviderInMultipleServicesConfig()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            // Register Service 1 and data provider 1
            ITestService1 testService1;
            var config = new ServiceConfiguration<ITestService1>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult = testServiceManager.TryCreateAndRegisterService<ITestService1>(config, out testService1);

            var dataProvider1config = new ServiceConfiguration<ITestDataProvider1>(typeof(TestDataProvider1), TestDataProvider1.TestName, 1, AllPlatforms.Platforms, null);
            var dataProvider1Result = testServiceManager.TryCreateAndRegisterDataProvider<ITestDataProvider1>(dataProvider1config, testService1);

            // Register Service 2 and data provider 2
            ITestService2 testService2;
            var config2 = new ServiceConfiguration<ITestService2>(typeof(TestService2), TestService2.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult2 = testServiceManager.TryCreateAndRegisterService<ITestService2>(config2, out testService2);

            var dataProvider2config = new ServiceConfiguration<ITestDataProvider2>(typeof(TestDataProvider2), TestDataProvider2.TestName, 1, AllPlatforms.Platforms, null);
            var dataProvider2Result = testServiceManager.TryCreateAndRegisterDataProvider<ITestDataProvider2>(dataProvider2config, testService2);

            // Tests
            Assert.IsTrue(dataProvider1Result, "Data Provider 1 was not registered");
            Assert.IsTrue(dataProvider2Result, "Data Provider 2 was not registered");
            Assert.AreEqual(activeSystemCount + 3, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_09_RegisterDataProviderMultipleTimesConfig()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeSystemCount = testServiceManager.ActiveServices.Count;

            // Register Service 1
            ITestService1 testService1;
            var config = new ServiceConfiguration<ITestService1>(typeof(TestService1), TestService1.TestName, 1, AllPlatforms.Platforms, null);
            var serviceResult = testServiceManager.TryCreateAndRegisterService<ITestService1>(config, out testService1);

            // Register Data Provider 1
            var dataProviderconfig = new ServiceConfiguration<ITestDataProvider1>(typeof(TestDataProvider1), TestDataProvider1.TestName, 1, AllPlatforms.Platforms, null);
            var dataProvider1Result = testServiceManager.TryCreateAndRegisterDataProvider<ITestDataProvider1>(dataProviderconfig, testService1);

            // Register Data Provider 1 a second time
            var dataProviderconfig2 = new ServiceConfiguration<ITestDataProvider1>(typeof(TestDataProvider1), TestDataProvider1.TestName, 1, AllPlatforms.Platforms, null);
            var dataProvider2Result = testServiceManager.TryCreateAndRegisterDataProvider<ITestDataProvider1>(dataProviderconfig2, testService1);
            LogAssert.Expect(LogType.Error, new Regex("There is already a ITestDataProvider1.Test Data Provider 1 registered!"));

            // Tests
            Assert.IsTrue(dataProvider1Result, "Data Provider 1 was not registered");
            Assert.IsFalse(dataProvider2Result, "Data Provider 2 was registered when it should not have been");
            Assert.AreEqual(activeSystemCount + 2, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        [Test]
        public void Test_03_10_RegisterServiceConfigurationsWithDataProviders()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            var activeServiceCount = testServiceManager.ActiveServices.Count;

            var configurations = new ServiceConfiguration<IService>[2];


            var testService1Profile = (TestService1Profile)ScriptableObject.CreateInstance(typeof(TestService1Profile));
            var dataProvider1Configuration = new ServiceConfiguration<ITestDataProvider1>(typeof(TestDataProvider1), TestDataProvider1.TestName, 1, AllPlatforms.Platforms, null);
            testService1Profile.AddConfiguration(dataProvider1Configuration);

            var testService2Profile = (TestService2Profile)ScriptableObject.CreateInstance(typeof(TestService2Profile));
            var dataProvider2Configuration = new ServiceConfiguration<ITestDataProvider2>(typeof(TestDataProvider2), TestDataProvider2.TestName, 1, AllPlatforms.Platforms, null);
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