// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace RealityCollective.ServiceFramework.Definitions.Platforms
{
    /// <summary>
    ///  Used by the Service Framework to signal that the feature is available on the iOS platform.
    /// </summary>
    [System.Runtime.InteropServices.Guid("99428B70-E46A-4A1A-82B3-4E29BAC872BD")]
    public class IOSPlatform : BasePlatform
    {
        /// <inheritdoc />
        public override bool IsAvailable
        {
            get
            {
#if PLATFORM_IOS || UNITY_IOS
                return !UnityEngine.Application.isEditor;
#else
                return false;
#endif // PLATFORM_IOS || UNITY_IOS
            }
        }

#if UNITY_EDITOR

        /// <inheritdoc />
        public override UnityEditor.BuildTarget[] ValidBuildTargets { get; } =
        {
            UnityEditor.BuildTarget.iOS
        };

#endif // UNITY_EDITOR
    }
}