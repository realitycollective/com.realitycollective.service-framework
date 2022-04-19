// Copyright (c) Reality Collective. All rights reserved.

namespace RealityToolkit.ServiceFramework.Definitions.Platforms
{
    /// <summary>
    /// Used by the XRTK to signal that the feature is available on the Android platform.
    /// </summary>
    [System.Runtime.InteropServices.Guid("E0D70D45-A52A-4B03-BCDF-8FE367555516")]
    public class AndroidPlatform : BasePlatform
    {
        /// <inheritdoc />
        public override bool IsAvailable
        {
            get
            {
#if PLATFORM_ANDROID
                return !UnityEngine.Application.isEditor;
#else
                return false;
#endif
            }
        }

#if UNITY_EDITOR

        /// <inheritdoc />
        public override UnityEditor.BuildTarget[] ValidBuildTargets { get; } = { UnityEditor.BuildTarget.Android };

#endif // UNITY_EDITOR
    }
}
