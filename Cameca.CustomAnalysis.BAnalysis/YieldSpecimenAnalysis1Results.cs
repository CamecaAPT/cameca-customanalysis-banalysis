using System.Collections.Generic;

namespace Cameca.CustomAnalysis.BAnalysis;

internal class YieldSpecimenAnalysis1Results
{
	public List<string> LogEntries { get; init; } = new List<string>();
	public List<RawCountsModel> Counts { get; init; } = new List<RawCountsModel>();
	public List<ProfileTableModel> ProxigramData { get; init; } = new List<ProfileTableModel>();
	public List<ProfileTableModel> Composition1DData { get; init; } = new List<ProfileTableModel>();
}