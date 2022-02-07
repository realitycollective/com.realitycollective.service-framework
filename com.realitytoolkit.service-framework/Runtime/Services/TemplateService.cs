// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using RealityToolkit.ServiceFramework.Definitions;
using RealityToolkit.ServiceFramework.Extensions;
using RealityToolkit.ServiceFramework.Interfaces;

namespace RealityToolkit.ServiceFramework.Services
{
    public class TemplateService : BaseServiceWithConstructor, IService
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="profile"></param>
        protected TemplateService(BaseProfile profile)
        {
            if (profile.IsNull())
            {
                throw new ArgumentException($"Missing the profile for {base.Name} system!");
            }
        }
    }
}