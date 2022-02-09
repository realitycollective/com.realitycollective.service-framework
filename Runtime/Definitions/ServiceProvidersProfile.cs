// Copyright (c) Reality Collective. All rights reserved.

using RealityToolkit.ServiceFramework.Interfaces;
using UnityEngine;

namespace RealityToolkit.ServiceFramework.Definitions
{
    [CreateAssetMenu(menuName = "RealityToolkit/Service Manager/Service Providers Profile", fileName = "ServiceProvidersProfile", order = (int)CreateProfileMenuItemIndices.ServiceProviders)]
    public class ServiceProvidersProfile : BaseServiceProfile<IService> { }
}