using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxStream.Application.DTOs.AI;
using TaxStream.Application.Services;
using TaxStream.WebAPI.Extensions;

namespace TaxStream.WebAPI.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly AiChatService _aiChatService;

    public AiController(AiChatService aiChatService)
    {
        _aiChatService = aiChatService;
    }

    /// <summary>Submit a prompt to the AI chat endpoint.</summary>
    [HttpPost("chat")]
    [ProducesResponseType(typeof(AiChatResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Chat([FromBody] AiChatRequestDto request, CancellationToken cancellationToken)
    {
        // UserId is derived from the validated JWT claim — never from request body
        var userId = User.GetUserId();
        var response = await _aiChatService.AskAsync(request.Prompt, userId, cancellationToken);
        return Ok(response);
    }

    /// <summary>Retrieve the authenticated user's AI conversation history (paginated).</summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(AiHistoryResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> History(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var history = await _aiChatService.GetHistoryAsync(userId, pageNumber, pageSize, cancellationToken);
        return Ok(history);
    }
}
