using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TaxStream.Application.DTOs.TaxRecords;
using TaxStream.Application.Services;
using TaxStream.Core.Entities;
using TaxStream.Core.Exceptions;
using TaxStream.Core.Interfaces;
using Xunit;

namespace TaxStream.Application.Tests;

public class TaxRecordServiceTests
{
    private readonly Mock<ITaxRecordRepository> _repoMock = new();
    private readonly Mock<ITaxCalculationService> _calcMock = new();
    private readonly TaxRecordService _sut;

    public TaxRecordServiceTests()
    {
        _sut = new TaxRecordService(
            _repoMock.Object,
            _calcMock.Object,
            NullLogger<TaxRecordService>.Instance);
    }

    [Fact]
    public async Task GetByIdAsync_WrongOwner_ThrowsAuthorizationException()
    {
        // Arrange — record exists but belongs to userA
        var userAId = Guid.NewGuid();
        var userBId = Guid.NewGuid();
        var recordId = Guid.NewGuid();

        _repoMock
            .Setup(r => r.GetByIdWithDeductionsAsync(recordId, default))
            .ReturnsAsync(new TaxRecord { Id = recordId, UserId = userAId, FilingStatus = "Single" });

        // Act — userB tries to access userA's record
        var act = () => _sut.GetByIdAsync(recordId, userBId);

        // Assert — must throw, not return the record
        await act.Should().ThrowAsync<AuthorizationException>();
    }

    [Fact]
    public async Task GetByIdAsync_RecordNotFound_ThrowsNotFoundException()
    {
        _repoMock
            .Setup(r => r.GetByIdWithDeductionsAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((TaxRecord?)null);

        var act = () => _sut.GetByIdAsync(Guid.NewGuid(), Guid.NewGuid());
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetByIdAsync_CorrectOwner_ReturnsTaxRecordDto()
    {
        var userId = Guid.NewGuid();
        var recordId = Guid.NewGuid();

        _repoMock
            .Setup(r => r.GetByIdWithDeductionsAsync(recordId, default))
            .ReturnsAsync(new TaxRecord
            {
                Id = recordId,
                UserId = userId,
                TaxYear = 2025,
                GrossIncome = 100_000m,
                FilingStatus = "Single",
                Deductions = new List<Deduction>()
            });

        var result = await _sut.GetByIdAsync(recordId, userId);

        result.Id.Should().Be(recordId);
        result.TaxYear.Should().Be(2025);
    }

    [Fact]
    public async Task CreateAsync_RecalculatesAndPersistsDerivedValues()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _calcMock.Setup(c => c.CalculateTaxLiability(It.IsAny<TaxRecord>())).Returns(15_000m);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<TaxRecord>(), default))
                 .ReturnsAsync((TaxRecord r, CancellationToken _) => r);

        var request = new CreateTaxRecordDto
        {
            TaxYear = 2025,
            GrossIncome = 100_000m,
            FilingStatus = "Single",
            Deductions = new List<CreateDeductionDto>
            {
                new() { Category = "Retirement", Amount = 20_000m }
            }
        };

        // Act
        var result = await _sut.CreateAsync(request, userId);

        // Assert — derived values must be calculated server-side
        result.TotalDeductions.Should().Be(20_000m);
        result.TaxableIncome.Should().Be(80_000m);  // 100k - 20k
        result.TaxLiability.Should().Be(15_000m);   // from calculator mock
    }

    [Fact]
    public async Task DeleteAsync_WrongOwner_ThrowsAuthorizationException()
    {
        var userAId = Guid.NewGuid();
        var userBId = Guid.NewGuid();
        var recordId = Guid.NewGuid();

        _repoMock
            .Setup(r => r.GetByIdAsync(recordId, default))
            .ReturnsAsync(new TaxRecord { Id = recordId, UserId = userAId, FilingStatus = "Single" });

        var act = () => _sut.DeleteAsync(recordId, userBId);
        await act.Should().ThrowAsync<AuthorizationException>();
    }
}

