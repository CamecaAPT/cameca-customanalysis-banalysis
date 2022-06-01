using Prism.Mvvm;

namespace Cameca.CustomAnalysis.BAnalysis;

public class YieldSpecimenAnalysis1Options : BindableBase
{
	private float profileMaxDistanceNm = 10.0f;
	private float profileBinWidthNm = 1.0f;
	private float isoLevelFractionNi = 0.10f;

	public float ProfileMaxDistanceNm
	{
		get => profileMaxDistanceNm;
		set => SetProperty(ref profileMaxDistanceNm, value);
	}

	public float ProfileBinWidthNm
	{
		get => profileBinWidthNm;
		set => SetProperty(ref profileBinWidthNm, value);
	}

	public float IsoLevelFraction_Ni
	{
		get => isoLevelFractionNi;
		set => SetProperty(ref isoLevelFractionNi, value);
	}
}
