// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Services;
using UnityEngine;

namespace RealityCollective.ServiceFramework
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class SceneBasedServiceManager : MonoBehaviour
    {
        private string sceneName = null;

        [SerializeField]
        [Tooltip("All the additional non-required services registered with the Service Manager.")]
        private ServiceProvidersProfile serviceProvidersProfile = null;

        #region MonoBehaviour Implementation
        private void OnEnable()
        {
            sceneName = gameObject.scene.name;
            ServiceManager.Instance?.AddServiceConfigurationForScene(sceneName, serviceProvidersProfile.ServiceConfigurations);
            ServiceManager.Instance?.LoadServicesForScene(sceneName);
        }

        private void OnDisable()
        {
            ServiceManager.Instance?.UnloadServicesForScene(sceneName);
        }
        #endregion MonoBehaviour Implementation
    }
}