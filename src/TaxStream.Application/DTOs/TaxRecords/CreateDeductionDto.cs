using System.ComponentModel.DataAnnotations;

namespace TaxStream.Application.DTOs.TaxRecords;

public class CreateDeductionDto
{
    [Required]
    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Amount must be non-negative.")]
    public decimal Amount { get; set; }
}
