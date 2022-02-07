// Copyright (c) Reality Collective. All rights reserved.

namespace RealityToolkit.ServiceFramework.Interfaces
{
    public interface IServiceDataProvider : IService
    {
        /// <summary>
        /// The <see cref="IService"/> this data provider is registered with.
        /// </summary>
        IService ParentService { get; }
    }
}