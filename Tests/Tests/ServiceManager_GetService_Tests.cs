// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using NUnit.Framework;
using RealityCollective.ServiceFramework.Services;
using RealityCollective.ServiceFramework.Tests.Interfaces;
using RealityCollective.ServiceFramework.Tests.Services;
using RealityCollective.ServiceFramework.Tests.Utilities;

namespace RealityCollective.ServiceFramework.Tests.L_ServiceInterfaceTests
{
    /// <summary>
    /// This class contains tests for all <see cref="ServiceManager"/> APIs
    /// used to retrieve a registered service instance.
    /// </summary>
    internal class ServiceManager_GetService_Tests
    {
        private ServiceManager serviceManager;

        [SetUp]
        public void SetupServiceManager_GetService_Tests()
        {
            TestUtilities.InitializeServiceManagerScene(ref serviceManager);
        }

        /// <summary>
        /// This test will test whether we can retrieve a service instance of type <see cref="TestService1"/>
        /// using it's dedicated interface type <see cref="ITestService1"/> after registering it using said interface type.
        /// </summary>
        [Test]
        public void ServiceManager_GetService_TopInterfaceType()
        {
            // Arrange
            var serviceInstance = new TestService1();
            serviceManager.TryRegisterService<ITestService1>(serviceInstance);

            // Act
            var retrievedServices = serviceManager.GetServices<ITestService1>();

            // Assert
            Assert.IsNotNull(retrievedServices);
            Assert.AreEqual(1, retrievedServices.Count, $"Number of services did not match the expected [1]");
            Assert.IsTrue(retrievedServices[0] is ITestService1);
            Assert.IsTrue(retrievedServices[0] == serviceInstance);
        }

        /// <summary>
        /// This test will test whether we can retrieve a service instance of type <see cref="TestService1"/>
        /// using it's dedicated interface type <see cref="ITestService1"/> after registering it using said interface type.
        /// With multiple services registered
        /// </summary>
        [Test]
        public void ServiceManager_GetService_TopInterfaceType_WithPopulation()
        {
            // Arrange
            var serviceInstance = new TestService1();
            serviceManager.TryRegisterService<ITestService1>(serviceInstance);
            var serviceInstance2 = new TestService2();
            serviceManager.TryRegisterService<ITestService2>(serviceInstance2);

            // Act
            var retrievedServices = serviceManager.GetServices<ITestService1>();

            // Assert
            Assert.IsNotNull(retrievedServices);
            Assert.AreEqual(1, retrievedServices.Count, $"Number of services did not match the expected [1]");
            Assert.IsTrue(retrievedServices[0] is ITestService1);
            Assert.IsTrue(retrievedServices[0] == serviceInstance);
        }

        /// <summary>
        /// This test will test whether we can retrieve a service instance of type <see cref="TestService1"/>
        /// using the base interface type <see cref="ITestService"/> which is valid for all test services. The service instance
        /// was registered using it's dedicated interface type <see cref="ITestService1"/>.
        /// </summary>
        [Test]
        public void ServiceManager_GetService_BaseInterfaceType()
        {
            // Arrange
            var serviceInstance = new TestService1();
            serviceManager.TryRegisterService<ITestService1>(serviceInstance);

            // Act
            var retrievedServices = serviceManager.GetServices<ITestService>();

            // Assert
            Assert.IsNotNull(retrievedServices);
            Assert.AreEqual(1, retrievedServices.Count, $"Number of services did not match the expected [1]");
            Assert.IsTrue(retrievedServices[0] is ITestService);
            Assert.IsTrue(retrievedServices[0] == serviceInstance);
        }


        /// <summary>
        /// This test will test whether we can retrieve a service instance of type <see cref="TestService1"/>
        /// using the base interface type <see cref="ITestService"/> which is valid for all test services. The service instance
        /// was registered using it's dedicated interface type <see cref="ITestService1"/>.
        /// With multiple services registered
        /// </summary>
        [Test]
        public void ServiceManager_GetService_BaseInterfaceType_WithPopulation()
        {
            // Arrange
            var serviceInstance = new TestService1();
            serviceManager.TryRegisterService<ITestService1>(serviceInstance);
            var serviceInstance2 = new TestService2();
            serviceManager.TryRegisterService<ITestService2>(serviceInstance2);

            // Act
            var retrievedServices = serviceManager.GetServices<ITestService>();

            // Assert
            Assert.IsNotNull(retrievedServices, "No services returned from registry");
            Assert.AreEqual(2, retrievedServices.Count, $"Number of services did not match the expected [2]");
            Assert.IsTrue(retrievedServices[0] is ITestService, $"Returned service is not of type {typeof(ITestService)}");
            Assert.IsTrue(retrievedServices[0] == serviceInstance, "Instance of service does not match the created service");
        }

