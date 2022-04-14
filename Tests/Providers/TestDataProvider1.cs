// Copyright (c) Reality Collective. All rights reserved.

using RealityToolkit.ServiceFramework.Definitions;
using RealityToolkit.ServiceFramework.Interfaces;
using RealityToolkit.ServiceFramework.Providers;
using RealityToolkit.ServiceFramework.Tests.Interfaces;

namespace RealityToolkit.ServiceFramework.Tests.Providers
{
    [System.Runtime.InteropServices.Guid("407D379E-3351-4B2D-9C88-1B54C42B5554")]
    internal class TestDataProvider1 : BaseServiceDataProvider, ITestDataProvider1
    {
        public const string TestName = "Test Data Provider 1";

        public TestDataProvider1(IService parentService, string name = TestName, uint priority = 1, BaseProfile profile = null)
            : base(name, priority, profile, parentService)
        { }
    }
}