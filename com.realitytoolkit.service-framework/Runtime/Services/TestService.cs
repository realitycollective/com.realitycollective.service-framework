using System;
using System.Collections.Generic;
using RealityToolkit.ServiceFramework.Interfaces;
using RealityToolkit.ServiceFramework.Profiles;
using UnityEngine;

namespace RealityToolkit.ServiceFramework.Services
{
    public class TestService : BaseServiceWithConstructor, ITestService
    {
        private readonly Dictionary<Guid, IServiceDataProvider> dataProviders =
            new Dictionary<Guid, IServiceDataProvider>();

        public TestService(string name, uint priority,TestServiceProfile profile) : base(name, priority)
        {
        }

        public override Guid RegisterDataProvider(IServiceDataProvider serviceDataProvider)
        {
            Guid providerID = serviceDataProvider.GetType().GUID;
            RegisterAuthenticationDataProvider(serviceDataProvider, providerID);
            return providerID;
        }

        public override void UnRegisterDataProvider(IServiceDataProvider serviceDataProvider)
        {
            UnregisterAuthenticationDataProvider(serviceDataProvider);
        }

        /// <inheritdoc />
        public bool RegisterAuthenticationDataProvider(IServiceDataProvider provider, Guid providerID)
        {
            if (dataProviders.ContainsKey(providerID))
            {
                return false;
            }

            dataProviders.Add(providerID, provider);
            return true;
        }

        /// <inheritdoc />
        public bool UnregisterAuthenticationDataProvider(IServiceDataProvider provider)
        {
            Guid providerID = provider.GetType().GUID;
            if (!dataProviders.ContainsKey(providerID))
            {
                return false;
            }

            dataProviders.Remove(providerID);
            return true;
        }

        public override void Initialize()
        {
            base.Initialize();
            Debug.Log("TestService initialized");
        }
    }
}
