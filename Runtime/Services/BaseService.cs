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
        private readonly HashSet<Interfaces.IServiceProvider> serviceProviders = new HashSet<Interfaces.IServiceProvider>();

        private static bool isDestroying = false;

        private Guid guid;

        #region IService Implementation

        /// <inheritdoc />
        public Guid ServiceGuid => guid;

        /// <inheritdoc />
        public virtual IReadOnlyCollection<Interfaces.IServiceProvider> ServiceProviders => serviceProviders;

        /// <inheritdoc />
        public virtual void RegisterServiceProvider(Interfaces.IServiceProvider serviceProvider)
        {
            if (!serviceProvider.ParentService.IsServiceRegistered)
            {
                Debug.LogError($"Cannot register {serviceProvider.GetType().Name} as its Parent Service [{serviceProvider.ParentService.Name}] is not registered");
                return;
            }
            serviceProviders.Add(serviceProvider);
        }

        /// <inheritdoc />
        public virtual void UnRegisterServiceProvider(Interfaces.IServiceProvider serviceProvider)
        {
            if (!isDestroying && serviceProvider.IsServiceRegistered)
            {
                ServiceManager.Instance?.TryUnregisterService(serviceProvider);
            }
            serviceProviders.Remove(serviceProvider);
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
        public virtual void Start() => StartAllServiceProviders();

        /// <inheritdoc />
        public virtual void Reset() => ResetAllServiceProviders();

        /// <inheritdoc />
        public virtual void Enable() => EnableAllServiceProviders();

        /// <inheritdoc />
        public virtual void Update() => UpdateAllServiceProviders();

        /// <inheritdoc />
        public virtual void LateUpdate() => LateUpdateAllServiceProviders();

        /// <inheritdoc />
        public virtual void FixedUpdate() => FixedUpdateAllServiceProviders();

        /// <inheritdoc />
        public virtual void Disable() => DisableAllServiceProviders();

        /// <inheritdoc />
        public virtual void Destroy()
        {
            isDestroying = true;
            IsEnabled = false;
            DestroyAllServiceProviders();
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

        public virtual bool RegisterServiceProviders => true;

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


        private bool NoServiceProvidersFound => (serviceProviders == null || serviceProviders.Count == 0);

        #region MonoBehaviour Replicators
        internal void StartAllServiceProviders()
        {
            // If the service providers are being registered in the Service Registry automatically, exit.
            if (!RegisterServiceProviders)
            {
                return;
            }

            // If there are no service providers are configured, exit
            if (NoServiceProvidersFound)
            {
                return;
            }

            // Start all service providers
            foreach (var provider in serviceProviders)
            {
                try
                {
                    if (provider.IsEnabled)
                    {
                        provider.Start();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        internal void ResetAllServiceProviders()
        {
            // If the service providers are being registered in the Service Registry automatically, exit.
            if (!RegisterServiceProviders)
            {
                return;
            }

            // If there are no service providers are configured, exit
            if (NoServiceProvidersFound)
            {
                return;
            }

            // Reset all service providers
            foreach (var provider in serviceProviders)
            {
                try
                {
                    provider.Reset();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        internal void EnableAllServiceProviders()
        {
            IsEnabled = true;

            // If the service providers are being registered in the Service Registry automatically, exit.
            if (!RegisterServiceProviders)
            {
                return;
            }

            // If there are no service providers are configured, exit
            if (NoServiceProvidersFound)
            {
                return;
            }

            // Enable all service providers
            foreach (var provider in serviceProviders)
            {
                try
                {
                    provider.Enable();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        internal void UpdateAllServiceProviders()
        {
            // If the service providers are being registered in the Service Registry automatically, exit.
            if (!RegisterServiceProviders)
            {
                return;
            }

            // If there are no service providers are configured, exit
            if (NoServiceProvidersFound)
            {
                return;
            }

            // Update all service providers
            foreach (var provider in serviceProviders)
            {
                try
                {
                    if (provider.IsEnabled)
                    {
                        provider.Update();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        internal void LateUpdateAllServiceProviders()
        {
            // If the service providers are being registered in the Service Registry automatically, exit.
            if (!RegisterServiceProviders)
            {
                return;
            }

            // If there are no service providers are configured, exit
            if (NoServiceProvidersFound)
            {
                return;
            }

            // Late update all service providers
            foreach (var provider in serviceProviders)
            {
                try
                {
                    if (provider.IsEnabled)
                    {
                        provider.LateUpdate();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        internal void FixedUpdateAllServiceProviders()
        {
            // If the service providers are being registered in the Service Registry automatically, exit.
            if (!RegisterServiceProviders)
            {
                return;
            }

            // If there are no service providers are configured, exit
            if (NoServiceProvidersFound)
            {
                return;
            }

            // Fix update all service providers
            foreach (var provider in serviceProviders)
            {
                try
                {
                    if (provider.IsEnabled)
                    {
                        provider.FixedUpdate();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        public void DisableAllServiceProviders()
        {
            IsEnabled = false;

            // If the service providers are being registered in the Service Registry automatically, exit.
            if (!RegisterServiceProviders)
            {
                return;
            }

            // If there are no service providers are configured, exit
            if (NoServiceProvidersFound)
            {
                return;
            }

            // Disable all service providers
            foreach (var provider in serviceProviders)
            {
                try
                {
                    provider.Disable();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        public void DestroyAllServiceProviders()
        {
            // If the service providers are being registered in the Service Registry automatically, exit.
            if (!RegisterServiceProviders)
            {
                return;
            }

            // If there are no service providers are configured, exit
            if (NoServiceProvidersFound)
            {
                return;
            }

            Interfaces.IServiceProvider[] serviceProvidersClone = new Interfaces.IServiceProvider[serviceProviders.Count];
            serviceProviders.CopyTo(serviceProvidersClone);
            serviceProviders.Clear();

            // Destroy all service providers
            foreach (var provider in serviceProvidersClone)
            {
                try
                {
                    provider.Destroy();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }

            // Dispose all service providers
            foreach (var provider in serviceProvidersClone)
            {
                try
                {
                    provider.Dispose();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }

            serviceProviders.Clear();
        }
        #endregion MonoBehaviour Replicators
    }
}