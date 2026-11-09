namespace TaxStream.Application.DTOs.TaxRecords;

public class TaxRecordDto
{
    public Guid Id { get; set; }
    public int TaxYear { get; set; }
    public decimal GrossIncome { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TaxableIncome { get; set; }
    public decimal TaxLiability { get; set; }
    public string FilingStatus { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public List<DeductionDto> Deductions { get; set; } = new();
}
