// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Editor.Extensions;
using RealityCollective.Extensions;
using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Editor.Profiles;
using RealityCollective.ServiceFramework.Editor.Utilities;
using RealityCollective.ServiceFramework.Services;
using UnityEditor;
using UnityEngine;

namespace RealityCollective.ServiceFramework.Editor
{
    [CustomEditor(typeof(ServiceManagerInstance))]
    public class ServiceManagerInspector : UnityEditor.Editor
    {
        private const string ObjectSelectorClosed = "ObjectSelectorClosed";
        private const string ObjectSelectorUpdated = "ObjectSelectorUpdated";

        private SerializedProperty serviceProvidersProfile;

        private int currentPickerWindow = -1;

        private UnityEditor.Editor profileInspector;

        private void Awake()
        {
            if (target.name != nameof(ServiceManagerInstance))
            {
                target.name = nameof(ServiceManagerInstance);
            }
        }

        private void OnEnable()
        {
            serviceProvidersProfile = serializedObject.FindProperty(nameof(serviceProvidersProfile));
            currentPickerWindow = -1;
            profileInspector.Destroy();
        }

        private void OnDisable()
        {
            profileInspector.Destroy();
        }

        private void OnDestroy()
        {
            profileInspector.Destroy();
        }

        public override void OnInspectorGUI()
        {
            ServiceFrameworkInspectorUtility.RenderLogo();

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField(new GUIContent("Service Framework Configuration Profile", "This profile is the configuration for the Service Framework."));

            EditorGUILayout.PropertyField(serviceProvidersProfile, GUIContent.none);

            if (serviceProvidersProfile.objectReferenceValue.IsNull())
            {
                if (GUILayout.Button("Create a new configuration profile"))
                {
                    var rootProfile = CreateInstance<ServiceProvidersProfile>().GetOrCreateAsset();
                    serviceProvidersProfile.objectReferenceValue = rootProfile;
                }
            }

            var changed = EditorGUI.EndChangeCheck();
            var commandName = Event.current.commandName;
            var rootProfiles = ScriptableObjectExtensions.GetAllInstances<ServiceProvidersProfile>();

            if (rootProfiles.Length == 0 &&
                currentPickerWindow == -1)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("No service manager configuration profiles found in project.\n\nCreate a new one using the '+' button above.", MessageType.Warning);
            }
            // If there is no profile configured, try to automatically find one, if only one found use it, else put up a prompt to select one
            else if (serviceProvidersProfile.objectReferenceValue.IsNull() &&
                currentPickerWindow == -1)
            {
                switch (rootProfiles.Length)
                {
                    case 1:
                        var rootProfilePath = AssetDatabase.GetAssetPath(rootProfiles[0]);

                        EditorApplication.delayCall += () =>
                        {
                            changed = true;
                            var rootProfile = AssetDatabase.LoadAssetAtPath<ServiceProvidersProfile>(rootProfilePath);
                            Debug.Assert(rootProfile != null);
                            serviceProvidersProfile.objectReferenceValue = rootProfile;
                            serializedObject.ApplyModifiedProperties();
                            ServiceManager.Instance?.ResetProfile(rootProfile);
                        };
                        break;
                    default:
                        try
                        {
                            currentPickerWindow = GUIUtility.GetControlID(FocusType.Passive);
                            EditorGUIUtility.ShowObjectPicker<ServiceProvidersProfile>(null, false, string.Empty, currentPickerWindow);
                        }
                        catch { }

                        break;
                }
            }

            if (EditorGUIUtility.GetObjectPickerControlID() == currentPickerWindow)
            {
                switch (commandName)
                {
                    case ObjectSelectorUpdated:
                        serviceProvidersProfile.objectReferenceValue = EditorGUIUtility.GetObjectPickerObject();
                        changed = true;
                        break;
                    case ObjectSelectorClosed:
                        serviceProvidersProfile.objectReferenceValue = EditorGUIUtility.GetObjectPickerObject();
                        currentPickerWindow = -1;
                        changed = true;
                        break;
                }
            }

            if (changed)
            {
                profileInspector.Destroy();
            }

            if (serviceProvidersProfile.objectReferenceValue.IsNotNull())
            {
                var rootProfile = serviceProvidersProfile.objectReferenceValue as ServiceProvidersProfile;

                if (profileInspector.IsNull())
                {
                    profileInspector = CreateEditor(rootProfile);
                }

                if (profileInspector is ServiceProfileInspector rootProfileInspector)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    var rect = new Rect(GUILayoutUtility.GetLastRect()) { height = 0.75f };
                    EditorGUI.DrawRect(rect, Color.gray);
                    EditorGUILayout.Space();

                    rootProfileInspector.RenderSystemFields();
                }
            }

            serializedObject.ApplyModifiedProperties();

            if (changed)
            {
                EditorApplication.delayCall += () => ServiceManager.Instance?.ResetProfile((ServiceProvidersProfile)serviceProvidersProfile.objectReferenceValue);
            }
        }
    }
}
