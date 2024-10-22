// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Services;
using RealityCollective.ServiceFramework.Tests.Interfaces;
using UnityEngine;

namespace RealityCollective.ServiceFramework.Tests.Services
{
    public class DependencyTestService2 : BaseServiceWithConstructor, ITestDependencyService2
    {
        public const string TestName = "Dependency Test Service 2";
        public ITestService1 testService1;
        public ITestDependencyService1 testService2;

        public DependencyTestService2(string name, uint priority, BaseProfile profile, ITestService1 testService1, ITestDependencyService1 testService2)
            : base(name, priority)
        {
            this.testService1 = testService1;
            this.testService2 = testService2;
        }

        public override void Initialize()
        {
            //base.Initialize();
            Debug.Log($"{TestName} is Initialised");
        }

        public override bool RegisterServiceModules => false;
    }
}