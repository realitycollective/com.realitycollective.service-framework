// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace RealityCollective.ServiceFramework.Definitions.Platforms
{
    /// <summary>
    /// Used by the Service Framework to signal that the feature is available on the Windows Standalone platform.
    /// </summary>
    [System.Runtime.InteropServices.Guid("5B39043A-BF08-4ECE-81C4-57F945760382")]
    public class WindowsStandalonePlatform : BasePlatform
    {
        /// <inheritdoc />
        public override bool IsAvailable
        {
            get
            {
#if UNITY_STANDALONE_WIN
                return !UnityEngine.Application.isEditor;
#else
                return false;
#endif // UNITY_STANDALONE_WIN
            }
        }

#if UNITY_EDITOR

        /// <inheritdoc />
        public override UnityEditor.BuildTarget[] ValidBuildTargets { get; } =
        {
            UnityEditor.BuildTarget.StandaloneWindows64,
            UnityEditor.BuildTarget.StandaloneWindows
        };

#endif // UNITY_EDITOR
    }
}