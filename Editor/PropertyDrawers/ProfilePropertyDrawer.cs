// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Extensions;
using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Editor.Utilities;
using RealityCollective.ServiceFramework.Services;
using System;
using UnityEditor;
using UnityEngine;

namespace RealityCollective.ServiceFramework.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(BaseProfile), true)]
    public class ProfilePropertyDrawer : PropertyDrawer
    {
        private const int BUTTON_PADDING = 4;

        private static readonly GUIContent NewProfileContent = new GUIContent("+", "Create New Profile");
        private static readonly GUIContent CloneProfileContent = new GUIContent("Clone", "Replace with a copy of the default profile.");

        public static bool DrawCloneButtons { get; set; } = true;

        public static Type ProfileTypeOverride { get; set; } = null;

        public static BaseProfile ParentProfileOverride { get; set; } = null;

        private BaseProfile parent = null;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            BaseProfile profile = null;

            if (parent.IsNull())
            {
                parent = ParentProfileOverride;

                if (parent.IsNull() && Selection.activeObject.IsNotNull())
                {
                    if (Selection.activeObject.name.Equals(nameof(ServiceManagerInstance)))
                    {
                        if (ServiceManager.Instance != null)
                        {
                            parent = ServiceManager.Instance.ActiveProfile;
                        }
                    }
                    else
                    {
                        parent = Selection.activeObject as BaseProfile;
                    }
                }

                ParentProfileOverride = null;
            }

            if (!property.objectReferenceValue.IsNull())
            {
                profile = property.objectReferenceValue as BaseProfile;
            }

            if (!profile.IsNull())
            {
                if (profile is ServiceProvidersProfile &&
                    !parent.IsNull())
                {
                    profile.ParentProfile = null;
                }
            }

            var propertyLabel = EditorGUI.BeginProperty(position, label, property);
            var profileType = ProfileTypeOverride ?? fieldInfo.FieldType;
            var hasSelection = property.objectReferenceValue != null;
            var buttonWidth = hasSelection ? 48f : 20f;
            var objectRect = position;

            if (DrawCloneButtons)
            {
                objectRect.width -= buttonWidth + BUTTON_PADDING;
            }

            EditorGUI.BeginChangeCheck();

            BaseProfile selectedProfile = null;
            try
            {
                selectedProfile = EditorGUI.ObjectField(objectRect, propertyLabel, profile, profileType, false) as BaseProfile;
            }
            catch { }

            if (EditorGUI.EndChangeCheck())
            {
                property.objectReferenceValue = selectedProfile;
                property.serializedObject.ApplyModifiedProperties();

                if (!(selectedProfile is null) &&
                    !(selectedProfile is ServiceProvidersProfile))
                {
                    Debug.Assert(!parent.IsNull(), $"Failed to find a valid parent profile for {selectedProfile.name}");
                    selectedProfile.ParentProfile = parent;
                }
            }

            if (DrawCloneButtons)
            {
                hasSelection = !selectedProfile.IsNull();
                var buttonContent = hasSelection ? CloneProfileContent : NewProfileContent;
                var buttonRect = new Rect(objectRect.xMax + BUTTON_PADDING, position.y, buttonWidth, position.height);

                if (GUI.Button(buttonRect, buttonContent, EditorStyles.miniButton))
                {
                    selectedProfile = parent.CreateNewProfileInstance(property, profileType, hasSelection);
                }
            }

            if (!(selectedProfile is null) &&
                !(selectedProfile is ServiceProvidersProfile))
            {
                if (selectedProfile.ParentProfile.IsNull() ||
                    selectedProfile.ParentProfile != parent)
                {
                    if (parent != null &&
                        parent != selectedProfile)
                    {
                        selectedProfile.ParentProfile = parent;
                    }
                }

                Debug.Assert(selectedProfile.ParentProfile != selectedProfile, $"{selectedProfile} cannot be a parent of itself!");
            }

            DrawCloneButtons = true;
            ProfileTypeOverride = null;
            EditorGUI.EndProperty();
        }
    }
}
