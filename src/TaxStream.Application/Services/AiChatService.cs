using Microsoft.Extensions.Logging;
using TaxStream.Application.Abstractions;
using TaxStream.Application.DTOs.AI;
using TaxStream.Core.Entities;
using TaxStream.Core.Interfaces;

namespace TaxStream.Application.Services;

public class AiChatService
{
    private readonly IAiProviderService _aiProviderService;
    private readonly ITaxRecordRepository _taxRecordRepository;
    private readonly IAiConversationRepository _conversationRepository;
    private readonly ILogger<AiChatService> _logger;

    private const string SystemPromptTemplate =
        "You are TaxStream AI, a helpful tax information assistant. " +
        "You provide informational guidance only — not legal or guaranteed tax advice. " +
        "Always remind users to verify with official sources or a qualified professional. " +
        "Do not make filing decisions, calculate authoritative liability, or access data beyond what is provided. " +
        "User tax context (for reference only): {0}";

    public AiChatService(
        IAiProviderService aiProviderService,
        ITaxRecordRepository taxRecordRepository,
        IAiConversationRepository conversationRepository,
        ILogger<AiChatService> logger)
    {
        _aiProviderService = aiProviderService;
        _taxRecordRepository = taxRecordRepository;
        _conversationRepository = conversationRepository;
        _logger = logger;
    }

    public async Task<AiChatResponseDto> AskAsync(string prompt, Guid userId, CancellationToken cancellationToken = default)
    {
        // Load only minimum context scoped to this user
        var taxRecords = await _taxRecordRepository.GetByUserIdAsync(userId, cancellationToken);
        var context = BuildContext(taxRecords);
        var systemPrompt = string.Format(SystemPromptTemplate, context);

        _logger.LogInformation("AI chat request by User: {UserId}", userId);

        string aiResponse;
        try
        {
            aiResponse = await _aiProviderService.SendAsync(systemPrompt, prompt, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI provider error for User: {UserId}", userId);
            throw;
        }

        var conversation = new AiConversation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserPrompt = prompt,
            AiResponse = aiResponse,
            Timestamp = DateTimeOffset.UtcNow
        };

        await _conversationRepository.AddAsync(conversation, cancellationToken);
        await _conversationRepository.SaveChangesAsync(cancellationToken);

        return new AiChatResponseDto
        {
            ConversationId = conversation.Id,
            Response = aiResponse
        };
    }

    public async Task<AiHistoryResponseDto> GetHistoryAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var items = await _conversationRepository.GetByUserIdAsync(userId, pageNumber, pageSize, cancellationToken);

        return new AiHistoryResponseDto
        {
            Items = items.Select(c => new AiHistoryItemDto
            {
                Id = c.Id,
                UserPrompt = c.UserPrompt,
                AiResponse = c.AiResponse,
                Timestamp = c.Timestamp
            }).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    private static string BuildContext(IReadOnlyList<TaxRecord> records)
    {
        if (!records.Any())
            return "No tax records on file.";

        // Minimum context: summary only, no detailed financial data beyond year and filing status
        var summaries = records.Select(r =>
            $"TaxYear={r.TaxYear}, FilingStatus={r.FilingStatus}, TaxableIncome={r.TaxableIncome:C2}");
        return string.Join("; ", summaries);
    }
}
