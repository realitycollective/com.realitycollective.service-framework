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
        [Tooltip("The service manager will only initialise services in the Editor when it is running\nThe default is to always be active and validating service configuration.")]
        private bool initialiseOnPlay = false;

        /// <summary>
        /// Configuration of the service manager for initialisation of services on play
        /// </summary>
        public bool InitialiseOnPlay => initialiseOnPlay;

        [SerializeField]
        [Tooltip("Ensure that the Service Manager Instance is not destroyed on scene change")]
        private bool dontDestroyServiceManagerOnLoad = true;

        /// <summary>
        /// Configuration of the service manager for initialisation of services on play
        /// </summary>
        public bool DontDestroyServiceManagerOnLoad => dontDestroyServiceManagerOnLoad;
    }
}
