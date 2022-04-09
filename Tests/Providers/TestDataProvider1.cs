// Copyright (c) Reality Collective. All rights reserved.

using RealityToolkit.ServiceFramework.Definitions;
using RealityToolkit.ServiceFramework.Providers;
using RealityToolkit.ServiceFramework.Tests.Interfaces;

namespace RealityToolkit.ServiceFramework.Tests.Providers
{
    internal class TestDataProvider1 : BaseServiceDataProvider, ITestDataProvider1
    {
        public const string TestName = "Test Data Provider 1";
        public TestDataProvider1(ITestService parentService, string name = TestName, uint priority = 1, BaseProfile profile = null)
            : base(name, priority, profile, parentService)
        { }

        public bool IsEnabled { get; private set; }

        public override void Enable()
        {
            IsEnabled = true;
        }

        public override void Disable()
        {
            IsEnabled = false;
        }
    }
}