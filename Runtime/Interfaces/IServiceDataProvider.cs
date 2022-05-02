// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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