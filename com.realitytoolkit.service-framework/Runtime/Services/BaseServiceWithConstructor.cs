// Copyright (c) Reality Collective. All rights reserved.

using System;

namespace RealityToolkit.ServiceFramework.Services
{
    /// <summary>
    /// Base <see cref="IService"/> with a constructor override.
    /// </summary>
    public abstract class BaseServiceWithConstructor : BaseService
    {
        private Guid guid;

        /// <summary>
        /// Cached Guid Reference for the Service / Data Provider
        /// </summary>
        public Guid ServiceGuid => guid;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The name of the service.</param>
        /// <param name="priority">The priority of the service.</param>
        protected BaseServiceWithConstructor(string name = "", uint priority = 10)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = GetType().Name;
            }

            this.Name = name;
            this.Priority = priority;
            this.guid = GetType().GUID;
        }
    }
}