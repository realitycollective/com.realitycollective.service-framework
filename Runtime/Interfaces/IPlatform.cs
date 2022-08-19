// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace RealityCollective.ServiceFramework.Interfaces
{
    /// <summary>
    /// Defines a target platform to run a service on.
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