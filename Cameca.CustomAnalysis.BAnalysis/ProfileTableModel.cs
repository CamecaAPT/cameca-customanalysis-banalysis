namespace Cameca.CustomAnalysis.BAnalysis;

internal class ProfileTableModel
{
	public int Entry { get; private set; }
	public double XValueNm { get; private set; }
	public double NiComp { get; private set; }
	public double BComp { get; private set; }
	public double SiP1Comp { get; private set; }
	public double SiP2Comp { get; private set; }
	public double SiCSR { get; private set; }

	public ProfileTableModel(int entry, double xValueNm, double niComp, double bComp,
		double siP1Comp, double siP2Comp, double siCSR)
	{
		Entry = entry;
		XValueNm = xValueNm;
		NiComp = niComp;
		BComp = bComp;
		SiP1Comp = siP1Comp;
		SiP2Comp = siP2Comp;
		SiCSR = siCSR;
	}
}
