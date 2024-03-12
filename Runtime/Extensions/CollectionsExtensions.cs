// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace RealityCollective.ServiceFramework.Extensions
{
    /// <summary>
    /// Extension methods for .Net Collection objects, e.g. Lists, Dictionaries, Arrays
    /// </summary>
    public static class CollectionsExtensions
    {
        /// <summary>
        /// Validate if a list contains an item and add it if not found.
        /// </summary>
        /// <typeparam name="T">Data type used in the List.</typeparam>
        /// <param name="list">The instance of the List to validate.</param>
        /// <param name="item">The item of Type T to add to the list if not found</param>
        /// <returns>True if a new item was added to the collection</returns>
        public static bool EnsureListItem<T>(this IList<T> list, T item)
        {
            if (!list.Contains(item))
            {
                list.Add(item);
                return true;
            }

            return false;
        }
    }
}