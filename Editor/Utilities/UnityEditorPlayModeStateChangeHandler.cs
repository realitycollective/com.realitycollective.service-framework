// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEditor;

namespace RealityCollective.ServiceFramework.Editor.Utilities
{
    /// <summary>
    /// Editor helper class to detect when the Unity Editor enters play mode, to assist skipping Editor "Validate" functions on Editor start
    /// </summary>
    [InitializeOnLoad]
    public static class UnityEditorPlayModeStateChangeHandler
    {
        static UnityEditorPlayModeStateChangeHandler()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange playModeState)
        {
            if (EditorApplication.isPlaying ||
                EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorPrefs.SetBool(ServiceFrameworkStatus.UnityEditorRunStateKey, true);
            }
            else
            {
                EditorPrefs.SetBool(ServiceFrameworkStatus.UnityEditorRunStateKey, false);
            }
        }
    }
}