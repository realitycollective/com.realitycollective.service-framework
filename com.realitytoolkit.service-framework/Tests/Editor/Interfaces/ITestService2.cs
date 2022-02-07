// Copyright (c) Reality Collective. All rights reserved.


using RealityToolkit.ServiceFramework.Interfaces;

namespace RealityToolkit.ServiceFramework.Tests.Interfaces
{
    internal interface ITestService2 : IService
    {
        bool IsEnabled { get; }
    }
}