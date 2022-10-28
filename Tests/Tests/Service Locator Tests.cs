// Copyright (c) Reality Collective. All rights reserved.

using NUnit.Framework;
using RealityCollective.ServiceFramework.Services;
using RealityCollective.ServiceFramework.Tests.Utilities;
using System.Text.RegularExpressions;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

namespace RealityCollective.ServiceFramework.Tests.A_ServiceManager
{
    internal class ServiceLocatorTests
    {
        private ServiceManager testServiceManager;

        #region 01 Service Locater

        [Test]
        public void Test_01_01_InitializeServiceManager()
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            testServiceManager = new ServiceManager();
            testServiceManager.Initialize();
            //LogAssert.Expect(LogType.Warning, $"There are multiple instances of the ServiceManager in this project, is this expected?");

            var confirm = testServiceManager.ConfirmInitialized();

            var managerGameObject = GameObject.Find(nameof(ServiceManager));
            ServiceManagerInstance instance = managerGameObject.GetComponent<ServiceManagerInstance>();
            Assert.IsNotNull(managerGameObject, "No manager found in the scene");

            //This is supposed to fail but is not :S
            Assert.AreEqual(instance.Manager.ServiceManagerInstanceGuid, testServiceManager.ServiceManagerInstanceGuid, "Service Manager not found");
        }

        [Test]
        public void Test_01_02_InitializeServiceManagerInstance()
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var serviceManagerGameObject = new GameObject("ServiceManager");
            testServiceManager = new ServiceManager(serviceManagerGameObject);

            Assert.IsNotNull(testServiceManager, "Service Manager not created");
            Assert.IsTrue(testServiceManager.IsInitialized, "Manager not Initialised");
        }

        [Test]
        public void Test_01_03_InitializeServiceManagerGameObject()
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var serviceManagerGameObject = new GameObject(nameof(ServiceManager));
            var serviceManagerInstance = serviceManagerGameObject.AddComponent<ServiceManagerInstance>();
            serviceManagerInstance.InitialiseServiceManager();

            var serviceManagerGO = GameObject.Find(nameof(ServiceManager));
            ServiceManagerInstance instance = serviceManagerGO.GetComponent<ServiceManagerInstance>();
            Assert.IsNotNull(serviceManagerGO, "Unable to find ServiceManager GO");
            Assert.IsNotNull(instance, "Service Manager class not found on ServiceManager GO");
            Assert.IsNotNull(instance.Manager, "Service Manager not instantiated on ServiceManagerInstance class");
            Assert.IsTrue(instance.Manager.IsInitialized, "Manager not Initialised");
        }

        [Test]
        public void Test_01_04_TestNoProfileFound()
        {
            testServiceManager = null;

            // Setup
            TestUtilities.CleanupScene();
            TestUtilities.InitializeServiceManager(ref testServiceManager);
            testServiceManager.ConfirmInitialized();
            Assert.IsNotNull(testServiceManager, "Service Manager instance not found");
            Assert.IsTrue(testServiceManager.IsInitialized, "Service Manager was not initialized");

            testServiceManager.ActiveProfile = null;

            // Tests
            LogAssert.Expect(LogType.Error, new Regex("No ServiceProvidersProfile found, cannot initialize the ServiceManager"));
            Assert.AreEqual(0, testServiceManager.ActiveServices.Count, "Service Manager services were found where none should exist");
            Assert.IsFalse(testServiceManager.HasActiveProfile, "Profile found for the Service Manager where none should exist");
            Assert.IsNull(testServiceManager.ActiveProfile, "Profile found for the Service Manager where none should exist for instance");
        }

        [Test]
        public void Test_01_05_CreateServiceManager()
        {
            testServiceManager = null;

            TestUtilities.InitializeServiceManagerScene(ref testServiceManager, false);

            // Tests
            Assert.AreEqual(0, testServiceManager.ActiveServices.Count, "More or less services found than was expected");
        }

        #endregion 01 Service Locater
    }
}