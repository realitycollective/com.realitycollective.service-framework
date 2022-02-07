// Copyright (c) Reality Collective. All rights reserved.

using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RealityToolkit.ServiceFramework.Utilities.AwaitYieldInstructions
{
    public class BackgroundThread
    {
        public static ConfiguredTaskAwaitable.ConfiguredTaskAwaiter GetAwaiter()
        {
            return Task.Run(() => { }).ConfigureAwait(false).GetAwaiter();
        }
    }
}