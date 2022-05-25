// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace RealityCollective.ServiceFramework.Definitions
{
    public abstract class BaseProfile : ScriptableObject
    {
        /// <summary>
        /// The profile's parent in the service graph hierarchy.
        /// </summary>
        public BaseProfile ParentProfile { get; set; } = null;
    }
}