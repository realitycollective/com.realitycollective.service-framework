// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Interfaces;
using UnityEngine;

namespace RealityCollective.ServiceFramework.Definitions
{
    [CreateAssetMenu(menuName = "Reality Collective/Service Framework/Service Providers Profile", fileName = "ServiceProvidersProfile", order = (int)CreateProfileMenuItemIndices.ServiceProviders)]
    public class ServiceProvidersProfile : BaseServiceProfile<IService>
    {
        [SerializeField]
        [Tooltip("The service manager will only initialise services in the Editor when it is running in play mode.\nThe default is to always be active and validating service configuration.")]
        private bool initializeOnPlay = false;

        /// <summary>
        /// The service manager will only initialise services in the Editor when it is running in play mode.
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
    }
}
