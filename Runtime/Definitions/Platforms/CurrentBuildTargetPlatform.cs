// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

#if UNITY_EDITOR
using System.Collections.Generic;
using RealityCollective.ServiceFramework.Interfaces;
#endif

namespace RealityCollective.ServiceFramework.Definitions.Platforms
{
    /// <summary>
    /// Used by the Service Framework to signal that the feature is only available when the current built target matches the platform target.
    /// </summary>
    [System.Runtime.InteropServices.Guid("5A504905-0968-4B2F-A537-7FE804F1BD8E")]
    public sealed class CurrentBuildTargetPlatform : BasePlatform
    {
        /// <inheritdoc />
        public override bool IsAvailable => Application.isEditor;

#if UNITY_EDITOR
        /// <summary>
        /// Checks to see if the current build target is available for the list of provided platform.
        /// </summary>
        /// <param name="platforms"></param>
        /// <returns>True, if any build target is active.</returns>
        public static bool IsBuildTargetActive(List<IPlatform> platforms)
        {
            var isBuildTargetActive = false;
            var isEditorPlatformActive = false;

            foreach (var platform in platforms)
            {
                switch (platform)
                {
                    case AllPlatforms _:
                        return true;
                    case EditorPlatform _:
                        return true;
                    case CurrentBuildTargetPlatform _:
                        isEditorPlatformActive = platform.IsAvailable;
                        break;
                    default:
                        isBuildTargetActive |= platform.IsBuildTargetAvailable;
                        break;
                }
            }

            return isBuildTargetActive && isEditorPlatformActive;
        }
#endif // UNITY_EDITOR
    }
}