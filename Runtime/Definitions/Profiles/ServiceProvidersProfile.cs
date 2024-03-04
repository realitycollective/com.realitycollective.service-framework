﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Interfaces;
using UnityEngine;

namespace RealityCollective.ServiceFramework.Definitions
{
    /// <summary>
    /// Service configuration profile, for loading services for the entire project.
    /// </summary>
    [CreateAssetMenu(menuName = RuntimeServiceFrameworkPreferences.Service_Framework_Editor_Menu_Keyword + "/Service Providers Profile", fileName = "ServiceProvidersProfile", order = (int)CreateProfileMenuItemIndices.ServiceProviders)]
    public class ServiceProvidersProfile : BaseServiceProfile<IService>
    {
        [SerializeField]
        [Tooltip("The service manager will only initialize services in the Editor when it is running in play mode.\nThe default is to always be active and validating service configuration.")]
        private bool initializeOnPlay = false;

        /// <summary>
        /// The service manager will only initialize services in the Editor when it is running in play mode.
        /// The default is to always be active and validating service configuration.
        /// </summary>
        public bool InitializeOnPlay => initializeOnPlay;

        [SerializeField]
        [Tooltip("Ensure that the Service Manager Instance is not destroyed on scene change.")]
        private bool doNotDestroyServiceManagerOnLoad = true;

        /// <summary>
        /// Ensure that the Service Manager Instance is not destroyed on scene change.
        /// </summary>
        public bool DoNotDestroyServiceManagerOnLoad => doNotDestroyServiceManagerOnLoad;

        [SerializeField]
        [Tooltip("The scene based service configuration.")]
        private SceneServiceConfiguration[] sceneServiceConfiguration;

        /// <summary>
        /// The scene based service configuration.
        /// </summary>
        public SceneServiceConfiguration[] SceneServiceConfiguration => sceneServiceConfiguration;
    }
}