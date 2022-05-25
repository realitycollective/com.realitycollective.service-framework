// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace RealityCollective.ServiceFramework.Definitions.Platforms
{
    /// <summary>
    ///  Used by the Service Framework to signal that the feature is available on the iOS platform.
    /// </summary>
    [System.Runtime.InteropServices.Guid("926F2418-99CC-4A9A-B909-C3C18AEA00CE")]
    public class PS5Platform : BasePlatform
    {
        /// <inheritdoc />
        public override bool IsAvailable
        {
            get
            {
#if PLATFORM_PS5 || UNITY_PS5
                return !UnityEngine.Application.isEditor;
#else
                return false;
#endif // PLATFORM_PS5 || UNITY_PS5
            }
        }

#if UNITY_EDITOR

        /// <inheritdoc />
        public override UnityEditor.BuildTarget[] ValidBuildTargets { get; } =
        {
            UnityEditor.BuildTarget.PS5
        };

#endif // UNITY_EDITOR
    }
}