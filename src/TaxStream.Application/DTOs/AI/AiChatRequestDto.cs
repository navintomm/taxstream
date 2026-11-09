using System.ComponentModel.DataAnnotations;

namespace TaxStream.Application.DTOs.AI;

public class AiChatRequestDto
{
    [Required]
    [MinLength(1)]
    [MaxLength(2000)]
    public string Prompt { get; set; } = string.Empty;
}
