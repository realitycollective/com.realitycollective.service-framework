// Copyright (c) Reality Collective. All rights reserved.

using NUnit.Framework;
using RealityCollective.ServiceFramework.Services;
using RealityCollective.ServiceFramework.Tests.Interfaces;
using RealityCollective.ServiceFramework.Tests.Services;
using RealityCollective.ServiceFramework.Tests.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace RealityCollective.ServiceFramework.Tests
{
    /// <summary>
    /// Tests for cached service retrieval and object pooling optimizations.
    /// Validates Fix #10: Cache invalidation on TryUnregisterService
    /// Validates Fix #11: Object pooling for GetServices list allocation reduction
    /// </summary>
    internal class ServiceManagerCachingAndPoolingTests : MonoBehaviour
    {
        private ServiceManager testServiceManager;

        #region Cached Retrieval Tests (Fix #10)

        [Test]
        public void Test_Cache_01_GetServiceCached_ReturnsService()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Arrange
            var testService = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService);

            // Act
            var cachedService = testServiceManager.GetServiceCached<ITestService1>();

            // Assert
            Assert.IsNotNull(cachedService, "Cached service should be returned");
            Assert.AreSame(testService, cachedService, "Should return the registered service instance");
        }

        [Test]
        public void Test_Cache_02_GetServiceCached_MultipleCallsReturnSameInstance()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Arrange
            var testService = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService);

            // Act - Multiple cached calls
            var cached1 = testServiceManager.GetServiceCached<ITestService1>();
            var cached2 = testServiceManager.GetServiceCached<ITestService1>();
            var cached3 = testServiceManager.GetServiceCached<ITestService1>();

            // Assert
            Assert.AreSame(testService, cached1);
            Assert.AreSame(cached1, cached2, "Multiple cached calls should return same instance");
            Assert.AreSame(cached2, cached3, "Multiple cached calls should return same instance");
        }

        [Test]
        public void Test_Cache_03_TryGetServiceCached_WhenServiceExists()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Arrange
            var testService = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService);

            // Act
            var found = testServiceManager.TryGetServiceCached<ITestService1>(out var cachedService);

            // Assert
            Assert.IsTrue(found, "Should find the service");
            Assert.IsNotNull(cachedService, "Cached service should be returned");
            Assert.AreSame(testService, cachedService);
        }

        [Test]
        public void Test_Cache_04_TryGetServiceCached_WhenServiceNotExists()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Act
            var found = testServiceManager.TryGetServiceCached<ITestService1>(out var cachedService);

            // Assert
            Assert.IsFalse(found, "Should not find non-existent service");
            Assert.IsNull(cachedService, "Service should be null");
        }

        [Test]
        public void Test_Cache_05_CacheInvalidatedOnUnregister()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Arrange - Register and cache service
            var testService1 = new TestService1("Service 1");
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            
            var cachedBefore = testServiceManager.GetServiceCached<ITestService1>();
            Assert.AreSame(testService1, cachedBefore, "Initial cache should return first service");

            // Act - Unregister (should invalidate cache - Fix #10)
            testServiceManager.TryUnregisterService<ITestService1>(testService1);

            // Register new service
            var testService2 = new TestService1("Service 2");
            testServiceManager.TryRegisterService<ITestService1>(testService2);

            // Get cached again
            var cachedAfter = testServiceManager.GetServiceCached<ITestService1>();

            // Assert - Should get new service (Fix #10 validation)
            Assert.IsNotNull(cachedAfter, "Should find new service");
            Assert.AreSame(testService2, cachedAfter, "Cache should return new service after invalidation");
            Assert.AreNotSame(cachedBefore, cachedAfter, "Should not return old cached instance");
        }

        [Test]
        public void Test_Cache_06_TryGetServiceCached_CacheInvalidatedOnUnregister()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Arrange
            var testService1 = new TestService1("Service 1");
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            
            testServiceManager.TryGetServiceCached<ITestService1>(out var cachedBefore);
            Assert.AreSame(testService1, cachedBefore);

            // Act - Unregister and register new service
            testServiceManager.TryUnregisterService<ITestService1>(testService1);
            var testService2 = new TestService1("Service 2");
            testServiceManager.TryRegisterService<ITestService1>(testService2);

            var found = testServiceManager.TryGetServiceCached<ITestService1>(out var cachedAfter);

            // Assert
            Assert.IsTrue(found, "Should find new service");
            Assert.AreSame(testService2, cachedAfter, "Should return new service");
            Assert.AreNotSame(cachedBefore, cachedAfter, "Cache should be invalidated");
        }

        [Test]
        public void Test_Cache_07_CachePerformance()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Arrange
            var testService = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService);

            // Warm up cache
            testServiceManager.GetServiceCached<ITestService1>();

            // Act - Measure cached retrieval performance
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < 10000; i++)
            {
                testServiceManager.GetServiceCached<ITestService1>();
            }
            stopwatch.Stop();

            // Assert - Cached retrieval should be very fast
            Assert.Less(stopwatch.ElapsedMilliseconds, 100, 
                "10,000 cached retrievals should complete in under 100ms (typically <10ms)");
        }

        #endregion

        #region Object Pooling Tests (Fix #11)

        [Test]
        public void Test_Pool_01_GetServices_ReturnsAllServices()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Arrange - Register services of different derived types
            // GetServices<ITestService> retrieves all services that implement ITestService
            var testService1 = new TestService1("Service 1");
            var testService2 = new TestService2("Service 2");
            
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestService2>(testService2);

            // Act - Get all services via base interface
            var services = testServiceManager.GetServices<ITestService>();

            // Assert
            Assert.IsNotNull(services, "Services list should not be null");
            Assert.AreEqual(2, services.Count, "Should return both ITestService implementations");
            Assert.IsTrue(services.Contains(testService1));
            Assert.IsTrue(services.Contains(testService2));
        }

        [Test]
        public void Test_Pool_02_GetServices_ReturnsReadOnlyList()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Arrange
            var testService = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService);

            // Act
            var services = testServiceManager.GetServices<ITestService1>();

            // Assert
            Assert.IsInstanceOf<IReadOnlyList<ITestService1>>(services, 
                "Should return IReadOnlyList to prevent external modification");
        }

        [Test]
        public void Test_Pool_03_GetServices_EmptyWhenNoServices()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Act - Get services when none registered
            var services = testServiceManager.GetServices<ITestService1>();

            // Assert
            Assert.IsNotNull(services, "Should return empty list, not null");
            Assert.AreEqual(0, services.Count, "Should return empty list");
        }

        [Test]
        public void Test_Pool_04_GetServices_MultipleCallsPerformance()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Arrange - Register two services (one of each type)
            var testService1 = new TestService1("Service 1");
            var testService2 = new TestService2("Service 2");
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestService2>(testService2);

            // Act - Multiple calls to GetServices (tests pooling behavior - Fix #11)
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                var services = testServiceManager.GetServices<ITestService>();
                Assert.AreEqual(2, services.Count);
            }
            stopwatch.Stop();

            // Assert - Pooling should make this fast (Fix #11 validation)
            Assert.Less(stopwatch.ElapsedMilliseconds, 500, 
                "1,000 GetServices calls with pooling should complete in under 500ms");
        }

        [Test]
        public void Test_Pool_05_TryGetServices_WhenServicesExist()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Arrange - Register different service types
            var testService1 = new TestService1("Service 1");
            var testService2 = new TestService2("Service 2");
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestService2>(testService2);

            // Act - Get all ITestService implementations
            List<ITestService> services = null;
            var found = testServiceManager.TryGetServices(typeof(ITestService), null, ref services);

            // Assert
            Assert.IsTrue(found, "Should find services");
            Assert.IsNotNull(services);
            Assert.AreEqual(2, services.Count);
            Assert.IsTrue(services.Contains(testService1));
            Assert.IsTrue(services.Contains(testService2));
        }

        [Test]
        public void Test_Pool_06_TryGetServices_WhenNoServicesExist()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Act
            List<ITestService1> services = null;
            var found = testServiceManager.TryGetServices(typeof(ITestService1), null, ref services);

            // Assert
            Assert.IsFalse(found, "Should not find services when none registered");
            Assert.IsNull(services, "Services should be null when not found");
        }

        [Test]
        public void Test_Pool_07_GetServices_DifferentServiceTypes()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Arrange - Register one of each service type
            var testService1 = new TestService1("Service 1");
            var testService2 = new TestService2("Service 2");

            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestService2>(testService2);

            // Act - Get specific types and base type
            var services1 = testServiceManager.GetServices<ITestService1>();
            var services2 = testServiceManager.GetServices<ITestService2>();
            var allServices = testServiceManager.GetServices<ITestService>();

            // Assert
            Assert.AreEqual(1, services1.Count, "Should have 1 ITestService1 instance");
            Assert.AreEqual(1, services2.Count, "Should have 1 ITestService2 instance");
            Assert.AreEqual(2, allServices.Count, "Should have 2 ITestService implementations");
            Assert.IsTrue(services1.Contains(testService1));
            Assert.IsTrue(services2.Contains(testService2));
            Assert.IsTrue(allServices.Contains(testService1));
            Assert.IsTrue(allServices.Contains(testService2));
        }

        [Test]
        public void Test_Pool_08_GetServices_AfterUnregister()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Arrange - Register two different service types
            var testService1 = new TestService1("Service 1");
            var testService2 = new TestService2("Service 2");

            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestService2>(testService2);

            // Verify initial state
            var initialServices = testServiceManager.GetServices<ITestService>();
            Assert.AreEqual(2, initialServices.Count);

            // Act - Unregister one service
            testServiceManager.TryUnregisterService<ITestService2>(testService2);
            var servicesAfterUnregister = testServiceManager.GetServices<ITestService>();

            // Assert
            Assert.AreEqual(1, servicesAfterUnregister.Count, "Should have 1 service after unregister");
            Assert.IsTrue(servicesAfterUnregister.Contains(testService1));
            Assert.IsFalse(servicesAfterUnregister.Contains(testService2), "Unregistered service should not be in list");
        }

        #endregion
    }
}
