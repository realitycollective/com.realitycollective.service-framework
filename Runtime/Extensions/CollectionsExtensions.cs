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

        /// <summary>
        /// Validate if a <see cref="Dictionary{TKey, TValue}"/> contains an item and add it if not found."/>
        /// </summary>
        /// <remarks>
        /// Will update the existing value when found unless overridden using the 'update' parameter
        /// </remarks>
        /// <typeparam name="TKey">Data type used in the Dictionary Key.</typeparam>
        /// <typeparam name="TValue">Data type used in the Dictionary Value.</typeparam>
        /// <param name="dictionary">The instance of the <see cref="Dictionary{TKey, TValue}"/> to validate.</param>
        /// <param name="key">The Key of a <see cref="KeyValuePair{TKey, TValue}"/> to validate against the dictionary with.</param>
        /// <param name="value">The Value of a <see cref="KeyValuePair{TKey, TValue}"/> to set the dictionary item with if required.</param>
        /// <param name="update">By default, the Ensure function will override the existing dictionary value if found, if this is not required it can be overridden with this bool.  Setting this to <see cref="false"/> will leave the dictionary item untouched if found.</param>
        /// <returns>True if a new item was added to the collection</returns>
        public static bool EnsureDictionaryItem<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value, bool update = true)
        {
            if (!dictionary.TryGetValue(key, out _))
            {
                dictionary.Add(key, value);
                return true;
            }

            if (update)
            {
                dictionary[key] = value;
            }
            return false;
        }
    }
}