// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Interfaces;

namespace RealityCollective.ServiceFramework.Definitions.Platforms
{
    /// <summary>
    /// Used by the Reality Toolkit to signal that the feature is available on every platform.
    /// </summary>
    [System.Runtime.InteropServices.Guid("9869B29E-43BB-44CC-AE49-EB5C913263F9")]
    public sealed class AllPlatforms : BasePlatform
    {
        /// <inheritdoc />
        public override bool IsAvailable => true;

#if UNITY_EDITOR

        /// <inheritdoc />
        public override bool IsBuildTargetAvailable => true;

#endif // UNITY_EDITOR

        public static IPlatform[] Platforms { get; } = { new AllPlatforms() };
    }
}