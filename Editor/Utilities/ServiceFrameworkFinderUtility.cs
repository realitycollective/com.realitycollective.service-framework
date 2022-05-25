// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RealityCollective.ServiceFramework.Editor.Utilities
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
        private const string SERVICE_FRAMEWORK_PATH_FINDER = "/Editor/Utilities/ServiceFrameworkEditorPathFinder.cs";

        private static readonly Dictionary<Type, string> PathFinderCache = new Dictionary<Type, string>();
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

        /// <summary>
        /// Resolves the path to the provided <see cref="IPathFinder"/>.<see cref="T:Type"/>
        /// </summary>
        /// <typeparam name="T"><see cref="IPathFinder"/> constraint.</typeparam>
        /// <param name="pathFinderType">The <see cref="T:Type"/> of <see cref="IPathFinder"/> to resolve the path for.</param>
        /// <returns>If found, the relative path to the root folder this <see cref="IPathFinder"/> references.</returns>
        public static string ResolvePath<T>(Type pathFinderType) where T : IPathFinder
        {
            if (pathFinderType is null)
            {
                Debug.LogError($"{nameof(pathFinderType)} is null!");
                return null;
            }

            if (!typeof(T).IsAssignableFrom(pathFinderType))
            {
                Debug.LogError($"{pathFinderType.Name} must implement {nameof(IPathFinder)}");
                return null;
            }

            if (!typeof(ScriptableObject).IsAssignableFrom(pathFinderType))
            {
                Debug.LogError($"{pathFinderType.Name} must derive from {nameof(ScriptableObject)}");
                return null;
            }

            if (!PathFinderCache.TryGetValue(pathFinderType, out var resolvedPath))
            {
                var pathFinder = ScriptableObject.CreateInstance(pathFinderType) as IPathFinder;
                Debug.Assert(pathFinder != null, $"{nameof(pathFinder)} != null");
                resolvedPath = AssetDatabase.GetAssetPath(
                    MonoScript.FromScriptableObject((ScriptableObject)pathFinder))
                        .Replace(pathFinder.Location, string.Empty);
                PathFinderCache.Add(pathFinderType, resolvedPath);
            }

            return resolvedPath.BackSlashes();
        }

        #region Service Framework Paths

        /// <summary>
        /// The absolute folder path to the Mixed Reality Toolkit in your project.
        /// </summary>
        public static string AbsoluteFolderPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(absoluteFolderPath))
                {
                    absoluteFolderPath = Path.GetFullPath(RelativeFolderPath);
                }

                return absoluteFolderPath.BackSlashes();
            }
        }

        private static string absoluteFolderPath = string.Empty;

        /// <summary>
        /// The relative folder path to the Service Framework folder in relation to either the "Assets" or "Packages" folders.
        /// </summary>
        public static string RelativeFolderPath
        {
            get
            {
                if (string.IsNullOrEmpty(relativeFolderPath))
                {
                    relativeFolderPath = ResolvePath(SERVICE_FRAMEWORK_PATH_FINDER);
                    Debug.Assert(!string.IsNullOrWhiteSpace(relativeFolderPath));
                }

                return relativeFolderPath;
            }
        }

        private static string relativeFolderPath = string.Empty;

        #endregion Service Framework Paths
    }
}