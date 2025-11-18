// Copyright (c) Reality Collective. All rights reserved.

using NUnit.Framework;
using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Definitions.Platforms;
using RealityCollective.ServiceFramework.Services;
using RealityCollective.ServiceFramework.Tests.Interfaces;
using RealityCollective.ServiceFramework.Tests.Services;
using RealityCollective.ServiceFramework.Tests.Utilities;
using UnityEngine;

namespace RealityCollective.ServiceFramework.Tests
{
    /// <summary>
    /// Tests for service lifecycle management including Initialize, Start, Update, Destroy, and Dispose.
    /// Validates proper lifecycle sequencing and priority-based ordering.
    /// </summary>
    internal class ServiceManagerLifecycleTests : MonoBehaviour
    {
        private ServiceManager testServiceManager;

        #region Initialize Tests

        [Test]
        public void Test_Lifecycle_01_ServiceInitializedOnRegistration()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Act - Register service
            var testService = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService);

            // Assert
            Assert.IsTrue(testService.IsInitialized, "Service should be initialized after registration");
        }

        [Test]
        public void Test_Lifecycle_02_MultipleServicesInitialized()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Act - Register multiple services
            var testService1 = new TestService1();
            var testService2 = new TestService2();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestService2>(testService2);

            // Assert
            Assert.IsTrue(testService1.IsInitialized, "Service 1 should be initialized");
            Assert.IsTrue(testService2.IsInitialized, "Service 2 should be initialized");
        }

        [Test]
        public void Test_Lifecycle_03_InitializeCalledBeforeStart()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Act - Register service
            var testService = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService);

            // Assert - After registration, should be initialized but not started
            Assert.IsTrue(testService.IsInitialized, "Should be initialized immediately");
            Assert.IsFalse(testService.IsStarted, "Should not be started yet (Start called on first frame)");
        }

        #endregion

        #region Start Tests

        [Test]
        public void Test_Lifecycle_04_ServiceStartedAfterInitialize()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Arrange - Register service
            var testService = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService);

            Assert.IsTrue(testService.IsInitialized, "Should be initialized");
            Assert.IsFalse(testService.IsStarted, "Should not be started yet");

            // Act - Manually call Start (simulating first frame)
            testService.Start();

            // Assert
            Assert.IsTrue(testService.IsStarted, "Should be started after Start() call");
        }

        [Test]
        public void Test_Lifecycle_05_MultipleServicesStarted()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Arrange
            var testService1 = new TestService1();
            var testService2 = new TestService2();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestService2>(testService2);

            // Act
            testService1.Start();
            testService2.Start();

            // Assert
            Assert.IsTrue(testService1.IsStarted, "Service 1 should be started");
            Assert.IsTrue(testService2.IsStarted, "Service 2 should be started");
        }

        #endregion

        #region Update Tests

        [Test]
        public void Test_Lifecycle_06_UpdateIncreasesCount()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Arrange
            var testService = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService);

            var initialUpdateCount = testService.UpdateCount;

            // Act - Call Update multiple times
            testService.Update();
            testService.Update();
            testService.Update();

            // Assert
            Assert.AreEqual(initialUpdateCount + 3, testService.UpdateCount, "Update count should increase by 3");
        }

        [Test]
        public void Test_Lifecycle_07_LateUpdateIncreasesCount()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Arrange
            var testService = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService);

            var initialLateUpdateCount = testService.LateUpdateCount;

            // Act - Call LateUpdate multiple times
            testService.LateUpdate();
            testService.LateUpdate();

            // Assert
            Assert.AreEqual(initialLateUpdateCount + 2, testService.LateUpdateCount, "LateUpdate count should increase by 2");
        }

        [Test]
        public void Test_Lifecycle_08_FixedUpdateIncreasesCount()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Arrange
            var testService = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService);

            var initialFixedUpdateCount = testService.FixedUpdateCount;

            // Act - Call FixedUpdate multiple times
            testService.FixedUpdate();
            testService.FixedUpdate();
            testService.FixedUpdate();
            testService.FixedUpdate();

            // Assert
            Assert.AreEqual(initialFixedUpdateCount + 4, testService.FixedUpdateCount, "FixedUpdate count should increase by 4");
        }

        #endregion

        #region Destroy Tests

        [Test]
        public void Test_Lifecycle_09_DestroyMarksServiceDestroyed()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Arrange
            var testService = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService);

            Assert.IsFalse(testService.IsDestroyed, "Service should not be destroyed initially");

            // Act
            testService.Destroy();

            // Assert
            Assert.IsTrue(testService.IsDestroyed, "Service should be marked as destroyed");
        }

        [Test]
        public void Test_Lifecycle_10_UnregisterCallsDestroy()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Arrange
            var testService = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService);

            // Act - Unregister service (should call Destroy internally)
            testServiceManager.TryUnregisterService<ITestService1>(testService);

            // Assert
            Assert.IsTrue(testService.IsDestroyed, "Service should be destroyed after unregistration");
        }

        [Test]
        public void Test_Lifecycle_11_DisposeMarksServiceDisposed()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Arrange
            var testService = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService);

            // Act - Call Destroy (which sets IsDisposed via computed property)
            testService.Destroy();

            // Assert
            Assert.IsTrue(testService.IsDisposed, "Service should be marked as disposed after destroy");
        }

        #endregion

        #region Priority Ordering Tests

        [Test]
        public void Test_Lifecycle_12_ServicePriorityOrdering()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Arrange - Register services with different priorities
            ITestService1 highPriorityService;
            ITestService2 lowPriorityService;

            var config1 = new ServiceConfiguration<ITestService1>(typeof(TestService1), "High Priority", 1, AllPlatforms.Platforms, null);
            var config2 = new ServiceConfiguration<ITestService2>(typeof(TestService2), "Low Priority", 100, AllPlatforms.Platforms, null);

            // Act
            testServiceManager.TryCreateAndRegisterService(config1, out highPriorityService);
            testServiceManager.TryCreateAndRegisterService(config2, out lowPriorityService);

            // Assert
            Assert.IsNotNull(highPriorityService);
            Assert.IsNotNull(lowPriorityService);
            Assert.AreEqual(1, highPriorityService.Priority, "High priority service should have priority 1");
            Assert.AreEqual(100, lowPriorityService.Priority, "Low priority service should have priority 100");
            Assert.Less(highPriorityService.Priority, lowPriorityService.Priority, "Lower number = higher priority");
        }

        [Test]
        public void Test_Lifecycle_13_ServicesRetrievedByPriority()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Arrange - Register different service types with different priorities
            // Note: ServiceManager only allows ONE service per interface type
            var config1 = new ServiceConfiguration<ITestService1>(typeof(TestService1), "High Priority Service", 10, AllPlatforms.Platforms, null);
            var config2 = new ServiceConfiguration<ITestService2>(typeof(TestService2), "Low Priority Service", 50, AllPlatforms.Platforms, null);

            testServiceManager.TryCreateAndRegisterService(config1, out ITestService1 service1);
            testServiceManager.TryCreateAndRegisterService(config2, out ITestService2 service2);

            // Act - Retrieve services and verify priority values
            var retrievedService1 = testServiceManager.GetService<ITestService1>();
            var retrievedService2 = testServiceManager.GetService<ITestService2>();

            // Assert
            Assert.IsNotNull(retrievedService1, "Should retrieve ITestService1");
            Assert.IsNotNull(retrievedService2, "Should retrieve ITestService2");
            Assert.AreEqual(10, retrievedService1.Priority, "Service1 should have priority 10");
            Assert.AreEqual(50, retrievedService2.Priority, "Service2 should have priority 50");
            Assert.Less(retrievedService1.Priority, retrievedService2.Priority, "Lower priority number = higher priority (initialized first)");
        }

        #endregion

        #region Full Lifecycle Sequence Tests

        [Test]
        public void Test_Lifecycle_14_FullLifecycleSequence()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Arrange
            var testService = new TestService1();

            // Phase 1: Registration (triggers Initialize)
            testServiceManager.TryRegisterService<ITestService1>(testService);
            Assert.IsTrue(testService.IsInitialized, "Phase 1: Should be initialized");
            Assert.IsFalse(testService.IsStarted, "Phase 1: Should not be started");
            Assert.IsFalse(testService.IsDestroyed, "Phase 1: Should not be destroyed");

            // Phase 2: Start
            testService.Start();
            Assert.IsTrue(testService.IsStarted, "Phase 2: Should be started");
            Assert.IsFalse(testService.IsDestroyed, "Phase 2: Should not be destroyed");

            // Phase 3: Update cycles
            testService.Update();
            testService.LateUpdate();
            testService.FixedUpdate();
            Assert.Greater(testService.UpdateCount, 0, "Phase 3: Update should have been called");
            Assert.Greater(testService.LateUpdateCount, 0, "Phase 3: LateUpdate should have been called");
            Assert.Greater(testService.FixedUpdateCount, 0, "Phase 3: FixedUpdate should have been called");

            // Phase 4: Destruction
            testService.Destroy();
            Assert.IsTrue(testService.IsDestroyed, "Phase 4: Should be destroyed");
            Assert.IsTrue(testService.IsDisposed, "Phase 4: Should be disposed");
        }

        [Test]
        public void Test_Lifecycle_15_ResetProfileDestroysAllServices()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Arrange - Register multiple services
            var testService1 = new TestService1();
            var testService2 = new TestService2();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestService2>(testService2);

            Assert.IsFalse(testService1.IsDestroyed);
            Assert.IsFalse(testService2.IsDestroyed);

            // Act - Reset profile (should destroy all services)
            var emptyProfile = ScriptableObject.CreateInstance<ServiceProvidersProfile>();
            testServiceManager.ResetProfile(emptyProfile);

            // Assert
            Assert.IsTrue(testService1.IsDestroyed, "Service 1 should be destroyed after profile reset");
            Assert.IsTrue(testService2.IsDestroyed, "Service 2 should be destroyed after profile reset");
        }

        #endregion
    }
}
