// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Interfaces;
using RealityCollective.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace RealityCollective.ServiceFramework.Extensions
{
    public static class ServiceConfigurationExtensions
    {
        internal static IServiceConfiguration<IService>[] OrderServiceConfigurationByDependencies(this IServiceConfiguration<IService>[] serviceConfiguration)
        {
            var orderedServiceConfiguration = new Dictionary<string, IServiceConfiguration<IService>>();
            List<string> configuredServiceInterfaceNames = new();
            List<string> dependentServiceNames = new();
            int loopCount = serviceConfiguration.Length + 1;

            while (loopCount > 0 && orderedServiceConfiguration.Count < serviceConfiguration.Length)
            {
                foreach (var configuration in serviceConfiguration)
                {
                    Type serviceType = configuration.InstancedType;
                    var serviceInterfaceType = serviceType.FindServiceInterfaceType();
                    if (serviceInterfaceType == null)
                    {
                        orderedServiceConfiguration.EnsureDictionaryItem(serviceType.FullName, configuration);
                        continue;
                    }
                    configuredServiceInterfaceNames.EnsureListItem(serviceInterfaceType.FullName);
                    ConstructorInfo[] constructors = serviceType.GetConstructors();
                    if (constructors.Length == 0)
                    {
                        // Cannot analyze service, so add to return and skip.
                        orderedServiceConfiguration.EnsureDictionaryItem(serviceInterfaceType.FullName, configuration);
                        continue;
                    }

                    // we are only focusing on the primary constructor for now.
                    var primaryConstructor = constructors[0];

                    ParameterInfo[] parameters = primaryConstructor.GetParameters();

                    // If there are no additional dependencies other than the base 3 (Name, Priority, Profile), then we can skip this.
                    if (parameters.Length == 0 || parameters.Length == 3)
                    {
                        orderedServiceConfiguration.EnsureDictionaryItem(serviceInterfaceType.FullName, configuration);
                        continue;
                    }
                    int sortedParameters = 0;
                    foreach (var parameter in parameters)
                    {
                        if (parameter.ParameterType.IsInterface && typeof(IService).IsAssignableFrom(parameter.ParameterType))
                        {
                            if (orderedServiceConfiguration.ContainsKey(parameter.ParameterType.FullName))
                            {
                                dependentServiceNames.EnsureListItem(parameter.ParameterType.FullName);
                                sortedParameters++;
                            }
                        }
                        else
                        {
                            sortedParameters++;
                        }
                    }
                    if (sortedParameters == parameters.Length)
                    {
                        orderedServiceConfiguration.Add(serviceType.FullName, configuration);
                    }
                }

                loopCount--;
            }

            List<string> missingServiceNames = dependentServiceNames.Except(configuredServiceInterfaceNames).ToList();
            if (missingServiceNames.Count > 0)
            {
                Debug.LogError($"Unable to resolve service dependencies.\nThe following services are missing from the service configuration: {string.Join(", ", missingServiceNames)}");
                return serviceConfiguration;
            }

            return orderedServiceConfiguration.Values.ToArray();
        }
    }
}