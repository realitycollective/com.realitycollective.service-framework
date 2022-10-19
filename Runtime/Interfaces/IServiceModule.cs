// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace RealityCollective.ServiceFramework.Interfaces
{
    public interface IServiceModule : IService
    {
        /// <summary>
        /// The <see cref="IService"/> this <see cref="IServiceModule"/> is registered with.
        /// </summary>
        IService ParentService { get; }
    }
}