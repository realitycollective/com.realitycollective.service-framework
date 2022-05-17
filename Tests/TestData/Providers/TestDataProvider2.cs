// Copyright (c) Reality Collective. All rights reserved.

using RealityToolkit.ServiceFramework.Definitions;
using RealityToolkit.ServiceFramework.Interfaces;
using RealityToolkit.ServiceFramework.Providers;
using RealityToolkit.ServiceFramework.Tests.Interfaces;
using UnityEngine;

namespace RealityToolkit.ServiceFramework.Tests.Providers
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