// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Definitions;
using UnityEngine;

namespace RealityCollective.ServiceFramework.Editor.Packages
{
    /// <summary>
    /// A package profile defines services and modules that the package has to offer and may be installed
    /// to a <see cref="ServiceProvidersProfile"/> to register those services and modules with the service container.
    /// </summary>
    [CreateAssetMenu(menuName = "Reality Collective/Service Framework/" + nameof(PackageInstallerProfile), fileName = nameof(PackageInstallerProfile), order = (int)CreateProfileMenuItemIndices.Configuration)]
    public class PackageInstallerProfile : BaseProfile
    {
        [SerializeField, Tooltip("The platforms the package can run on.")]
        private RuntimePlatformEntry platformEntries = new RuntimePlatformEntry();

        /// <summary>
        /// The platforms the package can run on.
        /// </summary>
        public RuntimePlatformEntry PlatformEntries => platformEntries;

        [SerializeField, Tooltip("The service and module configurations of the package.")]
        private ServiceConfiguration[] configurations = new ServiceConfiguration[0];

        /// <summary>
        /// The service and module configurations of the package that may be installed.
        /// </summary>
        public ServiceConfiguration[] Configurations => configurations;
    }
}