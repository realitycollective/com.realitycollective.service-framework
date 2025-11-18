// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Services;
using RealityCollective.ServiceFramework.Tests.Interfaces;
using UnityEngine;

namespace RealityCollective.ServiceFramework.Tests.Services
{
    public class TestService2 : BaseServiceWithConstructor, ITestService2
    {
        public const string TestName = "Test Service 2";

        public TestService2(string name = TestName, uint priority = 0, BaseProfile profile = null)
            : base(name, priority)
        { }

        // Lifecycle tracking properties for testing
        public bool IsInitialized { get; private set; }
        public bool IsStarted { get; private set; }
        public bool IsDestroyed { get; private set; }
        public bool IsDisposed => IsDestroyed; // Track via Destroy since Dispose is not virtual

        public override void Initialize()
        {
            //base.Initialize();
            IsInitialized = true;
            Debug.Log($"{TestName} is Initialised");
        }

        public override void Start()
        {
            base.Start();
            IsStarted = true;
        }

        public override void Destroy()
        {
            IsDestroyed = true;
            base.Destroy();
        }

        public override bool RegisterServiceModules => false;
    }
}