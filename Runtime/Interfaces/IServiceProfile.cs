// Copyright (c) Reality Collective. All rights reserved.

namespace RealityToolkit.ServiceFramework.Interfaces
{
    public interface IServiceProfile<out TService> where TService : IService
    {
        /// <summary>
        /// The <see cref="IServiceConfiguration"/>s registered for this profile.
        /// </summary>
        IServiceConfiguration<TService>[] ServiceConfigurations { get; }
    }
}