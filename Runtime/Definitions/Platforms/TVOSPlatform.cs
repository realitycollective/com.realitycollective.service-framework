// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace RealityCollective.ServiceFramework.Definitions.Platforms
{
    /// <summary>
    ///  Used by the Service Framework to signal that the feature is available on the iOS platform.
    /// </summary>
    [System.Runtime.InteropServices.Guid("A4D5D90E-8B1A-489F-8030-6FB2CDEB85A9")]
    public class TVOSPlatform : BasePlatform
    {
        /// <inheritdoc />
        public override bool IsAvailable
        {
            get
            {
#if PLATFORM_TVOS || UNITY_TVOS
                return !UnityEngine.Application.isEditor;
#else
                return false;
#endif // PLATFORM_TVOS || UNITY_TVOS
            }
        }

#if UNITY_EDITOR

        /// <inheritdoc />
        public override UnityEditor.BuildTarget[] ValidBuildTargets { get; } =
        {
            UnityEditor.BuildTarget.tvOS
        };

#endif // UNITY_EDITOR
    }
}