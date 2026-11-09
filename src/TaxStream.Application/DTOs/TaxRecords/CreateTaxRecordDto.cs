using System.ComponentModel.DataAnnotations;

namespace TaxStream.Application.DTOs.TaxRecords;

public class CreateTaxRecordDto
{
    [Required]
    [Range(2000, 2100, ErrorMessage = "TaxYear must be between 2000 and 2100.")]
    public int TaxYear { get; set; }

    [Required]
    [Range(0, (double)decimal.MaxValue, ErrorMessage = "GrossIncome must be non-negative.")]
    public decimal GrossIncome { get; set; }

    [Required]
    public string FilingStatus { get; set; } = string.Empty;

    public List<CreateDeductionDto> Deductions { get; set; } = new();
}
