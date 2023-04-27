// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Editor.Extensions;
using RealityCollective.Extensions;
using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Editor.Utilities;
using System;
using UnityEditor;
using UnityEngine;

namespace RealityCollective.ServiceFramework.Editor.Profiles
{
    /// <summary>
    /// Base class for all <see cref="BaseProfile"/> Inspectors to inherit from.
    /// </summary>
    [CustomEditor(typeof(BaseProfile), true, isFallback = true)]
    public class BaseProfileInspector : UnityEditor.Editor
    {
        protected static readonly string DefaultGuidString = default(Guid).ToString("N");

        private static SerializedObject targetProfile;
        private static BaseProfile currentlySelectedProfile;
        private static BaseProfile profileSource;

        /// <summary>
        /// The <see cref="Guid"/> string representation for this profile asset.
        /// </summary>
        protected string ThisProfileGuidString { get; private set; }

        /// <summary>
        /// The instanced reference of the currently rendered <see cref="BaseProfile"/>.
        /// </summary>
        protected BaseProfile ThisProfile { get; private set; }

        private bool isOverrideHeader = false;

        protected virtual void OnEnable()
        {
            targetProfile = serializedObject;

            currentlySelectedProfile = target as BaseProfile;
            Debug.Assert(!currentlySelectedProfile.IsNull());
            ThisProfile = currentlySelectedProfile;
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(ThisProfile, out var guidHex, out long _);
            ThisProfileGuidString = guidHex;
        }

        public override void OnInspectorGUI()
        {
            RenderHeader();
            DrawDefaultInspector();
        }

        protected void RenderHeader(string infoBoxText = "", Texture2D image = null)
        {
            if (!image.IsNull() ||
                !string.IsNullOrWhiteSpace(infoBoxText))
            {
                isOverrideHeader = true;
            }
            else
            {
                if (isOverrideHeader) { return; }
            }

            if (image.IsNull())
            {
                ServiceFrameworkInspectorUtility.RenderLogo();
            }
            else
            {
                ServiceFrameworkInspectorUtility.RenderInspectorHeader(image);
            }

            if (!ThisProfile.ParentProfile.IsNull() &&
                GUILayout.Button("Back to parent profile"))
            {
                Selection.activeObject = ThisProfile.ParentProfile;
            }

            EditorGUILayout.Space();
            ServiceFrameworkInspectorUtility.HorizontalLine(Color.gray);

            if (isOverrideHeader)
            {
                EditorGUILayout.HelpBox(infoBoxText, MessageType.Info);
            }
        }

        [MenuItem("CONTEXT/BaseProfile/Create Clone from Profile Values", false, 0)]
        protected static void CreateCloneProfile()
        {
            profileSource = currentlySelectedProfile;
            var newProfile = CreateInstance(currentlySelectedProfile.GetType().ToString());
            currentlySelectedProfile = newProfile.CreateAsset() as BaseProfile;
            Debug.Assert(!currentlySelectedProfile.IsNull());

            //await new WaitUntil(() => profileSource != currentlySelectedProfile);

            Selection.activeObject = null;
            PasteProfileValues();
            Selection.activeObject = currentlySelectedProfile;
            EditorGUIUtility.PingObject(currentlySelectedProfile);
        }

        [MenuItem("CONTEXT/BaseProfile/Copy Profile Values", false, 1)]
        private static void CopyProfileValues()
        {
            profileSource = currentlySelectedProfile;
        }

        [MenuItem("CONTEXT/BaseProfile/Paste Profile Values", true)]
        private static bool PasteProfileValuesValidation()
        {
            return !currentlySelectedProfile.IsNull() &&
                   targetProfile != null &&
                   !profileSource.IsNull() &&
                   currentlySelectedProfile.GetType() == profileSource.GetType();
        }

        [MenuItem("CONTEXT/BaseProfile/Paste Profile Values", false, 2)]
        private static void PasteProfileValues()
        {
            currentlySelectedProfile.CopySerializedValues(profileSource);
        }
    }
}