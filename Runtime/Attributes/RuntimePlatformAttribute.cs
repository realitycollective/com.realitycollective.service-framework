// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Interfaces;
using System;
using UnityEngine;

namespace RealityCollective.ServiceFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class RuntimePlatformAttribute : PropertyAttribute
    {
        public Type Platform { get; }

        public RuntimePlatformAttribute(Type platformType)
        {
            if (typeof(IPlatform).IsAssignableFrom(platformType))
            {
                Platform = platformType;
            }
            else
            {
                throw new ArgumentException($"{nameof(platformType)} must implement {nameof(IPlatform)}");
            }
        }
    }
}
