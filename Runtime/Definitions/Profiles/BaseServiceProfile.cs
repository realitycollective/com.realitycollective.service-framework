// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Interfaces;
using UnityEngine;

namespace RealityCollective.ServiceFramework.Definitions
{
    /// <summary>
    /// The base profile type to derive all <see cref="IService"/>s from.
    /// </summary>
    /// <typeparam name="TService">
    /// The <see cref="IService"/> type to constrain all of the valid <see cref="IServiceConfiguration.InstancedType"/>s to.
    /// Only types that implement the <see cref="TService"/> will show up in the inspector dropdown for the <see cref="IServiceConfiguration.InstancedType"/>
    /// </typeparam>
    public abstract class BaseServiceProfile<TService> : BaseProfile, IServiceProfile<TService> where TService : IService
    {
        [SerializeField]
        private ServiceConfiguration[] configurations = new ServiceConfiguration[0];

        private IServiceConfiguration<TService>[] serviceConfigurations;

        /// <inheritdoc />
        public IServiceConfiguration<TService>[] ServiceConfigurations
        {
            get
            {
                if (configurations == null)
                {
                    configurations = new ServiceConfiguration[0];
                }

                if (serviceConfigurations == null ||
                    serviceConfigurations.Length != configurations.Length)
                {
                    serviceConfigurations = new IServiceConfiguration<TService>[configurations.Length];
                }

                for (int i = 0; i < serviceConfigurations.Length; i++)
                {
                    var cachedConfig = configurations[i];
                    Debug.Assert(cachedConfig != null);
                    var serviceConfig = new ServiceConfiguration<TService>(cachedConfig);
                    Debug.Assert(serviceConfig != null);
                    serviceConfigurations[i] = serviceConfig;
                }

                return serviceConfigurations;
            }
            internal set
            {
                var serviceConfigurations = value;

                if (serviceConfigurations == null)
                {
                    configurations = new ServiceConfiguration[0];
                }
                else
                {
                    configurations = new ServiceConfiguration[serviceConfigurations.Length];

                    for (int i = 0; i < serviceConfigurations.Length; i++)
                    {
                        var serviceConfig = serviceConfigurations[i];
                        Debug.Assert(serviceConfig != null);
                        var newConfig = new ServiceConfiguration(serviceConfig.InstancedType, serviceConfig.Name, serviceConfig.Priority, serviceConfig.RuntimePlatforms, serviceConfig.Profile);
                        Debug.Assert(newConfig != null);
                        configurations[i] = newConfig;
                    }
                }
            }
        }

        public void AddConfiguration(IServiceConfiguration<TService> configuration)
        {
            var newConfigs = new IServiceConfiguration<TService>[ServiceConfigurations.Length + 1];

            for (int i = 0; i < newConfigs.Length; i++)
            {
                if (i != newConfigs.Length - 1)
                {
                    newConfigs[i] = ServiceConfigurations[i];
                }
                else
                {
                    newConfigs[i] = configuration;
                }
            }

            ServiceConfigurations = newConfigs;
        }
    }
}