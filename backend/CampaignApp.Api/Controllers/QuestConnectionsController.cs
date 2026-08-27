using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampaignApp.Api.Controllers;

[ApiController]
[Route("api/quest-connections")]
public class QuestConnectionsController : ControllerBase
{
    private readonly IQuestConnectionService _questConnectionService;

    public QuestConnectionsController(IQuestConnectionService questConnectionService)
    {
        _questConnectionService = questConnectionService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<QuestConnectionDto>> GetById(Guid id)
    {
        var connection = await _questConnectionService.GetByIdAsync(id);
        return connection is null ? NotFound() : Ok(connection);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<QuestConnectionDto>> Update(Guid id, [FromBody] QuestConnectionUpdateRequestDto request)
    {
        var result = await _questConnectionService.UpdateAsync(id, request);

        return result.Outcome switch
        {
            UpdateQuestConnectionOutcome.Success => Ok(result.Connection),
            UpdateQuestConnectionOutcome.Cycle => Conflict(new { reason = "Cycle" }),
            _ => NotFound(),
        };
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _questConnectionService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
