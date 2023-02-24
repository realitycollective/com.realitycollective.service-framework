// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Interfaces;
using RealityCollective.ServiceFramework.Services;
using RealityCollective.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RealityCollective.ServiceFramework.Extensions
{
    public static class TypeExtensions
    {
        private static readonly Dictionary<Type, Type> ServiceInterfaceCache = new Dictionary<Type, Type>();

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

                    var allInterfaces = FindCandidateInterfaceTypes(serviceType);
                    foreach (var typeInterface in allInterfaces)
                    {
                        if (IsValidServiceType(typeInterface, out returnType))
                        {
                            break;
                        }
                    }

                    ServiceInterfaceCache.Add(serviceType, returnType);
                }
            }

            return returnType;
        }

        private static HashSet<Type> FindCandidateInterfaceTypes(Type serviceType)
        {
            var derivedTypeInterfaces = new HashSet<Type>(serviceType.GetInterfaces());
            var toRemove = new HashSet<Type>();

            foreach (var type in derivedTypeInterfaces)
            {
                if (!IsValidServiceType(type, out _))
                {
                    toRemove.Add(type);
                }
            }
            derivedTypeInterfaces.ExceptWith(toRemove);

            if (serviceType.BaseType != null)
            {
                toRemove.Clear();
                var baseInterfaces = new HashSet<Type>(serviceType.BaseType.GetInterfaces());

                // If interfaces on the base type and most derived type match exactly, that means
                // that both declare the same set of interfaces, if we were to filter the base types out
                // now, we'd end up having nothing, because 1 - 1 = 0.
                // In this case we don't worry about filtering the base types because the next filter will
                // make sure interface inheritance is filtered out.
                foreach (var baseType in baseInterfaces)
                {
                    if (derivedTypeInterfaces.Contains(baseType))
                    {
                        continue;
                    }

                    toRemove.Add(baseType);
                }

                derivedTypeInterfaces.ExceptWith(toRemove);
            }

            // We want to remove interfaces that are implemented by other interfaces
            // i.e
            // public interface A : B {}
            // public interface B {}
            // public class Top : A {} → We only want to dump interface A so interface B must be removed

            // Considering class A given above allInterfaces contains A and B now.
            toRemove.Clear();
            foreach (var implementedByMostDerivedClass in derivedTypeInterfaces)
            {
                // For interface A this will only contain single element, namely B
                // For interface B this will an empty array
                foreach (var implementedByOtherInterfaces in implementedByMostDerivedClass.GetInterfaces())
                {
                    toRemove.Add(implementedByOtherInterfaces);
                }
            }

            // Finally remove the interfaces that do not belong to the most derived class.
            derivedTypeInterfaces.ExceptWith(toRemove);
            return derivedTypeInterfaces;
        }

        /// <summary>
        /// Checks if the <see cref="IService"/> has any valid implementations.
        /// </summary>
        /// <typeparam name="T">The specific <see cref="IService"/> interface to check.</typeparam>
        /// <returns>True, if the project contains valid implementations of <typeparamref name="T"/>.</returns>
        internal static bool HasValidImplementations<T>() where T : IService
        {
            var concreteTypes = TypeCache.Current
                .Select(pair => pair.Value)
                .Where(type => typeof(T).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract);

            var isValid = concreteTypes.Any();

            if (!isValid)
            {
                UnityEngine.Debug.LogError($"Failed to find valid implementations of {typeof(T).Name}");
            }

            return isValid;
        }

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