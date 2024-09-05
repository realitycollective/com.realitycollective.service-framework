// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.﻿

using RealityCollective.ServiceFramework.Extensions;
using System;
using System.Collections.Generic;

namespace RealityCollective.ServiceFramework
{
    public static class TypeCache
    {
        private static readonly Dictionary<Guid, Type> typeCache = new Dictionary<Guid, Type>();

        public static Dictionary<Guid, Type> Current
        {
            get
            {
                if (typeCache.Count == 0)
                {
                    TypeExtensions.BuildTypeCache(typeCache);
                }

                return typeCache;
            }
        }
    }
}