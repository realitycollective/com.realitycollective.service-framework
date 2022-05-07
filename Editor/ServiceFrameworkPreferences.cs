// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.ServiceFramework.Definitions.Platforms;
using RealityToolkit.ServiceFramework.Editor.Utilities;
using RealityToolkit.ServiceFramework.Extensions;
using RealityToolkit.ServiceFramework.Interfaces;
using RealityToolkit.ServiceFramework.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RealityToolkit.ServiceFramework
{
    public static class ServiceFrameworkPreferences
    {
        public const string Editor_Menu_Keyword = "Reality Toolkit";

        #region Current Platform Target

        private static bool isCurrentPlatformPreferenceLoaded;

        private static IPlatform currentPlatformTarget = null;

        /// <summary>
        /// The current <see cref="IMixedRealityPlatform"/> target.
        /// </summary>
        public static IPlatform CurrentPlatformTarget
        {
            get
            {
                if (!isCurrentPlatformPreferenceLoaded || currentPlatformTarget == null)
                {
                    isCurrentPlatformPreferenceLoaded = true;

                    ServiceManager.CheckPlatforms();

                    if (TypeExtensions.TryResolveType(EditorPreferences.Get(nameof(CurrentPlatformTarget), Guid.Empty.ToString()), out var platform))
                    {
                        foreach (var availablePlatform in ServiceManager.AvailablePlatforms)
                        {
                            if (availablePlatform is AllPlatforms ||
                                availablePlatform is EditorPlatform ||
                                availablePlatform is CurrentBuildTargetPlatform)
                            {
                                continue;
                            }

                            if (availablePlatform.GetType() == platform)
                            {
                                currentPlatformTarget = availablePlatform;
                                break;
                            }
                        }
                    }

                    if (currentPlatformTarget == null)
                    {
                        var possibleBuildTargets = new List<IPlatform>();

                        foreach (var availablePlatform in ServiceManager.AvailablePlatforms)
                        {
                            if (availablePlatform is AllPlatforms ||
                                availablePlatform is EditorPlatform ||
                                availablePlatform is CurrentBuildTargetPlatform)
                            {
                                continue;
                            }

                            foreach (var buildTarget in availablePlatform.ValidBuildTargets)
                            {
                                if (EditorUserBuildSettings.activeBuildTarget == buildTarget)
                                {
                                    possibleBuildTargets.Add(availablePlatform);
                                }
                            }
                        }

                        Debug.Assert(possibleBuildTargets.Count > 0);

                        currentPlatformTarget = possibleBuildTargets.Count == 1
                            ? possibleBuildTargets[0]
                            : possibleBuildTargets.FirstOrDefault(p => p.PlatformOverrides == null ||
                                                                       p.PlatformOverrides.Length == 0);
                        return currentPlatformTarget;
                    }
                }

                return currentPlatformTarget;
            }
            set
            {
                if (value is AllPlatforms ||
                    value is EditorPlatform ||
                    value is CurrentBuildTargetPlatform)
                {
                    return;
                }

                currentPlatformTarget = value;

                EditorPreferences.Set(nameof(CurrentPlatformTarget),
                    currentPlatformTarget != null
                        ? currentPlatformTarget.GetType().GUID.ToString()
                        : Guid.Empty.ToString());
            }
        }

        #endregion Current Platform Target
    }
}