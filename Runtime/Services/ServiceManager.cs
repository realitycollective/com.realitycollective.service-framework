// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Extensions;
using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Definitions.Platforms;
using RealityCollective.ServiceFramework.Extensions;
using RealityCollective.ServiceFramework.Interfaces;
using RealityCollective.Utilities.Async;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using Debug = UnityEngine.Debug;

// ServiceGenerator - interfacevalidation
// Limit Service Type lookups for "testing" - Type Service/DataProvider

namespace RealityCollective.ServiceFramework.Services
{
    [ExecuteInEditMode]
    public class ServiceManager : IDisposable
    {
        private static Type[] serviceInterfaceTypes = new[]
        {
            typeof(IService),
            typeof(IEventService),
            typeof(IServiceModule)
        };

        public static Type[] ServiceInterfaceTypes => serviceInterfaceTypes;

        private GameObject serviceManagerInstanceGameObject;

        private Guid serviceManagerInstanceGuid;

        public Guid ServiceManagerInstanceGuid => serviceManagerInstanceGuid;

        #region Service Manager Profile properties

        /// <summary>
        /// Checks if there is a valid instance of the Service Manager, then checks if there is there a valid Active Profile.
        /// </summary>
        public bool HasActiveProfile
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

                return ActiveProfile != null;
            }
        }

        public bool InitialiseOnPlay = false;

        /// <summary>
        /// The active profile of the Service Manager which controls which services are active and their initial settings.
        /// *Note a profile is used on project initialization or replacement, changes to properties while it is running has no effect.
        /// </summary>
        [SerializeField]
        [Tooltip("The current active settings for the Service Manager project")]
        private ServiceProvidersProfile activeProfile = null;

        /// <summary>
        /// The public property of the Active Profile, ensuring events are raised on the change of the reference
        /// </summary>
        public ServiceProvidersProfile ActiveProfile
        {
            get
            {
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
        public void ResetProfile(ServiceProvidersProfile profile, GameObject instance = null)
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

            InitializeServiceLocator(instance);

            isResetting = false;
        }

        private bool isResetting = false;

        #endregion Service Manager Profile properties

        #region Service Manager runtime service registry

        private readonly Dictionary<Type, IService> activeServices = new Dictionary<Type, IService>();

        /// <summary>
        /// Current active services registered with the ServiceManager.
        /// </summary>
        /// <remarks>
        /// Services can only be registered once by <see cref="Type"/> and are executed in a specific priority order.
        /// </remarks>
        public IReadOnlyDictionary<Type, IService> ActiveServices => activeServices;

        #endregion Service Manager runtime service registry

        #region Service Manager runtime platform registry

        // ReSharper disable once InconsistentNaming
        private static readonly List<IPlatform> availablePlatforms = new List<IPlatform>();

        /// <summary>
        /// The list of active platforms detected by the <see cref="ServiceManager"/>.
        /// </summary>
        public static IReadOnlyList<IPlatform> AvailablePlatforms
        {
            get
            {
                if (availablePlatforms.Count == 0)
                {
                    CheckPlatforms();
                }
                return availablePlatforms;
            }
        }

        // ReSharper disable once InconsistentNaming
        private static readonly List<IPlatform> activePlatforms = new List<IPlatform>();

        /// <summary>
        /// The list of active platforms detected by the <see cref="ServiceManager"/>.
        /// </summary>
        public static IReadOnlyList<IPlatform> ActivePlatforms => activePlatforms;

        #endregion Service Manager runtime platform registry

        #region Instance Management

        /// <summary>
        /// Returns the singleton instance of the <see cref="ServiceManager"/>.
        /// </summary>
        public static ServiceManager Instance { get; private set; }

        /// <summary>
        /// Gets whether there is an active <see cref="Instance"/> of the <see cref="ServiceManager"/>
        /// available and it <see cref="IsInitialized"/>.
        /// </summary>
        public static bool IsActiveAndInitialized => Instance != null && Instance.IsInitialized;

        /// <summary>
        /// Lock property for the Service Manager to prevent reinitialization
        /// </summary>
        private readonly object InitializedLock = new object();

        /// <summary>
        /// The <see cref="ServiceManager"/> <see cref="Instance"/> has finished initialzing.
        /// </summary>
        public static event Action<ServiceManager> Initialized;

        /// <summary>
        /// Constructor
        /// Each Service Manager MUST have a managed GameObject that can route the MonoBehaviours to, if you do not provide a <see cref="GameObject"/>, then a new <see cref="ServiceManagerInstance"/> will be created for you.
        /// </summary>
        /// <remarks>
        /// It is NOT supported to create a reference to the ServiceManager without a GameObject and then continue to use that reference, as this will actually create two separate ServiceManagers in memory.
        /// </remarks>
        /// <param name="instanceGameObject"></param>
        public ServiceManager(GameObject instanceGameObject = null, ServiceProvidersProfile profile = null, Type[] additionalBaseServiceTypes = null)
        {
            if (additionalBaseServiceTypes != null)
            {
                for (int i = additionalBaseServiceTypes.Length - 1; i >= 0; i--)
                {
                    if (!serviceInterfaceTypes.Contains(additionalBaseServiceTypes[i]))
                    {
                        serviceInterfaceTypes = serviceInterfaceTypes.AddItem(additionalBaseServiceTypes[i]);
                    }
                }
            }

            if (instanceGameObject.IsNotNull())
            {
                Initialize(instanceGameObject, profile);
            }
        }

        public void Initialize(GameObject instanceGameObject = null, ServiceProvidersProfile profile = null)
        {
            Instance = null;
            serviceManagerInstanceGuid = Guid.NewGuid();

            ServiceManagerInstance serviceManagerInstance;

            if (serviceManagerInstanceGameObject.IsNull())
            {
                if (instanceGameObject.IsNull())
                {
                    serviceManagerInstance = GameObject.FindObjectOfType<ServiceManagerInstance>();
                    if (serviceManagerInstance.IsNull())
                    {
                        var go = new GameObject(nameof(ServiceManager));
                        serviceManagerInstance = go.AddComponent<ServiceManagerInstance>();
                        serviceManagerInstanceGameObject = serviceManagerInstance.gameObject;
                    }
                    serviceManagerInstance.SubscribetoUnityEvents(this);
                }
                else
                {
                    serviceManagerInstanceGameObject = instanceGameObject;
                }
            }

            InitializeInstance(profile);
            Initialized?.Invoke(Instance);
        }

        private void InitializeInstance(ServiceProvidersProfile profile)
        {
            lock (InitializedLock)
            {
                if (IsInitialized &&
                    ServiceManager.Instance.ServiceManagerInstanceGuid != this.serviceManagerInstanceGuid)
                {
                    Debug.LogWarning($"There are multiple instances of the {nameof(ServiceManager)} in this project, is this expected?");
                    Debug.Log($"Instance [{Instance.ServiceManagerInstanceGuid}] - This [{this.ServiceManagerInstanceGuid}]");
                }

                if (IsInitialized) { return; }

                Instance = this;
                activeProfile = profile;

                Application.quitting += () =>
                {
                    DisableAllServices();
                    DestroyAllServices();
                    IsApplicationQuitting = true;
                };

#if UNITY_EDITOR
                UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

                void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange playModeState)
                {
                    switch (playModeState)
                    {
                        case UnityEditor.PlayModeStateChange.EnteredEditMode:
                            IsApplicationQuitting = false;
                            break;
                        case UnityEditor.PlayModeStateChange.ExitingEditMode:
                        case UnityEditor.PlayModeStateChange.EnteredPlayMode:
                        case UnityEditor.PlayModeStateChange.ExitingPlayMode:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(playModeState), playModeState, null);
                    }

                }
#endif // UNITY_EDITOR

                CheckPlatforms();

                if (HasActiveProfile)
                {
                    InitializeServiceLocator();
                }
            }
        }

        private bool isInitializing = false;

        /// <summary>
        /// Flag stating if the application is currently attempting to quit.
        /// </summary>
        public bool IsApplicationQuitting { get; private set; } = false;

        /// <summary>
        /// Expose an assertion whether the Service Manager class is initialized.
        /// </summary>
        public void AssertIsInitialized()
        {
            Debug.Assert(IsInitialized, $"The {nameof(ServiceManager)} has not been initialized.");
        }

        /// <summary>
        /// Returns whether the instance has been initialized or not.
        /// </summary>
        public bool IsInitialized => Instance != null && serviceManagerInstanceGameObject.IsNotNull();

        /// <summary>
        /// function to determine if the <see cref="ServiceManager"/> class has been initialized or not.
        /// </summary>
        public bool ConfirmInitialized()
        {
            var access = Instance;
            Debug.Assert(IsInitialized.Equals(access != null));
            return IsInitialized;
        }

        /// <summary>
        /// Initialize the Service Framework configured services.
        /// </summary>
        public void InitializeServiceManager() => InitializeServiceLocator();

        /// <summary>
        /// Waits for the <see cref="ServiceManager"/> to initialize until
        /// <paramref name="timeout"/> seconds have passed or <see cref="IsActiveAndInitialized"/>.
        /// </summary>
        /// <param name="timeout">Time to wait in seconds for <see cref="IsActiveAndInitialized"/> to become <c>true</c>.</param>
        public static async Task WaitUntilInitializedAsync(float timeout = 10f)
        {
            while (!IsActiveAndInitialized && timeout > 0f)
            {
                await Task.Yield();
                timeout -= Time.deltaTime;
            }
        }

        /// <summary>
        /// Once all services are registered and properties updated, the Service Manager will initialize all active services.
        /// This ensures all services can reference each other once started.
        /// </summary>
        private void InitializeServiceLocator(GameObject instance = null)
        {
            if (isInitializing)
            {
                Debug.LogWarning($"Already attempting to initialize the {nameof(ServiceManager)}!");
                return;
            }

            isInitializing = true;

            if (!IsInitialized)
            {
                Initialize(instance);
            }

            // If the Service Manager is not configured, stop.
            if (ActiveProfile == null)
            {
                Debug.LogError($"No {nameof(ServiceProvidersProfile)} found, cannot initialize the {nameof(ServiceManager)}");
                isInitializing = false;
                return;
            }

#if UNITY_EDITOR
            if (ActiveServices.Count > 0)
            {
                activeServices.Clear();
            }
#endif

            Debug.Assert(ActiveServices.Count == 0);

            ClearServiceCache();

            if (ActiveProfile?.ServiceConfigurations != null)
            {
                var orderedConfig = ActiveProfile.ServiceConfigurations.OrderBy(s => s.Priority).ToArray();
                TryRegisterServiceConfigurations(orderedConfig);
            }

#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                InitializeAllServices();
            }
            else if (!ActiveProfile.InitializeOnPlay)
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
        internal void OnValidate()
        {
            if (!IsInitialized &&
                !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                ConfirmInitialized();
            }
        }
