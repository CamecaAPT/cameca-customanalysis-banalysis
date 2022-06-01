using Cameca.CustomAnalysis.Interface;
using Prism.Commands;
using Prism.Events;

namespace Cameca.CustomAnalysis.BAnalysis;

internal class YieldSpecimenAnalysis1NodeMenuFactory : IAnalysisMenuFactory
{

	public const string UniqueId = "Cameca.CustomAnalysis.BAnalysis.YieldSpecimenAnalysis1NodeMenuFactory";

	private readonly IEventAggregator _eventAggregator;

	public YieldSpecimenAnalysis1NodeMenuFactory(IEventAggregator eventAggregator)
	{
		_eventAggregator = eventAggregator;
	}

	public IMenuItem CreateMenuItem(IAnalysisMenuContext context) => new MenuAction(
		YieldSpecimenAnalysis1Node.DisplayInfo.Title,
		new DelegateCommand(() => _eventAggregator.PublishCreateNode(
			YieldSpecimenAnalysis1Node.UniqueId,
			context.NodeId,
			YieldSpecimenAnalysis1Node.DisplayInfo.Title,
			YieldSpecimenAnalysis1Node.DisplayInfo.Icon)),
		YieldSpecimenAnalysis1Node.DisplayInfo.Icon);

	public AnalysisMenuLocation Location { get; } = AnalysisMenuLocation.Analysis;
}
