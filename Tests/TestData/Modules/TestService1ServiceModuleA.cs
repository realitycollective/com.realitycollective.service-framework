// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Interfaces;
using RealityCollective.ServiceFramework.Tests.Interfaces;
using UnityEngine;

namespace RealityCollective.ServiceFramework.Tests.Modules
{
    [System.Runtime.InteropServices.Guid("725535fd-25a8-4d79-b3bd-6f6865df2adb")]
    public class TestService1ServiceModuleA : BaseTestService1ServiceModule, ITestService1ServiceModuleA
    {
        public new const string TestName = "TestService1DataProviderA";

        public TestService1ServiceModuleA(string name = TestName, uint priority = 1, BaseProfile profile = null, IService parentService = null)
            : base(name, priority, profile, parentService)
        { }

        public override void Initialize()
        {
            base.Initialize();
            Debug.Log($"{TestName} is Initialised");
        }
    }
}