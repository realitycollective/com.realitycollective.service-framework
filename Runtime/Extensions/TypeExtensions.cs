// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.ServiceFramework.Interfaces;
using RealityToolkit.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RealityToolkit.ServiceFramework.Extensions
{
    public static class SFTypeExtensions
    {
        /// <summary>
        /// Checks if the <see cref="IMixedRealityService"/> has any valid implementations.
        /// </summary>
        /// <typeparam name="T">The specific <see cref="IMixedRealityService"/> interface to check.</typeparam>
        /// <returns>True, if the project contains valid implementations of <see cref="T"/>.</returns>
        public static bool HasValidImplementations<T>() where T : IService
        {
            var concreteTypes = TypeCache.Current
                .Select(pair => pair.Value)
                .Where(type => typeof(T).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract);

            var isValid = concreteTypes.Any();

            if (!isValid)
            {
                Debug.LogError($"Failed to find valid implementations of {typeof(T).Name}");
            }

            return isValid;
        }

        internal static Type FindServiceInterfaceType(this Type serviceType, Type interfaceType)
        {
            if (serviceType == null)
            {
                return null;
            }

            var returnType = interfaceType;

            if (typeof(IService).IsAssignableFrom(serviceType))
            {
                if (!ServiceInterfaceCache.TryGetValue(serviceType, out returnType))
                {
                    if (IsValidServiceType(interfaceType, out returnType))
                    {
                        // If the interface we pass in is a Valid Service Type, cache it and move on.
                        ServiceInterfaceCache.Add(serviceType, returnType);
                        return returnType;
                    }

                    var types = serviceType.GetInterfaces();

                    for (int i = 0; i < types.Length; i++)
                    {
                        if (IsValidServiceType(types[i], out returnType))
                        {
                            break;
                        }
                    }

                    ServiceInterfaceCache.Add(serviceType, returnType);
                }
            }

            return returnType;
        }

        private static readonly Dictionary<Type, Type> ServiceInterfaceCache = new Dictionary<Type, Type>();

        private static bool IsValidServiceType(Type inputType, out Type returnType)
        {
            returnType = null;

            if (!typeof(IService).IsAssignableFrom(inputType))
            {
                return false;
            }

            if (inputType != typeof(IService) &&
                inputType != typeof(IServiceDataProvider))
            {
                returnType = inputType;
                return true;
            }
            return false;
        }
    }
}