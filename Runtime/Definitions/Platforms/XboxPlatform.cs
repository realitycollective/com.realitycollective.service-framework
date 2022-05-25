// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace RealityCollective.ServiceFramework.Definitions.Platforms
{
    /// <summary>
    ///  Used by the Service Framework to signal that the feature is available on the iOS platform.
    /// </summary>
    [System.Runtime.InteropServices.Guid("01A560DF-D066-4E49-B3C5-77FF71E1B53E")]
    public class XboxPlatform : BasePlatform
    {
        /// <inheritdoc />
        public override bool IsAvailable
        {
            get
            {
#if PLATFORM_XBOXONE || UNITY_XBOXONE
                return !UnityEngine.Application.isEditor;
#else
                return false;
#endif // PLATFORM_XBOXONE || UNITY_XBOXONE
            }
        }

#if UNITY_EDITOR

        /// <inheritdoc />
        public override UnityEditor.BuildTarget[] ValidBuildTargets { get; } =
        {
            UnityEditor.BuildTarget.XboxOne
        };

#endif // UNITY_EDITOR
    }
}