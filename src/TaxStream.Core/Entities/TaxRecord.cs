namespace TaxStream.Core.Entities;

public class TaxRecord
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int TaxYear { get; set; }
    public decimal GrossIncome { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TaxableIncome { get; set; }
    public decimal TaxLiability { get; set; }
    public string FilingStatus { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public User? User { get; set; }
    public ICollection<Deduction> Deductions { get; set; } = new List<Deduction>();

    public void RecalculateDerivedValues()
    {
        TotalDeductions = Deductions.Sum(d => d.Amount);
        TaxableIncome = Math.Max(0, GrossIncome - TotalDeductions);
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
