using Microsoft.Extensions.Logging;
using TaxStream.Application.DTOs.TaxRecords;
using TaxStream.Core.Entities;
using TaxStream.Core.Exceptions;
using TaxStream.Core.Interfaces;

namespace TaxStream.Application.Services;

public class TaxRecordService
{
    private readonly ITaxRecordRepository _taxRecordRepository;
    private readonly ITaxCalculationService _taxCalculationService;
    private readonly ILogger<TaxRecordService> _logger;

    public TaxRecordService(
        ITaxRecordRepository taxRecordRepository,
        ITaxCalculationService taxCalculationService,
        ILogger<TaxRecordService> logger)
    {
        _taxRecordRepository = taxRecordRepository;
        _taxCalculationService = taxCalculationService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TaxRecordDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var records = await _taxRecordRepository.GetByUserIdAsync(userId, cancellationToken);
        return records.Select(MapToDto).ToList();
    }

    public async Task<TaxRecordDto> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var record = await _taxRecordRepository.GetByIdWithDeductionsAsync(id, cancellationToken);
        EnsureOwnership(record, id, userId);
        return MapToDto(record!);
    }

    public async Task<TaxRecordDto> CreateAsync(CreateTaxRecordDto request, Guid userId, CancellationToken cancellationToken = default)
    {
        var record = new TaxRecord
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TaxYear = request.TaxYear,
            GrossIncome = request.GrossIncome,
            FilingStatus = request.FilingStatus,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Deductions = request.Deductions.Select(d => new Deduction
            {
                Id = Guid.NewGuid(),
                Category = d.Category,
                Description = d.Description,
                Amount = d.Amount
            }).ToList()
        };

        record.RecalculateDerivedValues();
        record.TaxLiability = _taxCalculationService.CalculateTaxLiability(record);

        await _taxRecordRepository.AddAsync(record, cancellationToken);
        await _taxRecordRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("TaxRecord created: {RecordId} for User: {UserId}", record.Id, userId);
        return MapToDto(record);
    }

    public async Task<TaxRecordDto> UpdateAsync(Guid id, UpdateTaxRecordDto request, Guid userId, CancellationToken cancellationToken = default)
    {
        var record = await _taxRecordRepository.GetByIdWithDeductionsAsync(id, cancellationToken);
        EnsureOwnership(record, id, userId);

        record!.TaxYear = request.TaxYear;
        record.GrossIncome = request.GrossIncome;
        record.FilingStatus = request.FilingStatus;
        record.Deductions = request.Deductions.Select(d => new Deduction
        {
            Id = Guid.NewGuid(),
            TaxRecordId = record.Id,
            Category = d.Category,
            Description = d.Description,
            Amount = d.Amount
        }).ToList();

        record.RecalculateDerivedValues();
        record.TaxLiability = _taxCalculationService.CalculateTaxLiability(record);

        await _taxRecordRepository.UpdateAsync(record, cancellationToken);
        await _taxRecordRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("TaxRecord updated: {RecordId} for User: {UserId}", record.Id, userId);
        return MapToDto(record);
    }

    public async Task DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var record = await _taxRecordRepository.GetByIdAsync(id, cancellationToken);
        EnsureOwnership(record, id, userId);

        await _taxRecordRepository.DeleteAsync(record!, cancellationToken);
        await _taxRecordRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("TaxRecord deleted: {RecordId} for User: {UserId}", id, userId);
    }

    private static void EnsureOwnership(TaxRecord? record, Guid id, Guid userId)
    {
        if (record is null)
            throw new NotFoundException(nameof(TaxRecord), id);
        if (record.UserId != userId)
            throw new AuthorizationException();
    }

    private static TaxRecordDto MapToDto(TaxRecord r) => new()
    {
        Id = r.Id,
        TaxYear = r.TaxYear,
        GrossIncome = r.GrossIncome,
        TotalDeductions = r.TotalDeductions,
        TaxableIncome = r.TaxableIncome,
        TaxLiability = r.TaxLiability,
        FilingStatus = r.FilingStatus,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt,
        Deductions = r.Deductions.Select(d => new DeductionDto
        {
            Id = d.Id,
            Category = d.Category,
            Description = d.Description,
            Amount = d.Amount
        }).ToList()
    };
}
