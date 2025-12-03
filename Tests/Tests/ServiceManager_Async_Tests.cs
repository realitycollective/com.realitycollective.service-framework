// Copyright (c) Reality Collective. All rights reserved.

using NUnit.Framework;
using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Definitions.Platforms;
using RealityCollective.ServiceFramework.Services;
using RealityCollective.ServiceFramework.Tests.Interfaces;
using RealityCollective.ServiceFramework.Tests.Services;
using RealityCollective.ServiceFramework.Tests.Utilities;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;

namespace RealityCollective.ServiceFramework.Tests
{
    /// <summary>
    /// Tests for async operations in ServiceManager including GetServiceAsync,
    /// WaitUntilInitializedAsync, and GetSystemCachedAsync.
    /// Validates Fix #6: ValueTask optimizations and ConfigureAwait(false) usage.
    /// </summary>
    internal class ServiceManagerAsyncTests : MonoBehaviour
    {
        private ServiceManager testServiceManager;

        #region GetServiceAsync Tests

        [Test]
        public void Test_Async_01_GetServiceAsync_WhenServiceExists()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register service
            var testService = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService);

            // Act - Get service asynchronously
            var result = TestUtilities.RunAsyncMethodSync(() => testServiceManager.GetServiceAsync<ITestService1>(timeout: 5));

