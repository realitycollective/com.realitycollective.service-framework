// Copyright (c) Reality Collective. All rights reserved.

using RealityToolkit.ServiceFramework.Services;
using UnityEngine;

namespace RealityToolkit.ServiceFramework
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class MonoBehaviourRelay : MonoBehaviour
    {
        private ServiceManager serviceManagerInstace;

        public void SubscribetoUnityEvents(ServiceManager serviceManager)
        {
            serviceManagerInstace = serviceManager;
        }

        #region MonoBehaviour Implementation
#if UNITY_EDITOR
        private void OnValidate() => serviceManagerInstace?.OnValidate();
#endif // UNITY_EDITOR

        private void Awake() =>serviceManagerInstace?.Awake();

        private void OnEnable() => serviceManagerInstace?.OnEnable();

        private void Start() => serviceManagerInstace?.Start();

        private void Update() =>serviceManagerInstace?.Update();

        private void LateUpdate() => serviceManagerInstace?.LateUpdate();

        private void FixedUpdate() => serviceManagerInstace?.FixedUpdate();

        private void OnDisable() => serviceManagerInstace?.OnDisable();

        internal void OnDestroy() => serviceManagerInstace?.OnDestroy();

        private void OnApplicationFocus(bool focus) => serviceManagerInstace?.OnApplicationFocus(focus);

        private void OnApplicationPause(bool pause) => serviceManagerInstace?.OnApplicationPause(pause);

        #endregion MonoBehaviour Implementation
    }
}