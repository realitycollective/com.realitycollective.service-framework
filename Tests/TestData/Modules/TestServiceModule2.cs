// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Interfaces;
using RealityCollective.ServiceFramework.Providers;
using RealityCollective.ServiceFramework.Tests.Interfaces;
using UnityEngine;

namespace RealityCollective.ServiceFramework.Tests.Providers
{
    public class TestDataProvider2 : BaseServiceDataProvider, ITestDataProvider2
    {
        public const string TestName = "Test Data Provider 2";

        public TestDataProvider2(string name = TestName, uint priority = 2, BaseProfile profile = null, IService parentService = null)
            : base(name, priority, profile, parentService)
        { }

        public override void Initialize()
        {
            base.Initialize();
            Debug.Log($"{TestName} is Initialised");
        }
    }
}