// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace RealityCollective.ServiceFramework.Definitions.Platforms
{
    /// <summary>
    /// Used by the <see cref="Services.ServiceManager"/> to identify services that should run on the visionOS platform.
    /// </summary>
    [System.Runtime.InteropServices.Guid("ff382950-9384-41fd-8617-48d0b6573a94")]
    public class VisionOSPlatform : BasePlatform
    {
        /// <inheritdoc />
        public override bool IsAvailable
        {
            get
            {
#if PLATFORM_VISIONOS || UNITY_VISIONOS
                return !UnityEngine.Application.isEditor;
#else
                return false;
#endif // PLATFORM_VISIONOS || UNITY_VISIONOS
            }
        }

#if UNITY_EDITOR && UNITY_2022_3_OR_NEWER

        /// <inheritdoc />
        public override UnityEditor.BuildTarget[] ValidBuildTargets { get; } =
        {
            UnityEditor.BuildTarget.VisionOS
        };

#endif // UNITY_EDITOR
    }
}
