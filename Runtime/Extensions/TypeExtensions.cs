// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Interfaces;
using RealityCollective.ServiceFramework.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

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
                Debug.LogError($"Failed to find valid implementations of {typeof(T).Name}");
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

        internal static Dictionary<Guid, Type> BuildTypeCache(Dictionary<Guid, Type> typeCache)
        {
            if (typeCache == null)
            {
                throw new ArgumentNullException("typeCache", "No type cache dictionary supplied");
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies().FilterBlacklistedAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.BaseType != null && type.BaseType.Name.Contains("Delegate"))
                    {
                        continue;
                    }
                    if (type.IsClass && !type.IsAbstract && type.GUID != Guid.Empty)
                    {
                        try
                        {
                            var guid = type.GUID;
                            if (!typeCache.ContainsKey(guid))
                            {
                                typeCache.Add(guid, type);
                            }
                        }
                        catch (Exception ex)
                        {
                            // In some cases at runtime in a player build built using
                            // IL2CPP accessing Type.GUID throws an unsupported exception crashing the application.
                            // Tests have shown that catching the exception prevents the app from crashing
                            // without actually breaking functionality of the application.
                            // TODO: Why are some types causing these exceptions?
                            Debug.LogError($"Failed to add {type.Name} to type cache.");
                            Debug.LogException(ex);
                        }
                    }
                }
            }
            return typeCache;
        }

        private static string[] blacklistedAssemblies = new string[]
        {
            "UnityEngine",
            "Unity",
            "System",
            "Mono",
            "NetStandard",
            "nunit",
            "log4net",
            "Bee",
            "NiceIO"
        };

        private static Assembly[] FilterBlacklistedAssemblies(this Assembly[] assemblies)
        {
            var returnAssemblies = new List<Assembly>();

            for (int i = assemblies.Length - 1; i >= 0; i--)
            {
                var ignoreAssembly = false;
                var assembly = assemblies[i];
                const string customOrPackageManagerCodeBase = "Library/ScriptAssemblies/";

                for (var j = blacklistedAssemblies.Length - 1; j >= 0; j--)
                {
                    if (!string.IsNullOrEmpty(assembly.EscapedCodeBase) &&
                        assembly.EscapedCodeBase.Contains(customOrPackageManagerCodeBase))
                    {
                        // This is a custom user assembly or an assembly coming in via the package manager.
                        // Those should always be included to the type cache.
                        continue;
                    }

                    // All other assemblies may be Unity assemblies, .NET assemblies or any other
                    // assemblies that come as a dependency of using the Unity editor.
                    if (assembly.FullName.ToLower().Contains(blacklistedAssemblies[j].ToLower()))
                    {
                        ignoreAssembly = true;
                    }
                }

                if (!ignoreAssembly)
                {
                    returnAssemblies.Add(assemblies[i]);
                }
            }

            return returnAssemblies.ToArray();
        }

        /// <summary>
        /// Attempts to resolve the type using the class <see cref="Guid"/>.
        /// </summary>
        /// <param name="guid">Class <see cref="Guid"/> reference.</param>
        /// <param name="resolvedType">The resolved <see cref="Type"/>.</param>
        /// <returns><c>true</c> if the <paramref name="resolvedType"/> was successfully obtained from or added to the <see cref="TypeCache"/>, otherwise <c>false</c>.</returns>
        public static bool TryResolveType(Guid guid, out Type resolvedType)
        {
            resolvedType = null;

            if (guid == Guid.Empty)
            {
                return false;
            }

            if (!TypeCache.Current.TryGetValue(guid, out resolvedType))
            {
                // The type could not be found. It was potentially stripped by code stripping.
                var message = $"Configured Type Guid [{guid}] not found. It may be lost because of code stripping.\n Consider including a Link.xml that makes sure the assembly with the type missing is not stripped. To learn more about code stripping visit https://docs.unity3d.com/Manual/ManagedCodeStripping.html";
                Debug.LogError(message);
                System.Diagnostics.Debug.WriteLine(message);
                return false;
            }

            if (resolvedType != null && !resolvedType.IsAbstract)
            {
                if (!TypeCache.Current.ContainsKey(guid))
                {
                    TypeCache.Current.Add(guid, resolvedType);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to resolve the type using a the <see cref="Type.AssemblyQualifiedName"/> or <see cref="Type.GUID"/> as <see cref="string"/>.
        /// </summary>
        /// <param name="typeRef">The <see cref="Type.GUID"/> or <see cref="Type.AssemblyQualifiedName"/> as <see cref="string"/>.</param>
        /// <param name="resolvedType">The resolved <see cref="Type"/>.</param>
        /// <returns>True if the <see cref="resolvedType"/> was successfully obtained from or added to the <see cref="TypeCache"/>, otherwise false.</returns>
        public static bool TryResolveType(string typeRef, out Type resolvedType)
        {
            resolvedType = null;

            if (string.IsNullOrEmpty(typeRef))
            {
                return false;
            }

            if (Guid.TryParse(typeRef, out var guid))
            {
                return TryResolveType(guid, out resolvedType);
            }

            resolvedType = Type.GetType(typeRef);
            return resolvedType != null && !resolvedType.IsAbstract;
        }

        /// <summary>
        /// Recursively looks for generic type arguments in type hierarchy, starting with the
        /// root type provided. If no generic type arguments are found on a type, it's base
        /// type is checked.
        /// </summary>
        /// <param name="root">Root type to start looking for generic type arguments at.</param>
        /// <param name="maxRecursionDepth">The maximum recursion depth until execution gets canceled even if no results found.</param>
        /// <returns>Found generic type arguments array or null, if none found.</returns>
        public static Type[] FindTopmostGenericTypeArguments(this Type root, int maxRecursionDepth = 5)
        {
            var genericTypeArgs = root?.GenericTypeArguments;

            if (genericTypeArgs != null && genericTypeArgs.Length > 0)
            {
                return genericTypeArgs;
            }

            if (maxRecursionDepth > 0 && root != null)
            {
                return FindTopmostGenericTypeArguments(root.BaseType, --maxRecursionDepth);
            }

            Debug.LogError($"{nameof(FindTopmostGenericTypeArguments)} - Maximum recursion depth reached without finding generic type arguments.");
            return null;
        }
    }
}