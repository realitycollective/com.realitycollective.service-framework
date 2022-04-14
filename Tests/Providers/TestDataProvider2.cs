// Copyright (c) Reality Collective. All rights reserved.

using RealityToolkit.ServiceFramework.Definitions;
using RealityToolkit.ServiceFramework.Interfaces;
using RealityToolkit.ServiceFramework.Providers;
using RealityToolkit.ServiceFramework.Tests.Interfaces;

namespace RealityToolkit.ServiceFramework.Tests.Providers
{
    internal class TestDataProvider2 : BaseServiceDataProvider, ITestDataProvider2
    {
        public const string TestName = "Test Data Provider 2";

        public TestDataProvider2(IService parentService, string name = TestName, uint priority = 2, BaseProfile profile = null)
            : base(name, priority, profile, parentService)
        { }


        public override void Enable()
        {
            base.Enable();
        }

        public override void Disable()
        {
            base.Disable();
        }
    }
}