#endif // UNITY_EDITOR

        internal void Awake()
        {
            if (Application.isBatchMode || !Application.isEditor)
            {
                Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            }

            if (!Application.isPlaying) { return; }
        }

        internal void OnEnable()
        {
            if (Application.isPlaying)
            {
                EnableAllServices();
            }
        }

        internal void Start()
        {
            if (Application.isPlaying)
            {
                StartAllServices();
            }
        }

        internal void Update()
        {
            if (Application.isPlaying)
            {
                UpdateAllServices();
                UpdateSubscribedProcess();
            }
        }

        internal void LateUpdate()
        {
            if (Application.isPlaying)
            {
                LateUpdateAllServices();
            }
        }

        internal void FixedUpdate()
        {
            if (Application.isPlaying)
            {
                FixedUpdateAllServices();
            }
        }

        internal void OnDisable()
        {
            if (Application.isPlaying)
            {
                DisableAllServices();
            }
        }

        internal void OnDestroy()
        {
            DestroyAllServices();
            ClearServiceCache();
            Dispose();
        }

        internal void OnApplicationFocus(bool focus)
        {
            if (!Application.isPlaying) { return; }

            // If the Service Manager is not configured, stop.
            if (activeProfile == null) { return; }

            foreach (var service in activeServices)
            {
                service.Value.OnApplicationFocus(focus);
            }
        }

        internal void OnApplicationPause(bool pause)
        {
            if (!Application.isPlaying) { return; }

            // If the Service Manager is not configured, stop.
            if (activeProfile == null) { return; }

            foreach (var service in activeServices)
            {
                service.Value.OnApplicationPause(pause);
            }
        }

        #endregion MonoBehaviour Implementation

        #region Service Management

        #region Service Registration

        /// <summary>
        /// Registers all the <see cref="IService"/>s defined in the provided configuration collection.
        /// </summary>
        /// <typeparam name="T">The interface type for the <see cref="IService"/> to be registered.</typeparam>
        /// <param name="configurations">The list of <see cref="IServiceConfiguration{T}"/>s.</param>
        /// <returns>True, if all configurations successfully created and registered their services.</returns>
        public bool TryRegisterServiceConfigurations<T>(IServiceConfiguration<T>[] configurations) where T : IService
        {
            bool anyFailed = false;

            for (var i = 0; i < configurations?.Length; i++)
            {
                var configuration = configurations[i];

                if (TryCreateAndRegisterService(configuration, out var serviceInstance))
                {
                    if (serviceInstance != null &&
                        configuration.Profile is IServiceProfile<IServiceModule> profile &&
                        !TryRegisterServiceModuleConfigurations(profile.ServiceConfigurations, serviceInstance))
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
        /// Registers all the <see cref="IServiceModule"/>s defined in the provided configuration collection.
        /// </summary>
        /// <typeparam name="T">The interface type for the <see cref="IServiceModule"/> to be registered.</typeparam>
        /// <param name="configurations">The list of <see cref="IServiceConfiguration{T}"/>s.</param>
        /// <param name="serviceParent">The <see cref="IService"/> that the <see cref="IServiceModule"/> will be assigned to.</param>
        /// <returns>True, if all configurations successfully created and registered their service modules.</returns>
        public bool TryRegisterServiceModuleConfigurations<T>(IServiceConfiguration<T>[] configurations, IService serviceParent) where T : IServiceModule
        {
            bool anyFailed = false;

            for (var i = 0; i < configurations?.Length; i++)
            {
                var configuration = configurations[i];

                if (!TryCreateAndRegisterServiceModule(configuration, serviceParent))
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
        public bool TryRegisterService<T>(IService serviceInstance) where T : IService
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
        public bool TryCreateAndRegisterService<T>(IServiceConfiguration<T> configuration, out T service) where T : IService
        {
            return TryCreateAndRegisterServiceInternal(
                configuration.InstancedType,
                configuration.RuntimePlatforms,
                out service,
                configuration.Name,
                configuration.Priority,
                configuration.Profile);
        }

        /// <summary>
        /// Creates a new instance of a service module and registers it to the Service Manager service registry for the specified platform.
        /// </summary>
        /// <typeparam name="T">The interface type for the <see cref="IService"/> to be registered.</typeparam>
        /// <param name="configuration">The <see cref="IServiceConfiguration{T}"/> to use to create and register the service module.</param>
        /// <param name="serviceParent">The <see cref="IService"/> that the <see cref="IServiceModule"/> will be assigned to.</param>
        /// <returns>True, if the service was successfully created and registered.</returns>
        public bool TryCreateAndRegisterServiceModule<T>(IServiceConfiguration<T> configuration, IService serviceParent) where T : IServiceModule
        {
            return TryCreateAndRegisterServiceInternal<T>(
                configuration.InstancedType,
                configuration.RuntimePlatforms,
                out _,
                configuration.Name,
                configuration.Priority,
                configuration.Profile,
                serviceParent);
        }

        /// <summary>
        /// Creates a new instance of a service and registers it to the Reality Toolkit service registry for all platforms.
        /// </summary>
        /// <typeparam name="T">The interface type for the <see cref="IService"/> to be registered.</typeparam>
        /// <param name="concreteType">The concrete class type to instantiate.</param>
        /// <param name="service">If successful, then the new <see cref="IService"/> instance will be passed back out.</param>
        /// <param name="args">Optional arguments used when instantiating the concrete type.</param>
        /// <returns>True, if the service was successfully created and registered.</returns>
        public bool TryCreateAndRegisterService<T>(Type concreteType, out T service, params object[] args) where T : IService
        {
            return TryCreateAndRegisterServiceInternal(concreteType, AllPlatforms.Platforms, out service, args);
        }

        /// <summary>
        /// Creates a new instance of a service and registers it to the Reality Toolkit service registry for the specified platform(s).
        /// </summary>
        /// <typeparam name="T">The interface type for the <see cref="IService"/> to be registered.</typeparam>
        /// <param name="concreteType">The concrete class type to instantiate.</param>
        /// <param name="runtimePlatforms">The runtime <see cref="IPlatform"/>s to check against when registering.</param>
        /// <param name="service">If successful, then the new <see cref="IService"/> instance will be passed back out.</param>
        /// <param name="args">Optional arguments used when instantiating the concrete type.</param>
        /// <returns>True, if the service was successfully created and registered.</returns>
        public bool TryCreateAndRegisterService<T>(Type concreteType, IReadOnlyList<IPlatform> runtimePlatforms, out T service, params object[] args) where T : IService
        {
            return TryCreateAndRegisterServiceInternal(concreteType, runtimePlatforms, out service, args);
        }

        /// <summary>
        /// Creates a new instance of a service and registers it to the Service Manager service registry for the specified platform.
        /// </summary>
        /// <typeparam name="T">The interface type for the <see cref="IService"/> to be registered.</typeparam>
        /// <param name="concreteType">The concrete class type to instantiate.</param>
        /// <param name="service">If successful, then the new <see cref="IService"/> instance will be passed back out.</param>
        /// <param name="args">Optional arguments used when instantiating the concrete type.</param>
        /// <returns>True, if the service was successfully created and registered.</returns>
        private bool TryCreateAndRegisterServiceInternal<T>(Type concreteType, IReadOnlyList<IPlatform> runtimePlatforms, out T service, params object[] args) where T : IService
        {
            service = default;

            if (IsApplicationQuitting)
            {
                return false;
            }

            if (!PlatformMatch(concreteType, runtimePlatforms))
            {
                // We return true so we don't raise en error.
                // Even though we did not register the service,
                // it's expected that this is the intended behavior
                // when there isn't a valid platform to run the service on.
                return true;
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
            // If a service does not want its service modules registered, then do not add them to the registry.
            if (args.Length == 4 && !(args[3] as IService).RegisterServiceModules)
            {
                return true;
            }
            return TryRegisterService(typeof(T), serviceInstance);
        }

        /// <summary>
        /// Service registration.
        /// </summary>
        /// <param name="interfaceType">The interface type for the <see cref="IService"/> to be registered.</param>
        /// <param name="serviceInstance">Instance of the <see cref="IService"/> to register.</param>
        /// <returns>True if registration is successful, false otherwise.</returns>
        public bool TryRegisterService(Type interfaceType, IService serviceInstance)
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

            if (TryGetService(interfaceType, serviceInstance.Name, out var preExistingService))
            {
                Debug.LogError($"There is already a {interfaceType.Name}.{preExistingService.Name} registered!");
                return false;
            }

            try
            {
                activeServices.Add(interfaceType, serviceInstance);
            }
            catch (ArgumentException)
            {
                preExistingService = GetService(interfaceType, false);
                Debug.LogError($"There is already a {interfaceType.Name}.{preExistingService.Name} registered!");
                return false;
            }

            // If we have registered at least one event Service, we're gonna need the Unity UI event Service to be available.
            if (typeof(IEventService).IsAssignableFrom(interfaceType))
            {
                EnsureEventSystemSetup();
            }

            if (!isInitializing)
            {
                try
                {
                    if (serviceInstance.IsEnabled)
                    {
                        serviceInstance.Initialize();
                        serviceInstance.Enable();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }

            return true;
        }

        #endregion Service Registration

        #region Unregister Services

        /// <summary>
        /// Remove all services from the Service Manager active service registry for a given type
        /// </summary>
        public bool TryUnregisterServicesOfType<T>() where T : IService
        {
            return TryUnregisterService<T>(typeof(T), string.Empty);
        }

        /// <summary>
        /// Removes a specific service with the provided name.
        /// </summary>
        /// <param name="serviceInstance">The instance of the <see cref="IService"/> to remove.</param>
        public bool TryUnregisterService<T>(T serviceInstance) where T : IService
        {
            return TryUnregisterService<T>(typeof(T), serviceInstance.Name);
        }

        /// <summary>
        /// Remove services from the Service Manager active service registry for a given type and name
        /// </summary>
        /// <param name="interfaceType">The interface type for the Service to be removed.  E.G. InputService, BoundaryService</param>
        /// <param name="serviceName">The name of the service to be removed. (Only for runtime services) </param>
        private bool TryUnregisterService<T>(Type interfaceType, string serviceName) where T : IService
        {
            if (interfaceType == null)
            {
                Debug.LogError("Unable to remove null service type.");
                return false;
            }

            if (string.IsNullOrEmpty(serviceName))
            {
                bool result = true;

                if (!TryGetService<T>(out var activeService))
                {
                    Debug.LogWarning($"No {nameof(IService)} registered that implement {typeof(T).Name}.");
                    return false;
                }
                result &= TryUnregisterService<T>(interfaceType, activeService.Name);

                return result;
            }

            if (TryGetServiceByName(interfaceType, serviceName, out var serviceInstance))
            {
                var activeServiceModules = GetServices<IServiceModule>();

                bool result = true;

                for (int i = 0; i < activeServiceModules.Count; i++)
                {
                    var dataProvider = activeServiceModules[i];

                    if (dataProvider.ParentService.Equals(serviceInstance))
                    {
                        result &= TryUnregisterService(dataProvider);
                    }
                }

                if (!result)
                {
                    Debug.LogError($"Failed to unregister all the {nameof(IServiceModule)}s for this {serviceInstance.Name}!");
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

        #endregion Unregister Services

        #region Get Services

        /// <summary>
        /// Retrieve a <see cref="IService"/> from the <see cref="ActiveServices"/>.
        /// </summary>
        /// <param name="showLogs">Should the logs show when services cannot be found?</param>
        /// <typeparam name="T">The interface type for the service to be retrieved.</typeparam>
        /// <returns>The instance of the <see cref="IService"/> that is registered.</returns>
        public T GetService<T>(bool showLogs = true) where T : IService
            => (T)GetService(typeof(T), showLogs);

        /// <summary>
        /// Retrieve a <see cref="IService"/> from the <see cref="ActiveServices"/>.
        /// </summary>
        /// <param name="timeout">Optional, time out in seconds to wait before giving up search.</param>
        /// <typeparam name="T">The interface type for the service to be retrieved.</typeparam>
        /// <returns>The instance of the <see cref="IService"/> that is registered.</returns>
        public async Task<T> GetServiceAsync<T>(int timeout = 10) where T : IService
            => await GetService<T>().WaitUntil(service => service != null, timeout);

        /// <summary>
        /// Retrieve a <see cref="IService"/> from the <see cref="ActiveServices"/> by type.
        /// </summary>
        /// <param name="interfaceType">The interface type for the Service to be retrieved.</param>
        /// <param name="showLogs">Should the logs show when services cannot be found?</param>
        /// <returns>The instance of the <see cref="IService"/> that is registered.</returns>
        public IService GetService(Type interfaceType, bool showLogs = true)
            => GetServiceByName(interfaceType, string.Empty, showLogs);

        /// <summary>
        /// Retrieve a <see cref="IService"/> from the <see cref="ActiveServices"/>.
        /// </summary>
        /// <param name="interfaceType">The interface type for the Service to be retrieved.</param>
        /// <param name="serviceName">Name of the specific service.</param>
        /// <param name="showLogs">Should the logs show when services cannot be found?</param>
        /// <returns>The instance of the <see cref="IService"/> that is registered.</returns>
        public IService GetServiceByName<T>(string serviceName, bool showLogs = true) where T : IService
        {
            if (!TryGetServiceByName<T>(serviceName, out var serviceInstance) && showLogs)
            {
                Debug.LogError($"Unable to find {serviceName} service.");
            }

            return serviceInstance;
        }

        /// <summary>
        /// Retrieve a <see cref="IService"/> from the <see cref="ActiveServices"/>.
        /// </summary>
        /// <param name="interfaceType">The interface type for the Service to be retrieved.</param>
        /// <param name="serviceName">Name of the specific service.</param>
        /// <param name="showLogs">Should the logs show when services cannot be found?</param>
        /// <returns>The instance of the <see cref="IService"/> that is registered.</returns>
        public IService GetServiceByName(Type interfaceType, string serviceName, bool showLogs = true)
        {
            if (!TryGetService(interfaceType, serviceName, out var serviceInstance) && showLogs)
            {
                Debug.LogError($"Unable to find {(string.IsNullOrWhiteSpace(serviceName) ? interfaceType.Name : serviceName)} service.");
            }

            return serviceInstance;
        }

        /// <summary>
        /// Retrieve a <see cref="IService"/> from the <see cref="ActiveServices"/>.
        /// </summary>
        /// <typeparam name="T">The interface type for the service to be retrieved.</typeparam>
        /// <param name="service">The instance of the service class that is registered.</param>
        /// <returns>Returns true if the <see cref="IService"/> was found, otherwise false.</returns>
        public bool TryGetService<T>(out T service) where T : IService
        {
            service = GetService<T>(false);
            return service != null;
        }

        /// <summary>
        /// Retrieve the first <see cref="IService"/> from the <see cref="ActiveServices"/> that meets the selected type and name.
        /// </summary>
        /// <param name="interfaceType">Interface type of the service being requested.</param>
        /// <param name="serviceName">Name of the specific service.</param>
        /// <param name="serviceInstance">return parameter of the function.</param>
        public bool TryGetService(Type interfaceType, string serviceName, out IService serviceInstance)
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
        /// Retrieve a <see cref="IService"/> from the <see cref="ActiveServices"/> by name.
        /// </summary>
        /// <typeparam name="T">The interface type for the service to be retrieved.</typeparam>
        /// <param name="serviceName">Name of the specific service to search for.</param>
        /// <param name="service">The instance of the service class that is registered.</param>
        /// <returns>Returns true if the <see cref="IService"/> was found, otherwise false.</returns>
        public bool TryGetServiceByName<T>(string serviceName, out T service) where T : IService
        {
            service = (T)GetServiceByName(typeof(T), serviceName, false);
            return service != null;
        }

        /// <summary>
        /// Retrieve the first <see cref="IService"/> from the <see cref="ActiveServices"/> that meets the selected type and name.
        /// </summary>
        /// <param name="interfaceType">Interface type of the service being requested.</param>
        /// <param name="serviceName">Name of the specific service.</param>
        /// <param name="serviceInstance">return parameter of the function.</param>
        public bool TryGetServiceByName(Type interfaceType, string serviceName, out IService serviceInstance)
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
        /// Retrieve all services from the active service registry for a given type and an optional name
        /// </summary>
        /// <typeparam name="T">The <see cref="IService"/> interface type for the Service to be retrieved.  E.G. IStorageService.</typeparam>
        /// <returns>An array of services that meet the search criteria</returns>
        public List<T> GetServices<T>() where T : IService
        {
            return GetServices<T>(typeof(T));
        }

        /// <summary>
        /// Retrieve all services from the active service registry for a given type and an optional name
        /// </summary>
        /// <param name="interfaceType">The interface type for the Service to be retrieved.  E.G. Storage Service.</param>
        /// <returns>An array of services that meet the search criteria</returns>
        public List<T> GetServices<T>(Type interfaceType) where T : IService
        {
            return GetServices<T>(interfaceType, string.Empty);
        }

        /// <summary>
        /// Retrieve all services from the active service registry for a given type and name
        /// </summary>
        /// <param name="interfaceType">The interface type for the Service to be retrieved.  Storage Service.</param>
        /// <param name="serviceName">Name of the specific service</param>
        /// <returns>An array of services that meet the search criteria</returns>
        public List<T> GetServices<T>(Type interfaceType, string serviceName) where T : IService
        {
            var services = new List<T>();

            TryGetServices<T>(interfaceType, serviceName, ref services);

            return services;
        }

        /// <summary>
        /// Retrieve all services from the active service registry for a given type and name
        /// </summary>
        /// <param name="interfaceType">The interface type for the Service to be retrieved.  Storage Service.</param>
        /// <param name="serviceName">Name of the specific service</param>
        /// <returns>An array of services that meet the search criteria</returns>
        public bool TryGetServices<T>(Type interfaceType, string serviceName, ref List<T> services) where T : IService
        {
            if (interfaceType == null)
            {
                Debug.LogWarning("Unable to get services with a type of null.");
                return false;
            }

            if (services == null)
            {
                services = new List<T>();
            }

            if (!CanGetService(interfaceType, serviceName)) { return false; }

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
            //Get Service by name as there may be multiple instances of this specific interface, e.g. A Service Module
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

            return serviceCache.Count > 0;
        }

        /// <summary>
        /// Gets all <see cref="IService"/>s by type.
        /// </summary>
        /// <param name="interfaceType">The interface type to search for.</param>
        /// <param name="services">Memory reference value of the service list to update.</param>
        public List<IService> GetAllServices()
        {
            List<IService> services = new List<IService>();
            TryGetServices(typeof(IService), string.Empty, ref services);
            return services;
        }

        /// <summary>
        /// Gets all <see cref="IService"/>s by type.
        /// </summary>
        /// <param name="interfaceType">The interface type to search for.</param>
        /// <param name="services">Memory reference value of the service list to update.</param>
        public void GetAllServices(ref List<IService> services)
        {
            TryGetServices(typeof(IService), string.Empty, ref services);
        }

        /// <summary>
        /// Retrieve a cached refernece of an <see cref="IService"/> from the <see cref="ActiveServices"/>.
        /// </summary>
        /// <typeparam name="T">The interface type for the Service to be retrieved.</typeparam>
        /// <returns>The instance of the <see cref="IService"/> that is registered.</returns>
        /// <remarks>
        /// Internal function used for high performant services or components, not to be overused.
        /// </remarks>
        public T GetServiceCached<T>() where T : IService
        {
            if (!IsInitialized ||
                IsApplicationQuitting ||
                ActiveProfile.IsNull())
            {
                return default;
            }

            T service = default;

            if (!serviceCache.TryGetValue(typeof(T), out var cachedSystem))
            {
                if (IsServiceRegistered<T>())
                {
                    if (TryGetService(out service))
                    {
                        serviceCache.Add(typeof(T), service);
                    }

                    if (!searchedServiceTypes.Contains(typeof(T)))
                    {
                        searchedServiceTypes.Add(typeof(T));
                    }
                }
            }
            else
            {
                service = (T)cachedSystem;
            }

            return service;
        }

        /// <summary>
        /// Retrieve a <see cref="IService"/> from the <see cref="ActiveSystems"/>.
        /// </summary>
        /// <typeparam name="T">The interface type for the Service to be retrieved.</typeparam>
        /// <param name="timeout">Optional, time out in seconds to wait before giving up search.</param>
        /// <returns>The instance of the <see cref="IService"/> that is registered.</returns>
        public async Task<T> GetSystemCachedAsync<T>(int timeout = 10) where T : IService
            => await GetServiceCached<T>().WaitUntil(service => service != null, timeout);

        /// <summary>
        /// Retrieve a <see cref="IService"/> from the <see cref="ActiveSystems"/>.
        /// </summary>
        /// <typeparam name="T">The interface type for the Service to be retrieved.</typeparam>
        /// <param name="service">The instance of the Service class that is registered.</param>
        /// <returns>Returns true if the <see cref="IMiIServiceedRealitySystem"/> was found, otherwise false.</returns>
        public bool TryGetServiceCached<T>(out T service) where T : IService
        {
            service = GetServiceCached<T>();
            return service != null;
        }

        #endregion Get Services

        #region Enable Services

        /// <summary>
        /// Enables a services in the active service registry for a given name.
        /// </summary>
        /// <typeparam name="T">The <see cref="IService"/> interface type for the service to be enabled.  E.G. InputService, BoundaryService</typeparam>
        /// <param name="serviceName"></param>
        public void EnableService<T>(string serviceName) where T : IService
        {
            EnableAllServicesByTypeAndName(typeof(T), serviceName);
        }

        /// <summary>
        /// Enable services in the active service registry for a given type
        /// </summary>
        /// <typeparam name="T">The <see cref="IService"/> interface type for the service to be enabled.  E.G. InputService, BoundaryService</typeparam>
        public void EnableService<T>() where T : IService
        {
            EnableAllServicesByTypeAndName(typeof(T), string.Empty);
        }

        /// <summary>
        /// Enable all services in the active service registry for a given type and name
        /// </summary>
        /// <param name="interfaceType">The <see cref="IService"/> interface type for the Service to be enabled.  E.G. InputService, BoundaryService</param>
        /// <param name="serviceName">Name of the specific service</param>
        private void EnableAllServicesByTypeAndName(Type interfaceType, string serviceName)
        {
            if (interfaceType == null)
            {
                Debug.LogError("Unable to enable null service type.");
                return;
            }

            var services = new List<IService>();
            TryGetServices(interfaceType, serviceName, ref services);

            for (int i = 0; i < services?.Count; i++)
            {
                try
                {
                    services[i].Enable();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }
        #endregion Enable Services

        #region Disable Services

        /// <summary>
        /// Disable services in the active service registry for a given type
        /// </summary>
        /// <typeparam name="T">The interface type for the service to be enabled.  E.G. InputService, BoundaryService</typeparam>
        public void DisableService<T>() where T : IService
        {
            DisableAllServicesByTypeAndName(typeof(T), string.Empty);
        }

        /// <summary>
        /// DDisable services in the active service registry for a given name.
        /// </summary>
        /// <typeparam name="T">The interface type for the service to be enabled.  E.G. InputService, BoundaryService</typeparam>
        /// <param name="serviceName">Name of the specific service</param>
        public void DisableService<T>(string serviceName) where T : IService
        {
            DisableAllServicesByTypeAndName(typeof(T), serviceName);
        }

        /// <summary>
        /// Disable all services in the Mixed Reality Toolkit active service registry for a given type and name
        /// </summary>
        /// <param name="interfaceType">The interface type for the Service to be disabled.  E.G. InputService, BoundaryService</param>
        /// <param name="serviceName">Name of the specific service</param>
        private void DisableAllServicesByTypeAndName(Type interfaceType, string serviceName)
        {
            if (interfaceType == null)
            {
                Debug.LogError("Unable to disable null service type.");
                return;
            }

            var services = new List<IService>();
            TryGetServices(interfaceType, serviceName, ref services);

            for (int i = 0; i < services?.Count; i++)
            {
                try
                {
                    services[i].Disable();
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }
        #endregion Disable Services

        #region Service Initialization
        internal void InitializeAllServices()
        {
            // If the Service Manager is not configured, stop.
            if (activeProfile == null) { return; }

            // Initialize all service
            foreach (var service in activeServices)
            {
                try
                {
                    if (service.Value.IsEnabled)
                    {
                        service.Value.Initialize();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }
        #endregion Service Initialization

        #region MonoBehaviour Replicators
        internal void StartAllServices()
        {
            // If the Service Manager is not configured, stop.
            if (activeProfile == null) { return; }

            // Start all service
            foreach (var service in activeServices)
            {
                try
                {
                    if (service.Value.IsEnabled)
                    {
                        service.Value.Start();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        internal void ResetAllServices()
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

        internal void EnableAllServices()
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

        internal void UpdateAllServices()
        {
            // If the Service Manager is not configured, stop.
            if (activeProfile == null) { return; }

            // Update all service
            foreach (var service in activeServices)
            {
                try
                {
                    if (service.Value.IsEnabled)
                    {
                        service.Value.Update();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        internal void LateUpdateAllServices()
        {
            // If the Service Manager is not configured, stop.
            if (activeProfile == null) { return; }

            // Late update all service
            foreach (var service in activeServices)
            {
                try
                {
                    if (service.Value.IsEnabled)
                    {
                        service.Value.LateUpdate();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"{e.Message}\n{e.StackTrace}");
                }
            }
        }

        internal void FixedUpdateAllServices()
        {
            // If the Service Manager is not configured, stop.
            if (activeProfile == null) { return; }

            // Fix update all service
            foreach (var service in activeServices)
            {
                try
                {
                    if (service.Value.IsEnabled)
                    {
                        service.Value.FixedUpdate();
                    }
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
            if (activeProfile == null || activeServices == null || activeServices.Count == 0) { return; }

            var destroyingActiveServices = activeServices.ToArray();

            // Destroy all service
            foreach (var service in destroyingActiveServices)
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
            foreach (var service in destroyingActiveServices)
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
        #endregion MonoBehaviour Replicators

        #endregion Service Management

        #region Proccess Management

        private static readonly List<Proccess> activeProcess = new List<Proccess>();
        private float durationToleranceMs = 10;
        private bool doNotRemoveIfTooLong = false;

        /// <summary>
        /// Subscribe in order to have onUpdate called approximately every period seconds (or every frame, if period <= 0).
        /// Don't assume that onUpdate will be called in any particular order compared to other subscribers.
        /// </summary>
        public void AddProcessToUpdate(UpdateMethod onUpdate, float period)
        {
            if (onUpdate == null)
                return;
            foreach (Proccess currSub in activeProcess)
                if (currSub.updateMethod.Equals(onUpdate))
                    return;
            activeProcess.Add(new Proccess(onUpdate, period));
        }

        /// <summary>
        /// Safe to call even if onUpdate was not previously Subscribed.
        /// </summary>
        public void RemoveProcessFromUpdate(UpdateMethod onUpdate)
        {
            for (int sub = activeProcess.Count - 1; sub >= 0; sub--)
                if (activeProcess[sub].updateMethod.Equals(onUpdate))
                    activeProcess.RemoveAt(sub);
        }

        /// <summary>
        /// Some objects might need to be on a slower update loop than the usual MonoBehaviour Update and without precise timing, e.g. to refresh data from services.
        /// Each frame, advance all subscribers. Any that have hit their period should then act, though if they take too long they could be removed.
        /// </summary>
        private void UpdateSubscribedProcess()
        {
            // Iterate in reverse in case we need to remove something.
            for (int s = activeProcess.Count - 1; s >= 0; s--)
            {
                var sub = activeProcess[s];
                sub.periodCurrent += Time.deltaTime;
                if (sub.periodCurrent > sub.period)
                {
                    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                    UpdateMethod onUpdate = sub.updateMethod;

                    // In case something forgets to Unsubscribe when it dies.
                    if (onUpdate == null)
                    {
                        Remove(s, $"Did not Unsubscribe from UpdateSlow: {onUpdate.Target} : {onUpdate.Method}");
                        continue;
                    }

                    // Detect a local function that cannot be Unsubscribed since it could go out of scope.
                    if (onUpdate.Target == null)
                    {
                        Remove(s, $"Removed local function from UpdateSlow: {onUpdate.Target} : {onUpdate.Method}");
                        continue;
                    }

                    // Detect an anonymous function that cannot be Unsubscribed, by checking for a character that can't exist in a declared method name.
                    if (onUpdate.Method.ToString().Contains("<"))
                    {
                        Remove(s, $"Removed anonymous from UpdateSlow: {onUpdate.Target} : {onUpdate.Method}");
                        continue;
                    }

                    stopwatch.Restart();
                    onUpdate?.Invoke(sub.periodCurrent);
                    stopwatch.Stop();
                    sub.periodCurrent = 0;

                    if (stopwatch.ElapsedMilliseconds > durationToleranceMs)
                    {
                        if (!doNotRemoveIfTooLong)
                            Remove(s, $"UpdateSlow subscriber took too long, removing: {onUpdate.Target} : {onUpdate.Method}");
                        else
                            Debug.LogWarning($"UpdateSlow subscriber took too long: {onUpdate.Target} : {onUpdate.Method}");
                    }
                }
            }

            void Remove(int index, string msg)
            {
                activeProcess.RemoveAt(index);
                Debug.LogError(msg);
            }
        }

        #endregion Proccess Management

        #region Service Utilities

        private string[] ignoredNamespaces = { "Service.IDisposable",
                                                      "RealityCollective.ServiceFramework.Interfaces.IService",
                                                      "RealityCollective.ServiceFramework.Interfaces.IServiceDataProvider"};

        /// <summary>
        /// Query the <see cref="ActiveServices"/> for the existence of a <see cref="IService"/>.
        /// </summary>
        /// <typeparam name="T">The interface type for the service to be retrieved.</typeparam>
        /// <returns>Returns true, if there is a <see cref="IService"/> registered, otherwise false.</returns>
        public bool IsServiceRegistered<T>() where T : IService
        {
            return activeServices.TryGetValue(typeof(T), out _);
        }

        /// <summary>
        /// Query the <see cref="ActiveServices"/> for the existence of a <see cref="IService"/>.
        /// </summary>
        /// <typeparam name="T">The interface type for the service to be retrieved.</typeparam>
        /// <returns>Returns true, if there is a <see cref="IService"/> registered, otherwise false.</returns>
        public bool IsServiceRegistered(object concreteType)
        {
            var interfaces = GetInterfacesFromType(concreteType);
            var interfaceCount = interfaces.Length;
            for (int i = 0; i < interfaceCount; i++)
            {
                if (activeServices.TryGetValue(interfaces[i], out _))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsServiceEnabled<T>() where T : IService
        {
            if (TryGetService<T>(out var service))
            {
                return service.IsEnabled;
            }
            return false;
        }

        public bool IsServiceEnabledInProfile<T>(ServiceProvidersProfile rootProfile = null)
        {
            if (rootProfile.IsNull())
            {
                rootProfile = ActiveProfile;
            }

            if (!rootProfile.IsNull() && rootProfile.ServiceConfigurations != null)
            {
                foreach (var configuration in rootProfile.ServiceConfigurations)
                {
                    if (typeof(T).IsAssignableFrom(configuration.InstancedType.Type.FindServiceInterfaceType(typeof(T))) &&
                        configuration.Enabled)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Check if the interface type and name matches the registered interface type and service instance found.
        /// </summary>
        /// <param name="interfaceType">The interface type of the service to check.</param>
        /// <param name="registeredInterfaceType">The registered interface type.</param>
        /// <param name="serviceInstance">The instance of the registered service.</param>
        /// <returns>True, if the registered service contains the interface type and name.</returns>
        private bool CheckServiceMatch(Type interfaceType, string serviceName, Type registeredInterfaceType, IService serviceInstance)
        {
            bool isNameValid = string.IsNullOrEmpty(serviceName) || string.Equals(serviceInstance.Name, serviceName);
            bool isInstanceValid = interfaceType == registeredInterfaceType || interfaceType.IsInstanceOfType(serviceInstance);
            return isNameValid && isInstanceValid;
        }

        private bool CanGetService(Type interfaceType, string serviceName)
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
        /// Try to get the <see cref="TProfile"/> of the <see cref="TService"/>
        /// </summary>
        /// <param name="profile">The profile instance.</param>
        /// <param name="rootProfile">Optional root profile reference.</param>
        /// <returns>True if a <see cref="TService"/> type is matched and a valid <see cref="TProfile"/> is found, otherwise false.</returns>
        public bool TryGetServiceProfile<TService, TProfile>(out TProfile profile, ServiceProvidersProfile rootProfile = null)
            where TService : IService
            where TProfile : BaseProfile
        {
            if (rootProfile.IsNull())
            {
                rootProfile = ActiveProfile;
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

        private readonly Dictionary<Type, IService> serviceCache = new Dictionary<Type, IService>();
        private readonly HashSet<Type> searchedServiceTypes = new HashSet<Type>();

        private void ClearServiceCache()
        {
            serviceCache.Clear();
            searchedServiceTypes.Clear();
        }

        private Type[] GetInterfacesFromType(object concreteObject)
        {
            var interfaces = concreteObject.GetType().GetInterfaces();
            var interfaceCount = interfaces.Length;
            List<Type> detectedInterfaces = new List<Type>();

            for (int i = 0; i < interfaceCount; i++)
            {
                if (ignoredNamespaces.Contains(interfaces[i].FullName)) continue;

                detectedInterfaces.Add(interfaces[i]);
            }
            return detectedInterfaces.ToArray();
        }

        /// <summary>
        /// Check which platforms are active and available.
        /// </summary>
        internal static void CheckPlatforms()
        {
            activePlatforms.Clear();
            availablePlatforms.Clear();

            var platformTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(IPlatform).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract)
                .OrderBy(type => type.Name);

            var platformOverrides = new List<Type>();

            foreach (var platformType in platformTypes)
            {
                IPlatform platform = null;

                try
                {
                    platform = Activator.CreateInstance(platformType) as IPlatform;
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }

                if (platform == null) { continue; }

                availablePlatforms.Add(platform);

                if (platform.IsAvailable
#if UNITY_EDITOR
                    || platform.IsBuildTargetAvailable &&
                    RealityCollective.Extensions.TypeExtensions.TryResolveType(UnityEditor.EditorPrefs.GetString("CurrentPlatformTarget", string.Empty), out var resolvedPlatform) &&
                    resolvedPlatform == platformType
#endif
                )
                {
                    foreach (var platformOverride in platform.PlatformOverrides)
                    {
                        platformOverrides.Add(platformOverride.GetType());
                    }
                }
            }

            foreach (var platform in availablePlatforms)
            {
                if (Application.isPlaying &&
                    platformOverrides.Contains(platform.GetType()))
                {
                    continue;
                }

                if (platform.IsAvailable
#if UNITY_EDITOR
                    || platform.IsBuildTargetAvailable
#endif
                )
                {
                    activePlatforms.Add(platform);
                }
            }
        }


        /// <summary>
        /// Check if any of the provided runtime platforms are currently available in the running environment
        /// </summary>
        /// <param name="runtimePlatforms">The set of runtime platforms to test against</param>
        /// <returns>Returns false if there are no matching platforms, platform not running.</returns>
        internal bool PlatformMatch(Type concreteType, IReadOnlyList<IPlatform> runtimePlatforms)
        {
            if (runtimePlatforms == null || runtimePlatforms.Count == 0)
            {
                Debug.LogWarning($"No runtime platforms defined for the {concreteType?.Name} service.");
                return false;
            }

            var platforms = new List<IPlatform>();

            Debug.Assert(ActivePlatforms.Count > 0);

            for (var i = 0; i < ActivePlatforms.Count; i++)
            {
                var activePlatform = ActivePlatforms[i].GetType();

                for (var j = 0; j < runtimePlatforms?.Count; j++)
                {
                    var runtimePlatform = runtimePlatforms[j].GetType();

                    if (activePlatform == runtimePlatform)
                    {
                        platforms.Add(runtimePlatforms[j]);
                        break;
                    }
                }
            }

            if (platforms.Count == 0
#if UNITY_EDITOR
                    || !CurrentBuildTargetPlatform.IsBuildTargetActive(platforms)
#endif
                    )
            {
                // No matching platforms found
                return false;
            }
            return true;
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
            if (Instance == this)
            {
                Instance = null;
            }
        }

        #endregion IDisposable Implementation

        #region Service Dependencies

        private static void EnsureEventSystemSetup()
        {
            var eventSystems = UnityEngine.Object.FindObjectsOfType<EventSystem>();
            if (eventSystems.Length == 0)
            {
                new GameObject(nameof(EventSystem)).EnsureComponent<EventSystem>();
                Debug.Log($"There was no {nameof(EventSystem)} in the scene. The {nameof(ServiceManager)} requires one for registered {nameof(IEventService)}s to work. An {nameof(EventSystem)} game object was created.");
            }
            else if (eventSystems.Length > 1)
            {
                Debug.LogError($"There is more than one {nameof(EventSystem)} active in the scene. Please make sure only one instance of it exists as it may cause errors.");
            }
        }

        #endregion Service Dependencies
    }
}