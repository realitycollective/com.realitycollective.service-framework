// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace RealityCollective.ServiceFramework.Interfaces
{
    public interface IServiceProvider : IService
    {
        /// <summary>
        /// The <see cref="IService"/> this service provider is registered with.
        /// </summary>
        IService ParentService { get; }
    }
}