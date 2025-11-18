// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Services;
using RealityCollective.ServiceFramework.Tests.Interfaces;
using UnityEngine;

namespace RealityCollective.ServiceFramework.Tests.Services
{
    [System.Runtime.InteropServices.Guid("80B2B43B-F18B-4E68-A9AB-505290D31110")]
    public class TestService1 : BaseServiceWithConstructor, ITestService1
    {
        public const string TestName = "Test Service 1";

        public TestService1(string name = TestName, uint priority = 0, BaseProfile profile = null)
            : base(name, priority)
        { }

        // Lifecycle tracking properties for testing
        public bool IsInitialized { get; private set; }
        public bool IsStarted { get; private set; }
        public int UpdateCount { get; private set; }
        public int LateUpdateCount { get; private set; }
        public int FixedUpdateCount { get; private set; }
        public bool IsDestroyed { get; private set; }
        public bool IsDisposed => IsDestroyed; // Track via Destroy since Dispose is not virtual

        public override void Initialize()
        {            base.Initialize();
            IsInitialized = true;
            Debug.Log($"{TestName} is Initialised");
        }

        public override void Start()
        {
            base.Start();
            IsStarted = true;
        }

        public override void Update()
        {
            base.Update();
            UpdateCount++;
        }

        public override void LateUpdate()
        {
            base.LateUpdate();
            LateUpdateCount++;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            FixedUpdateCount++;
        }

        public override void Destroy()
        {
            IsDestroyed = true;
            base.Destroy();
        }
    }
}