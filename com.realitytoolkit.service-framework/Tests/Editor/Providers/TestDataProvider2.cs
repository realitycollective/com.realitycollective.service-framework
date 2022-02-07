// Copyright (c) Reality Collective. All rights reserved.

using RealityToolkit.ServiceFramework.Definitions;
using RealityToolkit.ServiceFramework.Providers;
using RealityToolkit.ServiceFramework.Tests.Interfaces;

namespace RealityToolkit.ServiceFramework.Tests.Providers
{
    internal class TestDataProvider2 : BaseServiceDataProvider, ITestDataProvider2
    {
        public TestDataProvider2(ITestService parentService, string name = "TestDataProvider2", uint priority = 2, BaseProfile profile = null)
            : base(name, priority, profile, parentService)
        { }

        public bool IsEnabled { get; private set; }

        public override void Enable()
        {
            base.Enable();
            IsEnabled = true;
        }

        public override void Disable()
        {
            base.Disable();
            IsEnabled = false;
        }
    }
}