        /// <summary>
        /// This test will test whether we can retrieve a service instance of type <see cref="TestService1"/>
        /// using it's dedicated interface type <see cref="ITestService1"/> after registering it using the base interface type <see cref="ITestService"/>.
        /// </summary>
        [Test]
        public void ServiceManager_GetService_TopInterfaceType_RegisteredByBaseInterfaceType()
        {
            // Arrange
            var serviceInstance = new TestService1();
            serviceManager.TryRegisterService<ITestService>(serviceInstance);

            // Act
            var retrievedServices = serviceManager.GetServices<ITestService1>();

            // Assert
            Assert.IsNotNull(retrievedServices);
            Assert.AreEqual(1, retrievedServices.Count, $"Number of services did not match the expected [1]");
            Assert.IsTrue(retrievedServices[0] is ITestService1);
            Assert.IsTrue(retrievedServices[0] == serviceInstance);
        }

        /// <summary>
        /// This test will test whether we can retrieve a service instance of type <see cref="TestService1"/>
        /// using it's dedicated interface type <see cref="ITestService1"/> after registering it using the base interface type <see cref="ITestService"/>.
        /// With multiple services registered
        /// </summary>
        [Test]
        public void ServiceManager_GetService_TopInterfaceType_RegisteredByBaseInterfaceType_WithPopulation()
        {
            // Arrange
            var serviceInstance = new TestService1();
            serviceManager.TryRegisterService<ITestService>(serviceInstance);
            var serviceInstance2 = new TestService2();
            serviceManager.TryRegisterService<ITestService2>(serviceInstance2);

            // Act
            var retrievedServices = serviceManager.GetServices<ITestService1>();

            // Assert
            Assert.IsNotNull(retrievedServices, "No services returned from registry");
            Assert.AreEqual(1, retrievedServices.Count, $"Number of services did not match the expected [1]");
            Assert.IsTrue(retrievedServices[0] is ITestService1, $"Returned service is not of type {typeof(ITestService)}");
            Assert.IsTrue(retrievedServices[0] == serviceInstance, "Instance of service does not match the created service");
        }

        /// <summary>
        /// This test will test whether we can retrieve a service instance of type <see cref="TestService1"/> 
        /// with multiple instances of <see cref="TestService1"/> registered using all of its interfaces
        /// </summary>
        [Test]
        public void ServiceManager_GetService_TopInterfaceType_RegisteredByBaseInterfaceType_Twice()
        {
            // Arrange
            var serviceInstance = new TestService1();
            serviceManager.TryRegisterService<ITestService>(serviceInstance);
            var serviceInstance2 = new TestService1();
            serviceManager.TryRegisterService<ITestService1>(serviceInstance2);

            // Act
            var retrievedServices = serviceManager.GetServices<ITestService1>();

            // Assert
            Assert.IsNotNull(retrievedServices, "No services returned from registry");
            Assert.AreEqual(2, retrievedServices.Count, $"Number of services did not match the expected [1]");
            Assert.IsTrue(retrievedServices[0] is ITestService1, $"Returned service is not of type {typeof(ITestService)}");
            Assert.IsTrue(retrievedServices[0] == serviceInstance, "Instance of service does not match the created service");
        }

        /// <summary>
        /// This test will test whether we can retrieve a service instance of type <see cref="TestService1"/> 
        /// with multiple instances of <see cref="TestService1"/> registered using all of its interfaces
        /// With multiple services registered
        /// </summary>
        [Test]
        public void ServiceManager_GetService_TopInterfaceType_RegisteredByBaseInterfaceType_Twice_WithPopulation()
        {
            // Arrange
            var serviceInstance = new TestService1();
            serviceManager.TryRegisterService<ITestService>(serviceInstance);
            var serviceInstance2 = new TestService1();
            serviceManager.TryRegisterService<ITestService1>(serviceInstance2);
            var serviceInstance3 = new TestService2();
            serviceManager.TryRegisterService<ITestService2>(serviceInstance3);

            // Act
            var retrievedServices = serviceManager.GetServices<ITestService1>();

            // Assert
            Assert.IsNotNull(retrievedServices, "No services returned from registry");
            Assert.AreEqual(2, retrievedServices.Count, $"Number of services did not match the expected [1]");
            Assert.IsTrue(retrievedServices[0] is ITestService1, $"Returned service is not of type {typeof(ITestService)}");
            Assert.IsTrue(retrievedServices[0] == serviceInstance, "Instance of service does not match the created service");
        }

