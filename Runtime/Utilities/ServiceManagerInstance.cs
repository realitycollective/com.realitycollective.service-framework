// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.ServiceFramework.Services;
using UnityEngine;

namespace RealityToolkit.ServiceFramework
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class ServiceManagerInstance : MonoBehaviour
    {
        private ServiceManager serviceManagerInstance;

        public void SubscribetoUnityEvents(ServiceManager serviceManager)
        {
            serviceManagerInstance = serviceManager;
        }

        #region MonoBehaviour Implementation
#if UNITY_EDITOR
        private void OnValidate() => serviceManagerInstance?.OnValidate();
#endif // UNITY_EDITOR

        private void Awake() =>serviceManagerInstance?.Awake();

        private void OnEnable() => serviceManagerInstance?.OnEnable();

        private void Start() => serviceManagerInstance?.Start();

        private void Update() =>serviceManagerInstance?.Update();

        private void LateUpdate() => serviceManagerInstance?.LateUpdate();

        private void FixedUpdate() => serviceManagerInstance?.FixedUpdate();

        private void OnDisable() => serviceManagerInstance?.OnDisable();

        internal void OnDestroy() => serviceManagerInstance?.OnDestroy();

        private void OnApplicationFocus(bool focus) => serviceManagerInstance?.OnApplicationFocus(focus);

        private void OnApplicationPause(bool pause) => serviceManagerInstance?.OnApplicationPause(pause);

        #endregion MonoBehaviour Implementation
    }
}