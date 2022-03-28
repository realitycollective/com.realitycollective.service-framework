// Copyright (c) Reality Collective. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RealityToolkit.ServiceFramework.Definitions;
using RealityToolkit.ServiceFramework.Extensions;
using RealityToolkit.ServiceFramework.Interfaces;
using RealityToolkit.ServiceFramework.Utilities;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace RealityToolkit.ServiceFramework.Services
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class ServiceManager : MonoBehaviour, IDisposable
    {
        #region Service Manager Profile properties

        /// <summary>
        /// Checks if there is a valid instance of the Service Manager, then checks if there is there a valid Active Profile.
        /// </summary>
        public static bool HasActiveProfile
        {
            get
            {
                if (!IsInitialized)
                {
                    return false;
                }

                if (!ConfirmInitialized())
                {
                    return false;
                }

                return Instance.ActiveProfile != null;
            }
        }

        public static bool InitialiseOnPlay = false;
        /// <summary>
        /// The active profile of the Service Manager which controls which services are active and their initial settings.
        /// *Note a profile is used on project initialization or replacement, changes to properties while it is running has no effect.
        /// </summary>
        [SerializeField]
        [Tooltip("The current active settings for the Service Manager project")]
        private ServiceManagerRootProfile activeProfile = null;

        /// <summary>
        /// The public property of the Active Profile, ensuring events are raised on the change of the reference
        /// </summary>
        public ServiceManagerRootProfile ActiveProfile
        {
            get
            {
#if UNITY_EDITOR
                if (!Application.isPlaying &&
                    activeProfile.IsNull() &&
                    UnityEditor.Selection.activeObject != Instance)
                {
                    UnityEditor.Selection.activeObject = Instance;
                }
#endif // UNITY_EDITOR
                return activeProfile;
            }
            set
            {
                ResetProfile(value);
            }
        }

        /// <summary>
        /// When a profile is replaced with a new one, force all services to reset and read the new values
        /// </summary>
        /// <param name="profile"></param>
        public void ResetProfile(ServiceManagerRootProfile profile)
        {
            if (Application.isEditor && Application.isPlaying)
            {
                // The application is running in editor play mode, can't
                // reset profiles in this state as it will cause destruction
                // and reinitialization of services in use.
                return;
            }

            if (isResetting)
            {
                Debug.LogWarning("Already attempting to reset the root profile!");
                return;
            }

            if (isInitializing)
            {
                Debug.LogWarning("Already attempting to initialize the root profile!");
                return;
            }

            isResetting = true;

            if (!activeProfile.IsNull())
            {
                DisableAllServices();
                DestroyAllServices();
            }

            activeProfile = profile;

            if (!profile.IsNull())
            {
                DisableAllServices();
                DestroyAllServices();
            }

            InitializeServiceLocator();

            isResetting = false;
        }

        private static bool isResetting = false;

        #endregion Service Manager Profile properties

        #region Service Manager runtime service registry

        private static readonly Dictionary<Type, IService> activeServices = new Dictionary<Type, IService>();

        /// <summary>
        /// Current active services registered with the ServiceManager.
        /// </summary>
        /// <remarks>
        /// Services can only be registered once by <see cref="Type"/> and are executed in a specific priority order.
        /// </remarks>
        public static IReadOnlyDictionary<Type, IService> ActiveServices => activeServices;

        #endregion Service Manager runtime service registry

        #region Instance Management

        private static bool isGettingInstance = false;

        /// <summary>
        /// Returns the Singleton instance of the classes type.
        /// If no instance is found, then we search for an instance in the scene.
        /// If more than one instance is found, we log an error and no instance is returned.
        /// </summary>
        public static ServiceManager Instance
        {
            get
            {
                if (IsInitialized)
                {
                    return instance;
                }

                if (isGettingInstance ||
                   (Application.isPlaying && !searchForInstance))
                {
                    return null;
                }

                isGettingInstance = true;

                var objects = FindObjectsOfType<ServiceManager>();
                searchForInstance = false;
                ServiceManager newInstance;

                switch (objects.Length)
                {
                    case 0:
                        newInstance = new GameObject(nameof(ServiceManager)).AddComponent<ServiceManager>();
                        break;
                    case 1:
                        newInstance = objects[0];
                        break;
                    default:
                        Debug.LogError($"Expected exactly 1 {nameof(ServiceManager)} but found {objects.Length}.");
                        isGettingInstance = false;
                        return null;
                }

                if (newInstance == null)
                {
                    Debug.LogError("Failed to get instance!");
                    isGettingInstance = false;
                    return null;
                }

                if (!IsApplicationQuitting)
                {
                    // Setup any additional things the instance needs.
                    newInstance.InitializeInstance();
                }
                else
                {
                    // Don't do any additional setup because the app is quitting.
                    instance = newInstance;
                }

                if (instance == null)
                {
                    Debug.LogError("Failed to get instance!");
                    isGettingInstance = false;
                    return null;
                }

                isGettingInstance = false;
                return instance;
            }
        }

        private static ServiceManager instance;

        /// <summary>
        /// Lock property for the Service Manager to prevent reinitialization
        /// </summary>
        private static readonly object InitializedLock = new object();

        public void InitializeServiceManager(IServiceConfiguration<IService>[] serviceConfigurations = null, IServiceConfiguration<IService>[] serviceProviderConfigurations = null)
        {
            InitializeServiceLocator(serviceConfigurations,serviceProviderConfigurations);
        }

        private void InitializeInstance()
        {
            lock (InitializedLock)
            {
                if (IsInitialized) { return; }

                instance = this;

                if (Application.isPlaying)
                {
                    DontDestroyOnLoad(instance.transform.root);
                }

                Application.quitting += () =>
                {
                    DisableAllServices();
                    DestroyAllServices();
                    IsApplicationQuitting = true;
                };

#if UNITY_EDITOR
                UnityEditor.EditorApplication.hierarchyChanged += OnHierarchyChanged;
                UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

                void OnHierarchyChanged()
                {
                    if (instance != null)
                    {
                        Debug.Assert(instance.transform.parent == null, $"The {nameof(ServiceManager)} should not be parented under any other GameObject!");
                        Debug.Assert(instance.transform.childCount == 0, $"The {nameof(ServiceManager)} should not have GameObject children!");
                    }
                }

                void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange playModeState)
                {
                    switch (playModeState)
                    {
                        case UnityEditor.PlayModeStateChange.EnteredEditMode:
                            IsApplicationQuitting = false;
                            break;
                        case UnityEditor.PlayModeStateChange.ExitingEditMode:
                            if (activeProfile.IsNull())
                            {
                                Debug.LogError($"{nameof(ServiceManager)} has no active profile! Exiting playmode...");
                                UnityEditor.EditorApplication.isPlaying = false;
                                UnityEditor.Selection.activeObject = Instance;
                                UnityEditor.EditorApplication.delayCall += () =>
                                    UnityEditor.EditorGUIUtility.PingObject(Instance);
                            }
                            break;
                        case UnityEditor.PlayModeStateChange.EnteredPlayMode:
                        case UnityEditor.PlayModeStateChange.ExitingPlayMode:
                            // Nothing for now.
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(playModeState), playModeState, null);
                    }

                }
#endif // UNITY_EDITOR
                
                if (HasActiveProfile)
                {
                    InitializeServiceLocator();
                }
            }
        }

        /// <summary>
        /// Flag to search for instance the first time Instance property is called.
        /// Subsequent attempts will generally switch this flag false, unless the instance was destroyed.
        /// </summary>
        private static bool searchForInstance = true;

        private static bool isInitializing = false;

        /// <summary>
        /// Flag stating if the application is currently attempting to quit.
        /// </summary>
        public static bool IsApplicationQuitting { get; private set; } = false;

        /// <summary>
        /// Expose an assertion whether the Service Manager class is initialized.
        /// </summary>
        public static void AssertIsInitialized()
        {
            Debug.Assert(IsInitialized, $"The {nameof(ServiceManager)} has not been initialized.");
        }

        /// <summary>
        /// Returns whether the instance has been initialized or not.
        /// </summary>
        public static bool IsInitialized => instance != null;

        /// <summary>
        /// Static function to determine if the <see cref="ServiceManager"/> class has been initialized or not.
        /// </summary>
        public static bool ConfirmInitialized()
        {
            var access = Instance;
            Debug.Assert(IsInitialized.Equals(access != null));
            return IsInitialized;
        }

        /// <summary>
        /// Once all services are registered and properties updated, the Service Manager will initialize all active services.
        /// This ensures all services can reference each other once started.
        /// </summary>
        private void InitializeServiceLocator(IServiceConfiguration<IService>[] serviceConfigurations = null, IServiceConfiguration<IService>[] serviceProviderConfigurations = null)
        {
            if (isInitializing)
            {
                Debug.LogWarning($"Already attempting to initialize the {nameof(ServiceManager)}!");
                return;
            }

            isInitializing = true;
            if (serviceConfigurations == null && serviceProviderConfigurations == null)
            {
                // If the Service Manager is not configured, stop.
                if (ActiveProfile == null)
                {
                    Debug.LogError($"No {nameof(ServiceManagerRootProfile)} found, cannot initialize the {nameof(ServiceManager)}");
                    isInitializing = false;
                    return;
                }

#if UNITY_EDITOR
                if (ActiveServices.Count > 0)
                {
                    activeServices.Clear();
                }
#endif
                serviceConfigurations = ActiveProfile.ServiceConfigurations;
                serviceProviderConfigurations = ActiveProfile.ServiceProvidersProfile.ServiceConfigurations;
            }
            
            Debug.Assert(ActiveServices.Count == 0);

            ClearSystemCache();
            if (serviceConfigurations != null)
            {
                foreach (var configuration in serviceConfigurations)
                {
                    if (configuration.Enabled)
                    {
                        if (TryCreateAndRegisterService(configuration, out var service) && service != null)
                        {
                            if (configuration.Profile is IServiceProfile<IServiceDataProvider> profile)
                            {
                                TryRegisterDataProviderConfigurations(profile.ServiceConfigurations, service);
                            }
                        }
                        else
                        {
                            Debug.LogError($"Failed to start {configuration.Name}!");
                        }
                    }
                }  
            }

            if (serviceProviderConfigurations != null)
            {
                TryRegisterServiceConfigurations(serviceProviderConfigurations);
            }

            var orderedCoreSystems = activeServices.OrderBy(m => m.Value.Priority).ToArray();
            activeServices.Clear();

            foreach (var service in orderedCoreSystems)
            {
                TryRegisterService(service.Key, service.Value);
            }

#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                InitializeAllServices();
            }
            else if(!ActiveProfile.InitialiseOnPlay)
            {
                UnityEditor.EditorApplication.delayCall += InitializeAllServices;
            }
