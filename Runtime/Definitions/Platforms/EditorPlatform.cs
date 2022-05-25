// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace RealityCollective.ServiceFramework.Definitions.Platforms
{
    /// <summary>
    /// Used by the Service Framework to signal that the feature is available in the Unity Editor.
    /// </summary>
    /// <remarks>
    /// Defines any editor platform for Win, OSX, and Linux.
    /// </remarks>
    [System.Runtime.InteropServices.Guid("3DFB96A6-7F67-4F83-835B-32725BC0A2C0")]
    public sealed class EditorPlatform : BasePlatform
    {
        /// <inheritdoc />
        public override bool IsAvailable => Application.isEditor;
    }
}