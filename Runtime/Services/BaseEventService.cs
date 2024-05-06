// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Interfaces;
using RealityCollective.Utilities.Async;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RealityCollective.ServiceFramework.Services
{
    /// <summary>
    /// Base Event System that can be inherited from to give other system features event capabilities.
    /// </summary>
    public abstract class BaseEventService : BaseServiceWithConstructor, IEventService
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The service display name.</param>
        /// <param name="priority">The service initialization priority.</param>
        /// <param name="profile">The service configuration profile.</param>
        protected BaseEventService(string name, uint priority, BaseProfile profile) : base(name, priority) { }

        private static int eventExecutionDepth = 0;
        private readonly List<GameObject> eventListeners = new List<GameObject>();

        /// <inheritdoc />
        public IReadOnlyList<GameObject> EventListeners => eventListeners;

        /// <inheritdoc />
        public virtual void HandleEvent<T>(BaseEventData eventData, ExecuteEvents.EventFunction<T> eventHandler) where T : IEventSystemHandler
        {
            Debug.Assert(!eventData.used);
            eventExecutionDepth++;

            for (int i = EventListeners.Count - 1; i >= 0; i--)
            {
                var eventListener = EventListeners[i];
                Debug.Assert(eventListener != null, $"An object at index {i} has been destroyed but remains in the event handler list for {Name}.{nameof(EventListeners)}");
                ExecuteEvents.Execute(eventListener, eventData, eventHandler);
            }

            eventExecutionDepth--;
        }

        /// <inheritdoc />
        public virtual async void Register(GameObject listener)
        {
            if (eventListeners.Contains(listener)) { return; }

            if (eventExecutionDepth > 0)
            {
                try
                {
                    await eventExecutionDepth.WaitUntil(depth => eventExecutionDepth == 0);
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                    return;
                }
            }

            eventListeners.Add(listener);
        }

        /// <inheritdoc />
        public virtual async void Unregister(GameObject listener)
        {
            if (!eventListeners.Contains(listener)) { return; }

            if (eventExecutionDepth > 0)
            {
                try
                {
                    await eventExecutionDepth.WaitUntil(depth => eventExecutionDepth == 0);
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                    return;
                }
            }

            eventListeners.Remove(listener);
        }

        // Example Event Pattern #############################################################

        //public void RaiseGenericEvent(IEventSource eventSource)
        //{
        //    genericEventData.Initialize(eventSource);
        //    HandleEvent(genericEventData, GenericEventHandler);
        //}

        //private static readonly ExecuteEvents.EventFunction<IEventHandler> GenericEventHandler =
        //    delegate (IEventHandler handler, BaseEventData eventData)
        //    {
        //        var casted = ExecuteEvents.ValidateEventData<GenericBaseEventData>(eventData);
        //        handler.OnEventRaised(casted);
        //    };

        // Example Event Pattern #############################################################
    }
}
