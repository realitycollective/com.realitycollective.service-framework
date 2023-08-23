// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Editor.Utilities;
using RealityCollective.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RealityCollective.ServiceFramework.Editor.Packages
{
    /// <summary>
    /// Installs plugin package assets.
    /// </summary>
    public static class AssetsInstaller
    {
        /// <summary>
        /// Contains information about an <see cref="AssetsInstaller"/>
        /// operation.
        /// </summary>
        public struct AssetInstallerEventArgs
        {
            /// <summary>
            /// List of paths to assets intalled into the project.
            /// </summary>
            public List<string> InstalledAssets { get; set; }

            /// <summary>
            /// If set, a silent install was requested.
            /// </summary>
            public bool SkipDialog { get; set; }
        }

        /// <summary>
        /// The installer has installed package assets to the project.
        /// </summary>
        public static Action<AssetInstallerEventArgs> AssetsInstalled;

        /// <summary>
        /// Attempt to copy any assets found in the source path into the project.
        /// </summary>
        /// <param name="sourcePath">The source path of the assets to be installed. This should typically be from a hidden upm package folder marked with a "~".</param>
        /// <param name="destinationPath">The destination path, typically inside the projects "Assets" directory.</param>
        /// <param name="regenerateGuids">Should the guids for the copied assets be regenerated?</param>
        /// <param name="skipDialog">If set, assets and configuration is installed without prompting the user.</param>
        /// <returns><c>true</c> if the assets were successfully installed to the project.</returns>
        public static bool TryInstallAssets(string sourcePath, string destinationPath, bool regenerateGuids = false, bool skipDialog = false, bool onlyUnityAssets = false)
            => TryInstallAssets(new Dictionary<string, string> { { sourcePath, destinationPath } }, regenerateGuids, skipDialog, onlyUnityAssets);

        /// <summary>
        /// Attempt to copy any assets found in the source path into the project.
        /// </summary>
        /// <param name="installationPaths">The assets paths to be installed. Key is the source path of the assets to be installed. This should typically be from a hidden upm package folder marked with a "~". Value is the destination.</param>
        /// <param name="regenerateGuids">Should the guids for the copied assets be regenerated?</param>
        /// <param name="skipDialog">If set, assets and configuration is installed without prompting the user.</param>
        /// <returns><c>true</c> if the assets were successfully installed to the project.</returns>
        public static bool TryInstallAssets(Dictionary<string, string> installationPaths, bool regenerateGuids = false, bool skipDialog = false, bool onlyUnityAssets = false)
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

                    if (onlyUnityAssets)
                    {
                        installedAssets.AddRange(UnityFileHelper.GetUnityAssetsAtPath(destinationPath));
                    }
                    else
                    {
                        installedAssets.AddRange(UnityFileHelper.GetAllFilesAtPath(destinationPath));
                    }

                    for (int i = 0; i < installedAssets.Count; i++)
                    {
                        EditorUtility.DisplayProgressBar("Verifying assets...", Path.GetFileNameWithoutExtension(installedAssets[i]), i / (float)installedAssets.Count);
                        installedAssets[i] = installedAssets[i].Replace($"{PackageInstaller.ProjectRootPath}{Path.DirectorySeparatorChar}", string.Empty).BackSlashes();
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

                    var copiedAssets = new List<string>();

                    if (onlyUnityAssets)
                    {
                        copiedAssets = UnityFileHelper.GetUnityAssetsAtPath(destinationPath);
                    }
                    else
                    {
                        copiedAssets = UnityFileHelper.GetAllFilesAtPath(sourcePath);
                    }

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

            if (installedAssets.Where(asset => asset.Contains(".mat")).Any())
            {
                EditorUtility.DisplayDialog("Attention!", $"Materials were included in the Asset bundle copied to\n[{installationPaths.Values.First()}]\n\n If you are using URP or HDRP, we recommend you upgrade the shaders for these materials for use with URP/HDRP using Unity's tool found under\n  Window > Rendering > Render Pipeline Converter.", "OK");
            }

            EditorUtility.ClearProgressBar();
            AssetsInstalled?.Invoke(new AssetInstallerEventArgs
            {
                InstalledAssets = installedAssets,
                SkipDialog = skipDialog
            });

            return true;
        }

        private static string CopyAsset(this string rootPath, string sourceAssetPath, string destinationPath)
        {
            sourceAssetPath = sourceAssetPath.BackSlashes();
            destinationPath = $"{destinationPath}{sourceAssetPath.Replace(Path.GetFullPath(rootPath), string.Empty)}".BackSlashes();
            destinationPath = Path.Combine(PackageInstaller.ProjectRootPath, destinationPath).BackSlashes();

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

            return destinationPath.Replace($"{PackageInstaller.ProjectRootPath}{Path.DirectorySeparatorChar}", string.Empty);
        }
    }
}