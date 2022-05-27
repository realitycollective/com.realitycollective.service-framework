// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Services;
using UnityEditor;
using UnityEngine;

namespace RealityCollective.ServiceFramework
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class ServiceManagerInstance : MonoBehaviour
    {
        private ServiceManager serviceManagerInstance;

        public ServiceManager Manager => serviceManagerInstance;

        [SerializeField]
        [Tooltip("All the additional non-required services registered with the Service Manager.")]
        private ServiceProvidersProfile serviceProvidersProfile = null;

        /// <summary>
        /// Check to see if the Service Manager is available and a profile is available to apply
        /// </summary>
        private bool isServiceManagerInstanceConfigured => serviceManagerInstance != null && serviceProvidersProfile != null;

        /// <summary>
        /// All the additional non-required systems, features, and managers registered with the Service Manager.
        /// </summary>
        public ServiceProvidersProfile ServiceProvidersProfile
        {
            get => serviceManagerInstance.ActiveProfile;
            internal set => serviceManagerInstance.ResetProfile(value);
        }

        /// <summary>
        /// Wrapper to enable the Service Manager to maintain this management <see cref="GameObject"/>
        /// </summary>
        /// <param name="serviceManager"></param>
        internal void SubscribetoUnityEvents(ServiceManager serviceManager)
        {
            serviceManagerInstance = serviceManager;
        }

        internal void InitialiseServiceManager()
        {
            if (serviceManagerInstance == null)
            {
                serviceManagerInstance = new ServiceManager(this.gameObject, serviceProvidersProfile);

                if (Application.isPlaying)
                {
                    DontDestroyOnLoad(transform.root);
                }
            }
        }

        #region MonoBehaviour Implementation
#if UNITY_EDITOR
        private void OnValidate()
        {
            if(EditorApplication.isPlaying || !gameObject.activeInHierarchy || !enabled)
            {
                return;
            }

            if(GameObject.FindObjectsOfType<ServiceManagerInstance>().Length > 1)
            {
                Debug.LogError($"There are multiple instances of the {nameof(ServiceManagerInstance)} in the Scene, this is not supported");
            }

            if (serviceManagerInstance == null)
            {
                InitialiseServiceManager();
            }

            if (isServiceManagerInstanceConfigured && 
                (serviceManagerInstance.ActiveProfile == null || 
                serviceManagerInstance.ActiveProfile != serviceProvidersProfile))
            {
                serviceManagerInstance.ResetProfile(serviceProvidersProfile);
            }
            serviceManagerInstance?.OnValidate();
        }
#endif // UNITY_EDITOR

        private void Awake()
        {
            if (Application.isPlaying && gameObject.activeInHierarchy && enabled)
            {
                InitialiseServiceManager();
            }
            serviceManagerInstance?.Awake();
        }

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