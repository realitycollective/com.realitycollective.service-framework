// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Definitions.Utilities;
using RealityCollective.ServiceFramework.Definitions;
using System.Collections.Generic;

namespace RealityCollective.ServiceFramework.Interfaces
{
    public interface IServiceConfiguration<out T> : IServiceConfiguration where T : IService { }

    /// <summary>
    /// This interface is meant to be used with serialized structs that define valid <see cref="IService"/> configurations.
    /// </summary>
    public interface IServiceConfiguration
    {
        /// <summary>
        /// Is this service enabled?
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// The concrete type for the <see cref="IService"/> that will be instantiated and ran by the service locator.
        /// </summary>
        SystemType InstancedType { get; }

        /// <summary>
        /// The simple, human readable name for the <see cref="IService"/>.
        /// </summary>
        /// <remarks>
        /// This name should be unique.
        /// </remarks>
        string Name { get; }

        /// <summary>
        /// The priority order of execution for this <see cref="IService"/>.
        /// </summary>
        /// <remarks>
        /// Multiple <see cref="IService"/>s may be running at the same priority for services that are not specifically registered to the <see cref="ServiceManager.ActiveServices"/>.
        /// </remarks>
        uint Priority { get; }

        /// <summary>
        /// The <see cref="BaseProfile"/> for <see cref="IService"/>.
        /// </summary>
        BaseProfile Profile { get; }

        /// <summary>
        /// The runtime <see cref="IPlatform"/>(s) to run this <see cref="IService"/> to run on.
        /// </summary>
        IReadOnlyList<IPlatform> RuntimePlatforms { get; }
    }
}