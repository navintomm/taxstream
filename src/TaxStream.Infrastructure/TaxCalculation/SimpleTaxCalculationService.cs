using TaxStream.Core.Entities;
using TaxStream.Core.Interfaces;

namespace TaxStream.Infrastructure.TaxCalculation;

/// <summary>
/// Placeholder tax calculation service.
/// Replace with jurisdiction-specific, versioned calculation rules once jurisdiction is confirmed.
/// </summary>
public class SimpleTaxCalculationService : ITaxCalculationService
{
    public decimal CalculateTaxLiability(TaxRecord taxRecord)
    {
        // TODO: Replace with verified, jurisdiction-specific calculation rules.
        // The AI provider must NOT be used for official tax liability calculation.
        // This placeholder returns 0 until real rules are implemented.
        return 0m;
    }
}