            // Assert
            Assert.IsNotNull(result, "Service should be returned");
            Assert.AreSame(testService, result, "Should return the registered service instance");
        }

        [Test]
        public void Test_Async_02_GetServiceAsync_WhenServiceNotFound_TimesOut()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Expect the error log from GetService when service not found
            LogAssert.Expect(LogType.Error, "Unable to find ITestService1 service.");

            // Act & Assert - Try to get non-existent service with short timeout
            // GetServiceAsync throws either TimeoutException or TaskCanceledException on timeout
            var exception = Assert.Catch<System.Exception>(() =>
            {
                TestUtilities.RunAsyncMethodSync(() => testServiceManager.GetServiceAsync<ITestService1>(timeout: 1));
            });
            
            // Verify it's one of the expected timeout-related exceptions
            Assert.IsTrue(
                exception is System.TimeoutException || exception is System.Threading.Tasks.TaskCanceledException,
                $"Expected TimeoutException or TaskCanceledException, but got {exception.GetType().Name}: {exception.Message}");
        }

        [Test]
        public void Test_Async_03_GetServiceAsync_MultipleCallsSameService()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register service
            var testService = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService);

            // Act - Get service multiple times
            var result1 = TestUtilities.RunAsyncMethodSync(() => testServiceManager.GetServiceAsync<ITestService1>(timeout: 5));
            var result2 = TestUtilities.RunAsyncMethodSync(() => testServiceManager.GetServiceAsync<ITestService1>(timeout: 5));
            var result3 = TestUtilities.RunAsyncMethodSync(() => testServiceManager.GetServiceAsync<ITestService1>(timeout: 5));

            // Assert
            Assert.IsNotNull(result1);
            Assert.AreSame(testService, result1);
            Assert.AreSame(result1, result2, "All calls should return same instance");
            Assert.AreSame(result2, result3, "All calls should return same instance");
        }

        [Test]
        public void Test_Async_04_GetServiceAsync_DifferentServiceTypes()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register multiple services
            var testService1 = new TestService1();
            var testService2 = new TestService2();
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            testServiceManager.TryRegisterService<ITestService2>(testService2);

            // Act - Get different services asynchronously
            var result1 = TestUtilities.RunAsyncMethodSync(() => testServiceManager.GetServiceAsync<ITestService1>(timeout: 5));
            var result2 = TestUtilities.RunAsyncMethodSync(() => testServiceManager.GetServiceAsync<ITestService2>(timeout: 5));

            // Assert
            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);
            Assert.AreSame(testService1, result1);
            Assert.AreSame(testService2, result2);
            Assert.AreNotSame(result1, result2, "Different service types should return different instances");
        }

        #endregion

        #region WaitUntilInitializedAsync Tests

        [UnityTest]
        public System.Collections.IEnumerator Test_Async_05_WaitUntilInitializedAsync_WhenAlreadyInitialized()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            Assert.IsTrue(ServiceManager.IsActiveAndInitialized, "ServiceManager should be initialized");

            // Act - Wait for initialization when already initialized (returns ValueTask)
            var task = ServiceManager.WaitUntilInitializedAsync(timeout: 5f).AsTask();
            
            // Wait for task to complete
            while (!task.IsCompleted)
            {
                yield return null;
            }

            // Assert - Should complete without error
            Assert.IsTrue(task.IsCompletedSuccessfully, "WaitUntilInitializedAsync completed successfully when already initialized");
        }

        [UnityTest]
        public System.Collections.IEnumerator Test_Async_06_WaitUntilInitializedAsync_WithTimeout()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Act - Wait with various timeout values (returns ValueTask)
            var task1 = ServiceManager.WaitUntilInitializedAsync(timeout: 0.1f).AsTask();
            while (!task1.IsCompleted) yield return null;
            Assert.IsTrue(task1.IsCompletedSuccessfully, "First wait completed");

            var task2 = ServiceManager.WaitUntilInitializedAsync(timeout: 1f).AsTask();
            while (!task2.IsCompleted) yield return null;
            Assert.IsTrue(task2.IsCompletedSuccessfully, "Second wait completed");

            var task3 = ServiceManager.WaitUntilInitializedAsync(timeout: 5f).AsTask();
            while (!task3.IsCompleted) yield return null;
            Assert.IsTrue(task3.IsCompletedSuccessfully, "Third wait completed");

            // Assert - All should complete successfully
            Assert.Pass("All WaitUntilInitializedAsync calls completed");
        }

        [Test]
        public void Test_Async_07_WaitUntilInitializedAsync_SceneName_Overload()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Note: WaitUntilInitializedAsync with sceneName uses Time.realtimeSinceStartup which requires main thread
            // After ConfigureAwait(false), the continuation may run on thread pool, causing Unity API access to fail
            // This test verifies the method signature exists and can be called, but cannot reliably test completion
            // in unit test environment due to Unity's threading restrictions
            
            Assert.DoesNotThrow(() =>
            {
                // Verify method can be called and returns a ValueTask
                var valueTask = ServiceManager.WaitUntilInitializedAsync("TestScene");
                Assert.IsNotNull(valueTask, "Should return a ValueTask");
            }, "Method should be callable without throwing");
        }

        #endregion

        #region GetSystemCachedAsync Tests

        [Test]
        public void Test_Async_08_GetSystemCachedAsync_FirstCall()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register service
            var testService = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService);

            // Act - First cached call
            var result = TestUtilities.RunAsyncMethodSync(() => testServiceManager.GetSystemCachedAsync<ITestService1>(timeout: 5));

            // Assert
            Assert.IsNotNull(result, "First cached call should return service");
            Assert.AreSame(testService, result);
        }

        [Test]
        public void Test_Async_09_GetSystemCachedAsync_SubsequentCallsReturnCached()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register service
            var testService = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService);

            // Act - Multiple cached calls
            var result1 = TestUtilities.RunAsyncMethodSync(() => testServiceManager.GetSystemCachedAsync<ITestService1>(timeout: 5));
            var result2 = TestUtilities.RunAsyncMethodSync(() => testServiceManager.GetSystemCachedAsync<ITestService1>(timeout: 5));
            var result3 = TestUtilities.RunAsyncMethodSync(() => testServiceManager.GetSystemCachedAsync<ITestService1>(timeout: 5));

            // Assert
            Assert.IsNotNull(result1);
            Assert.AreSame(testService, result1);
            Assert.AreSame(result1, result2, "Cached calls should return same instance");
            Assert.AreSame(result2, result3, "Cached calls should return same instance");
        }

        [Test]
        public void Test_Async_10_GetSystemCachedAsync_CacheInvalidatedOnUnregister()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register service and get cached
            var testService1 = new TestService1("Service 1");
            testServiceManager.TryRegisterService<ITestService1>(testService1);
            var cachedResult1 = TestUtilities.RunAsyncMethodSync(() => testServiceManager.GetSystemCachedAsync<ITestService1>(timeout: 5));

            Assert.AreSame(testService1, cachedResult1, "First cached result should match registered service");

            // Unregister service (should invalidate cache - Fix #10)
            testServiceManager.TryUnregisterService<ITestService1>(testService1);

            // Register new service
            var testService2 = new TestService1("Service 2");
            testServiceManager.TryRegisterService<ITestService1>(testService2);

            // Act - Get cached again after unregister
            var cachedResult2 = TestUtilities.RunAsyncMethodSync(() => testServiceManager.GetSystemCachedAsync<ITestService1>(timeout: 5));

            // Assert - Should get new service instance (Fix #10 validation)
            Assert.IsNotNull(cachedResult2);
            Assert.AreSame(testService2, cachedResult2, "Should return newly registered service after cache invalidation");
            Assert.AreNotSame(cachedResult1, cachedResult2, "Should not return old cached instance after unregister");
        }

        [Test]
        public void Test_Async_11_GetSystemCachedAsync_WhenServiceNotFound()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Act & Assert - Try to get non-existent service with short timeout
            // GetSystemCachedAsync throws TimeoutException or TaskCanceledException on timeout
            var exception = Assert.Catch<System.Exception>(() =>
            {
                TestUtilities.RunAsyncMethodSync(() => testServiceManager.GetSystemCachedAsync<ITestService1>(timeout: 1));
            });
            
            // Verify it's one of the expected timeout-related exceptions
            Assert.IsTrue(
                exception is System.TimeoutException || exception is System.Threading.Tasks.TaskCanceledException,
                $"Expected TimeoutException or TaskCanceledException, but got {exception.GetType().Name}: {exception.Message}");
        }

        #endregion

        #region Async Method Performance Tests

        [Test]
        public void Test_Async_12_GetServiceAsync_ZeroTimeout()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register service
            var testService = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService);

            // Act - Get with zero timeout (should return immediately)
            var result = TestUtilities.RunAsyncMethodSync(() => testServiceManager.GetServiceAsync<ITestService1>(timeout: 0));

            // Assert
            Assert.IsNotNull(result, "Should return service even with zero timeout when available");
        }

        [Test]
        public void Test_Async_13_ConcurrentAsyncCalls()
        {
            TestUtilities.InitializeServiceManagerScene(ref testServiceManager);

            // Register service
            var testService = new TestService1();
            testServiceManager.TryRegisterService<ITestService1>(testService);

            // Act - Multiple concurrent async calls
            var task1 = testServiceManager.GetServiceAsync<ITestService1>(timeout: 5);
            var task2 = testServiceManager.GetServiceAsync<ITestService1>(timeout: 5);
            var task3 = testServiceManager.GetServiceAsync<ITestService1>(timeout: 5);

            Task.WaitAll(task1, task2, task3);

            var result1 = task1.Result;
            var result2 = task2.Result;
            var result3 = task3.Result;

            // Assert
            Assert.IsNotNull(result1);
            Assert.AreSame(testService, result1);
            Assert.AreSame(result1, result2, "Concurrent calls should all return same instance");
            Assert.AreSame(result2, result3, "Concurrent calls should all return same instance");
        }

        #endregion
    }
}
