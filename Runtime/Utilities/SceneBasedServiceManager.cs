// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Services;
using UnityEngine;

namespace RealityCollective.ServiceFramework
{
    /// <summary>
    /// Manages the services for a scene.
    /// </summary>
    /// <remarks>
    /// This component is used to manage the services for a scene. It will register the services with the Service Manager when the scene is loaded and unregister them when the scene is unloaded.
    /// This includes when the component is enabled and disabled, which will load and unload the services respectively.
    /// </remarks>
    [AddComponentMenu(RuntimeServiceFrameworkPreferences.Service_Framework_Editor_Menu_Keyword + "/Scene Based Service Manager")]
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class SceneBasedServiceManager : MonoBehaviour
    {
        private string sceneName = null;

        [SerializeField]
        [Tooltip("The services to registered with the Service Manager for the scene this manager is in. Services will be unloaded on scene exit (or manager if the manager is disabled).")]
        private ServiceProvidersProfile serviceProvidersProfile = null;

        #region MonoBehaviour Implementation
        private void OnEnable()
        {
            sceneName = gameObject.scene.name;
            if (ServiceManager.IsActiveAndInitialized)
            {
                ServiceManager.Instance.AddServiceConfigurationForScene(sceneName, serviceProvidersProfile.ServiceConfigurations);
                ServiceManager.Instance.LoadServicesForScene(sceneName);
            }
        }

        private void OnDisable()
        {
            ServiceManager.Instance?.UnloadServicesForScene(sceneName);
        }
        #endregion MonoBehaviour Implementation
    }
}