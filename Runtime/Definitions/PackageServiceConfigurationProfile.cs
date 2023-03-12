// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace RealityCollective.ServiceFramework.Definitions
{
    [CreateAssetMenu(menuName = "Reality Collective/Service Framework/" + nameof(PackageServiceConfigurationProfile), fileName = nameof(PackageServiceConfigurationProfile), order = (int)CreateProfileMenuItemIndices.Configuration)]
    public class PackageServiceConfigurationProfile : BaseProfile
    {
        [SerializeField]
        private RuntimePlatformEntry platformEntries = new RuntimePlatformEntry();

        public RuntimePlatformEntry PlatformEntries => platformEntries;

        [SerializeField]
        private ServiceConfiguration[] configurations = new ServiceConfiguration[0];

        public ServiceConfiguration[] Configurations => configurations;
    }
}