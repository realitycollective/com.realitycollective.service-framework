// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// Submitted by Joost van Schaik

using System;

namespace RealityCollective.ServiceFramework.Attributes
{
    /// <summary>
    /// Used as a generic inspector to render properties of a decorated object instead of the ToString value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Interface)]
    public class InspectorExpandAttribute : Attribute
    { }
}