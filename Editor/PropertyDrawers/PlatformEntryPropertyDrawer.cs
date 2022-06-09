// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Extensions;
using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Definitions.Platforms;
using RealityCollective.ServiceFramework.Services;
using System;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace RealityCollective.ServiceFramework.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(RuntimePlatformEntry))]
    public class PlatformEntryPropertyDrawer : PropertyDrawer
    {
        private const string Nothing = "Nothing";
        private const string Everything = "Everything";
        private const string Platform = "Platform";
        private const string EditorAnd = "Editor &";
        private const string TypeReferenceUpdated = "TypeReferenceUpdated";

        private static readonly GUIContent TempContent = new GUIContent();
        private static readonly GUIContent EditorContent = new GUIContent("Editor");
        private static readonly GUIContent NothingContent = new GUIContent(Nothing);
        private static readonly GUIContent EverythingContent = new GUIContent(Everything);
        private static readonly GUIContent RuntimePlatformContent = new GUIContent("Runtime Platforms", "Which runtime platforms will this service be activated on?");
        private static readonly GUIContent EditorBuildTargetContent = new GUIContent($"{EditorAnd}Build Target");

        private static readonly int ControlHint = typeof(PlatformEntryPropertyDrawer).GetHashCode();

        private static readonly Type AllPlatformsType = typeof(AllPlatforms);
        private static readonly Type EditorPlatformType = typeof(EditorPlatform);
        private static readonly Type CurrentBuildTargetType = typeof(CurrentBuildTargetPlatform);

        private static readonly Guid AllPlatformsGuid = AllPlatformsType.GUID;
        private static readonly Guid EditorPlatformGuid = EditorPlatformType.GUID;
        private static readonly Guid CurrentEditorBuildTargetGuid = CurrentBuildTargetType.GUID;

        private static int selectionControlId;
        private static int arraySize = 0;

        private class SerializedTypeProperty
        {
            private readonly SerializedProperty reference;

            public Type ReferenceType
            {
                get
                {
                    TypeExtensions.TryResolveType(reference.stringValue, out Type referenceType);
                    return referenceType;
                }
            }

            public Guid TypeReference
            {
                get => Guid.TryParse(reference.stringValue, out var guid) ? guid : Guid.Empty;
                set => reference.stringValue = value.ToString();
            }

            public SerializedTypeProperty(SerializedProperty property)
            {
                reference = property.FindPropertyRelative(nameof(reference));
            }

            public void ApplyModifiedProperties()
            {
                reference.serializedObject.ApplyModifiedProperties();
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, RuntimePlatformContent);

            var runtimePlatformsProperty = property.FindPropertyRelative("runtimePlatforms");

            DrawTypeSelectionControl(position, runtimePlatformsProperty);
        }

        private static void DrawTypeSelectionControl(Rect position, SerializedProperty runtimePlatformsProperty)
        {
            var triggerDropDown = false;
            var controlId = GUIUtility.GetControlID(ControlHint, FocusType.Keyboard, position);

            switch (Event.current.GetTypeForControl(controlId))
            {
                case EventType.ExecuteCommand:
                    if (Event.current.commandName == TypeReferenceUpdated &&
                        selectionControlId == controlId)
                    {
                        if (runtimePlatformsProperty.arraySize != arraySize)
                        {
                            GUI.changed = true;
                        }

                        arraySize = 0;
                        selectionControlId = 0;
                    }

                    break;

                case EventType.MouseDown:
                    if (GUI.enabled && position.Contains(Event.current.mousePosition))
                    {
                        GUIUtility.keyboardControl = controlId;
                        triggerDropDown = true;
                        Event.current.Use();
                    }

                    break;

                case EventType.KeyDown:
                    if (GUI.enabled && GUIUtility.keyboardControl == controlId)
                    {
                        if (Event.current.keyCode == KeyCode.Return ||
                            Event.current.keyCode == KeyCode.Space)
                        {
                            triggerDropDown = true;
                            Event.current.Use();
                        }
                    }

                    break;

                case EventType.Repaint:
                    TempContent.text = GetDropdownContentText();
                    EditorStyles.popup.Draw(position, TempContent, controlId);
                    break;
            }

            if (triggerDropDown)
            {
                arraySize = runtimePlatformsProperty.arraySize;
                selectionControlId = controlId;

                var menu = new GenericMenu();

                var editorIsActive = IsPlatformActive(EditorPlatformType);
                var isAllPlatformsActive = IsPlatformActive(AllPlatformsType);
                var editorBuildTargetIsActive = IsPlatformActive(CurrentBuildTargetType);

                if (editorIsActive || editorBuildTargetIsActive)
                {
                    Debug.Assert(editorIsActive != editorBuildTargetIsActive);
                }

                menu.AddItem(NothingContent, arraySize == 0, OnNothingSelected, null);
                menu.AddItem(EverythingContent, isAllPlatformsActive, OnEverythingSelected, null);

                menu.AddItem(EditorContent, isAllPlatformsActive || editorIsActive, OnEditorSelected, null);
                menu.AddSeparator(string.Empty);

                for (var i = 0; i < ServiceManager.AvailablePlatforms.Count; i++)
                {
                    var platform = ServiceManager.AvailablePlatforms[i];
                    var platformType = platform.GetType();

                    if (platformType == AllPlatformsType ||
                        platformType == EditorPlatformType ||
                        platformType == CurrentBuildTargetType)
                    {
                        continue;
                    }

                    menu.AddItem(new GUIContent(platformType.Name.Replace(Platform, string.Empty).ToProperCase()), IsPlatformActive(platformType) || isAllPlatformsActive, OnSelectedTypeName, platformType);
                }

                menu.DropDown(position);
            }

            bool IsPlatformActive(Type platformType)
            {
                var isActive = false;

                for (int i = 0; i < runtimePlatformsProperty.arraySize; i++)
                {
                    var serializedType = new SerializedTypeProperty(runtimePlatformsProperty.GetArrayElementAtIndex(i));

                    // Clean up any broken references
                    if (serializedType.ReferenceType == null)
                    {
                        Debug.LogError($"Failed to resolve {serializedType.TypeReference}! Removing from runtime platform entry...");
                        runtimePlatformsProperty.DeleteArrayElementAtIndex(i);
                        runtimePlatformsProperty.serializedObject.ApplyModifiedProperties();
                        continue;
                    }

                    if (platformType == serializedType.ReferenceType)
                    {
                        isActive = true;
                    }
                }

                return isActive;
            }

            string GetDropdownContentText()
            {
                if (runtimePlatformsProperty.arraySize == 0)
                {
                    return Nothing;
                }

                if (runtimePlatformsProperty.arraySize == 1)
                {
                    if (IsPlatformActive(AllPlatformsType))
                    {
                        return Everything;
                    }

                    var systemTypeProperty = new SerializedTypeProperty(runtimePlatformsProperty.GetArrayElementAtIndex(0));

                    return systemTypeProperty.ReferenceType != null
                        ? systemTypeProperty.ReferenceType.Name.Replace(Platform, string.Empty).ToProperCase()
                        : Nothing;
                }

                var contentText = "Multiple...";

                if (IsPlatformActive(CurrentBuildTargetType))
                {
                    if (runtimePlatformsProperty.arraySize == 2)
                    {
                        Type type = null;

                        for (int i = 0; i < runtimePlatformsProperty.arraySize; i++)
                        {
                            var systemTypeProperty = new SerializedTypeProperty(runtimePlatformsProperty.GetArrayElementAtIndex(i));

                            if (systemTypeProperty.ReferenceType != CurrentBuildTargetType)
                            {
                                type = systemTypeProperty.ReferenceType;
                                break;
                            }
                        }

                        Debug.Assert(type != null, "Failed to resolve platform type");
                        contentText = type.Name.Replace(Platform, string.Empty).ToProperCase();
                        return $"{EditorAnd} {contentText}";
                    }

                    contentText = $"{EditorAnd} {contentText}";
                }

                return contentText;
            }

            void OnNothingSelected(object _)
            {
                runtimePlatformsProperty.ClearArray();
                runtimePlatformsProperty.serializedObject.ApplyModifiedProperties();
                EditorWindow.focusedWindow.SendEvent(EditorGUIUtility.CommandEvent(TypeReferenceUpdated));
            }

            void OnEverythingSelected(object _)
            {
                runtimePlatformsProperty.ClearArray();
                TryAddPlatformReference(AllPlatformsGuid);
                runtimePlatformsProperty.serializedObject.ApplyModifiedProperties();
                EditorWindow.focusedWindow.SendEvent(EditorGUIUtility.CommandEvent(TypeReferenceUpdated));
            }

            void OnEditorSelected(object _)
            {
                var isAllPlatformsActive = false;
                var isCurrentBuildTargetPlatformActive = false;

                for (int i = 0; i < runtimePlatformsProperty.arraySize; i++)
                {
                    var typeProperty = new SerializedTypeProperty(runtimePlatformsProperty.GetArrayElementAtIndex(i));

                    if (typeProperty.ReferenceType == CurrentBuildTargetType)
                    {
                        isCurrentBuildTargetPlatformActive = true;
                    }

                    if (typeProperty.ReferenceType == AllPlatformsType)
                    {
                        isAllPlatformsActive = true;
                    }
                }

                if (isAllPlatformsActive)
                {
                    runtimePlatformsProperty.ClearArray();

                    for (int i = 0; i < ServiceManager.AvailablePlatforms.Count; i++)
                    {
                        var platformType = ServiceManager.AvailablePlatforms[i].GetType();

                        if (platformType == AllPlatformsType ||
                            platformType == EditorPlatformType ||
                            platformType == CurrentBuildTargetType)
                        {
                            continue;
                        }

                        TryAddPlatformReference(platformType.GUID);
                    }
                }
                else
                {
                    if (isCurrentBuildTargetPlatformActive)
                    {
                        TryRemovePlatformReference(CurrentEditorBuildTargetGuid);

                        if (runtimePlatformsProperty.arraySize == ServiceManager.AvailablePlatforms.Count - 3)
                        {
                            OnEverythingSelected(null);
                        }
                    }
                    else
                    {
                        TryAddPlatformReference(EditorPlatformGuid);
                    }
                }

                EditorWindow.focusedWindow.SendEvent(EditorGUIUtility.CommandEvent(TypeReferenceUpdated));
            }

            void OnSelectedTypeName(object typeRef)
            {
                if (!(typeRef is Type selectedPlatformType)) { return; }

                var selectedTypeGuid = selectedPlatformType.GUID;

                if (selectedTypeGuid == Guid.Empty)
                {
                    Debug.LogError($"{selectedPlatformType.Name} does not implement a required {nameof(GuidAttribute)}");
                    return;
                }

                if (!TryRemovePlatformReference(selectedTypeGuid))
                {
                    if (runtimePlatformsProperty.arraySize == ServiceManager.AvailablePlatforms.Count - 3)
                    {
                        OnEverythingSelected(null);
                    }
                    else
                    {
                        TryAddPlatformReference(selectedTypeGuid);
                    }
                }

                EditorWindow.focusedWindow.SendEvent(EditorGUIUtility.CommandEvent(TypeReferenceUpdated));
            }

            void TryAddPlatformReference(Guid classReference)
            {
                if (TryRemovePlatformReference(EditorPlatformGuid))
                {
                    TryAddPlatformReference(CurrentEditorBuildTargetGuid);
                }

                if (!TypeExtensions.TryResolveType(classReference, out var selectedPlatformType)) { return; }

                for (int i = 0; i < runtimePlatformsProperty.arraySize; i++)
                {
                    var existingSystemTypeProperty = new SerializedTypeProperty(runtimePlatformsProperty.GetArrayElementAtIndex(i));

                    if (existingSystemTypeProperty.ReferenceType == null ||
                        existingSystemTypeProperty.ReferenceType == selectedPlatformType)
                    {
                        return;
                    }
                }

                var index = runtimePlatformsProperty.arraySize;
                runtimePlatformsProperty.serializedObject.ApplyModifiedProperties();
                runtimePlatformsProperty.InsertArrayElementAtIndex(index);
                var systemTypeProperty = new SerializedTypeProperty(runtimePlatformsProperty.GetArrayElementAtIndex(index))
                {
                    TypeReference = classReference
                };
                systemTypeProperty.ApplyModifiedProperties();
                runtimePlatformsProperty.serializedObject.ApplyModifiedProperties();
            }

            bool TryRemovePlatformReference(Guid classReference)
            {
                TypeExtensions.TryResolveType(classReference, out var selectedPlatformType);

                if (IsPlatformActive(AllPlatformsType))
                {
                    runtimePlatformsProperty.ClearArray();

                    for (int j = 0; j < ServiceManager.AvailablePlatforms.Count; j++)
                    {
                        var platformType = ServiceManager.AvailablePlatforms[j].GetType();

                        if (platformType != selectedPlatformType &&
                            platformType != AllPlatformsType &&
                            platformType != EditorPlatformType)
                        {
                            TryAddPlatformReference(platformType.GUID);
                        }
                    }

                    return true;
                }

                for (int i = 0; i < runtimePlatformsProperty.arraySize; i++)
                {
                    var systemTypeProperty = new SerializedTypeProperty(runtimePlatformsProperty.GetArrayElementAtIndex(i));

                    if (systemTypeProperty.ReferenceType == selectedPlatformType)
                    {
                        if (runtimePlatformsProperty.arraySize == 2 &&
                            IsPlatformActive(CurrentBuildTargetType) &&
                            selectedPlatformType != CurrentBuildTargetType)
                        {
                            runtimePlatformsProperty.ClearArray();
                            runtimePlatformsProperty.serializedObject.ApplyModifiedProperties();
                            TryAddPlatformReference(EditorPlatformGuid);
                            return true;
                        }

                        runtimePlatformsProperty.DeleteArrayElementAtIndex(i);
                        runtimePlatformsProperty.serializedObject.ApplyModifiedProperties();
                        return true;
                    }
                }

                return false;
            }
        }
    }
}