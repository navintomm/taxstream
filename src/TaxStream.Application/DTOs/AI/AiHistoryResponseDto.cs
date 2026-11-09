namespace TaxStream.Application.DTOs.AI;

public class AiHistoryResponseDto
{
    public List<AiHistoryItemDto> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}
