// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Interfaces;
using System.Collections.Generic;
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

        /// <summary>
        /// Adds the <paramref name="configuration"/> to the profile.
        /// </summary>
        /// <param name="configuration">The <see cref="IServiceConfiguration"/> to add.</param>
        public void AddConfiguration(IServiceConfiguration<TService> configuration)
        {
            // If no configuration is passed to add, do nothing.
            if (configuration is null)
            {
                return;
            }

            var serviceConfigurations = new List<IServiceConfiguration<TService>>();

            // If there are existing Service Configurations, import them.
            if (ServiceConfigurations != null && ServiceConfigurations.Length > 0)
            {
                serviceConfigurations.AddRange(ServiceConfigurations);
            }

            serviceConfigurations.Add(configuration);
            ServiceConfigurations = serviceConfigurations.ToArray();
        }
    }
}