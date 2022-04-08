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
       public static void InitializeServiceManager()
        {
            ServiceManager.ConfirmInitialized();
        }

        public static void CleanupScene()
        {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        }

        public static void InitializeServiceManagerScene(bool useDefaultProfile)
        {
            // Setup
            CleanupScene();
            Assert.IsTrue(!ServiceManager.IsInitialized);
            Assert.AreEqual(0, ServiceManager.ActiveServices.Count);
            InitializeServiceManager();

            // Tests
            Assert.IsTrue(ServiceManager.IsInitialized);
            Assert.IsNotNull(ServiceManager.Instance);
            Assert.IsFalse(ServiceManager.HasActiveProfile);

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
            ServiceManager.Instance.ResetProfile(configuration);
            Assert.IsTrue(ServiceManager.Instance.ActiveProfile != null);
            Assert.IsTrue(ServiceManager.IsInitialized);
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