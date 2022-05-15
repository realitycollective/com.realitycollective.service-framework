// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.ServiceFramework.Interfaces;
using RealityToolkit.ServiceFramework.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RealityToolkit.ServiceFramework.Extensions
{
    public static class TypeExtensions
    {
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

            if (!ServiceManager.ServiceInterfaceTypes.Contains(inputType))
            {
                returnType = inputType;
                return true;
            }
            return false;
        }
    }
}