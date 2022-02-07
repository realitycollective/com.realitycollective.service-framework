// Copyright (c) Reality Collective. All rights reserved.

using UnityEngine;

namespace RealityToolkit.ServiceFramework.Definitions
{
    public abstract class BaseProfile : ScriptableObject
    {
        /// <summary>
        /// The profile's parent in the service graph hierarchy.
        /// </summary>
        public BaseProfile ParentProfile { get; set; } = null;
    }
}