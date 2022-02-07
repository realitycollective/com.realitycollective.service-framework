using RealityToolkit.ServiceFramework.Definitions;
using RealityToolkit.ServiceFramework.Interfaces;
using UnityEngine;

namespace RealityToolkit.ServiceFramework.Profiles
{
    [CreateAssetMenu(menuName = "RealityToolkit/Services/Test Service Profile", fileName = "TestServiceProfile", order = (int)CreateProfileMenuItemIndices.ServiceConfig)]
    public class TestServiceProfile : BaseServiceProfile<IServiceDataProvider>
    {
        
    }
}