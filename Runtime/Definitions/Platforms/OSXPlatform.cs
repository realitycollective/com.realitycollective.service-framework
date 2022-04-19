// Copyright (c) Reality Collective. All rights reserved.

namespace RealityToolkit.ServiceFramework.Definitions.Platforms
{
    /// <summary>
    /// Used by the XRTK to signal that the feature is available on the OSX platform.
    /// </summary>
    [System.Runtime.InteropServices.Guid("A95E05D3-09B8-4772-99F1-BDE8097264B4")]
    public class OSXPlatform : BasePlatform
    {
        /// <inheritdoc />
        public override bool IsAvailable
        {
            get
            {
#if UNITY_STANDALONE_OSX
                return !UnityEngine.Application.isEditor;
#else
                return false;
#endif
            }
        }

#if UNITY_EDITOR

        /// <inheritdoc />
        public override UnityEditor.BuildTarget[] ValidBuildTargets { get; } = { UnityEditor.BuildTarget.StandaloneOSX };

#endif // UNITY_EDITOR
    }
}
