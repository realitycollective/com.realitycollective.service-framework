// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Interfaces;

#if UNITY_EDITOR
using System.Linq;
#endif

namespace RealityCollective.ServiceFramework.Definitions.Platforms
{
    /// <summary>
    /// Base platform class to derive all <see cref="IPlatform"/>s from.
    /// </summary>
    public abstract class BasePlatform : IPlatform
    {
        private string name = null;

        /// <inheritdoc />
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(name))
                {
                    name = GetType().Name.Replace("Platform", string.Empty);
                }

                return name;
            }
        }

        /// <inheritdoc />
        public virtual bool IsAvailable => false;

        /// <inheritdoc />
        public virtual IPlatform[] PlatformOverrides { get; } = new IPlatform[0];

#if UNITY_EDITOR

        /// <inheritdoc />
        public virtual bool IsBuildTargetAvailable => ValidBuildTargets != null &&
                                                      ValidBuildTargets.Any(buildTarget => UnityEditor.EditorUserBuildSettings.activeBuildTarget == buildTarget);

        /// <inheritdoc />
        public virtual UnityEditor.BuildTarget[] ValidBuildTargets => null;

#endif // UNITY_EDITOR

        /// <inheritdoc />
        public override bool Equals(object other) => other?.GetType() == GetType();

        /// <inheritdoc />
        public override int GetHashCode() => GetType().GetHashCode();
    }
}