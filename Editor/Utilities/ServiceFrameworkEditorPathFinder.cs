// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace RealityCollective.ServiceFramework.Editor.Utilities
{
    /// <summary>
    /// Dummy scriptable object used to find the relative path to the Service Framework Editor folder
    /// </summary>
    /// <inheritdoc cref="IPathFinder" />
    public class ServiceFrameworkEditorPathFinder : ScriptableObject, IPathFinder
    {
        /// <inheritdoc />
        public string Location => $"/Editor/Utilities/{nameof(ServiceFrameworkEditorPathFinder)}.cs";
    }
}