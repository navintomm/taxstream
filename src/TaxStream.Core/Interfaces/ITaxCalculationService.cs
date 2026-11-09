using TaxStream.Core.Entities;

namespace TaxStream.Core.Interfaces;

public interface ITaxCalculationService
{
    /// <summary>
    /// Calculates TaxLiability for the given TaxRecord (with Deductions loaded).
    /// Must be deterministic for given jurisdiction, tax year, and input set.
    /// </summary>
    decimal CalculateTaxLiability(TaxRecord taxRecord);
}
