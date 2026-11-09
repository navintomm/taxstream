namespace TaxStream.Application.DTOs.TaxRecords;

public class DeductionDto
{
    public Guid Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
}
