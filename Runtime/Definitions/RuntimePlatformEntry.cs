// Copyright (c) Reality Collective. All rights reserved.

using RealityToolkit.ServiceFramework.Attributes;
using RealityToolkit.ServiceFramework.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityToolkit.ServiceFramework.Definitions
{
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