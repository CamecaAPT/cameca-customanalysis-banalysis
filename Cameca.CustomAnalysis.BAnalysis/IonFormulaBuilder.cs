using System;
using System.Linq;
using System.Text.RegularExpressions;
using Cameca.CustomAnalysis.Interface;

namespace Cameca.CustomAnalysis.BAnalysis;

internal static class IonFormulaBuilder
{
    private const string IonFormulaComponentPattern = @"(?<Name>[A-Z][a-z]?)(?<Count>\d?)";
    public static IonFormula Parse(string formula)
    {
        var parsedComponent = Regex.Matches(formula, IonFormulaComponentPattern, RegexOptions.CultureInvariant);
        if (parsedComponent is null) throw new ArgumentException(nameof(formula));  // TODO: Handle better: maybe Try/Out pattern
        return new IonFormula(parsedComponent.Select(x => new IonFormula.Component(
            x.Groups["Name"].Value,
            int.TryParse(x.Groups["Count"].Value, out int count) ? Math.Max(count, 1) : 1)));
    }
}