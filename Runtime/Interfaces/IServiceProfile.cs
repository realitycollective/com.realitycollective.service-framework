// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace RealityCollective.ServiceFramework.Interfaces
{
    public interface IServiceProfile<out TService> where TService : IService
    {
        /// <summary>
        /// The <see cref="IServiceConfiguration"/>s registered for this profile.
        /// </summary>
        IServiceConfiguration<TService>[] ServiceConfigurations { get; }
    }
}