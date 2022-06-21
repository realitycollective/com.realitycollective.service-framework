// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace RealityCollective.ServiceFramework
{
    internal static class ServiceFrameworkStatus
    {
        /// <summary>
        /// Editor Prefs key name to store the current Unity editor play state
        /// </summary>
        internal const string UnityEditorRunStateKey = "RealityCollective_UnityEditorRunState";

        /// <summary>
        /// Internal editor flag to capture when the Unity Editor is entering Run mode
        /// </summary>
        internal static bool EditorPlayModeStateChanging = false;
    }
}