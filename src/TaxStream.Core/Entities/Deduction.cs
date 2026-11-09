namespace TaxStream.Core.Entities;

public class Deduction
{
    public Guid Id { get; set; }
    public Guid TaxRecordId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }

    // Navigation
    public TaxRecord? TaxRecord { get; set; }
}
