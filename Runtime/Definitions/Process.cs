// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace RealityCollective.ServiceFramework.Definitions
{
    public delegate void UpdateMethod(float dt);

    /// <summary>
    /// Process description for subscribing to the service manager update loop as a process not as a full service or module.
    /// </summary>
    public class Process
    {
        public UpdateMethod updateMethod;
        public readonly float period;
        public float periodCurrent;

        public Process(UpdateMethod updateMethod, float period)
        {
            this.updateMethod = updateMethod;
            this.period = period;
            periodCurrent = 0;
        }
    }
}