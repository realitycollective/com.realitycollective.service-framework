// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace RealityToolkit.ServiceFramework.Editor.Utilities
{
    /// <summary>
    /// Dummy scriptable object used to find the relative path to com.xrtk.core.
    /// </summary>
    /// <inheritdoc cref="IPathFinder" />
    public class ServiceFrameworkPathFinder : ScriptableObject, IPathFinder
    {
        /// <inheritdoc />
        public string Location => $"/Editor/Utilities/{nameof(ServiceFrameworkPathFinder)}.cs";
    }
}