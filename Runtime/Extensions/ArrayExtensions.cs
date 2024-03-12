// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace RealityCollective.ServiceFramework.Extensions
{
    /// <summary>
    /// <see cref="Array"/> type method extensions.
    /// </summary>
    public static class ArrayExtensions
    {
        /// <summary>
        /// Extends an existing array to add a new item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">The array to extend</param>
        /// <param name="newItem">The item to add to the array</param>
        /// <returns></returns>
        public static T[] AddItem<T>(this T[] array, T newItem)
        {
            // Initialise the array if it is null
            if (array == null)
            {
                return new[] { newItem };
            }

            //Extend the array, copy the items and add the new one
            var newArray = new T[array.Length + 1];
            array.CopyTo(newArray, 0);
            newArray[array.Length] = newItem;
            return newArray;
        }

        /// <summary>
        /// Provides a List style Contains search for an array containing <see cref="IComparable"/> Items
        /// </summary>
        /// <typeparam name="T">The type of data contained with the array</typeparam>
        /// <param name="array">The Array of items to search</param>
        /// <param name="item">The item to search for in the array</param>
        /// <returns>True if the array contains the supplied item</returns>
        public static bool Contains<T>(this T[] array, T item) where T : IComparable<T>
        {
            if (array == null || array.Length == 0)
            {
                return false;
            }
            bool isFound = false;

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].CompareTo(item) == 0)
                {
                    isFound = true;
                    break;
                }
            }

            return isFound;
        }
    }
}