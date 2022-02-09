// Copyright (c) Reality Collective. All rights reserved.

using System;
using RealityToolkit.ServiceFramework.Attributes;
using RealityToolkit.ServiceFramework.Interfaces;
using UnityEngine;

namespace RealityToolkit.ServiceFramework.Definitions
{
    /// <inheritdoc cref="ServiceConfiguration" />
    public class ServiceConfiguration<T> : ServiceConfiguration, IServiceConfiguration<T>
        where T : IService
    {
        /// <inheritdoc />
        public ServiceConfiguration(IServiceConfiguration configuration)
            : base(configuration.InstancedType, configuration.Name, configuration.Priority, configuration.Profile)
        {
        }

        /// <inheritdoc />
        public ServiceConfiguration(SystemType instancedType, string name, uint priority, BaseProfile profile)
            : base(instancedType, name, priority, profile)
        {
        }

        /// <inheritdoc />
        public override bool Enabled
            => typeof(IService).IsAssignableFrom(typeof(T))
                ? Profile != null && base.Enabled // All IServices require a profile
                : base.Enabled;
    }

    /// <summary>
    /// Defines a <see cref="IService"/> to be registered with the <see cref="ServiceManager"/>.
    /// </summary>
    [Serializable]
    public class ServiceConfiguration : IServiceConfiguration
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="instancedType">The concrete type for the <see cref="IService"/>.</param>
        /// <param name="name">The simple, human readable name for the <see cref="IService"/>.</param>
        /// <param name="priority">The priority this <see cref="IService"/> will be initialized in.</param>
        /// <param name="profile">The <see cref="BaseServiceProfile"/> for <see cref="IService"/>.</param>
        public ServiceConfiguration(SystemType instancedType, string name, uint priority, BaseProfile profile)
        {
            this.instancedType = instancedType;
            this.name = name;
            this.priority = priority;
            this.profile = profile;
        }

        /// <inheritdoc />
        public virtual bool Enabled => instancedType.Type != null;

        [SerializeField]
        [Implements(typeof(IService), TypeGrouping.ByNamespaceFlat)]
        private SystemType instancedType;

        /// <inheritdoc />
        public SystemType InstancedType
        {
            get => instancedType;
            internal set => instancedType = value;
        }

        [SerializeField]
        private string name;

        /// <inheritdoc />
        public string Name
        {
            get => name;
            internal set => name = value;
        }

        [SerializeField]
        private uint priority;

        /// <inheritdoc />
        public uint Priority
        {
            get => priority;
            internal set => priority = value;
        }

        [SerializeField]
        private BaseProfile profile;

        /// <inheritdoc />
        public BaseProfile Profile
        {
            get => profile;
            internal set => profile = value;
        }
    }
}