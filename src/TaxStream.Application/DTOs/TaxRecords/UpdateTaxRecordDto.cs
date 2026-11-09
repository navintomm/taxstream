using System.ComponentModel.DataAnnotations;

namespace TaxStream.Application.DTOs.TaxRecords;

public class UpdateTaxRecordDto
{
    [Required]
    [Range(2000, 2100)]
    public int TaxYear { get; set; }

    [Required]
    [Range(0, (double)decimal.MaxValue)]
    public decimal GrossIncome { get; set; }

    [Required]
    public string FilingStatus { get; set; } = string.Empty;

    public List<CreateDeductionDto> Deductions { get; set; } = new();
}
