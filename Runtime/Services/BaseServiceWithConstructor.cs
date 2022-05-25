// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace RealityCollective.ServiceFramework.Services
{
    /// <summary>
    /// Base <see cref="IService"/> with a constructor override.
    /// </summary>
    public abstract class BaseServiceWithConstructor : BaseService
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The name of the service.</param>
        /// <param name="priority">The priority of the service.</param>
        protected BaseServiceWithConstructor(string name = "", uint priority = 10) : base()
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = GetType().Name;
            }

            this.Name = name;
            this.Priority = priority;
        }
    }
}