// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace RealityCollective.ServiceFramework.Definitions.Platforms
{
    /// <summary>
    /// Used by the Service Framework to signal that the feature is available on the Windows Universal Platform.
    /// </summary>
    [System.Runtime.InteropServices.Guid("16A3125F-31D5-4EC6-A120-8BE889A74D27")]
    public class UniversalWindowsPlatform : BasePlatform
    {
        /// <inheritdoc />
        public override bool IsAvailable
        {
            get
            {
#if WINDOWS_UWP
                return !UnityEngine.Application.isEditor;
#else
                return false;
#endif // WINDOWS_UWP
            }
        }

#if UNITY_EDITOR

        /// <inheritdoc />
        public override UnityEditor.BuildTarget[] ValidBuildTargets { get; } =
        {
            UnityEditor.BuildTarget.WSAPlayer
        };

#endif // UNITY_EDITOR
    }
}