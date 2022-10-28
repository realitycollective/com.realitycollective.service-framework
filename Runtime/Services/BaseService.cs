// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityCollective.ServiceFramework.Services
{
    /// <summary>
    /// Base <see cref="IService"/> Implementation.
    /// </summary>
    public class BaseService : IService
    {
        private readonly HashSet<IServiceModule> serviceModules = new HashSet<IServiceModule>();

        private static bool isDestroying = false;

        private Guid guid;

        #region IService Implementation

        /// <inheritdoc />
        public Guid ServiceGuid => guid;

        /// <inheritdoc />
        public virtual IReadOnlyCollection<IServiceModule> ServiceModules => serviceModules;

        /// <inheritdoc />
        public virtual void RegisterServiceModule(IServiceModule serviceModule)
        {
            if (!serviceModule.ParentService.IsServiceRegistered)
            {
                Debug.LogError($"Cannot register {serviceModule.GetType().Name} as its Parent Service [{serviceModule.ParentService.Name}] is not registered");
                return;
            }
            serviceModules.Add(serviceModule);
        }

        /// <inheritdoc />
        public virtual void UnRegisterServiceModule(IServiceModule serviceModule)
        {
            if (!isDestroying && serviceModule.IsServiceRegistered)
            {
                ServiceManager.Instance?.TryUnregisterService(serviceModule);
            }
            serviceModules.Remove(serviceModule);
        }

        /// <inheritdoc />
        public virtual string Name { get; protected set; }

        /// <inheritdoc />
        public virtual uint Priority { get; protected set; }

        /// <inheritdoc />
        public virtual bool IsEnabled { get; protected set; } = true;

        /// <inheritdoc />
        public virtual void Initialize() { }

        /// <inheritdoc />
        public virtual void Start() => StartAllserviceModules();

        /// <inheritdoc />
        public virtual void Reset() => ResetAllserviceModules();

        /// <inheritdoc />
        public virtual void Enable() => EnableAllserviceModules();

        /// <inheritdoc />
        public virtual void Update() => UpdateAllserviceModules();

        /// <inheritdoc />
        public virtual void LateUpdate() => LateUpdateAllserviceModules();

        /// <inheritdoc />
        public virtual void FixedUpdate() => FixedUpdateAllserviceModules();

        /// <inheritdoc />
        public virtual void Disable() => DisableAllserviceModules();

        /// <inheritdoc />
        public virtual void Destroy()
        {
            isDestroying = true;
            IsEnabled = false;
            DestroyAllserviceModules();
        }

        /// <inheritdoc />
        public virtual void OnApplicationFocus(bool isFocused) { }

        /// <inheritdoc />
        public virtual void OnApplicationPause(bool isPaused) { }

        /// <inheritdoc />
        public virtual bool IsServiceRegistered
        {
            get
            {
                if (ServiceManager.Instance == null)
                {
                    return false;
                }
                return ServiceManager.Instance.IsServiceRegistered(this);
            }
        }

        public virtual bool RegisterServiceModules => true;

        #endregion IService Implementation

        /// <summary>
        /// Constructor.
        /// </summary>
        protected BaseService()
        {
            this.guid = GetType().GUID;
            isDestroying = false;
        }

        #region IDisposable Implementation

        private bool disposed;

        ~BaseService()
        {
            OnDispose(true);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (disposed) { return; }
            disposed = true;
            GC.SuppressFinalize(this);
            OnDispose(false);
            isDestroying = true;
        }

        protected virtual void OnDispose(bool finalizing) { }

        #endregion IDisposable Implementation


        private bool noServiceModulesFound => (serviceModules == null || serviceModules.Count == 0);

        #region MonoBehaviour Replicators
        internal void StartAllserviceModules()
        {
            // If the Service Modules are being registered in the Service Registry automatically, exit.
            if (!RegisterServiceModules)
            {
                return;
            }

            // If there are no Service Modules are configured, exit
            if (noServiceModulesFound)
            {
                return;
            }

            // Start all Service Modules
            foreach (var serviceModule in serviceModules)
            {
                try
                {
                    if (serviceModule.IsEnabled)
                    {
                        serviceModule.Start();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        internal void ResetAllserviceModules()
        {
            // If the Service Modules are being registered in the Service Registry automatically, exit.
            if (!RegisterServiceModules)
            {
                return;
            }

            // If there are no Service Modules are configured, exit
            if (noServiceModulesFound)
            {
                return;
            }

            // Reset all Service Modules
            foreach (var serviceModule in serviceModules)
            {
                try
                {
                    serviceModule.Reset();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        internal void EnableAllserviceModules()
        {
            IsEnabled = true;

            // If the Service Modules are being registered in the Service Registry automatically, exit.
            if (!RegisterServiceModules)
            {
                return;
            }

            // If there are no Service Modules are configured, exit
            if (noServiceModulesFound)
            {
                return;
            }

            // Enable all Service Modules
            foreach (var serviceModule in serviceModules)
            {
                try
                {
                    serviceModule.Enable();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        internal void UpdateAllserviceModules()
        {
            // If the Service Modules are being registered in the Service Registry automatically, exit.
            if (!RegisterServiceModules)
            {
                return;
            }

            // If there are no Service Modules are configured, exit
            if (noServiceModulesFound)
            {
                return;
            }

            // Update all Service Modules
            foreach (var serviceModule in serviceModules)
            {
                try
                {
                    if (serviceModule.IsEnabled)
                    {
                        serviceModule.Update();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        internal void LateUpdateAllserviceModules()
        {
            // If the Service Modules are being registered in the Service Registry automatically, exit.
            if (!RegisterServiceModules)
            {
                return;
            }

            // If there are no Service Modules are configured, exit
            if (noServiceModulesFound)
            {
                return;
            }

            // Late update all Service Modules
            foreach (var serviceModule in serviceModules)
            {
                try
                {
                    if (serviceModule.IsEnabled)
                    {
                        serviceModule.LateUpdate();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        internal void FixedUpdateAllserviceModules()
        {
            // If the Service Modules are being registered in the Service Registry automatically, exit.
            if (!RegisterServiceModules)
            {
                return;
            }

            // If there are no Service Modules are configured, exit
            if (noServiceModulesFound)
            {
                return;
            }

            // Fix update all Service Modules
            foreach (var serviceModule in serviceModules)
            {
                try
                {
                    if (serviceModule.IsEnabled)
                    {
                        serviceModule.FixedUpdate();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        public void DisableAllserviceModules()
        {
            IsEnabled = false;

            // If the Service Modules are being registered in the Service Registry automatically, exit.
            if (!RegisterServiceModules)
            {
                return;
            }

            // If there are no Service Modules are configured, exit
            if (noServiceModulesFound)
            {
                return;
            }

            // Disable all Service Modules
            foreach (var serviceModule in serviceModules)
            {
                try
                {
                    serviceModule.Disable();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        public void DestroyAllserviceModules()
        {
            // If the Service Modules are being registered in the Service Registry automatically, exit.
            if (!RegisterServiceModules)
            {
                return;
            }

            // If there are no Service Modules are configured, exit
            if (noServiceModulesFound)
            {
                return;
            }

            IServiceModule[] serviceModulesClone = new IServiceModule[serviceModules.Count];
            serviceModules.CopyTo(serviceModulesClone);
            serviceModules.Clear();

            // Destroy all Service Modules
            foreach (var serviceModule in serviceModulesClone)
            {
                try
                {
                    serviceModule.Destroy();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }

            // Dispose all Service Modules
            foreach (var serviceModule in serviceModulesClone)
            {
                try
                {
                    serviceModule.Dispose();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }

            serviceModules.Clear();
        }
        #endregion MonoBehaviour Replicators
    }
}