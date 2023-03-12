// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Editor.Extensions;
using RealityCollective.Editor.Utilities;
using RealityCollective.Extensions;
using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Interfaces;
using RealityCollective.ServiceFramework.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace RealityCollective.ServiceFramework.Editor
{
    public static class PackageInstaller
    {
        private static string ProjectRootPath => Directory.GetParent(Application.dataPath).FullName.BackSlashes();

        /// <summary>
        /// Attempt to copy any assets found in the source path into the project.
        /// </summary>
        /// <param name="sourcePath">The source path of the assets to be installed. This should typically be from a hidden upm package folder marked with a "~".</param>
        /// <param name="destinationPath">The destination path, typically inside the projects "Assets" directory.</param>
        /// <param name="regenerateGuids">Should the guids for the copied assets be regenerated?</param>
        /// <param name="skipDialog">If set, assets and configuration is installed without prompting the user.</param>
        /// <returns>Returns true if the profiles were successfully copies, installed, and added to the <see cref="MixedRealityToolkitRootProfile"/>.</returns>
        public static bool TryInstallAssets(string sourcePath, string destinationPath, bool regenerateGuids = false, bool skipDialog = false)
            => TryInstallAssets(new Dictionary<string, string> { { sourcePath, destinationPath } }, regenerateGuids, skipDialog);

        /// <summary>
        /// Attempt to copy any assets found in the source path into the project.
        /// </summary>
        /// <param name="installationPaths">The assets paths to be installed. Key is the source path of the assets to be installed. This should typically be from a hidden upm package folder marked with a "~". Value is the destination.</param>
        /// <param name="regenerateGuids">Should the guids for the copied assets be regenerated?</param>
        /// <param name="skipDialog">If set, assets and configuration is installed without prompting the user.</param>
        /// <returns>Returns true if the profiles were successfully copies, installed, and added to the <see cref="MixedRealityToolkitRootProfile"/>.</returns>
        public static bool TryInstallAssets(Dictionary<string, string> installationPaths, bool regenerateGuids = false, bool skipDialog = false)
        {
            var anyFail = false;
            var newInstall = true;
            var installedAssets = new List<string>();
            var installedDirectories = new List<string>();

            foreach (var installationPath in installationPaths)
            {
                var sourcePath = installationPath.Key.BackSlashes();
                var destinationPath = installationPath.Value.BackSlashes();
                installedDirectories.Add(destinationPath);

                if (Directory.Exists(destinationPath))
                {
                    newInstall = false;
                    EditorUtility.DisplayProgressBar("Verifying assets...", $"{sourcePath} -> {destinationPath}", 0);

                    installedAssets.AddRange(UnityFileHelper.GetUnityAssetsAtPath(destinationPath));

                    for (int i = 0; i < installedAssets.Count; i++)
                    {
                        EditorUtility.DisplayProgressBar("Verifying assets...", Path.GetFileNameWithoutExtension(installedAssets[i]), i / (float)installedAssets.Count);
                        installedAssets[i] = installedAssets[i].Replace($"{ProjectRootPath}{Path.DirectorySeparatorChar}", string.Empty).BackSlashes();
                    }

                    EditorUtility.ClearProgressBar();
                }
                else
                {
                    var destinationDirectory = Path.GetFullPath(destinationPath);

                    // Check if directory or symbolic link exists before attempting to create it
                    if (!Directory.Exists(destinationDirectory) &&
                        !File.Exists(destinationDirectory))
                    {
                        Directory.CreateDirectory(destinationDirectory);
                    }

                    EditorUtility.DisplayProgressBar("Copying assets...", $"{sourcePath} -> {destinationPath}", 0);

                    var copiedAssets = UnityFileHelper.GetUnityAssetsAtPath(sourcePath);

                    for (var i = 0; i < copiedAssets.Count; i++)
                    {
                        EditorUtility.DisplayProgressBar("Copying assets...", Path.GetFileNameWithoutExtension(copiedAssets[i]), i / (float)copiedAssets.Count);

                        try
                        {
                            copiedAssets[i] = CopyAsset(sourcePath, copiedAssets[i], destinationPath);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                            anyFail = true;
                        }
                    }

                    if (!anyFail)
                    {
                        installedAssets.AddRange(copiedAssets);
                    }

                    EditorUtility.ClearProgressBar();
                }
            }

            if (anyFail)
            {
                foreach (var installedDirectory in installedDirectories)
                {
                    try
                    {
                        if (Directory.Exists(installedDirectory))
                        {
                            Directory.Delete(installedDirectory);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            }

            if (newInstall && regenerateGuids)
            {
                GuidRegenerator.RegenerateGuids(installedDirectories);
            }

            if (!Application.isBatchMode)
            {
                EditorApplication.delayCall += () =>
                {
                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                    EditorApplication.delayCall += () =>
                        AddConfigurations(installedAssets, skipDialog);
                };
            }

            EditorUtility.ClearProgressBar();
            return true;
        }

        private static void AddConfigurations(List<string> profiles, bool skipDialog = false)
        {
            ServiceProvidersProfile rootProfile;

            if (ServiceManager.IsActiveAndInitialized)
            {
                rootProfile = ServiceManager.Instance.ActiveProfile;
            }
            else
            {
                var availableRootProfiles = ScriptableObjectExtensions.GetAllInstances<ServiceProvidersProfile>();
                rootProfile = availableRootProfiles.Length > 0 ? availableRootProfiles[0] : null;
            }

            // Only if a root profile is available at all it makes sense to display the
            // platform configuration import dialog. If the user does not have a root profile yet,
            // for whatever reason, there is nothing we can do here.
            if (rootProfile.IsNull())
            {
                EditorUtility.DisplayDialog("Attention!", "Each service and service module in the platform configuration will need to be manually registered as no existing Service Framework Instance was found.\nUse the Platform Installer in the Profiles folder for the package once a Service Manager has been configured.", "OK");
                return;
            }

            Selection.activeObject = null;

            foreach (var profile in profiles.Where(x => x.EndsWith(".asset")))
            {
                var platformConfigurationProfile = AssetDatabase.LoadAssetAtPath<PackageServiceConfigurationProfile>(profile);

                if (platformConfigurationProfile.IsNull()) { continue; }

                if (skipDialog || EditorUtility.DisplayDialog("We found a new Platform Configuration",
                    $"We found the {platformConfigurationProfile.name.ToProperCase()}. Would you like to add this platform configuration to your {rootProfile.name}?",
                    "Yes, Absolutely!",
                    "later"))
                {
                    InstallConfiguration(platformConfigurationProfile, rootProfile);
                }
            }
        }

        private static string CopyAsset(this string rootPath, string sourceAssetPath, string destinationPath)
        {
            sourceAssetPath = sourceAssetPath.BackSlashes();
            destinationPath = $"{destinationPath}{sourceAssetPath.Replace(Path.GetFullPath(rootPath), string.Empty)}".BackSlashes();
            destinationPath = Path.Combine(ProjectRootPath, destinationPath).BackSlashes();

            if (!File.Exists(destinationPath))
            {
                if (!Directory.Exists(Directory.GetParent(destinationPath).FullName))
                {
                    Directory.CreateDirectory(Directory.GetParent(destinationPath).FullName);
                }

                try
                {
                    File.Copy(sourceAssetPath, destinationPath);
                }
                catch
                {
                    Debug.LogError($"$Failed to copy asset!\n{sourceAssetPath}\n{destinationPath}");
                    throw;
                }
            }

            return destinationPath.Replace($"{ProjectRootPath}{Path.DirectorySeparatorChar}", string.Empty);
        }

        /// <summary>
        /// Installs the provided <see cref="PackageServiceConfigurationProfile"/> in the provided <see cref="ServiceProvidersProfile"/>.
        /// </summary>
        /// <param name="platformConfigurationProfile">The <see cref="PackageServiceConfigurationProfile"/> to install.</param>
        /// <param name="rootProfile">The root profile to install the <paramref name="platformConfigurationProfile"/> to.</param>
        public static void InstallConfiguration(PackageServiceConfigurationProfile platformConfigurationProfile, ServiceProvidersProfile rootProfile)
        {
            if (ServiceManager.Instance == null ||
                ServiceManager.Instance.ActiveProfile.IsNull())
            {
                Debug.LogError($"Cannot install service configurations. There is no active {nameof(ServiceManager)} or it does not have a valid profile.");
                return;
            }

            var didInstallConfigurations = false;
            foreach (var configuration in platformConfigurationProfile.Configurations)
            {
                var configurationType = configuration.InstancedType.Type;

                if (configurationType == null)
                {
                    Debug.LogError($"Failed to find a valid {nameof(configuration.InstancedType)} for {configuration.Name}!");
                    continue;
                }

                if (typeof(IServiceModule).IsAssignableFrom(configurationType))
                {
                    try
                    {
                        var types = new Type[4];
                        types[0] = typeof(string);
                        types[1] = typeof(uint);
                        types[2] = typeof(BaseProfile);
                        types[3] = typeof(IService);

                        var constructorInfo = configurationType.GetConstructor(
                            BindingFlags.Instance | BindingFlags.Public, null,
                            CallingConventions.HasThis, types, null);

                        if (constructorInfo == null)
                        {
                            continue;
                        }

                        //Debug.LogError("Unable to install configuration as the corresponding parent services were not available.");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                        Debug.LogError($"Failed to install {configuration.Name} to {rootProfile.name}.");
                    }

                    //var inputDataProviderConfiguration = new ServiceConfiguration<IMixedRealityInputServiceModule>(configuration);

                    //if (inputSystemProfile.ServiceConfigurations.Any(serviceConfiguration => serviceConfiguration.InstancedType.Type == inputDataProviderConfiguration.InstancedType.Type))
                    //{
                    //    configurationsAlreadyInstalled = true;
                    //}
                    //else if (inputSystemProfile.ServiceConfigurations.All(serviceConfiguration => serviceConfiguration.InstancedType.Type != inputDataProviderConfiguration.InstancedType.Type))
                    //{
                    //    Debug.Log($"Added {configuration.Name} to {rootProfile.name}");
                    //    inputSystemProfile.AddConfiguration(inputDataProviderConfiguration);
                    //    EditorUtility.SetDirty(inputSystemProfile);
                    //    didInstallConfigurations = true;
                    //}
                }
                if (typeof(IService).IsAssignableFrom(configurationType))
                {
                    try
                    {
                        var serviceConfiguration = new ServiceConfiguration<IService>(configurationType, configuration.Name, configuration.Priority, configuration.RuntimePlatforms, configuration.Profile);
                        if (rootProfile.ServiceConfigurations.All(sc => sc.InstancedType.Type != serviceConfiguration.InstancedType.Type))
                        {
                            Debug.Log($"Installed {configuration.Name} to {rootProfile.name}");
                            rootProfile.AddConfiguration(serviceConfiguration);
                            EditorUtility.SetDirty(rootProfile);
                            didInstallConfigurations = true;
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
