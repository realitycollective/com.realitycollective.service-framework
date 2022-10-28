// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace RealityCollective.ServiceFramework.Definitions.Platforms
{
    /// <summary>
    ///  Used by the Service Framework to signal that the feature is available on the iOS platform.
    /// </summary>
    [System.Runtime.InteropServices.Guid("36F9AC5D-6A78-444D-BA8E-130862A043AB")]
    public class PS4Platform : BasePlatform
    {
        /// <inheritdoc />
        public override bool IsAvailable
        {
            get
            {
#if PLATFORM_PS4 || UNITY_PS4
                return !UnityEngine.Application.isEditor;
#else
                return false;
#endif // PLATFORM_PS4 || UNITY_PS4
            }
        }

#if UNITY_EDITOR

        /// <inheritdoc />
        public override UnityEditor.BuildTarget[] ValidBuildTargets { get; } =
        {
            UnityEditor.BuildTarget.PS4
        };

#endif // UNITY_EDITOR
    }
}