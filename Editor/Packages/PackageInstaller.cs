// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Extensions;
using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Interfaces;
using RealityCollective.ServiceFramework.Packages;
using RealityCollective.ServiceFramework.Services;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace RealityCollective.ServiceFramework.Editor.Packages
{
    /// <summary>
    /// Installs a service framework plugin package to the active <see cref="ServiceProvidersProfile"/>.
    /// </summary>
    public static class PackageInstaller
    {
        public static string ProjectRootPath => Directory.GetParent(Application.dataPath).FullName.BackSlashes();

        /// <summary>
        /// Installs the <see cref="IService"/>s contained in the <see cref="PackageInstallerProfile"/> to the provided <see cref="ServiceProvidersProfile"/>.
        /// </summary>
        /// <param name="packageInstallerProfile">The <see cref="PackageInstallerProfile"/> to install.</param>
        /// <param name="rootProfile">The root profile to install the <paramref name="packageInstallerProfile"/> to.</param>
        public static void InstallPackage(PackageInstallerProfile packageInstallerProfile, ServiceProvidersProfile rootProfile)
        {
            if (ServiceManager.Instance == null ||
                ServiceManager.Instance.ActiveProfile.IsNull())
            {
                Debug.LogError($"Cannot install service configurations. There is no active {nameof(ServiceManager)} or it does not have a valid profile.");
                return;
            }

            var didInstallConfigurations = false;
            foreach (var configuration in packageInstallerProfile.Configurations)
            {
                var configurationType = configuration.InstancedType.Type;

                if (configurationType == null)
                {
                    Debug.LogError($"Failed to find a valid {nameof(configuration.InstancedType)} for {configuration.Name}!");
                    continue;
                }

                // If the service to install is a service module, we have to lookup the parent service and profile
                // for that to work.
                if (typeof(IServiceModule).IsAssignableFrom(configurationType))
                {
                    try
                    {
                        // Use the default contructor found on all service modules to identify the parent service type.
                        var didFindParentService = false;
                        var constructors = configurationType.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
                        foreach (var constructor in constructors)
                        {
                            var parameters = constructor.GetParameters();
                            if (parameters.Length == 4)
                            {
                                var parentTypeParameter = parameters[3];
                                var parentServiceConfiguration = rootProfile.ServiceConfigurations.FirstOrDefault(sc => parentTypeParameter.ParameterType.IsAssignableFrom(sc.InstancedType.Type));

                                // If we have found the parent service type, we can then lookup its profile because that's where we want to install to.
                                if (parentServiceConfiguration != null &&
                                    typeof(IServiceProfile<IServiceModule>).IsAssignableFrom(parentServiceConfiguration.Profile.GetType()))
                                {
                                    // Looks like we have all we need. Last thing to ensure is that the service module we want to install, is not already installed.
                                    var parentServiceProvidersProfile = parentServiceConfiguration.Profile as IServiceProfile<IServiceModule>;
                                    var serviceConfiguration = new ServiceConfiguration<IService>(configurationType, configuration.Name, configuration.Priority, configuration.RuntimePlatforms, configuration.Profile);
                                    if (parentServiceProvidersProfile.ServiceConfigurations.All(sc => sc.InstancedType.Type != serviceConfiguration.InstancedType.Type))
                                    {
                                        // Bada bing bada boom, install the service module to the parent service profile.
                                        parentServiceProvidersProfile.AddConfiguration(serviceConfiguration);
                                        EditorUtility.SetDirty((UnityEngine.Object)parentServiceProvidersProfile);
                                        didInstallConfigurations = true;
                                        didFindParentService = true;
                                        Debug.Log($"Installed {configuration.Name} to {parentServiceConfiguration.Profile.name}");
                                    }
                                }
                            }
                        }

                        if (!didFindParentService)
                        {
                            Debug.LogError("Unable to install configuration as the corresponding parent service was not available or its profile was invalid.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                        Debug.LogError($"Failed to install {configuration.Name} to {rootProfile.name}.");
                    }
                }
                // If the service is a top level service, we only need to make sure that the service is not already installed.
                else if (typeof(IService).IsAssignableFrom(configurationType))
                {
                    try
                    {
                        var serviceConfiguration = new ServiceConfiguration<IService>(configurationType, configuration.Name, configuration.Priority, configuration.RuntimePlatforms, configuration.Profile);
                        if (rootProfile.ServiceConfigurations.All(sc => sc.InstancedType.Type != serviceConfiguration.InstancedType.Type))
                        {
                            // Bada bing bada boom, install the service to the root profile.
                            rootProfile.AddConfiguration(serviceConfiguration);
                            EditorUtility.SetDirty(rootProfile);
                            didInstallConfigurations = true;
                            Debug.Log($"Installed {configuration.Name} to {rootProfile.name}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                        Debug.LogError($"Failed to install {configuration.Name} to {rootProfile.name}.");
                    }
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
