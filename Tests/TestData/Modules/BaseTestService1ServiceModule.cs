// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Interfaces;
using RealityCollective.ServiceFramework.Modules;
using RealityCollective.ServiceFramework.Tests.Interfaces;
using UnityEngine;

namespace RealityCollective.ServiceFramework.Tests.Modules
{
    [System.Runtime.InteropServices.Guid("f5309e95-8811-4200-8831-49a8043d6afa")]
    public class BaseTestService1ServiceModule : BaseServiceModule, ITestService1ServiceModule
    {
        public const string TestName = "BaseTestService1DataProvider";

        public BaseTestService1ServiceModule(string name = TestName, uint priority = 1, BaseProfile profile = null, IService parentService = null)
            : base(name, priority, profile, parentService)
        { }

        public override void Initialize()
        {
            base.Initialize();
            Debug.Log($"{TestName} is Initialised");
        }
    }
}