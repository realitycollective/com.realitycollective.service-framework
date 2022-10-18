// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Interfaces;
using RealityCollective.ServiceFramework.Services;

namespace RealityCollective.ServiceFramework.Providers
{
    /// <summary>
    /// The base service provider implements <see cref="IServiceProvider"/> and provides default properties for all service providers.
    /// </summary>
    public abstract class BaseServiceProvider : BaseServiceWithConstructor, Interfaces.IServiceProvider
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The name of the service.</param>
        /// <param name="priority">The priority of the service.</param>
        /// <param name="profile">The optional <see cref="BaseProfile"/> for the service provider.</param>
        /// <param name="parentService">The <see cref="IService"/> that this <see cref="IServiceProvider"/> is assigned to.</param>
        protected BaseServiceProvider(string name, uint priority, BaseProfile profile, IService parentService) : base(name, priority)
        {
            ParentService = parentService ?? throw new ArgumentNullException($"{nameof(parentService)} cannot be null");
            parentService.RegisterServiceProvider(this);
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

            ParentService?.UnRegisterServiceProvider(this);
        }
    }
}