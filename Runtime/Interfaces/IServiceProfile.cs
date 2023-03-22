// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace RealityCollective.ServiceFramework.Interfaces
{
    /// <summary>
    /// Generic interface for loading different types of <see cref="IService"/>.
    /// </summary>
    /// <typeparam name="TService">The type of <see cref="IService"/>s configured in the profile.</typeparam>
    public interface IServiceProfile<out TService>  where TService : IService
    {
        /// <summary>
        /// The <see cref="IServiceConfiguration"/>s registered for this profile.
        /// </summary>
        IServiceConfiguration<TService>[] ServiceConfigurations { get; }

        /// <summary>
        /// Adds the <paramref name="configuration"/> to the profile.
        /// </summary>
        /// <param name="configuration">The <see cref="IServiceConfiguration"/> to add.</param>
        void AddConfiguration(IServiceConfiguration<IService> configuration);
    }
}