// Copyright (c) Reality Collective. All rights reserved.

namespace RealityToolkit.ServiceFramework.Interfaces
{
    /// <summary>
    /// Defines the platform to be registered
    /// </summary>
    public interface IPlatform
    {
        /// <summary>
        /// The human readable name for this platform.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Is this platform currently available?
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// The list of platforms that this specific platform will override and make the others return not available.
        /// </summary>
        IPlatform[] PlatformOverrides { get; }

#if UNITY_EDITOR

        /// <summary>
        /// The this platform build target available?
        /// </summary>
        /// <remarks>
        /// Only returns true in editor.
        /// </remarks>
        bool IsBuildTargetAvailable { get; }

        /// <summary>
        /// The array of valid <see cref="UnityEditor.BuildTarget"/>s.
        /// </summary>
        UnityEditor.BuildTarget[] ValidBuildTargets { get; }

#endif // UNITY_EDITOR
    }
}