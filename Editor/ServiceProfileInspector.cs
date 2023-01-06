// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Definitions.Utilities;
using RealityCollective.Editor.Extensions;
using RealityCollective.Extensions;
using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Definitions.Platforms;
using RealityCollective.ServiceFramework.Editor.PropertyDrawers;
using RealityCollective.ServiceFramework.Editor.Utilities;
using RealityCollective.ServiceFramework.Interfaces;
using RealityCollective.ServiceFramework.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace RealityCollective.ServiceFramework.Editor.Profiles
{
    [CustomEditor(typeof(BaseServiceProfile<>), true, isFallback = true)]
    public class ServiceProfileInspector : BaseProfileInspector
    {
        private static readonly Type AllPlatformsType = typeof(AllPlatforms);
        private static readonly Guid AllPlatformsGuid = AllPlatformsType.GUID;
        private readonly GUIContent nameContent = new GUIContent("Name", "The referenced name of the service");
        private readonly GUIContent instancedTypeContent = new GUIContent("Instanced Type", "The concrete type of the service to instantiate");
        private readonly GUIContent profileContent = new GUIContent("Profile", "The settings profile for this service.");
        private ReorderableList configurationList;
        private int currentlySelectedConfigurationOption;
        private List<string> excludedProperties = new List<string> { "m_Script", nameof(configurations)};

        private SerializedProperty configurations; // Cannot be auto property bc field is serialized.

        protected SerializedProperty Configurations => configurations;

        /// <summary>
        /// Gets the service constraint used to filter options listed in the
        /// <see cref="configurations"/> instance type dropdown. Set after
        /// <see cref="OnEnable"/> was called to override.
        /// </summary>
        protected Type ServiceConstraint { get; set; } = null;

        private List<Tuple<bool, bool>> configListHeightFlags;

        private GUIStyle buttonGuiStyle = null;

        private GUIStyle ButtonGuiStyle => buttonGuiStyle ?? (buttonGuiStyle =
            new GUIStyle(EditorStyles.toolbarButton)
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold
            });

        private int platformIndex;
        private readonly List<IPlatform> platforms = new List<IPlatform>();

        private List<IPlatform> Platforms
        {
            get
            {
                if (platforms.Count == 0)
                {
                    foreach (var availablePlatform in ServiceManager.AvailablePlatforms)
                    {
                        if (availablePlatform is AllPlatforms ||
                            availablePlatform is EditorPlatform ||
                            availablePlatform is CurrentBuildTargetPlatform)
                        {
                            continue;
                        }

                        platforms.Add(availablePlatform);
                    }

                    for (var i = 0; i < platforms.Count; i++)
                    {
                        if (ServiceFrameworkPreferences.CurrentPlatformTarget.GetType() == platforms[i].GetType())
                        {
                            platformIndex = i;
                            break;
                        }
                    }
                }

                return platforms;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            configurations = serializedObject.FindProperty(nameof(configurations));

            Debug.Assert(configurations != null);

            var baseType = ThisProfile.GetType().BaseType;
            var genericTypeArgs = baseType?.FindTopmostGenericTypeArguments();
            Debug.Assert(genericTypeArgs != null);
            ServiceConstraint = genericTypeArgs[0];
            Debug.Assert(ServiceConstraint != null);

            configurationList = new ReorderableList(serializedObject, configurations, true, false, true, true);
            configListHeightFlags = new List<Tuple<bool, bool>>(configurations.arraySize);

            for (int i = 0; i < configurations.arraySize; i++)
            {
                configListHeightFlags.Add(new Tuple<bool, bool>(true, false));
            }

            configurationList.drawElementCallback += DrawConfigurationOptionElement;
            configurationList.onAddCallback += OnConfigurationOptionAdded;
            configurationList.onRemoveCallback += OnConfigurationOptionRemoved;
            configurationList.elementHeightCallback += ElementHeightCallback;
            configurationList.onReorderCallback += OnElementReorderedCallback;

            platforms.Clear();
        }

        public override void OnInspectorGUI()
        {
            RenderHeader();
            EditorGUILayout.Space();
            RenderConfigurationOptions();
        }

        protected virtual void RenderConfigurationOptions(bool forceExpanded = false)
        {
            if (forceExpanded)
            {
                configurations.isExpanded = true;
            }
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"{ThisProfile.name.ToProperCase()} Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                //Interesting side effect of letting the Inspector show all children, you get a debug view of all properties
                if (!ServiceFrameworkPreferences.ShowInspectorDebugView)
                {
                    enterChildren = false;
                }

                if (!excludedProperties.Contains(iterator.name))
                {
                    EditorGUILayout.PropertyField(iterator, true);
                }
            }

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }

            DrawServiceModulePropertyDrawer();
        }

        protected void DrawServiceModulePropertyDrawer()
        {
            EditorGUILayout.Space();
            ServiceFrameworkInspectorUtility.HorizontalLine(Color.gray);
            EditorGUILayout.Space();

            configurations.isExpanded = EditorGUILayoutExtensions.FoldoutWithBoldLabel(configurations.isExpanded, new GUIContent($"{ServiceConstraint.Name} Configuration Options"));

            if (configurations.isExpanded)
            {
                serializedObject.Update();
                EditorGUILayout.Space();
                configurationList.DoLayoutList();

                if (configurations == null || configurations.arraySize == 0)
                {
                    EditorGUILayout.HelpBox($"Register a new {ServiceConstraint.Name} Configuration", MessageType.Warning);
                }

                serializedObject.ApplyModifiedProperties();
            }

            TypeReferencePropertyDrawer.CreateNewTypeOverride = null;
        }

        private float ElementHeightCallback(int index)
        {
            if (configListHeightFlags.Count == 0)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            var (isExpanded, hasProfile) = configListHeightFlags[index];
            var modifier = isExpanded
                ? hasProfile
                    ? 5.5f
                    : 4f
                : 1.5f;
            return EditorGUIUtility.singleLineHeight * modifier;
        }

        private void DrawConfigurationOptionElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (isFocused)
            {
                currentlySelectedConfigurationOption = index;
            }

            serializedObject.Update();

            var configurationProperty = configurations.GetArrayElementAtIndex(index);
            SerializedProperty priority;
            SerializedProperty instancedType;
            SerializedProperty platformEntries;
            SerializedProperty runtimePlatforms;
            SerializedProperty profile;

            var nameProperty = configurationProperty.FindPropertyRelative(nameof(name));
            priority = configurationProperty.FindPropertyRelative(nameof(priority));
            instancedType = configurationProperty.FindPropertyRelative(nameof(instancedType));
            var systemTypeReference = new SystemType(instancedType);
            platformEntries = configurationProperty.FindPropertyRelative(nameof(platformEntries));
            runtimePlatforms = platformEntries.FindPropertyRelative(nameof(runtimePlatforms));
            profile = configurationProperty.FindPropertyRelative(nameof(profile));

            var hasProfile = false;
            Type profileType = null;

            if (systemTypeReference.Type != null)
            {
                if (nameProperty.stringValue.Contains("New Configuration"))
                {
                    nameProperty.stringValue = systemTypeReference.Type.Name.ToProperCase();
                }

                var constructors = systemTypeReference.Type.GetConstructors();

                foreach (var constructorInfo in constructors)
                {
                    var parameters = constructorInfo.GetParameters();

                    foreach (var parameterInfo in parameters)
                    {
                        if (parameterInfo.ParameterType.IsAbstract) { continue; }

                        if (parameterInfo.ParameterType.IsSubclassOf(typeof(BaseProfile)))
                        {
                            profileType = parameterInfo.ParameterType;
                            break;
                        }
                    }

                    if (profileType != null)
                    {
                        hasProfile = true;
                        break;
                    }
                }
            }

            priority.intValue = index - 1;

            var lastMode = EditorGUIUtility.wideMode;
            var prevLabelWidth = EditorGUIUtility.labelWidth;

            EditorGUIUtility.labelWidth = prevLabelWidth - 18f;
            EditorGUIUtility.wideMode = true;

            var halfFieldHeight = EditorGUIUtility.singleLineHeight * 0.25f;

            var rectX = rect.x + 12;
            var rectWidth = rect.width - 12;
            var elementX = rectX + 6;
            var elementWidth = rectWidth - 6;
            var dropdownRect = new Rect(rectX, rect.y + halfFieldHeight, rectWidth, EditorGUIUtility.singleLineHeight);
            var labelRect = new Rect(elementX, rect.y + halfFieldHeight, elementWidth, EditorGUIUtility.singleLineHeight);
            var typeRect = new Rect(elementX, rect.y + halfFieldHeight * 6, elementWidth, EditorGUIUtility.singleLineHeight);
            var profileRect = new Rect(elementX, rect.y + halfFieldHeight * 11, elementWidth, EditorGUIUtility.singleLineHeight);
            var runtimeRect = new Rect(elementX, rect.y + halfFieldHeight * (hasProfile ? 16 : 11), elementWidth, EditorGUIUtility.singleLineHeight);

            EditorGUI.BeginChangeCheck();

            if (configurationProperty.isExpanded)
            {
                EditorGUI.PropertyField(labelRect, nameProperty, nameContent);
                configurationProperty.isExpanded = EditorGUI.Foldout(dropdownRect, configurationProperty.isExpanded, GUIContent.none, true);

                if (!configurationProperty.isExpanded)
                {
                    GUI.FocusControl(null);
                }
            }
            else
            {
                configurationProperty.isExpanded = EditorGUI.Foldout(dropdownRect, configurationProperty.isExpanded, GUIContent.none, false) ||
                                                   hasProfile && profile.objectReferenceValue == null;

                if (!profile.isExpanded)
                {
                    if (hasProfile)
                    {
                        if (GUI.Button(labelRect, nameProperty.stringValue, ButtonGuiStyle) &&
                            profile.objectReferenceValue != null)
                        {
                            var profileInstance = profile.objectReferenceValue as BaseProfile;

                            Debug.Assert(profileInstance != null);

                            if (profileInstance.ParentProfile.IsNull() ||
                                profileInstance.ParentProfile != ThisProfile)
                            {
                                profileInstance.ParentProfile = ThisProfile;
                            }

                            Selection.activeObject = profileInstance;
                        }
                    }
                    else
                    {
                        GUI.Label(labelRect, nameProperty.stringValue, ButtonGuiStyle);
                    }
                }
            }

            if (configurationProperty.isExpanded)
            {
                TypeReferencePropertyDrawer.FilterConstraintOverride = type =>
                {
                    var isValid = !type.IsAbstract &&
                                  type.GetInterfaces().Any(interfaceType => interfaceType == ServiceConstraint);

                    return isValid;
                };
                TypeReferencePropertyDrawer.CreateNewTypeOverride = ServiceConstraint;

                ICollection<Type> GetExcludedTypeCollection() => ServiceFrameworkPreferences.ExcludedTemplateServices;
                TypeReferencePropertyDrawer.ExcludedTypeCollectionGetter = GetExcludedTypeCollection;

                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(typeRect, instancedType, instancedTypeContent);
                systemTypeReference = new SystemType(instancedType);

                if (EditorGUI.EndChangeCheck())
                {
                    profile.objectReferenceValue = null;

                    if (systemTypeReference.Type == null)
                    {
                        nameProperty.stringValue = string.Empty;
                    }
                    else
                    {
                        nameProperty.stringValue = systemTypeReference.Type.Name.ToProperCase();
                    }
                }

                EditorGUI.PropertyField(runtimeRect, platformEntries);
                runtimePlatforms = platformEntries.FindPropertyRelative(nameof(runtimePlatforms));

                if (hasProfile)
                {
                    ProfilePropertyDrawer.ProfileTypeOverride = profileType;
                    EditorGUI.PropertyField(profileRect, profile, profileContent);
                }

                if (profile.objectReferenceValue != null)
                {
                    var renderedProfile = profile.objectReferenceValue as BaseProfile;
                    Debug.Assert(renderedProfile != null);

                    if (renderedProfile.ParentProfile.IsNull() ||
                        renderedProfile.ParentProfile != ThisProfile)
                    {
                        renderedProfile.ParentProfile = ThisProfile;
                    }
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                if (ServiceManager.IsActiveAndInitialized &&
                    runtimePlatforms.arraySize > 0 &&
                    systemTypeReference.Type != null)
                {
                    ServiceManager.Instance.ResetProfile(ServiceManager.Instance.ActiveProfile);
                }
            }

            EditorGUIUtility.wideMode = lastMode;
            EditorGUIUtility.labelWidth = prevLabelWidth;
            configListHeightFlags[index] = new Tuple<bool, bool>(configurationProperty.isExpanded, hasProfile);
        }

        private void OnConfigurationOptionAdded(ReorderableList list)
        {
            configurations.arraySize += 1;
            var index = configurations.arraySize - 1;

            var configuration = new ConfigurationProperty(configurations.GetArrayElementAtIndex(index), true)
            {
                IsExpanded = true,
                Name = $"New Configuration {index}",
                InstancedType = null,
                Priority = (uint)index,
                Profile = null
            };

            configuration.ApplyModifiedProperties();
            configListHeightFlags.Add(new Tuple<bool, bool>(true, false));
            serializedObject.ApplyModifiedProperties();
        }

        private void OnConfigurationOptionRemoved(ReorderableList list)
        {
            if (currentlySelectedConfigurationOption >= 0)
            {
                configurations.DeleteArrayElementAtIndex(currentlySelectedConfigurationOption);
            }

            configListHeightFlags.RemoveAt(0);

            serializedObject.ApplyModifiedProperties();

            if (ServiceManager.IsActiveAndInitialized)
            {
                EditorApplication.delayCall += () => ServiceManager.Instance.ResetProfile(ServiceManager.Instance.ActiveProfile);
            }
        }

        private void OnElementReorderedCallback(ReorderableList list)
        {
            if (ServiceManager.IsActiveAndInitialized)
            {
                EditorApplication.delayCall += () => ServiceManager.Instance.ResetProfile(ServiceManager.Instance.ActiveProfile);
            }
        }

        internal void RenderSystemFields()
        {
            EditorGUILayout.LabelField($"Platform Target Selection", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Changing the 'Platform Target Selection' dropdown will automatically change the platform the project is currently targetting.  Automating the 'Switch Targets' selection in the Build Settings Window.", EditorStyles.helpBox);

            var currentPlatform = ServiceFrameworkPreferences.CurrentPlatformTarget;

            for (var i = 0; i < Platforms.Count; i++)
            {
                if (currentPlatform.GetType() == Platforms[i].GetType())
                {
                    platformIndex = i;
                    break;
                }
            }

            EditorGUI.BeginChangeCheck();
            var prevPlatformIndex = platformIndex;
            platformIndex = EditorGUILayout.Popup("Platform Target", platformIndex, Platforms.Select(p => p.Name).ToArray());
            EditorGUILayout.Space();
            ServiceFrameworkInspectorUtility.HorizontalLine(Color.gray);

            if (EditorGUI.EndChangeCheck())
            {
                for (int i = 0; i < Platforms.Count; i++)
                {
                    if (i == platformIndex)
                    {
                        var platform = Platforms[i];

                        var buildTarget = platform.ValidBuildTargets[0]; // For now just get the highest priority one.

                        if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildPipeline.GetBuildTargetGroup(buildTarget), buildTarget))
                        {
                            platformIndex = prevPlatformIndex;
                            Debug.LogWarning($"Failed to switch {platform.Name} active build target to {buildTarget}");
                        }
                        else
                        {
                            ServiceFrameworkPreferences.CurrentPlatformTarget = platform;
                        }
                    }
                }
            }

            RenderConfigurationOptions(true);

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            serializedObject.ApplyModifiedProperties();
        }
    }
}