using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cameca.CustomAnalysis.Interface;

namespace Cameca.CustomAnalysis.BAnalysis;

/// <summary>
/// Performs analysis of Si CSR and B in dopant region of yield specimen.
/// If Ni is found then a proxigram is performed, otherwise a 1d comp is done.
/// ionData is expected to be ranged with AT LEAST:
/// Ni, B, SiP1*, SiP2* 
/// (* these are exact names of unknown ion types.)
/// </summary>
internal class YieldSpecimenAnalysis1
{
	private readonly IIsosurfaceAnalysisProvider _isosurfaceAnalysisProvider;
	private readonly IProxigramAnalysisProvider _proxigramAnalysisProvider;
	private readonly IComposition1DAnalysisProvider _composition1DAnalysisProvider;

	private int IonType_B { get; set; }
	private int IonType_Ni { get; set; }
	private int IonType_SiP1 { get; set; }
	private int IonType_SiP2 { get; set; }

	private Guid instanceId = Guid.Empty;
	private IIonData? IonData { get; set; }
	
	private YieldSpecimenAnalysis1Results results = new YieldSpecimenAnalysis1Results();

	public YieldSpecimenAnalysis1(
		IIsosurfaceAnalysisProvider isosurfaceAnalysisProvider,
		IProxigramAnalysisProvider proxigramAnalysisProvider,
		IComposition1DAnalysisProvider composition1DAnalysisProvider)
	{
		_isosurfaceAnalysisProvider = isosurfaceAnalysisProvider;
		_proxigramAnalysisProvider = proxigramAnalysisProvider;
		_composition1DAnalysisProvider = composition1DAnalysisProvider;
	}

	public async Task<YieldSpecimenAnalysis1Results> Run(Guid instanceId, IIonData ionData, YieldSpecimenAnalysis1Options options)
	{
		results = new YieldSpecimenAnalysis1Results();
		IonData = ionData;
		this.instanceId = instanceId;

		bool success = SetupIonIds(ionData);
		if (!success)
		{
			results.LogEntries.Add("SetupIonIds failed, expecting B, Ni, and unknowns SiP1 and SiP2 to be ranged.  Exiting.");
			return results;
		}



		//Do a Bulk Composition Analysis
		IList<RawCountsModel> rawCountsList = GetRawComposition(ionData);
		ShowRawComposition(rawCountsList);
		int listLen = rawCountsList.Count;

		double niCounts = 0.0;
		double totalCounts = 0.0;
		for (int iName = 0; iName < listLen; ++iName)
		{
			totalCounts += rawCountsList[iName].Count;
			if (rawCountsList[iName].Name.Equals("Ni"))
			{
				IonType_Ni = iName;
				niCounts += rawCountsList[iName].Count;
			}
		}

		double niFrac = niCounts / totalCounts;
		results.LogEntries.Add("Ni Fraction = " + niFrac);

		if (niFrac > 0.01)
		{
			await ProcessWithNiCap(ionData, options);
		}
		else
		{
			await ProcessWithoutNiCap(ionData, options);
		}

		return results;
	}

	private async Task ProcessWithNiCap(IIonData ionData, YieldSpecimenAnalysis1Options Options)
	{
		results.LogEntries.Add("ProcessWithNiCap");

		// Build a 3d grid
		if (_isosurfaceAnalysisProvider.Resolve(instanceId) is not { } isosurfaceAnalysis) return;
		if (_proxigramAnalysisProvider.Resolve(instanceId) is not { } proxigramAnalysis) return;

		var allIons = ionData.Ions.Select(x => x.Formula).ToList();
		var ni_IonFormula = IonFormulaBuilder.Parse("Ni");
		var ratio = new IonRatio(
			new[] { ni_IonFormula },
			allIons);
		var isoSurfaceParams = new BuildIsosurfaceParameters(ratio);

		//float isoLevelFraction = 0.20f;
		var isosurface = await isosurfaceAnalysis.BuildIsosurface(isoSurfaceParams, Options.IsoLevelFraction_Ni);

		int numberOfInterfaces = isosurface.Length;
		results.LogEntries.Add("Number of interfaces =  " + numberOfInterfaces);

		int selectedInterface = 0;
		IInterfaceMetrics[] interfaceMetrics = new IInterfaceMetrics[1];
		interfaceMetrics[selectedInterface] = await isosurfaceAnalysis.CalculateInterfaceMetrics(isosurface.Span[selectedInterface].Mesh);
		
		results.LogEntries.Add("Number of Polys = " + interfaceMetrics[selectedInterface].NumPolygons);


		bool flipDirection = false;
		var subgroupRoi = new InterfaceSubgroupROI(
			isosurface[selectedInterface..(selectedInterface+1)],
			flipDirection ? InterfaceGradientDirection.Low : InterfaceGradientDirection.High);

		ProxigramParameters parameters = new ProxigramParameters
		{
			BinSize = Options.ProfileBinWidthNm,
			MaximumDistance = Options.ProfileMaxDistanceNm,
		};
		
		var proxigramData = await proxigramAnalysis.Run(subgroupRoi, parameters);

		DumpProxigramData(ionData, proxigramData);
	}

