// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.ServiceFramework.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealityToolkit.ServiceFramework.Services
{
    /// <summary>
    /// Base <see cref="IService"/> Implementation.
    /// </summary>
    public class BaseService : IService
    {
        private readonly HashSet<IServiceDataProvider> dataProviders = new HashSet<IServiceDataProvider>();

        private static bool isDestroying = false;

        private Guid guid;

        #region IService Implementation

        /// <inheritdoc />
        public Guid ServiceGuid => guid;

        /// <inheritdoc />
        public virtual IReadOnlyCollection<IServiceDataProvider> DataProviders => dataProviders;

        /// <inheritdoc />
        public virtual void RegisterDataProvider(IServiceDataProvider dataProvider)
        {
            if (!dataProvider.ParentService.IsServiceRegistered)
            {
                Debug.LogError($"Cannot register {nameof(dataProvider)} as its Parent Service [{dataProvider.ParentService.Name}] is not registered");
                return;
            }
            dataProviders.Add(dataProvider);
        }

        /// <inheritdoc />
        public virtual void UnRegisterDataProvider(IServiceDataProvider dataProvider)
        {
            if (!isDestroying && dataProvider.IsServiceRegistered)
            {
                ServiceManager.Instance?.TryUnregisterService(dataProvider);
            }
            dataProviders.Remove(dataProvider);
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
        public virtual void Start() => StartAllDataProviders();

        /// <inheritdoc />
        public virtual void Reset() => ResetAllDataProviders();

        /// <inheritdoc />
        public virtual void Enable() => EnableAllDataProviders();

        /// <inheritdoc />
        public virtual void Update() => UpdateAllDataProviders();

        /// <inheritdoc />
        public virtual void LateUpdate() => LateUpdateAllDataProviders();

        /// <inheritdoc />
        public virtual void FixedUpdate() => FixedUpdateAllDataProviders();

        /// <inheritdoc />
        public virtual void Disable() => DisableAllDataProviders();

        /// <inheritdoc />
        public virtual void Destroy()
        {
            isDestroying = true;
            IsEnabled = false;
            DestroyAllDataProviders();
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

        public virtual bool RegisterDataProviders => true;

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


        private bool NoDataProvidersFound => (dataProviders == null || dataProviders.Count == 0);

        #region MonoBehaviour Replicators
        internal void StartAllDataProviders()
        {
            // If the data providers are being registered in the Service Registry automatically, exit.
            if (!RegisterDataProviders)
            {
                return;
            }

            // If there are no data providers are configured, exit
            if (NoDataProvidersFound)
            {
                return;
            }

            // Start all data providers
            foreach (var dataProvider in dataProviders)
            {
                try
                {
                    if (dataProvider.IsEnabled)
                    {
                        dataProvider.Start();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        internal void ResetAllDataProviders()
        {
            // If the data providers are being registered in the Service Registry automatically, exit.
            if (!RegisterDataProviders)
            {
                return;
            }

            // If there are no data providers are configured, exit
            if (NoDataProvidersFound)
            {
                return;
            }

            // Reset all data providers
            foreach (var dataProvider in dataProviders)
            {
                try
                {
                    dataProvider.Reset();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        internal void EnableAllDataProviders()
        {
            IsEnabled = true;

            // If the data providers are being registered in the Service Registry automatically, exit.
            if (!RegisterDataProviders)
            {
                return;
            }

            // If there are no data providers are configured, exit
            if (NoDataProvidersFound)
            {
                return;
            }

            // Enable all data providers
            foreach (var dataProvider in dataProviders)
            {
                try
                {
                    dataProvider.Enable();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        internal void UpdateAllDataProviders()
        {
            // If the data providers are being registered in the Service Registry automatically, exit.
            if (!RegisterDataProviders)
            {
                return;
            }

            // If there are no data providers are configured, exit
            if (NoDataProvidersFound)
            {
                return;
            }

            // Update all data providers
            foreach (var dataProvider in dataProviders)
            {
                try
                {
                    if (dataProvider.IsEnabled)
                    {
                        dataProvider.Update();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        internal void LateUpdateAllDataProviders()
        {
            // If the data providers are being registered in the Service Registry automatically, exit.
            if (!RegisterDataProviders)
            {
                return;
            }

            // If there are no data providers are configured, exit
            if (NoDataProvidersFound)
            {
                return;
            }

            // Late update all data providers
            foreach (var dataProvider in dataProviders)
            {
                try
                {
                    if (dataProvider.IsEnabled)
                    {
                        dataProvider.LateUpdate();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        internal void FixedUpdateAllDataProviders()
        {
            // If the data providers are being registered in the Service Registry automatically, exit.
            if (!RegisterDataProviders)
            {
                return;
            }

            // If there are no data providers are configured, exit
            if (NoDataProvidersFound)
            {
                return;
            }

            // Fix update all data providers
            foreach (var dataProvider in dataProviders)
            {
                try
                {
                    if (dataProvider.IsEnabled)
                    {
                        dataProvider.FixedUpdate();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        public void DisableAllDataProviders()
        {
            IsEnabled = false;

            // If the data providers are being registered in the Service Registry automatically, exit.
            if (!RegisterDataProviders)
            {
                return;
            }

            // If there are no data providers are configured, exit
            if (NoDataProvidersFound)
            {
                return;
            }

            // Disable all data providers
            foreach (var dataProvider in dataProviders)
            {
                try
                {
                    dataProvider.Disable();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        public void DestroyAllDataProviders()
        {
            // If the data providers are being registered in the Service Registry automatically, exit.
            if (!RegisterDataProviders)
            {
                return;
            }

            // If there are no data providers are configured, exit
            if (NoDataProvidersFound)
            {
                return;
            }

            IServiceDataProvider[] dataProvidersClone = new IServiceDataProvider[dataProviders.Count];
            dataProviders.CopyTo(dataProvidersClone);
            dataProviders.Clear();

            // Destroy all data providers
            foreach (var dataProvider in dataProvidersClone)
            {
                try
                {
                    dataProvider.Destroy();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }

            // Dispose all data providers
            foreach (var dataProvider in dataProvidersClone)
            {
                try
                {
                    dataProvider.Dispose();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }

            dataProviders.Clear();
        }
        #endregion MonoBehaviour Replicators
    }
}