namespace Cameca.CustomAnalysis.BAnalysis;

internal class RawCountsModel
{
	public string Name { get; private set; }
	public ulong Count { get; private set; }

	public RawCountsModel(string name, ulong count)
	{
		Name = name;
		Count = count;
	}
}
