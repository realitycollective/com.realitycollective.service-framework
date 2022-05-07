﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.ServiceFramework.Interfaces;
using UnityEngine;

namespace RealityToolkit.ServiceFramework.Definitions
{
    [CreateAssetMenu(menuName = "RealityToolkit/Service Manager/Service Providers Profile", fileName = "ServiceProvidersProfile", order = (int)CreateProfileMenuItemIndices.ServiceProviders)]
    public class ServiceProvidersProfile : BaseServiceProfile<IService> 
    {
        [SerializeField]
        [Tooltip("The service manager will do the initialisation of services on play")]
        private bool initialiseOnPlay = false;

        /// <summary>
        /// Configuration of the service manager for initialisation of services on play
        /// </summary>
        public bool InitialiseOnPlay => initialiseOnPlay;
    }
}