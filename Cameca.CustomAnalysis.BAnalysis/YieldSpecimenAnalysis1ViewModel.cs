using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Cameca.CustomAnalysis.Interface;
using Cameca.CustomAnalysis.Utilities;
using CommunityToolkit.Mvvm.Input;

namespace Cameca.CustomAnalysis.BAnalysis;

internal class YieldSpecimenAnalysis1ViewModel : AnalysisViewModelBase<YieldSpecimenAnalysis1Node>
{
	public const string UniqueId = "Cameca.CustomAnalysis.BAnalysis.YieldSpecimenAnalysis1ViewModel";

	private readonly IRenderDataFactory renderDataFactory;
	private bool optionsChanged = false;

	private readonly AsyncRelayCommand runCommand;
	public ICommand RunCommand => runCommand;

	public YieldSpecimenAnalysis1Options Options => Node!.Options;

	public ObservableCollection<object> Tabs { get; } = new();

	private object? selectedTab;
	public object? SelectedTab
	{
		get => selectedTab;
		set => SetProperty(ref selectedTab, value);
	}


	public YieldSpecimenAnalysis1ViewModel(
		IAnalysisViewModelBaseServices services,
		IRenderDataFactory renderDataFactory) : base(services)
	{
		this.renderDataFactory = renderDataFactory;
		runCommand = new AsyncRelayCommand(OnRun, UpdateSelectedEventCountsEnabled);
	}

	protected override void OnCreated(ViewModelCreatedEventArgs eventArgs)
	{
		base.OnCreated(eventArgs);
		if (Node is { } node)
		{
			node.Options.PropertyChanged += OptionsOnPropertyChanged;
		}
	}

	private async Task OnRun()
	{
		foreach (var item in Tabs)
		{
			if (item is IDisposable disposable)
				disposable.Dispose();
		}
		Tabs.Clear();

		var data = await Node!.Run();
		if (data is null) return;

		Tabs.Add(new TableContentViewModel("Log", data.LogEntries.Select(x => (object)x).ToList()));

		if (data.Counts.Any())
		{
			Tabs.Add(new TableContentViewModel("Counts", data.Counts.Select(x => (object)x).ToList()));
		}
		if (data.ProxigramData.Any())
		{
			Tabs.Add(new TableContentViewModel("Proxigram Data", data.ProxigramData.Select(x => (object)x).ToList()));
		}
		if (data.Composition1DData.Any())
		{
			Tabs.Add(new TableContentViewModel("Composition 1D Data", data.Composition1DData.Select(x => (object)x).ToList()));
		}

		// var experimentalLineRenderData = renderDataFactory.CreateLine(
		// 	data.Experimental,
		// 	Colors.Red,
		// 	name: "Experimental");
		// var randomizedLineRenderData = renderDataFactory.CreateLine(
		// 	data.Experimental,
		// 	Colors.Blue,
		// 	name: "Randomized");
		// var chart2DContentViewModel = new Chart2DContentViewModel(
		// 	"Frequency distribution",
		// 	new IRenderData[]
		// 	{
		// 		experimentalLineRenderData,
		// 		randomizedLineRenderData
		// 	});
		// Tabs.Add(chart2DContentViewModel);
		SelectedTab = Tabs.FirstOrDefault();
	}

	private void OptionsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		optionsChanged = true;
		runCommand.NotifyCanExecuteChanged();
	}


	private bool UpdateSelectedEventCountsEnabled() => !Tabs.Any() || optionsChanged;
}
