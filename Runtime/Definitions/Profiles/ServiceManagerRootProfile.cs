// Copyright (c) Reality Collective. All rights reserved.

using RealityToolkit.ServiceFramework.Interfaces;
using UnityEngine;

namespace RealityToolkit.ServiceFramework.Definitions
{
    [CreateAssetMenu(menuName = "RealityToolkit/Service Manager/ Root Profile", fileName = "ServiceManagerRootProfile", order = (int)CreateProfileMenuItemIndices.Configuration - 1)]
    public class ServiceManagerRootProfile : BaseServiceProfile<IService>
    {
        [SerializeField] 
        [Tooltip("The service manager will do the initialisation of services on play")]
        private bool initialiseOnPlay = false;

        /// <summary>
        /// Configuration of the service manager for initialisation of services on play
        /// </summary>
        public bool InitialiseOnPlay => initialiseOnPlay;

        [SerializeField]
        [Tooltip("All the additional non-required services registered with the Service Manager.")]
        private ServiceProvidersProfile serviceProvidersProfile = null;

        /// <summary>
        /// All the additional non-required systems, features, and managers registered with the Service Manager.
        /// </summary>
        public ServiceProvidersProfile ServiceProvidersProfile
        {
            get => serviceProvidersProfile;
            internal set => serviceProvidersProfile = value;
        }
    }
}