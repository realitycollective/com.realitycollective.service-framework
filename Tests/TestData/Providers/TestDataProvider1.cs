// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Interfaces;
using RealityCollective.ServiceFramework.Providers;
using RealityCollective.ServiceFramework.Tests.Interfaces;
using UnityEngine;

namespace RealityCollective.ServiceFramework.Tests.Providers
{
    [System.Runtime.InteropServices.Guid("407D379E-3351-4B2D-9C88-1B54C42B5554")]
    public class TestDataProvider1 : BaseServiceDataProvider, ITestDataProvider1
    {
        public const string TestName = "Test Data Provider 1";

        public TestDataProvider1(string name = TestName, uint priority = 1, BaseProfile profile = null, IService parentService = null)
            : base(name, priority, profile, parentService)
        { }

        public override void Initialize()
        {
            base.Initialize();
            Debug.Log($"{TestName} is Initialised");
        }
    }
}