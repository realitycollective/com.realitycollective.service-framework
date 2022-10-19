﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Interfaces;
using RealityCollective.ServiceFramework.Services;

namespace RealityCollective.ServiceFramework.Providers
{
    /// <summary>
    /// The base data provider implements <see cref="IServiceDataProvider"/> and provides default properties for all data providers.
    /// </summary>
    public abstract class BaseServiceDataProvider : BaseServiceWithConstructor, IServiceDataProvider
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The name of the service.</param>
        /// <param name="priority">The priority of the service.</param>
        /// <param name="profile">The optional <see cref="BaseProfile"/> for the data provider.</param>
        /// <param name="parentService">The <see cref="IService"/> that this <see cref="IServiceDataProvider"/> is assigned to.</param>
        protected BaseServiceDataProvider(string name, uint priority, BaseProfile profile, IService parentService) : base(name, priority)
        {
            ParentService = parentService ?? throw new ArgumentNullException($"{nameof(parentService)} cannot be null");
            parentService.RegisterDataProvider(this);
        }

        /// <inheritdoc />
        public IService ParentService { get; }

        /// <inheritdoc />
        public override uint Priority
        {
            get => base.Priority + ParentService.Priority;
        }

        /// <inheritdoc />
        public override void Destroy()
        {
            base.Destroy();

            ParentService?.UnRegisterDataProvider(this);
        }
    }
}