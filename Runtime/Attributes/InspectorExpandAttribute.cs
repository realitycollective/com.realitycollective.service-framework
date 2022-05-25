using System;

namespace RealityCollective.ServiceFramework.Attributes
{
    /// <summary>
    /// Can be used in to for a generic inspector to render properties of the decorated object
    /// in stead of the ToString value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Interface)]
    public class InspectorExpandAttribute : Attribute
    {
    }
}
