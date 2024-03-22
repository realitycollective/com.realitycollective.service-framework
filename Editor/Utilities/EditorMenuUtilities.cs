// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Extensions;
using UnityEditor;
using UnityEngine;

namespace RealityCollective.ServiceFramework.Editor.Utilities
{
    public static class EditorMenuUtilities
    {
        /// <summary>
        /// Simple scene helper to create the beginnings of a scene, creating the scene root and a floor.
        /// </summary>
        [MenuItem(ServiceFrameworkPreferences.Service_Framework_Editor_Menu_Keyword + "/Add to Scene", false, 1)]
        public static void CreateServiceManagerInstance()
        {
#if UNITY_2023_1_OR_NEWER
            var existingCheck = GameObject.FindFirstObjectByType<ServiceManagerInstance>();
#else
            var existingCheck = GameObject.FindObjectOfType<ServiceManagerInstance>();
#endif
            GameObject serviceManagerGO;
            if (existingCheck.IsNull())
            {
                serviceManagerGO = new GameObject("GlobalServiceManager");
                serviceManagerGO.AddComponent<ServiceManagerInstance>();
            }
            else
            {
                serviceManagerGO = existingCheck.gameObject;
            }
            Selection.activeObject = serviceManagerGO;
            EditorGUIUtility.PingObject(serviceManagerGO);
        }
    }
}