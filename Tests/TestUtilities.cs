// Copyright (c) Reality Collective. All rights reserved.

using System;
using System.Linq;
using NUnit.Framework;
using System.Threading.Tasks;
using RealityToolkit.ServiceFramework.Definitions;
using RealityToolkit.ServiceFramework.Extensions;
using RealityToolkit.ServiceFramework.Services;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RealityToolkit.ServiceFramework.Tests.Utilities
{
    public static class TestUtilities
    {
       public static void InitializeServiceManager(ref ServiceManager serviceManager)
        {
            if (serviceManager == null)
            {
                serviceManager = new ServiceManager();
                serviceManager.Initialize();
            }
            serviceManager.ConfirmInitialized();
        }

        public static void CleanupScene()
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        }

        public static void InitializeServiceManagerScene(ServiceManager serviceManager = null, bool useDefaultProfile = false)
        {
            // Setup
            CleanupScene();
            InitializeServiceManager(ref serviceManager);
            Assert.AreEqual(0, serviceManager.ActiveServices.Count);

            // Tests
            Assert.IsTrue(serviceManager.IsInitialized);
            Assert.IsNotNull(serviceManager);
            Assert.IsFalse(serviceManager.HasActiveProfile);

            ServiceManagerRootProfile configuration;

            if (useDefaultProfile)
            {
                configuration = GetDefaultServiceManagerProfile<ServiceManagerRootProfile>();
            }
            else
            {
                configuration = ScriptableObject.CreateInstance<ServiceManagerRootProfile>();
            }

            Assert.IsTrue(configuration != null, "Failed to find the Service Manager Profile");
            serviceManager.ResetProfile(configuration);
            Assert.IsTrue(serviceManager.ActiveProfile != null);
            Assert.IsTrue(serviceManager.IsInitialized);
        }
        
        public static T RunAsyncMethodSync<T>(Func<Task<T>> asyncFunc) {
            return Task.Run(async () => await asyncFunc()).GetAwaiter().GetResult();
        }
        public static void RunAsyncMethodSync(Func<Task> asyncFunc) {
            Task.Run(async () => await asyncFunc()).GetAwaiter().GetResult();
        }

        private static T GetDefaultServiceManagerProfile<T>() where T : BaseProfile
        {
            return ScriptableObjectExtensions.GetAllInstances<T>().FirstOrDefault(profile => profile.name.Equals(typeof(T).Name));
        }  
    }
}