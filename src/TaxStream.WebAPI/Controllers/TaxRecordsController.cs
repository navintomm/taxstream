using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxStream.Application.DTOs.TaxRecords;
using TaxStream.Application.Services;
using TaxStream.WebAPI.Extensions;

namespace TaxStream.WebAPI.Controllers;

[ApiController]
[Route("api/taxrecords")]
[Authorize]
public class TaxRecordsController : ControllerBase
{
    private readonly TaxRecordService _taxRecordService;

    public TaxRecordsController(TaxRecordService taxRecordService)
    {
        _taxRecordService = taxRecordService;
    }

    /// <summary>List all tax records owned by the authenticated user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaxRecordDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var records = await _taxRecordService.GetAllAsync(userId, cancellationToken);
        return Ok(records);
    }

    /// <summary>Get a single tax record by ID (must be owned by authenticated user).</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaxRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var record = await _taxRecordService.GetByIdAsync(id, userId, cancellationToken);
        return Ok(record);
    }

    /// <summary>Create a new tax record with optional deductions.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TaxRecordDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTaxRecordDto request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var record = await _taxRecordService.CreateAsync(request, userId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
    }

    /// <summary>Update (replace) an owned tax record and its deductions.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TaxRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaxRecordDto request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var record = await _taxRecordService.UpdateAsync(id, request, userId, cancellationToken);
        return Ok(record);
    }

    /// <summary>Delete an owned tax record.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        await _taxRecordService.DeleteAsync(id, userId, cancellationToken);
        return NoContent();
    }
}
