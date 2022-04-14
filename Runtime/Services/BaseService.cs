// Copyright (c) Reality Collective. All rights reserved.

using System;
using System.Collections.Generic;
using RealityToolkit.ServiceFramework.Interfaces;
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
                ServiceManager.TryUnregisterService(dataProvider);
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
        public virtual void Start() { }

        /// <inheritdoc />
        public virtual void Reset() { }

        /// <inheritdoc />
        public virtual void Enable() => IsEnabled = true;

        /// <inheritdoc />
        public virtual void Update() { }

        /// <inheritdoc />
        public virtual void LateUpdate() { }

        /// <inheritdoc />
        public virtual void FixedUpdate() { }

        /// <inheritdoc />
        public virtual void Disable() => IsEnabled = false;

        /// <inheritdoc />
        public virtual void Destroy()
        {
            isDestroying = true;
            IsEnabled = false;
        }

        /// <inheritdoc />
        public virtual void OnApplicationFocus(bool isFocused) { }

        /// <inheritdoc />
        public virtual void OnApplicationPause(bool isPaused) { }

        /// <inheritdoc />
        public virtual bool IsServiceRegistered => ServiceManager.IsServiceRegistered(this);

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
    }
}