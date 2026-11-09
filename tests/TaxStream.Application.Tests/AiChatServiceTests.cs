using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TaxStream.Application.Abstractions;
using TaxStream.Application.Services;
using TaxStream.Core.Entities;
using TaxStream.Core.Interfaces;
using Xunit;

namespace TaxStream.Application.Tests;

public class AiChatServiceTests
{
    private readonly Mock<IAiProviderService> _aiProviderMock = new();
    private readonly Mock<ITaxRecordRepository> _taxRepoMock = new();
    private readonly Mock<IAiConversationRepository> _conversationRepoMock = new();
    private readonly AiChatService _sut;

    public AiChatServiceTests()
    {
        _sut = new AiChatService(
            _aiProviderMock.Object,
            _taxRepoMock.Object,
            _conversationRepoMock.Object,
            NullLogger<AiChatService>.Instance);
    }

    [Fact]
    public async Task AskAsync_CallsAiWithUserScopedContext_AndPersistsConversation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taxRecords = new List<TaxRecord>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, TaxYear = 2025, FilingStatus = "Single", TaxableIncome = 80_000m }
        };

        _taxRepoMock
            .Setup(r => r.GetByUserIdAsync(userId, default))
            .ReturnsAsync(taxRecords);

        _aiProviderMock
            .Setup(ai => ai.SendAsync(It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync("You may be able to claim home office expenses.");

        AiConversation? savedConversation = null;
        _conversationRepoMock
            .Setup(r => r.AddAsync(It.IsAny<AiConversation>(), default))
            .Callback<AiConversation, CancellationToken>((c, _) => savedConversation = c)
            .ReturnsAsync((AiConversation c, CancellationToken _) => c);

        // Act
        var result = await _sut.AskAsync("Can I claim my home office?", userId);

        // Assert
        result.Response.Should().Be("You may be able to claim home office expenses.");
        result.ConversationId.Should().NotBeEmpty();
        result.Disclaimer.Should().NotBeEmpty(); // AI disclaimer must always be present

        // Verify conversation was persisted
        _conversationRepoMock.Verify(r => r.AddAsync(It.IsAny<AiConversation>(), default), Times.Once);
        _conversationRepoMock.Verify(r => r.SaveChangesAsync(default), Times.Once);

        savedConversation.Should().NotBeNull();
        savedConversation!.UserId.Should().Be(userId); // Scoped to correct user
        savedConversation.UserPrompt.Should().Be("Can I claim my home office?");
    }

    [Fact]
    public async Task AskAsync_AiProviderFails_ThrowsAndDoesNotPersist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _taxRepoMock
            .Setup(r => r.GetByUserIdAsync(userId, default))
            .ReturnsAsync(new List<TaxRecord>());

        _aiProviderMock
            .Setup(ai => ai.SendAsync(It.IsAny<string>(), It.IsAny<string>(), default))
            .ThrowsAsync(new HttpRequestException("AI provider unavailable"));

        // Act
        var act = () => _sut.AskAsync("test prompt", userId);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();

        // Conversation must NOT be persisted on provider failure
        _conversationRepoMock.Verify(r => r.AddAsync(It.IsAny<AiConversation>(), default), Times.Never);
    }

    [Fact]
    public async Task AskAsync_UsesOnlyAuthenticatedUserTaxRecords()
    {
        // Arrange — the repo is set up with userId-scoped records
        var userId = Guid.NewGuid();

        _taxRepoMock
            .Setup(r => r.GetByUserIdAsync(userId, default))
            .ReturnsAsync(new List<TaxRecord>());

        _aiProviderMock
            .Setup(ai => ai.SendAsync(It.IsAny<string>(), It.IsAny<string>(), default))
            .ReturnsAsync("response");

        _conversationRepoMock
            .Setup(r => r.AddAsync(It.IsAny<AiConversation>(), default))
            .ReturnsAsync((AiConversation c, CancellationToken _) => c);

        // Act
        await _sut.AskAsync("prompt", userId);

        // Assert — repo was called with the AUTHENTICATED userId, not any other source
        _taxRepoMock.Verify(r => r.GetByUserIdAsync(userId, default), Times.Once);
    }

    [Fact]
    public async Task GetHistoryAsync_ReturnsOnlyUserHistory()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var conversations = new List<AiConversation>
        {
            new() { Id = Guid.NewGuid(), UserId = userId, UserPrompt = "q1", AiResponse = "a1", Timestamp = DateTimeOffset.UtcNow },
            new() { Id = Guid.NewGuid(), UserId = userId, UserPrompt = "q2", AiResponse = "a2", Timestamp = DateTimeOffset.UtcNow }
        };

        _conversationRepoMock
            .Setup(r => r.GetByUserIdAsync(userId, 1, 20, default))
            .ReturnsAsync(conversations);

        // Act
        var result = await _sut.GetHistoryAsync(userId, 1, 20);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.All(i => !string.IsNullOrEmpty(i.UserPrompt)).Should().BeTrue();
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(20);
    }
}

