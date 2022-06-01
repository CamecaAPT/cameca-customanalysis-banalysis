using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Serialization;
using Cameca.CustomAnalysis.Interface;
using Cameca.CustomAnalysis.Utilities;

namespace Cameca.CustomAnalysis.BAnalysis;

[DefaultView(YieldSpecimenAnalysis1ViewModel.UniqueId, typeof(YieldSpecimenAnalysis1ViewModel))]
internal class YieldSpecimenAnalysis1Node : AnalysisNodeBase
{
	public class NodeDisplayInfo : INodeDisplayInfo
	{
		public string Title { get; } = "BAnalysis";
		public ImageSource? Icon { get; } = ImageContainer.BAnalysisIcon;
	}

	public static NodeDisplayInfo DisplayInfo { get; } = new();

	public const string UniqueId = "Cameca.CustomAnalysis.BAnalysis.YieldSpecimenAnalysis1Node";

	private readonly YieldSpecimenAnalysis1 yieldSpecimenAnalysis;

	public YieldSpecimenAnalysis1Options Options { get; private set; } = new();

	public YieldSpecimenAnalysis1Node(IAnalysisNodeBaseServices services, YieldSpecimenAnalysis1 yieldSpecimenAnalysis)
		: base(services)
	{
		this.yieldSpecimenAnalysis = yieldSpecimenAnalysis;
	}

	public async Task<YieldSpecimenAnalysis1Results?> Run()
	{
		if (await Services.IonDataProvider.GetIonData(InstanceId) is not { } ionData)
			return null;

		return await yieldSpecimenAnalysis.Run(InstanceId, ionData, Options);
	}

	protected override byte[]? GetSaveContent()
	{
		var serializer = new XmlSerializer(typeof(YieldSpecimenAnalysis1Options));
		using var stringWriter = new StringWriter();
		serializer.Serialize(stringWriter, Options);
		return Encoding.UTF8.GetBytes(stringWriter.ToString());
	}

	protected override void OnLoaded(NodeLoadedEventArgs eventArgs)
	{
		if (eventArgs.Data is not { } data) return;
		var xmlData = Encoding.UTF8.GetString(data);
		var serializer = new XmlSerializer(typeof(YieldSpecimenAnalysis1Options));
		using var stringReader = new StringReader(xmlData);
		if (serializer.Deserialize(stringReader) is YieldSpecimenAnalysis1Options loadedOptions)
		{
			Options = loadedOptions;
		}
	}

	protected override void OnInstantiated(INodeInstantiatedEventArgs eventArgs)
	{
		base.OnInstantiated(eventArgs);
		Options.PropertyChanged += OptionsOnPropertyChanged;
	}

	private void OptionsOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (CanSaveState is { } canSaveState)
		{
			canSaveState.CanSave = true;
		}
	}
}
