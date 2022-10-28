// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RealityCollective.ServiceFramework.Interfaces
{
    /// <summary>
    /// Interface used to implement an event service that is compatible with Unity's <see cref="EventSystem"/>.
    /// </summary>
    public interface IEventService : IService
    {
        /// <summary>
        /// List of event listeners that are registered to this event service.
        /// </summary>
        IReadOnlyList<GameObject> EventListeners { get; }

        /// <summary>
        /// The main function for handling and forwarding all events to their intended recipients.
        /// </summary>
        /// <typeparam name="T">Event handler interface type</typeparam>
        /// <param name="eventData">Event data</param>
        /// <param name="eventHandler">Event handler delegate</param>
        /// <remarks>See: https://docs.unity3d.com/Manual/MessagingSystem.html for more information.</remarks>
        void HandleEvent<T>(BaseEventData eventData, ExecuteEvents.EventFunction<T> eventHandler) where T : IEventSystemHandler;

        /// <summary>
        /// Registers a <see cref="GameObject"/> to listen for events from this event service.
        /// </summary>
        /// <param name="listener"><see cref="GameObject"/> to add to <see cref="EventListeners"/>.</param>
        void Register(GameObject listener);

        /// <summary>
        /// Unregisters a <see cref="GameObject"/> from listening for events from this event service.
        /// </summary>
        /// <param name="listener"><see cref="GameObject"/> to remove from <see cref="EventListeners"/>.</param>
        void Unregister(GameObject listener);
    }
}