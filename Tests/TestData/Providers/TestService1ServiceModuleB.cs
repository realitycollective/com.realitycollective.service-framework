// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Interfaces;
using RealityCollective.ServiceFramework.Tests.Interfaces;
using UnityEngine;

namespace RealityCollective.ServiceFramework.Tests.Providers
{
    [System.Runtime.InteropServices.Guid("df746fcc-cf9b-414f-bf7e-0d311cfdd8ac")]
    public class TestService1DataProviderB : BaseTestService1DataProvider, ITestService1DataProviderB
    {
        public new const string TestName = "TestService1DataProviderB";

        public TestService1DataProviderB(string name = TestName, uint priority = 1, BaseProfile profile = null, IService parentService = null)
            : base(name, priority, profile, parentService)
        { }

        public override void Initialize()
        {
            base.Initialize();
            Debug.Log($"{TestName} is Initialised");
        }
    }
}