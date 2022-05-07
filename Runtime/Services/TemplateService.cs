// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityToolkit.ServiceFramework.Definitions;
using RealityToolkit.ServiceFramework.Extensions;
using RealityToolkit.ServiceFramework.Interfaces;
using System;

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