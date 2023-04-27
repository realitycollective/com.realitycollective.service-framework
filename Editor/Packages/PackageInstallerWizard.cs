// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Editor.Extensions;
using RealityCollective.Extensions;
using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Services;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RealityCollective.ServiceFramework.Editor.Packages
{
    [InitializeOnLoad]
    public static class PackageInstallerWizard
    {
        static PackageInstallerWizard()
        {
            AssetsInstaller.AssetsInstalled += AssetsInstaller_AssetsInstalled;
        }

        private static void AssetsInstaller_AssetsInstalled(AssetsInstaller.AssetInstallerEventArgs eventArgs)
        {
            if (!Application.isBatchMode)
            {
                EditorApplication.delayCall += () =>
                {
                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                    EditorApplication.delayCall += () =>
                        AddConfigurations(eventArgs.InstalledAssets, eventArgs.SkipDialog);
                };
            }
        }

        private static void AddConfigurations(List<string> profiles, bool skipDialog = false)
        {
            if (skipDialog)
            {
                return;
            }

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
            // packkage configuration import dialog. If the user does not have a root profile yet,
            // for whatever reason, there is nothing we can do here.
            if (rootProfile.IsNull())
            {
                EditorUtility.DisplayDialog("Attention!", $"Each service and service module in the package will need to be manually registered as no existing Service Framework Instance was found.\nUse the {nameof(PackageInstallerProfile)} in the Profiles folder for the package once a Service Manager has been configured.", "OK");
                return;
            }

            Selection.activeObject = null;

            foreach (var profile in profiles.Where(x => x.EndsWith(".asset")))
            {
                var packageInstallerProfile = AssetDatabase.LoadAssetAtPath<PackageInstallerProfile>(profile);
                if (packageInstallerProfile.IsNull())
                {
                    continue;
                }

                if (EditorUtility.DisplayDialog("New package detected",
                    $"We found the {packageInstallerProfile.name.ToProperCase()}. Would you like to add this package configuration to your {rootProfile.name}?",
                    "Yes!",
                    "later"))
                {
                    PackageInstaller.InstallPackage(packageInstallerProfile);
                }
            }
        }
    }
}