// Copyright (c) xRealityLabs. All rights reserved.

using RealityToolkit.ServiceFramework.Definitions;
using RealityToolkit.ServiceFramework.Services;
using RealityToolkit.ServiceFramework.Tests.Interfaces;
using UnityEngine;

namespace RealityToolkit.ServiceFramework.Tests.Services
{
    public class TestService2 : BaseServiceWithConstructor, ITestService2
    {
        public const string TestName = "Test Service 2";

        public TestService2(string name = TestName, uint priority = 0, BaseProfile profile = null)
            : base(name, priority)
        { }

        public override void Initialize()
        {
            //base.Initialize();
            Debug.Log($"{TestName} is Initialised");
        }

        public override bool RegisterDataProviders => false;
    }
}