	private async Task ProcessWithoutNiCap(IIonData ionData, YieldSpecimenAnalysis1Options options)
	{
		if (_composition1DAnalysisProvider.Resolve(instanceId) is not { } composition1DAnalysis) return;

		results.LogEntries.Add("ProcessWithoutNiCap");

		// Do a 1d comp profile
		var analysisDirection = Composition1DDirection.Z;
		var computeMode = Composition1DMode.FixedBinWidth;
		int ionsPerSample = 0;
		int ionsPerStep = 0;
		var parameters = new Composition1DParameters
		{
			Mode = computeMode,
			Direction = analysisDirection,
			BinWidth = options.ProfileBinWidthNm,
			IonsPerSample = ionsPerSample,
			IonsPerStep = ionsPerStep,
		};
		var composition1DData = await composition1DAnalysis.Run(parameters);

		DumpComposition1DData(ionData, composition1DData);
	}

	public bool SetupIonIds(IIonData ionData)
	{
		IonType_B = -1;
		IonType_Ni = -1;
		IonType_SiP1 = -1;
		IonType_SiP2 = -1;

		var B_formula = IonFormulaBuilder.Parse("B");
		var Ni_formula = IonFormulaBuilder.Parse("Ni");
		var SiP1_formula = IonFormulaBuilder.Parse("SiP1");
		var SiP2_formula = IonFormulaBuilder.Parse("SiP2");

		var formulas = ionData.Ions.Select(x => x.Formula).ToList();
		for (int iName = 0; iName < formulas.Count; ++iName)
		{
			if (formulas.Contains(B_formula))
			{
				IonType_B = iName;
			}
			if (formulas.Contains(Ni_formula))
			{
				IonType_Ni = iName;
			}
			if (formulas.Contains(SiP1_formula))
			{
				IonType_SiP1 = iName;
			}
			if (formulas.Contains(SiP2_formula))
			{
				IonType_SiP2 = iName;
			}
		}

		bool success = true;
		if (IonType_B == -1)
		{
			success = false;
			results.LogEntries.Add("B was unranged");
		}
		if (IonType_Ni == -1)
		{
			success = false;
			results.LogEntries.Add("Ni was unranged");
		}
		if (IonType_SiP1 == -1)
		{
			success = false;
			results.LogEntries.Add("SiP1 was unranged");
		}
		if (IonType_SiP2 == -1)
		{
			success = false;
			results.LogEntries.Add("SiP2 was unranged");
		}

		return success;
	}

	public void DumpProxigramData(IIonData ionData, IProxigramResults proxigramData)
	{
		// Do the ionic distribution
		// [ROI, IonType, Bin] 
		var results_Ionic = proxigramData.DistributedResults_Ionic;
		var counts_Ionic = proxigramData.DistributedCounts_Ionic;
		var xArray_Ionic = proxigramData.XValues;
		var errors_Ionic = proxigramData.DistributedErrors_Ionic;

		List<ProfileTableModel> tableContent = new List<ProfileTableModel>();
		for (int i = 0; i < xArray_Ionic.Length; i++)
		{
			double siCSR = results_Ionic[0].Span[IonType_SiP2, i] / results_Ionic[0].Span[IonType_SiP1, i];
			var tableModel = new ProfileTableModel(i,
				xArray_Ionic.Span[i],
				results_Ionic[0].Span[IonType_Ni, i],
				results_Ionic[0].Span[IonType_B, i],
				results_Ionic[0].Span[IonType_SiP1, i],
				results_Ionic[0].Span[IonType_SiP2, i],
				siCSR
				);
			results.ProxigramData.Add(tableModel);
		}

	}

	public void DumpComposition1DData(IIonData ionData, IComposition1DResults composition1DData)
	{
		// Do the ionic distribution
		// [IonType, Bin] 
		var xArray_Ionic = composition1DData.XValues;
		var results_Ionic = composition1DData.DistributedYVals_Ionic;
		List<ProfileTableModel> tableContent = new List<ProfileTableModel>();
		for (int i = 0; i < xArray_Ionic.Length; i++)
		{
			double siCSR = results_Ionic.Span[IonType_SiP2, i] / results_Ionic.Span[IonType_SiP1, i];
			var tableModel = new ProfileTableModel(i,
				xArray_Ionic.Span[i],
				results_Ionic.Span[IonType_Ni, i],
				results_Ionic.Span[IonType_B, i],
				results_Ionic.Span[IonType_SiP1, i],
				results_Ionic.Span[IonType_SiP2, i],
				siCSR
				);
			results.Composition1DData.Add(tableModel);
		}

	}

	public IList<RawCountsModel> GetRawComposition(IIonData ionData)
	{
		List<RawCountsModel> tableContent = new List<RawCountsModel>();
		foreach (var (ionTypeInfo, count) in ionData.GetIonTypeCounts())
		{
			tableContent.Add(new RawCountsModel(ionTypeInfo.Name, count));
		}
		return tableContent;
	}

	public void ShowRawComposition(IEnumerable<RawCountsModel> rawCountsList)
	{
		results.Counts.AddRange(rawCountsList);
	}
}
