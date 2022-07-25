using Cameca.CustomAnalysis.Interface;
using Cameca.CustomAnalysis.Utilities;
using Prism.Ioc;
using Prism.Modularity;

namespace Cameca.CustomAnalysis.BAnalysis;

public class YieldSpecimenAnalysis1Module : IModule
{
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.AddCustomAnalysisUtilities();
        containerRegistry.Register<YieldSpecimenAnalysis1>();

        containerRegistry.Register<object, YieldSpecimenAnalysis1Node>(YieldSpecimenAnalysis1Node.UniqueId);
        containerRegistry.RegisterInstance<INodeDisplayInfo>(YieldSpecimenAnalysis1Node.DisplayInfo, YieldSpecimenAnalysis1Node.UniqueId);
        containerRegistry.Register<IAnalysisMenuFactory, YieldSpecimenAnalysis1NodeMenuFactory>(YieldSpecimenAnalysis1NodeMenuFactory.UniqueId);
        containerRegistry.Register<object, YieldSpecimenAnalysis1ViewModel>(YieldSpecimenAnalysis1ViewModel.UniqueId);
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
        var extensionRegistry = containerProvider.Resolve<IExtensionRegistry>();
        extensionRegistry.RegisterAnalysisView<YieldSpecimenAnalysis1View, YieldSpecimenAnalysis1ViewModel>(AnalysisViewLocation.Top);
    }
}