#else
            InitializeAllServices();
#endif

            isInitializing = false;
        }
        
        #endregion

        #region MonoBehaviour Implementation

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!IsInitialized &&
                !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                ConfirmInitialized();
            }
        }
#endif // UNITY_EDITOR

        private void Awake()
        {
            if (Application.isBatchMode || !Application.isEditor)
            {
                Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            }

            if (!Application.isPlaying) { return; }

            if (IsInitialized && instance != this)
            {
                gameObject.Destroy();
                Debug.LogWarning($"Trying to instantiate a second instance of the {nameof(ServiceManager)}. Additional Instance was destroyed");
            }
            else if (!IsInitialized && !InitialiseOnPlay)
            {
                InitializeInstance();
            }
        }

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                EnableAllServices();
            }
        }

        private void Start()
        {
            if (Application.isPlaying)
            {
                StartAllServices();
            }
        }

        private void Update()
        {
            if (Application.isPlaying)
            {
                UpdateAllServices();
            }
        }

        private void LateUpdate()
        {
            if (Application.isPlaying)
            {
                LateUpdateAllServices();
            }
        }

        private void FixedUpdate()
        {
            if (Application.isPlaying)
            {
                FixedUpdateAllServices();
            }
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                DisableAllServices();
            }
        }

        private void OnDestroy()
        {
            DestroyAllServices();
            ClearSystemCache();
            Dispose();
        }

        private void OnApplicationFocus(bool focus)
        {
            if (!Application.isPlaying) { return; }

            // If the Service Manager is not configured, stop.
            if (activeProfile == null) { return; }

            foreach (var system in activeServices)
            {
                system.Value.OnApplicationFocus(focus);
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (!Application.isPlaying) { return; }

            // If the Service Manager is not configured, stop.
            if (activeProfile == null) { return; }

            foreach (var system in activeServices)
            {
                system.Value.OnApplicationPause(pause);
            }
        }

        #endregion MonoBehaviour Implementation

        #region Registration

        /// <summary>
        /// Registers all the <see cref="IService"/>s defined in the provided configuration collection.
        /// </summary>
        /// <typeparam name="T">The interface type for the <see cref="IService"/> to be registered.</typeparam>
        /// <param name="configurations">The list of <see cref="IServiceConfiguration{T}"/>s.</param>
        /// <returns>True, if all configurations successfully created and registered their services.</returns>
        public static bool TryRegisterServiceConfigurations<T>(IServiceConfiguration<T>[] configurations) where T : IService
        {
            bool anyFailed = false;

            for (var i = 0; i < configurations?.Length; i++)
            {
                var configuration = configurations[i];

                if (TryCreateAndRegisterService(configuration, out var serviceInstance))
                {
                    if (configuration.Profile is IServiceProfile<IServiceDataProvider> profile &&
                        !TryRegisterDataProviderConfigurations(profile.ServiceConfigurations, serviceInstance))
                    {
                        anyFailed = true;
                    }
                }
                else
                {
                    Debug.LogError($"Failed to start {configuration.Name}!");
                    anyFailed = true;
                }
            }

            return !anyFailed;
        }

        /// <summary>
        /// Registers all the <see cref="IServiceDataProvider"/>s defined in the provided configuration collection.
        /// </summary>
        /// <typeparam name="T">The interface type for the <see cref="IServiceDataProvider"/> to be registered.</typeparam>
        /// <param name="configurations">The list of <see cref="IServiceConfiguration{T}"/>s.</param>
        /// <param name="serviceParent">The <see cref="IService"/> that the <see cref="IServiceDataProvider"/> will be assigned to.</param>
        /// <returns>True, if all configurations successfully created and registered their data providers.</returns>
        public static bool TryRegisterDataProviderConfigurations<T>(IServiceConfiguration<T>[] configurations, IService serviceParent) where T : IServiceDataProvider
        {
            bool anyFailed = false;

            for (var i = 0; i < configurations?.Length; i++)
            {
                var configuration = configurations[i];

                if (!TryCreateAndRegisterDataProvider(configuration, serviceParent))
                {
                    Debug.LogError($"Failed to start {configuration.Name}!");
                    anyFailed = true;
                }
            }

            return !anyFailed;
        }

        /// <summary>
        /// Add a service instance to the Service Manager active service registry.
        /// </summary>
        /// <typeparam name="T">The interface type for the <see cref="IService"/> to be registered.</typeparam>
        /// <param name="serviceInstance">Instance of the <see cref="IService"/> to register.</param>
        /// <returns>True, if the service was successfully registered.</returns>
        public static bool TryRegisterService<T>(IService serviceInstance) where T : IService
        {
            return TryRegisterService(typeof(T), serviceInstance);
        }

        /// <summary>
        /// Creates a new instance of a service and registers it to the Service Manager service registry for the specified platform.
        /// </summary>
        /// <typeparam name="T">The interface type for the <see cref="IService"/> to be registered.</typeparam>
        /// <param name="configuration">The <see cref="IServiceConfiguration{T}"/> to use to create and register the service.</param>
        /// <param name="service">If successful, then the new <see cref="IService"/> instance will be passed back out.</param>
        /// <returns>True, if the service was successfully created and registered.</returns>
        public static bool TryCreateAndRegisterService<T>(IServiceConfiguration<T> configuration, out T service) where T : IService
        {
            return TryCreateAndRegisterService(
                configuration.InstancedType,
                out service,
                configuration.Name,
                configuration.Priority,
                configuration.Profile);
        }

        /// <summary>
        /// Creates a new instance of a data provider and registers it to the Service Manager service registry for the specified platform.
        /// </summary>
        /// <typeparam name="T">The interface type for the <see cref="IService"/> to be registered.</typeparam>
        /// <param name="configuration">The <see cref="IServiceConfiguration{T}"/> to use to create and register the data provider.</param>
        /// <param name="serviceParent">The <see cref="IService"/> that the <see cref="IServiceDataProvider"/> will be assigned to.</param>
        /// <returns>True, if the service was successfully created and registered.</returns>
        public static bool TryCreateAndRegisterDataProvider<T>(IServiceConfiguration<T> configuration, IService serviceParent) where T : IServiceDataProvider
        {
            return TryCreateAndRegisterService<T>(
                configuration.InstancedType,
                out _,
                configuration.Name,
                configuration.Priority,
                configuration.Profile,
                serviceParent);
        }

        /// <summary>
        /// Creates a new instance of a service and registers it to the Service Manager service registry for the specified platform.
        /// </summary>
        /// <typeparam name="T">The interface type for the <see cref="IService"/> to be registered.</typeparam>
        /// <param name="concreteType">The concrete class type to instantiate.</param>
        /// <param name="service">If successful, then the new <see cref="IService"/> instance will be passed back out.</param>
        /// <param name="args">Optional arguments used when instantiating the concrete type.</param>
        /// <returns>True, if the service was successfully created and registered.</returns>
        private static bool TryCreateAndRegisterService<T>(Type concreteType, out T service, params object[] args) where T : IService
        {
            service = default;

            if (IsApplicationQuitting)
            {
                return false;
            }

            if (concreteType == null)
            {
                Debug.LogError($"Unable to register a service with a null concrete {typeof(T).Name} type.");
                return false;
            }

            if (!typeof(IService).IsAssignableFrom(concreteType))
            {
                Debug.LogError($"Unable to register the {concreteType.Name} service. It does not implement {typeof(IService)}.");
                return false;
            }

            IService serviceInstance;

            try
            {
                serviceInstance = Activator.CreateInstance(concreteType, args) as IService;
            }
            catch (System.Reflection.TargetInvocationException e)
            {
                Debug.LogError($"Failed to register the {concreteType.Name} service: {e.InnerException?.GetType()} - {e.InnerException?.Message}\n{e.InnerException?.StackTrace}");
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to register the {concreteType.Name} service: {e.GetType()} - {e.Message}\n{e.StackTrace}");
                return false;
            }

            service = (T)serviceInstance;

            if (service == null ||
                serviceInstance == null)
            {
                Debug.LogError($"Failed to create a valid instance of {concreteType.Name}!");
                return false;
            }

            return TryRegisterService(typeof(T), serviceInstance);
        }

        /// <summary>
        /// Service registration.
        /// </summary>
        /// <param name="interfaceType">The interface type for the <see cref="IService"/> to be registered.</param>
        /// <param name="serviceInstance">Instance of the <see cref="IService"/> to register.</param>
        /// <returns>True if registration is successful, false otherwise.</returns>
        public static bool TryRegisterService(Type interfaceType, IService serviceInstance)
        {
            if (serviceInstance == null)
            {
                Debug.LogWarning($"Unable to add a {interfaceType.Name} service with a null instance.");
                return false;
            }

            interfaceType = serviceInstance.GetType().FindServiceInterfaceType(interfaceType);

            if (!interfaceType.IsInstanceOfType(serviceInstance))
            {
                Debug.LogError($"{serviceInstance.Name} does not implement {interfaceType.Name}");
                return false;
            }

            if (string.IsNullOrWhiteSpace(serviceInstance.Name))
            {
                Debug.LogError($"{serviceInstance.GetType().Name} doesn't have a valid name!");
                return false;
            }

            if (!CanGetService(interfaceType, serviceInstance.Name)) { return false; }

            if (GetService(interfaceType, serviceInstance.Name, out var preExistingService))
            {
                Debug.LogError($"There's already a {interfaceType.Name}.{preExistingService.Name} registered!");
                return false;
            }

            try
            {
                activeServices.Add(interfaceType, serviceInstance as IService);
            }
            catch (ArgumentException)
            {
                preExistingService = GetService(interfaceType, false);
                Debug.LogError($"There's already a {interfaceType.Name}.{preExistingService.Name} registered!");
                return false;
            }

            if (!isInitializing)
            {
                try
                {
                    serviceInstance.Initialize();
                    serviceInstance.Enable();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }

            return true;
        }

        #endregion Registration

        #region Unregistration

        /// <summary>
        /// Remove all services from the Service Manager active service registry for a given type
        /// </summary>
        public static bool TryUnregisterServices<T>() where T : IService
        {
            return TryUnregisterService<T>(typeof(T), string.Empty);
        }

        /// <summary>
        /// Removes a specific service with the provided name.
        /// </summary>
        /// <param name="serviceInstance">The instance of the <see cref="IService"/> to remove.</param>
        public static bool TryUnregisterService<T>(T serviceInstance) where T : IService
        {
            return TryUnregisterService<T>(typeof(T), serviceInstance.Name);
        }

        /// <summary>
        /// Removes a specific service with the provided name.
        /// </summary>
        /// <param name="serviceName">The name of the service to be removed. (Only for runtime services) </param>
        public static bool TryUnregisterService<T>(string serviceName) where T : IService
        {
            return TryUnregisterService<T>(typeof(T), serviceName);
        }

        /// <summary>
        /// Remove services from the Service Manager active service registry for a given type and name
        /// </summary>
        /// <param name="interfaceType">The interface type for the system to be removed.  E.G. InputSystem, BoundarySystem</param>
        /// <param name="serviceName">The name of the service to be removed. (Only for runtime services) </param>
        private static bool TryUnregisterService<T>(Type interfaceType, string serviceName) where T : IService
        {
            if (interfaceType == null)
            {
                Debug.LogError("Unable to remove null service type.");
                return false;
            }

            if (string.IsNullOrEmpty(serviceName))
            {
                bool result = true;

                if (!TryGetService<T>(interfaceType.Name, out var activeService))
                {
                    Debug.LogWarning($"No {nameof(IService)} registered that implement {typeof(T).Name}.");
                    return false;
                }
                result &= TryUnregisterService<T>(interfaceType, activeService.Name);

                return result;
            }

            if (GetServiceByName(interfaceType, serviceName, out var serviceInstance))
            {
                var activeDataProviders = GetServices<IServiceDataProvider>();

                bool result = true;

                for (int i = 0; i < activeDataProviders.Count; i++)
                {
                    var dataProvider = activeDataProviders[i];

                    if (dataProvider.ParentService.Equals(serviceInstance))
                    {
                        result &= TryUnregisterService(dataProvider);
                    }
                }

                if (!result)
                {
                    Debug.LogError($"Failed to unregister all the {nameof(IServiceDataProvider)}s for this {serviceInstance.Name}!");
                }

                try
                {
                    serviceInstance.Disable();
                    serviceInstance.Destroy();
                    serviceInstance.Dispose();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }

                if (activeServices.ContainsKey(interfaceType))
                {
                    activeServices.Remove(interfaceType);
                    return true;
                }
                else
                {
                    Type serviceToRemove = null;
                    foreach (var service in activeServices)
                    {
                        if (service.Value.Name == serviceName)
                        {
                            serviceToRemove = service.Key;
                        }
                    }
                    activeServices.Remove(serviceToRemove);
                }
                return true;
            }

            return false;
        }

        #endregion Unregistration

        #region Service Management
        /// <summary>
        /// Retrieve all services from the active service registry for a given type and an optional name
        /// </summary>
        /// <typeparam name="T">The interface type for the system to be retrieved.  E.G. Storage Service.</typeparam>
        /// <returns>An array of services that meet the search criteria</returns>
        public static List<T> GetServices<T>() where T : IService
        {
            return GetServices<T>(typeof(T));
        }

        /// <summary>
        /// Retrieve all services from the active service registry for a given type and an optional name
        /// </summary>
        /// <param name="interfaceType">The interface type for the system to be retrieved.  E.G. Storage Service.</param>
        /// <returns>An array of services that meet the search criteria</returns>
        private static List<T> GetServices<T>(Type interfaceType) where T : IService
        {
            return GetServices<T>(interfaceType, string.Empty);
        }

        /// <summary>
        /// Retrieve all services from the active service registry for a given type and name
        /// </summary>
        /// <param name="interfaceType">The interface type for the system to be retrieved.  Storage Service.</param>
        /// <param name="serviceName">Name of the specific service</param>
        /// <returns>An array of services that meet the search criteria</returns>
        private static List<T> GetServices<T>(Type interfaceType, string serviceName) where T : IService
        {
            var services = new List<T>();

            if (interfaceType == null)
            {
                Debug.LogWarning("Unable to get services with a type of null.");
                return services;
            }

            //Get Service by interface as we do not have its name
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                foreach (var service in activeServices)
                {
                    if (interfaceType.IsAssignableFrom(service.Key))
                    {
                        services.Add((T)service.Value);
                    }
                }
            }
            //Get Service by name as there may be multiple instances of this specific interface, e.g. A Data Provider
            else
            {
                foreach (var service in activeServices)
                {
                    if (CheckServiceMatch(interfaceType, serviceName, service.Key, service.Value))
                    {
                        services.Add((T)service.Value);
                    }
                }
            }

            return services;
        }

        private void InitializeAllServices()
        {
            // If the Service Manager is not configured, stop.
            if (activeProfile == null) { return; }

            // Initialize all service
            foreach (var service in activeServices)
            {
                try
                {
                    service.Value.Initialize();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        private void StartAllServices()
        {
            // If the Service Manager is not configured, stop.
            if (activeProfile == null) { return; }

            // Start all service
            foreach (var service in activeServices)
            {
                try
                {
                    service.Value.Start();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        private void ResetAllServices()
        {
            // If the Service Manager is not configured, stop.
            if (activeProfile == null) { return; }

            // Reset all service
            foreach (var service in activeServices)
            {
                try
                {
                    service.Value.Reset();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        private void EnableAllServices()
        {
            // If the Service Manager is not configured, stop.
            if (activeProfile == null) { return; }

            // Enable all service
            foreach (var service in activeServices)
            {
                try
                {
                    service.Value.Enable();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        private void UpdateAllServices()
        {
            // If the Service Manager is not configured, stop.
            if (activeProfile == null) { return; }

            // Update all service
            foreach (var service in activeServices)
            {
                try
                {
                    service.Value.Update();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        private void LateUpdateAllServices()
        {
            // If the Service Manager is not configured, stop.
            if (activeProfile == null) { return; }

            // Late update all service
            foreach (var service in activeServices)
            {
                try
                {
                    service.Value.LateUpdate();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        private void FixedUpdateAllServices()
        {
            // If the Service Manager is not configured, stop.
            if (activeProfile == null) { return; }

            // Fix update all service
            foreach (var service in activeServices)
            {
                try
                {
                    service.Value.FixedUpdate();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        public void DisableAllServices()
        {
            // If the Service Manager is not configured, stop.
            if (activeProfile == null) { return; }

            // Disable all service
            foreach (var service in activeServices)
            {
                try
                {
                    service.Value.Disable();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        public void DestroyAllServices()
        {
            // If the Service Manager is not configured, stop.
            if (activeProfile == null) { return; }

            // Destroy all service
            foreach (var service in activeServices)
            {
                try
                {
                    service.Value.Destroy();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }

            // Dispose all service
            foreach (var service in activeServices)
            {
                try
                {
                    service.Value.Dispose();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }

            activeServices.Clear();
        }

        #endregion Service Management

        #region Service Utilities

        /// <summary>
        /// Query the <see cref="ActiveServices"/> for the existence of a <see cref="IService"/>.
        /// </summary>
        /// <typeparam name="T">The interface type for the service to be retrieved.</typeparam>
        /// <returns>Returns true, if there is a <see cref="IService"/> registered, otherwise false.</returns>
        public static bool IsServiceRegistered<T>() where T : IService
            => GetService(typeof(T)) != null;

        /// <summary>
        /// Retrieve a <see cref="IService"/> from the <see cref="ActiveServices"/>.
        /// </summary>
        /// <param name="showLogs">Should the logs show when services cannot be found?</param>
        /// <typeparam name="T">The interface type for the service to be retrieved.</typeparam>
        /// <returns>The instance of the <see cref="IService"/> that is registered.</returns>
        public static T GetService<T>(bool showLogs = true) where T : IService
            => (T)GetService(typeof(T), showLogs);

        /// <summary>
        /// Retrieve a <see cref="IService"/> from the <see cref="ActiveServices"/>.
        /// </summary>
        /// <param name="timeout">Optional, time out in seconds to wait before giving up search.</param>
        /// <typeparam name="T">The interface type for the service to be retrieved.</typeparam>
        /// <returns>The instance of the <see cref="IService"/> that is registered.</returns>
        public static async Task<T> GetServiceAsync<T>(int timeout = 10) where T : IService
            => await GetService<T>().WaitUntil(system => system != null, timeout);

        /// <summary>
        /// Retrieve a <see cref="IService"/> from the <see cref="ActiveServices"/>.
        /// </summary>
        /// <typeparam name="T">The interface type for the service to be retrieved.</typeparam>
        /// <param name="service">The instance of the service class that is registered.</param>
        /// <param name="showLogs">Should the logs show when services cannot be found?</param>
        /// <returns>Returns true if the <see cref="IService"/> was found, otherwise false.</returns>
        public static bool TryGetService<T>(out T service, bool showLogs = true) where T : IService
        {
            service = GetService<T>(showLogs);
            return service != null;
        }

        /*
        /// <summary>
        /// Retrieve a <see cref="IService"/> from the <see cref="RegisteredServices"/> by name.
        /// </summary>
        /// <param name="serviceName">Name of the specific service to search for.</param>
        /// <param name="showLogs">Should the logs show when services cannot be found?</param>
        /// <returns>The instance of the <see cref="IService"/> that is registered.</returns>
        public static T GetService<T>(string serviceName, bool showLogs = true) where T : IService
            => (T)GetService(typeof(T), serviceName, showLogs);
            */

        /// <summary>
        /// Retrieve a <see cref="IService"/> from the <see cref="ActiveServices"/> by name.
        /// </summary>
        /// <typeparam name="T">The interface type for the service to be retrieved.</typeparam>
        /// <param name="serviceName">Name of the specific service to search for.</param>
        /// <param name="service">The instance of the service class that is registered.</param>
        /// <param name="showLogs">Should the logs show when services cannot be found?</param>
        /// <returns>Returns true if the <see cref="IService"/> was found, otherwise false.</returns>
        public static bool TryGetService<T>(string serviceName, out T service, bool showLogs = true) where T : IService
        {
            service = (T)GetService(typeof(T), serviceName, showLogs);
            return service != null;
        }

        /// <summary>
        /// Retrieve a <see cref="IService"/> from the <see cref="ActiveServices"/> by type.
        /// </summary>
        /// <param name="interfaceType">The interface type for the system to be retrieved.</param>
        /// <param name="showLogs">Should the logs show when services cannot be found?</param>
        /// <returns>The instance of the <see cref="IService"/> that is registered.</returns>
        private static IService GetService(Type interfaceType, bool showLogs = true)
            => GetService(interfaceType, string.Empty, showLogs);

        /// <summary>
        /// Retrieve a <see cref="IService"/> from the <see cref="ActiveServices"/>.
        /// </summary>
        /// <param name="interfaceType">The interface type for the system to be retrieved.</param>
        /// <param name="serviceName">Name of the specific service.</param>
        /// <param name="showLogs">Should the logs show when services cannot be found?</param>
        /// <returns>The instance of the <see cref="IService"/> that is registered.</returns>
        public static IService GetService(Type interfaceType, string serviceName, bool showLogs = true)
        {
            if (!GetService(interfaceType, serviceName, out var serviceInstance) && showLogs)
            {
                Debug.LogError($"Unable to find {(string.IsNullOrWhiteSpace(serviceName) ? interfaceType.Name : serviceName)} service.");
            }

            return serviceInstance;
        }

        /// <summary>
        /// Retrieve the first <see cref="IService"/> from the <see cref="ActiveServices"/> that meets the selected type and name.
        /// </summary>
        /// <param name="interfaceType">Interface type of the service being requested.</param>
        /// <param name="serviceName">Name of the specific service.</param>
        /// <param name="serviceInstance">return parameter of the function.</param>
        private static bool GetService(Type interfaceType, string serviceName, out IService serviceInstance)
        {
            serviceInstance = null;

            if (!CanGetService(interfaceType, serviceName)) { return false; }

            if (activeServices.TryGetValue(interfaceType, out var service))
            {
                serviceInstance = service;

                if (CheckServiceMatch(interfaceType, serviceName, interfaceType, service))
                {
                    return true;
                }

                serviceInstance = null;
            }

            return false;
        }

        /// <summary>
        /// Retrieve the first <see cref="IService"/> from the <see cref="ActiveServices"/> that meets the selected type and name.
        /// </summary>
        /// <param name="interfaceType">Interface type of the service being requested.</param>
        /// <param name="serviceName">Name of the specific service.</param>
        /// <param name="serviceInstance">return parameter of the function.</param>
        private static bool GetServiceByName(Type interfaceType, string serviceName, out IService serviceInstance)
        {
            serviceInstance = null;

            if (!CanGetService(interfaceType, serviceName)) { return false; }

            var foundServices = GetServices<IService>(interfaceType, serviceName);

            switch (foundServices.Count)
            {
                case 0:
                    return false;
                case 1:
                    serviceInstance = foundServices[0];
                    return true;
                default:
                    Debug.LogError($"Found multiple instances of {interfaceType.Name}! For better results, pass the name of the service or use GetActiveServices<T>()");
                    return false;
            }
        }

        /// <summary>
        /// Check if the interface type and name matches the registered interface type and service instance found.
        /// </summary>
        /// <param name="interfaceType">The interface type of the service to check.</param>
        /// <param name="serviceName">The name of the service to check.</param>
        /// <param name="registeredInterfaceType">The registered interface type.</param>
        /// <param name="serviceInstance">The instance of the registered service.</param>
        /// <returns>True, if the registered service contains the interface type and name.</returns>
        private static bool CheckServiceMatch(Type interfaceType, string serviceName, Type registeredInterfaceType, IService serviceInstance)
        {
            bool isNameValid = string.IsNullOrEmpty(serviceName) || serviceInstance.Name == serviceName;
            bool isInstanceValid = interfaceType == registeredInterfaceType || interfaceType.IsInstanceOfType(serviceInstance);
            return isNameValid && isInstanceValid;
            //return isInstanceValid;
        }

        private static bool CanGetService(Type interfaceType, string serviceName)
        {
            if (IsApplicationQuitting)
            {
                return false;
            }

            if (interfaceType == null)
            {
                Debug.LogError($"{serviceName} interface type is null.");
                return false;
            }

            if (!typeof(IService).IsAssignableFrom(interfaceType))
            {
                Debug.LogError($"{interfaceType.Name} does not implement {nameof(IService)}.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Try to get the <see cref="TProfile"/> of the <see cref="TSystem"/>
        /// </summary>
        /// <param name="profile">The profile instance.</param>
        /// <param name="rootProfile">Optional root profile reference.</param>
        /// <returns>True if a <see cref="TSystem"/> type is matched and a valid <see cref="TProfile"/> is found, otherwise false.</returns>
        public static bool TryGetSystemProfile<TService, TProfile>(out TProfile profile, ServiceManagerRootProfile rootProfile = null)
            where TService : IService
            where TProfile : BaseProfile
        {
            if (rootProfile.IsNull())
            {
                rootProfile = instance.activeProfile;
            }

            if (!rootProfile.IsNull())
            {
                foreach (var configuration in rootProfile.ServiceConfigurations)
                {
                    if (typeof(TService).IsAssignableFrom(configuration.InstancedType))
                    {
                        profile = (TProfile)configuration.Profile;
                        return profile != null;
                    }
                }
            }

            profile = null;
            return false;
        }

        private static readonly Dictionary<Type, IService> SystemCache = new Dictionary<Type, IService>();
        private static readonly HashSet<Type> SearchedSystemTypes = new HashSet<Type>();
        private static void ClearSystemCache()
        {
            SystemCache.Clear();
            SearchedSystemTypes.Clear();
        }
        #endregion Service Utilities

        #region IDisposable Implementation

        private bool disposed;

        ~ServiceManager()
        {
            OnDispose(true);
        }

        /// <summary>
        /// Dispose the <see cref="ServiceManager"/> object.
        /// </summary>
        public void Dispose()
        {
            if (disposed) { return; }
            disposed = true;
            GC.SuppressFinalize(this);
            OnDispose(false);
        }

        private void OnDispose(bool finalizing)
        {
            if (instance == this)
            {
                instance = null;
                searchForInstance = true;
            }
        }

        #endregion IDisposable Implementation
    }
}