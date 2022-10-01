// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using NUnit.Framework;
using RealityCollective.ServiceFramework.Services;
using RealityCollective.ServiceFramework.Tests.Interfaces;
using RealityCollective.ServiceFramework.Tests.Services;
using RealityCollective.ServiceFramework.Tests.Utilities;
using UnityEngine;

namespace RealityCollective.ServiceFramework.Tests
{
    /// <summary>
    /// This class contains tests for all <see cref="ServiceManager"/> APIs
    /// used to retrieve a registered service instance.
    /// </summary>
    internal class ServiceManager_GetService_Tests
    {
        private ServiceManager serviceManager;

        /// <summary>
        /// This test will test whether we can retrieve a service instance of type <see cref="TestService1"/>
        /// using it's dedicated interface type <see cref="ITestService1"/> after registering it using said interface type,
        /// when the service is the only registered service.
        /// </summary>
        [Test]
        public void ServiceManager_GetService_TopInterfaceType_SingleService()
        {
            TestUtilities.InitializeServiceManagerScene(ref serviceManager);

            // Arrange
            var serviceInstance = new TestService1();
            serviceManager.TryRegisterService<ITestService1>(serviceInstance);

            // Act
            var retrievedServices = serviceManager.GetServices<ITestService1>();
            var retrievedService = serviceManager.GetService<ITestService1>();

            // Assert
            Assert.IsNotNull(retrievedServices, $"Expected return value from {nameof(serviceManager.GetServices)} to not be null.");
            Assert.IsNotNull(retrievedService, $"Expected return value from {nameof(serviceManager.GetService)} to not be null.");
            Assert.AreEqual(1, retrievedServices.Count, $"Expected {1} service to be returned, but got {retrievedServices.Count} instead.");
            Assert.IsTrue(retrievedServices[0] is ITestService1, $"Returned service type does not match expected type {nameof(ITestService1)}.");
            Assert.IsTrue(retrievedServices[0] == serviceInstance, $"Returned service is not the expected instance.");
            Assert.IsTrue(retrievedService == serviceInstance, $"Returned service is not the expected instance.");
        }

        /// <summary>
        /// This test will test whether we can retrieve a service instance of type <see cref="TestService1"/>
        /// using it's dedicated interface type <see cref="ITestService1"/> after registering it using said interface type,
        /// when there is other services registered.
        /// </summary>
        [Test]
        public void ServiceManager_GetService_TopInterfaceType_MultiService()
        {
            TestUtilities.InitializeServiceManagerScene(ref serviceManager);

            // Arrange
            var serviceInstance = new TestService1();
            serviceManager.TryRegisterService<ITestService1>(serviceInstance);
            serviceManager.TryRegisterService<ITestService2>(new TestService2());

            // Act
            var retrievedServices = serviceManager.GetServices<ITestService1>();
            var retrievedService = serviceManager.GetService<ITestService1>();

            // Assert
            Assert.IsNotNull(retrievedServices, $"Expected return value from {nameof(serviceManager.GetServices)} to not be null.");
            Assert.IsNotNull(retrievedService, $"Expected return value from {nameof(serviceManager.GetService)} to not be null.");
            Assert.AreEqual(1, retrievedServices.Count, $"Expected {1} service to be returned, but got {retrievedServices.Count} instead.");
            Assert.IsTrue(retrievedServices[0] is ITestService1, $"Returned service type does not match expected type {nameof(ITestService1)}.");
            Assert.IsTrue(retrievedServices[0] == serviceInstance, $"Returned service is not the expected instance.");
            Assert.IsTrue(retrievedService == serviceInstance, $"Returned service is not the expected instance.");
        }

        /// <summary>
        /// This test will test whether we can retrieve a service instance of type <see cref="TestService1"/>
        /// using the base interface type <see cref="ITestService"/> which is valid for all test services. The service instance
        /// was registered using it's dedicated interface type <see cref="ITestService1"/> and it is the only registered service.
        /// </summary>
        [Test]
        public void ServiceManager_GetService_BaseInterfaceType_SingleService()
        {
            TestUtilities.InitializeServiceManagerScene(ref serviceManager);
 
            // Arrange
            var serviceInstance = new TestService1();
            serviceManager.TryRegisterService<ITestService1>(serviceInstance);

            // Act
            var retrievedServices = serviceManager.GetServices<ITestService>();
            var retrievedService = serviceManager.GetService<ITestService>(false);

            // Assert
            Assert.IsNotNull(retrievedServices, $"Expected return value from {nameof(serviceManager.GetServices)} to not be null.");
            Assert.IsNull(retrievedService, $"Expected return value from {nameof(serviceManager.GetService)} to be null.");
            Assert.AreEqual(1, retrievedServices.Count, $"Expected {1} service to be returned, but got {retrievedServices.Count} instead.");
            Assert.IsTrue(retrievedServices[0] is ITestService, $"Returned service type does not match expected type {nameof(ITestService)}.");
            Assert.IsTrue(retrievedServices[0] == serviceInstance, $"Returned service is not the expected instance.");
        }

