// Copyright (c) xRealityLabs. All rights reserved.

using RealityToolkit.ServiceFramework.Services;
using RealityToolkit.ServiceFramework.Tests.Interfaces;

namespace RealityToolkit.ServiceFramework.Tests.Services
{
    public class TestService2 : BaseServiceWithConstructor, ITestService2
    {
        public const string TestName = "Test Service 2";

        public TestService2(string name = TestName, uint priority = 0)
            : base(name, priority)
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