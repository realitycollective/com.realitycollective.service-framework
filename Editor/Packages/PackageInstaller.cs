// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Extensions;
using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Interfaces;
using RealityCollective.ServiceFramework.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RealityCollective.ServiceFramework.Editor.Packages
{
    /// <summary>
    /// Installs a service framework plugin package's <see cref="IService"/>s and <see cref="IServiceModule"/>s
    /// to a <see cref="ServiceProvidersProfile"/>.
    /// </summary>
    public static class PackageInstaller
    {
        private static List<IPackageModulesInstaller> modulesInstallers = new List<IPackageModulesInstaller>();

        public static string ProjectRootPath => Directory.GetParent(Application.dataPath).FullName.BackSlashes();

        /// <summary>
        /// Registers a <see cref="IPackageModulesInstaller"/> with the package installer. The <see cref="IPackageModulesInstaller"/>
        /// will be consulted by the packgae installer whenever a <see cref="IServiceModule"/> needs to be installed into its parent service
        /// profile.
        /// </summary>
        /// <param name="modulesInstaller"></param>
        public static void RegisterModulesInstaller(IPackageModulesInstaller modulesInstaller)
            => modulesInstallers.EnsureListItem(modulesInstaller);

        /// <summary>
        /// Installs the <see cref="IService"/>s contained in the <see cref="PackageInstallerProfile"/> to the provided <see cref="ServiceProvidersProfile"/>.
        /// </summary>
        /// <param name="packageInstallerProfile">The <see cref="PackageInstallerProfile"/> to install.</param>
        public static void InstallPackage(PackageInstallerProfile packageInstallerProfile)
        {
            if (ServiceManager.Instance == null ||
                ServiceManager.Instance.ActiveProfile.IsNull())
            {
                Debug.LogError($"Cannot install service configurations. There is no active {nameof(ServiceManager)} or it does not have a valid profile.");
                return;
            }

            var rootProfile = ServiceManager.Instance.ActiveProfile;
            var didInstallConfigurations = false;
            foreach (var configuration in packageInstallerProfile.Configurations)
            {
                try
                {
                    var configurationType = configuration.InstancedType.Type;

                    if (configurationType == null)
                    {
                        Debug.LogError($"Failed to find a valid {nameof(configuration.InstancedType)} for {configuration.Name}!");
                        continue;
                    }

                    // If the service to install is a service module, we have to lookup the service module installer
                    // for that specific module type and ask it to install the module.
                    if (typeof(IServiceModule).IsAssignableFrom(configurationType))
                    {
                        // Check with all registered module installers, whether the module is a fit.
                        var didInstallServiceModule = false;
                        foreach (var modulesInstaller in modulesInstallers)
                        {
                            didInstallServiceModule = modulesInstaller.Install(configuration);
                            if (didInstallServiceModule)
                            {
                                break;
                            }
                        }

                        if (!didInstallServiceModule)
                        {
                            Debug.LogError($"Unable to install {configurationType.Name}. Installation was denied by the installer or no module installer was available for type {configurationType.Name}.");
                        }
                    }
                    // If the service is a top level service, we only need to make sure that the service is not already installed.
                    // in the target profile.
                    else if (typeof(IService).IsAssignableFrom(configurationType))
                    {
                        // Setup the configuration.
                        var serviceConfiguration = new ServiceConfiguration<IService>(configurationType, configuration.Name, configuration.Priority, configuration.RuntimePlatforms, configuration.Profile);

                        // Make sure it's not already in the target profile.
                        if (rootProfile.ServiceConfigurations.All(sc => sc.InstancedType.Type != serviceConfiguration.InstancedType.Type))
                        {
                            // Bada bing bada boom, install the service to the target profile.
                            rootProfile.AddConfiguration(serviceConfiguration);
                            EditorUtility.SetDirty(rootProfile);
                            didInstallConfigurations = true;
                            Debug.Log($"Installed {serviceConfiguration.Name}.");
                        }
                        else
                        {
                            Debug.Log($"Skipped installing {serviceConfiguration.Name}. Already installed.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    Debug.LogError($"Failed to install {configuration.Name}.");
                }
            }

            AssetDatabase.SaveAssets();
            EditorApplication.delayCall += () => AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            if (didInstallConfigurations)
            {
                ServiceManager.Instance.ResetProfile(ServiceManager.Instance.ActiveProfile);
            }
        }
    }
}
