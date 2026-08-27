using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampaignApp.Api.Controllers;

[ApiController]
[Route("api/quest-npcs")]
public class QuestNpcRelationshipsController : ControllerBase
{
    private readonly IQuestNpcService _questNpcService;

    public QuestNpcRelationshipsController(IQuestNpcService questNpcService)
    {
        _questNpcService = questNpcService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<QuestNpcDto>> GetById(Guid id)
    {
        var relationship = await _questNpcService.GetByIdAsync(id);
        return relationship is null ? NotFound() : Ok(relationship);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<QuestNpcDto>> Update(Guid id, [FromBody] QuestNpcUpdateRequestDto request)
    {
        var relationship = await _questNpcService.UpdateAsync(id, request);
        return relationship is null ? NotFound() : Ok(relationship);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _questNpcService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
