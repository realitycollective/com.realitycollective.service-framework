// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.ServiceFramework.Interfaces;
using System;
using System.Collections.Generic;

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
            if (interfaceType == null)
            {
                if (!ServiceInterfaceCache.TryGetValue(serviceType, out returnType))
                {
                    var types = serviceType.GetInterfaces();

                    for (int i = 0; i < types.Length; i++)
                    {
                        if (!typeof(IService).IsAssignableFrom(types[i]))
                        {
                            continue;
                        }

                        if (types[i] != typeof(IService) &&
                            types[i] != typeof(IServiceDataProvider))
                        {
                            returnType = types[i];
                            break;
                        }
                    }

                    ServiceInterfaceCache.Add(serviceType, returnType);
                }
            }

            return returnType;
        }

        private static readonly Dictionary<Type, Type> ServiceInterfaceCache = new Dictionary<Type, Type>();
    }
}