// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RealityToolkit.ServiceFramework.Editor.Utilities
{
    /// <summary>
    /// Interface to implement on a <see cref="ScriptableObject"/> to make it easier to find relative/absolute folder paths using the <see cref="PathFinderUtility"/>.
    /// </summary>
    /// <remarks>
    /// Required to be a standalone class in a separate file or else <see cref="MonoScript.FromScriptableObject"/> returns an empty string path.
    /// </remarks>
    public interface IPathFinder
    {
        /// <summary>
        /// The relative path to this <see cref="IPathFinder"/> class from either the Assets or Packages folder.
        /// </summary>
        string Location { get; }
    }
    public class ServiceFrameworkFinderUtility
    {
        private const string SERVICE_FRAMEWORK_PATH_FINDER = "/Editor/Utilities/ServiceFrameworkPathFinder.cs";
        
        private static readonly Dictionary<string, string> ResolvedFinderCache = new Dictionary<string, string>();
        private static List<Type> GetAllPathFinders
        {
            get
            {
                return AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(type => typeof(IPathFinder).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract)
                    .OrderBy(type => type.Name)
                    .ToList();
            }
        }
        private static string ResolvePath(string finderPath)
        {
            if (!ResolvedFinderCache.TryGetValue(finderPath, out var resolvedPath))
            {
                foreach (var type in GetAllPathFinders)
                {
                    if (type.Name == Path.GetFileNameWithoutExtension(finderPath))
                    {
                        resolvedPath = AssetDatabase.GetAssetPath(
                                MonoScript.FromScriptableObject(
                                    ScriptableObject.CreateInstance(type)))
                            .Replace(finderPath, string.Empty);
                        ResolvedFinderCache.Add(finderPath, resolvedPath);
                        break;
                    }
                }
            }

            return resolvedPath;
        }

        public static string AbsoluteFolderPath
        {
            get
            {
                string resolvePath = ResolvePath(SERVICE_FRAMEWORK_PATH_FINDER);
                return Path.GetFullPath(resolvePath);
            }
        }
    }
}