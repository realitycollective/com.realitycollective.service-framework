using RealityCollective.ServiceFramework.Definitions;
using RealityCollective.ServiceFramework.Interfaces;

namespace RealityCollective.ServiceFramework.Editor.Packages
{
    /// <summary>
    /// A package installer that will install <see cref="IServiceModule"/>s coming from a third
    /// party package into the respective <see cref="IService"/>'s <see cref="BaseServiceProfile{TService}"/>.
    /// </summary>
    public interface IPackageModulesInstaller
    {
        /// <summary>
        /// Installs the <paramref name="serviceConfiguration"/>.
        /// </summary>
        /// <param name="serviceConfiguration">The <see cref="IServiceConfiguration{T}"/> containing
        /// the configured <see cref="IServiceModule"/> to install.</param>
        /// <returns><c>true</c>, if the <see cref="IServiceModule"/> was approved and installed.</returns>
        bool Install(ServiceConfiguration serviceConfiguration);
    }
}