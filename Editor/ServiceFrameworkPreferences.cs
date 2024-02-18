// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Editor.Utilities;
using RealityCollective.Extensions;
using RealityCollective.ServiceFramework.Definitions.Platforms;
using RealityCollective.ServiceFramework.Interfaces;
using RealityCollective.ServiceFramework.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RealityCollective.ServiceFramework
{
    public static class ServiceFrameworkPreferences
    {
        public const string Editor_Menu_Keyword = "Reality Collective";

        public const string Service_Framework_Editor_Menu_Keyword = "Tools/Service Framework";

        private static readonly string[] Package_Keywords = { "Reality", "Collective", "Mixed", "Reality", "Service", "Framework" };

        public static readonly HashSet<Type> ExcludedTemplateServices = new HashSet<Type>
        {
            typeof(BaseService),
            typeof(TemplateService)
        };

        #region Show Inspector Debug View settings prompt
        private static readonly GUIContent ShowInspectorDebugViewContent = new GUIContent("Show services debug properties", "Enables the debug view for Service Profiles and Modules in the inspector view.");
        private static readonly string ShowInspectorDebugViewKey = $"{Application.productName}_RealityCollective_Editor_ShowInspectorDebugView";
        private static bool showInspectorDebugViewPrefLoaded;
        private static bool showInspectorDebugView = false;

        /// <summary>
        /// Should the settings prompt show on startup?
        /// </summary>
        public static bool ShowInspectorDebugView
        {
            get
            {
                if (!showInspectorDebugViewPrefLoaded)
                {
                    showInspectorDebugView = EditorPrefs.GetBool(ShowInspectorDebugViewKey, false);
                    showInspectorDebugViewPrefLoaded = true;
                }

                return showInspectorDebugView;
            }
            set => EditorPrefs.SetBool(ShowInspectorDebugViewKey, showInspectorDebugView = value);
        }
        #endregion Show Inspector Debug View settings prompt

        #region Current Platform Target

        private static bool isCurrentPlatformPreferenceLoaded;

        private static IPlatform currentPlatformTarget = null;

        /// <summary>
        /// The current <see cref="IPlatform"/> target.
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

        #region Menu Redirect
                /// <summary>
        /// Simple scene helper to create the beginnings of a scene, creating the scene root and a floor.
        /// </summary>
        [MenuItem(ServiceFrameworkPreferences.Editor_Menu_Keyword + "/Service Framework/Moved to Tools-Service Framework", false, 1)]
        public static void RedirectNotice()
        {
        }
        #endregion
        [SettingsProvider]
        private static SettingsProvider Preferences()
        {
            return new SettingsProvider("Preferences/ServiceFramework", SettingsScope.User, Package_Keywords)
            {
                label = "Service Framework",
                guiHandler = OnPreferencesGui,
                keywords = new HashSet<string>(Package_Keywords)
            };
        }

        private static void OnPreferencesGui(string searchContext)
        {
            var prevLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 200f;

            #region Show Inspector Debug View Setting Preference

            EditorGUI.BeginChangeCheck();
            showInspectorDebugView = EditorGUILayout.Toggle(ShowInspectorDebugViewContent, ShowInspectorDebugView);

            if (EditorGUI.EndChangeCheck())
            {
                ShowInspectorDebugView = showInspectorDebugView;
            }

            #endregion  Show Inspector Debug View Setting Preference

            EditorGUIUtility.labelWidth = prevLabelWidth;
        }
    }
}