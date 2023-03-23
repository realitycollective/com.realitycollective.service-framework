using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Interfaces;
using System;

namespace RealityCollective.ServiceFramework.Editor.Packages
{
    /// <summary>
    /// A package installer that will install <see cref="IServiceModule"/>s coming from a third
    /// party package into the respective <see cref="IService"/>'s <see cref="BaseServiceProfile{TService}"/>.
    /// </summary>
    public interface IPackageModulesInstaller<TModule> where TModule : IServiceModule
    {
        Type SupportedServiceModuleType { get; }

        bool Install(IServiceConfiguration<TModule> serviceConfiguration, ServiceProvidersProfile rootProfile);
    }
}