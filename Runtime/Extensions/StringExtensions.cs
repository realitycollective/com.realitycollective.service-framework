﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Text;

namespace RealityCollective.ServiceFramework.Extensions
{
    /// <summary>
    /// <see cref="string"/> Extensions.
    /// </summary>
    public static class StringExtensions
    {
        public const string WhiteSpace = " ";

        /// <summary>
        /// Capitalize the first character and add a space before each capitalized letter (except the first character).
        /// </summary>
        /// <param name="value"></param>
        public static string ToProperCase(this string value)
        {
            // If there are 0 or 1 characters, just return the string.
            if (value == null) { return string.Empty; }
            if (value.Length < 4) { return value.ToUpper(); }
            // If there's already spaces in the string, return.
            if (value.Contains(WhiteSpace)) { return value; }

            // Start with the first character.
            var result = new StringBuilder(value.Substring(0, 1).ToUpper());

            // Add the remaining characters.
            for (int i = 1; i < value.Length; i++)
            {
                var wasLastCharUpper = char.IsUpper(value[i - 1]);
                var nextIsLower = i + 1 < value.Length && char.IsLower(value[i + 1]);
                var isUpper = char.IsLetter(value[i]) && char.IsUpper(value[i]);

                if (isUpper && !wasLastCharUpper && nextIsLower)
                {
                    result.Append(WhiteSpace);
                }

                result.Append(value[i]);

                if (isUpper && wasLastCharUpper && !nextIsLower)
                {
                    result.Append(WhiteSpace);
                }
            }

            return result.ToString();
        }

        /// <summary>
        /// Replaces all back slashes in the string with forward slashes.
        /// </summary>
        public static string ForwardSlashes(this string value)
            => value.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        /// <summary>
        /// Replaces all forward slashes in the string with back slashes.
        /// </summary>
        public static string BackSlashes(this string value)
            => value.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
    }
}