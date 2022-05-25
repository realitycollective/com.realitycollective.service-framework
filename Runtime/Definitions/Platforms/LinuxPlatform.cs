// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace RealityCollective.ServiceFramework.Definitions.Platforms
{
    /// <summary>
    /// Used by the Service Framework to signal that the feature is available on the OSX platform.
    /// </summary>
    [System.Runtime.InteropServices.Guid("47F5A60F-2505-4CFC-B493-977296FCC6F0")]
    public class LinuxPlatform : BasePlatform
    {
        /// <inheritdoc />
        public override bool IsAvailable
        {
            get
            {
#if UNITY_STANDALONE_LINUX
                return !UnityEngine.Application.isEditor;
#else
                return false;
#endif // UNITY_STANDALONE_LINUX
            }
        }

#if UNITY_EDITOR

        /// <inheritdoc />
        public override UnityEditor.BuildTarget[] ValidBuildTargets { get; } =
        {
            UnityEditor.BuildTarget.StandaloneLinux64
        };

#endif // UNITY_EDITOR
    }
}