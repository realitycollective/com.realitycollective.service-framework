// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace RealityCollective.ServiceFramework.Extensions
{
    /// <summary>
    /// Extensions methods for the Unity Component class.
    /// This also includes some component-related extensions for the GameObject class.
    /// </summary>
    public static class ComponentExtensions
    {
        /// <summary>
        /// Ensure that a component of type <typeparamref name="T"/> exists on the game object.
        /// If it doesn't exist, creates it.
        /// </summary>
        /// <typeparam name="T">Type of the component.</typeparam>
        /// <param name="gameObject">Game object on which component should be.</param>
        /// <returns>The component that was retrieved or created.</returns>
        /// <remarks>
        /// This extension has to remain in this class as it is required by the <see cref="EnsureComponent{T}(Component)"/> method
        /// </remarks>
        public static T EnsureComponent<T>(this GameObject gameObject) where T : Component
        {
            T foundComponent = gameObject.GetComponent<T>();
            return foundComponent.IsNull() ? gameObject.AddComponent<T>() : foundComponent;
        }
    }
}