// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Interfaces;
using RealityCollective.ServiceFramework.Services;

namespace RealityCollective.ServiceFramework.Modules
{
    /// <summary>
    /// The base Service Module implements <see cref="IServiceModule"/> and provides default properties for all Service Modules.
    /// </summary>
    public abstract class BaseServiceModule : BaseServiceWithConstructor, IServiceModule
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The name of the service.</param>
        /// <param name="priority">The priority of the service.</param>
        /// <param name="profile">The optional <see cref="BaseProfile"/> for the Service Module.</param>
        /// <param name="parentService">The <see cref="IService"/> that this <see cref="IServiceModule"/> is assigned to.</param>
        protected BaseServiceModule(string name, uint priority, BaseProfile profile, IService parentService) : base(name, priority)
        {
            ParentService = parentService ?? throw new ArgumentNullException($"{nameof(parentService)} cannot be null");
            parentService.RegisterServiceModule(this);
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

            ParentService?.UnRegisterServiceModule(this);
        }
    }
}