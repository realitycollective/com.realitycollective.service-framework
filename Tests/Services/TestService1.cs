// Copyright (c) xRealityLabs. All rights reserved.

using RealityToolkit.ServiceFramework.Definitions;
using RealityToolkit.ServiceFramework.Services;
using RealityToolkit.ServiceFramework.Tests.Interfaces;

namespace RealityToolkit.ServiceFramework.Tests.Services
{
    [System.Runtime.InteropServices.Guid("80B2B43B-F18B-4E68-A9AB-505290D31110")]
    public class TestService1 : BaseServiceWithConstructor, ITestService
    {
        public const string TestName = "Test Service 1";

        public TestService1(string name = TestName, uint priority = 0, BaseProfile profile = null)
            : base(name, priority)
        { }
    }
}