using System.Collections.Generic;

namespace Cameca.CustomAnalysis.BAnalysis;

internal class TableContentViewModel
{
	public string Title { get; }

	public ICollection<object> Items { get; }

	public TableContentViewModel(string title, ICollection<object> items)
	{
		Title = title;
		Items = items;
	}
}
