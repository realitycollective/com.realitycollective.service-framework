// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using System;
using RealityToolkit.ServiceFramework.Definitions;

namespace RealityToolkit.ServiceFramework.Attributes
{
    public class SystemTypeAttribute : PropertyAttribute
    {
        /// <summary>
        /// Gets or sets grouping of selectable classes. Defaults to <see cref="TypeGrouping.ByNamespaceFlat"/> unless explicitly specified.
        /// </summary>
        public TypeGrouping Grouping { get; protected set; }

        /// <summary>
        /// Gets or sets whether abstract classes can be selected from drop-down.
        /// Defaults to a value of <c>false</c> unless explicitly specified.
        /// </summary>
        public bool AllowAbstract { get; protected set; } = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">Initializes a new instance of the <see cref="SystemTypeAttribute"/> class.</param>
        /// <param name="grouping">Gets or sets grouping of selectable classes. Defaults to <see cref="TypeGrouping.ByNamespaceFlat"/> unless explicitly specified.</param>
        public SystemTypeAttribute(Type type, TypeGrouping grouping = TypeGrouping.ByNamespaceFlat)
        {
            bool isValid = type.IsClass || type.IsInterface || type.IsValueType && !type.IsEnum;
            Debug.Assert(isValid, $"Invalid Type {type} in attribute.");
            Grouping = grouping;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Type"/> satisfies filter constraint.
        /// </summary>
        /// <param name="type">Type to test.</param>
        /// <returns>
        /// A <see cref="bool"/> value indicating if the type specified by <paramref name="type"/>
        /// satisfies this constraint and should thus be selectable.
        /// </returns>
        public virtual bool IsConstraintSatisfied(Type type)
        {
            return AllowAbstract || !type.IsAbstract;
        }
    }
}