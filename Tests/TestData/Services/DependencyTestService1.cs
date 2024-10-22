// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Services;
using RealityCollective.ServiceFramework.Tests.Interfaces;
using UnityEngine;

namespace RealityCollective.ServiceFramework.Tests.Services
{
    public class DependencyTestService1 : BaseServiceWithConstructor, ITestDependencyService1
    {
        public const string TestName = "Dependency Test Service 1";
        public ITestService1 testService1;

        public DependencyTestService1(string name, uint priority, BaseProfile profile, ITestService1 testService1)
            : base(name, priority)
        {
            this.testService1 = testService1;
        }

        public override void Initialize()
        {
            //base.Initialize();
            Debug.Log($"{TestName} is Initialised");
        }

        public override bool RegisterServiceModules => false;
    }
}