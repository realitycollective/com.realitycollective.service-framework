﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Attributes;
using RealityCollective.ServiceFramework.Definitions.Utilities;
using RealityCollective.ServiceFramework.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityCollective.ServiceFramework.Definitions
{
    /// <summary>
    /// Runtime platform entry, for loading services for specific platforms.
    /// </summary>
    [Serializable]
    public class RuntimePlatformEntry
    {
        public RuntimePlatformEntry()
        {
            runtimePlatforms = new SystemType[0];
        }

        public RuntimePlatformEntry(IReadOnlyList<IPlatform> runtimePlatforms)
        {
            this.runtimePlatforms = new SystemType[runtimePlatforms.Count];

            for (int i = 0; i < runtimePlatforms.Count; i++)
            {
                this.runtimePlatforms[i] = new SystemType(runtimePlatforms[i].GetType());
            }
        }

        [SerializeField]
        [Implements(typeof(IPlatform), TypeGrouping.ByNamespaceFlat)]
        private SystemType[] runtimePlatforms;

        public SystemType[] RuntimePlatforms => runtimePlatforms;
    }
}