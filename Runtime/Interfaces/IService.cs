// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace RealityCollective.ServiceFramework.Interfaces
{
    /// <summary>
    /// Generic interface for all Services
    /// </summary>    
    public interface IService : IDisposable
    {
        /// <summary>
        /// Cached <see cref="Guid"/> Reference for the Service / Module
        /// </summary>
        Guid ServiceGuid { get; }

        /// <summary>
        /// The service display name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Optional Priority to reorder registered managers based on their respective priority, reduces the risk of race conditions by prioritizing the order in which services are evaluated.
        /// </summary>
        uint Priority { get; }

        /// <summary>
        /// Base property to denote whether a service is currently active and being updated by the Service Manager
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// The initialize function is used to setup the service once created.
        /// This method is called once all services have been registered in the ServiceManager.
        /// </summary>
        void Initialize();

        /// <summary>
        /// The start function is used to for running on the first frame.
        /// This method is called once all services have been registered in the ServiceManager and the application is in play mode.
        /// </summary>
        void Start();

        /// <summary>
        /// Optional Reset function to perform that will Reset the service, for example, whenever there is a profile change.
        /// </summary>
        void Reset();

        /// <summary>
        /// Optional Enable function to enable / re-enable the service.
        /// </summary>
        void Enable();

        /// <summary>
        /// Optional Update function to perform per-frame updates of the service.
        /// </summary>
        void Update();

        /// <summary>
        /// Optional LateUpdate function to perform updates after the end of the last Update cycle.
        /// </summary>
        void LateUpdate();

        /// <summary>
        /// Optional FixedUpdate function to perform updates that are fixed to the Physics timestep.
        /// </summary>
        void FixedUpdate();

        /// <summary>
        /// Optional Disable function to pause the service.
        /// </summary>
        void Disable();

        /// <summary>
        /// Optional Destroy function to perform cleanup of the service before the Service Manager is destroyed.
        /// </summary>
        void Destroy();

        /// <summary>
        /// Optional function that is called when the application gains or looses focus.
        /// </summary>
        /// <param name="isFocused"></param>
        void OnApplicationFocus(bool isFocused);

        /// <summary>
        /// Optional function that is called when the application is paused or un-paused.
        /// </summary>
        /// <param name="isPaused"></param>
        void OnApplicationPause(bool isPaused);

        /// <summary>
        /// List of Service Modules to be managed by a Service Implementation.
        /// Not to be used for Service Modules themselves.
        /// </summary>
        IReadOnlyCollection<IServiceModule> ServiceModules { get; }

        /// <summary>
        /// Register a Service Module with its parent service
        /// </summary>
        /// <param name="serviceModule"></param>
        void RegisterServiceModule(IServiceModule serviceModule);

        /// <summary>
        /// UnRegister a Service Module with its parent service
        /// </summary>
        /// <param name="serviceModule"></param>
        void UnRegisterServiceModule(IServiceModule serviceModule);

        /// <summary>
        /// Is this service currently registered with the Service Manager?
        /// </summary>
        /// <returns></returns>
        bool IsServiceRegistered { get; }

        /// <summary>
        /// Should services modules be automatically registered for this Service and be maintained by the Service Framework, or are they managed internally by the Service itself.
        /// </summary>
        bool RegisterServiceModules { get; }
    }
}