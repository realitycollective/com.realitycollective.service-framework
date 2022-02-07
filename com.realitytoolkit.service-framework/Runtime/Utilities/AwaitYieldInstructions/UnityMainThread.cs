// Copyright (c) Reality Collective. All rights reserved.

using UnityEngine;

namespace RealityToolkit.ServiceFramework.Utilities.AwaitYieldInstructions
{
    public class UnityMainThread : CustomYieldInstruction
    {
        public override bool keepWaiting => false;
    }
}