        /// <summary>
        /// This test will test whether we can retrieve a service instance of type <see cref="TestService1"/>
        /// using the base interface type <see cref="ITestService"/> which is valid for all test services. The service instance
        /// was registered using it's dedicated interface type <see cref="ITestService1"/> and there is other services registered.
        /// </summary>
        [Test]
        public void ServiceManager_GetService_BaseInterfaceType_MultiService()
        {
            TestUtilities.InitializeServiceManagerScene(ref serviceManager);

            // Arrange
            var serviceInstance = new TestService1();
            serviceManager.TryRegisterService<ITestService1>(serviceInstance);
            serviceManager.TryRegisterService<ITestService2>(new TestService2());

            // Act
            var retrievedServices = serviceManager.GetServices<ITestService>();
            var retrievedService = serviceManager.GetService<ITestService>(false);

            // Assert
            Assert.IsNotNull(retrievedServices, $"Expected return value from {nameof(serviceManager.GetServices)} to not be null.");
            Assert.IsNull(retrievedService, $"Expected return value from {nameof(serviceManager.GetService)} to be null.");
            Assert.AreEqual(2, retrievedServices.Count, $"Expected {2} service to be returned, but got {retrievedServices.Count} instead.");
            Assert.IsTrue(retrievedServices[0] is ITestService, $"Returned service type does not match expected type {nameof(ITestService)}.");
            Assert.IsTrue(retrievedServices[0] == serviceInstance, $"Returned service is not the expected instance.");
        }

        /// <summary>
        /// This test will test whether we can retrieve a service instance of type <see cref="TestService1"/>
        /// using it's dedicated interface type <see cref="ITestService1"/> after registering it using the base interface type <see cref="ITestService"/>.
        /// The service is the only registered service in this case.
        /// </summary>
        [Test]
        public void ServiceManager_GetService_TopInterfaceType_RegisteredByBaseInterfaceType_SingleService()
        {
            TestUtilities.InitializeServiceManagerScene(ref serviceManager);

            // Arrange
            var serviceInstance = new TestService1();
            serviceManager.TryRegisterService<ITestService>(serviceInstance);

            // Act
            var retrievedServices = serviceManager.GetServices<ITestService1>();
            var retrievedService = serviceManager.GetService<ITestService1>(false);

            // Assert
            Assert.IsNotNull(retrievedServices, $"Expected return value from {nameof(serviceManager.GetServices)} to not be null.");
            Assert.IsNotNull(retrievedService, $"Expected return value from {nameof(serviceManager.GetService)} to not be null.");
            Assert.AreEqual(1, retrievedServices.Count, $"Expected {1} service to be returned, but got {retrievedServices.Count} instead.");
            Assert.IsTrue(retrievedServices[0] is ITestService1, $"Returned service type does not match expected type {nameof(ITestService1)}.");
            Assert.IsTrue(retrievedServices[0] == serviceInstance, $"Returned service is not the expected instance.");
        }

        /// <summary>
        /// This test will test whether we can retrieve a service instance of type <see cref="TestService1"/>
        /// using it's dedicated interface type <see cref="ITestService1"/> after registering it using the base interface type <see cref="ITestService"/>.
        /// There is other registered services in this case.
        /// </summary>
        [Test]
        public void ServiceManager_GetService_TopInterfaceType_RegisteredByBaseInterfaceType_MultiService()
        {
            TestUtilities.InitializeServiceManagerScene(ref serviceManager);

            // Arrange
            var serviceInstance = new TestService1();
            serviceManager.TryRegisterService<ITestService>(serviceInstance);
            serviceManager.TryRegisterService<ITestService>(new TestService2());

            // Act
            var retrievedServices = serviceManager.GetServices<ITestService1>();
            var retrievedService = serviceManager.GetService<ITestService1>(false);

            // Assert
            Assert.IsNotNull(retrievedServices, $"Expected return value from {nameof(serviceManager.GetServices)} to not be null.");
            Assert.IsNotNull(retrievedService, $"Expected return value from {nameof(serviceManager.GetService)} to not be null.");
            Assert.AreEqual(1, retrievedServices.Count, $"Expected {1} service to be returned, but got {retrievedServices.Count} instead.");
            Assert.IsTrue(retrievedServices[0] is ITestService1, $"Returned service type does not match expected type {nameof(ITestService1)}.");
            Assert.IsTrue(retrievedServices[0] == serviceInstance, $"Returned service is not the expected instance.");
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
            TestUtilities.InitializeServiceManagerScene(ref serviceManager);
 
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
            Assert.AreEqual(2, retrievedServices.Count, $"Expected {2} service to be returned, but got {retrievedServices.Count} instead.");
            Assert.IsNotNull(retrievedServices1, $"Expected return value from {nameof(serviceManager.GetServices)} for {nameof(ITestService1)} to not be null.");
            Assert.AreEqual(1, retrievedServices1.Count, $"Expected {1} service to be returned, but got {retrievedServices.Count} instead.");
            Assert.IsTrue(retrievedServices1[0] is ITestService1, $"Returned service type does not match expected type {nameof(ITestService1)}.");
            Assert.IsTrue(retrievedServices1[0] == serviceInstance1, $"Returned service is not the expected instance.");
            Assert.IsNotNull(retrievedServices2, $"Expected return value from {nameof(serviceManager.GetServices)} for {nameof(ITestService2)} to not be null.");
            Assert.AreEqual(1, retrievedServices2.Count, $"Expected {1} service to be returned, but got {retrievedServices.Count} instead.");
            Assert.IsTrue(retrievedServices2[0] is ITestService2, $"Returned service type does not match expected type {nameof(ITestService2)}.");
            Assert.IsTrue(retrievedServices2[0] == serviceInstance2, $"Returned service is not the expected instance.");
        }
    }
}