        /// <summary>
        /// This test will test whether we can retrieve a service instance of type <see cref="TestService1"/>
        /// using the base interface type <see cref="ITestService"/> which is valid for all test services. The service instance
        /// was registered using the base interface type <see cref="ITestService"/>.
        /// </summary>
        [Test]
        public void ServiceManager_GetService_BaseInterfaceType_RegisteredByBaseInterfaceType()
        {
            // Arrange
            var serviceInstance = new TestService1();
            serviceManager.TryRegisterService<ITestService>(serviceInstance);

            // Act
            var retrievedServices = serviceManager.GetServices<ITestService>();

            // Assert
            Assert.IsNotNull(retrievedServices);
            Assert.AreEqual(1, retrievedServices.Count, $"Number of services did not match the expected [1]");
            Assert.IsTrue(retrievedServices[0] is ITestService);
            Assert.IsTrue(retrievedServices[0] == serviceInstance);
        }

        /// <summary>
        /// This test will test whether we can retrieve a service instance of type <see cref="TestService1"/>
        /// using the base interface type <see cref="ITestService"/> which is valid for all test services. The service instance
        /// was registered using the base interface type <see cref="ITestService"/>.
        /// With multiple services registered
        /// </summary>
        [Test]
        public void ServiceManager_GetService_BaseInterfaceType_RegisteredByBaseInterfaceType_WithPopulation()
        {
            // Arrange
            var serviceInstance = new TestService1();
            serviceManager.TryRegisterService<ITestService>(serviceInstance);
            var serviceInstance2 = new TestService2();
            serviceManager.TryRegisterService<ITestService2>(serviceInstance2);

            // Act
            var retrievedServices = serviceManager.GetServices<ITestService>();

            // Assert
            Assert.IsNotNull(retrievedServices, "No services returned from registry");
            Assert.AreEqual(2, retrievedServices.Count, $"Number of services did not match the expected [2]");
            Assert.IsTrue(retrievedServices[0] is ITestService, $"Returned service is not of type {typeof(ITestService)}");
            Assert.IsTrue(retrievedServices[0] == serviceInstance, "Instance of service does not match the created service");
        }

        /// <summary>
        /// This test will test whether we can retrieve the service instances <see cref="TestService1"/>
        /// and <see cref="TestService2"/> using their respective dedicated interface types <see cref="ITestService1"/>
        /// and <see cref="ITestService2"/>. Additionally the test ensures both service instances can be retrieved
        /// using the base interface type <see cref="ITestService"/>.
        /// </summary>
        [Test]
        public void ServiceManager_GetServices_BaseInterfaceType()
        {
            // Arrange
            var serviceInstance1 = new TestService1();
            serviceManager.TryRegisterService<ITestService1>(serviceInstance1);
            var serviceInstance2 = new TestService2();
            serviceManager.TryRegisterService<ITestService2>(serviceInstance2);

            // Act
            var retrievedServices = serviceManager.GetServices<ITestService>();
            var retrievedServices1 = serviceManager.GetServices<ITestService1>();
            var retrievedServices2 = serviceManager.GetServices<ITestService2>();

            // Assert
            Assert.AreEqual(2, retrievedServices.Count, $"Number of services did not match the expected [2]");
            Assert.IsTrue(retrievedServices.Contains(serviceInstance1));
            Assert.IsTrue(retrievedServices.Contains(serviceInstance2));
            Assert.IsNotNull(retrievedServices1);
            Assert.AreEqual(1, retrievedServices1.Count, $"Number of services did not match the expected [1]");
            Assert.IsTrue(retrievedServices1[0] is ITestService1);
            Assert.IsTrue(retrievedServices1[0] == serviceInstance1);
            Assert.IsNotNull(retrievedServices2);
            Assert.AreEqual(1, retrievedServices2.Count, $"Number of services did not match the expected [1]");
            Assert.IsTrue(retrievedServices2[0] is ITestService2);
            Assert.IsTrue(retrievedServices2[0] == serviceInstance2);
        }

        [TearDown]
        public void TearDownServiceManager_GetService_Tests()
        {
            serviceManager.Dispose();
            serviceManager = null;
        }
    }
}