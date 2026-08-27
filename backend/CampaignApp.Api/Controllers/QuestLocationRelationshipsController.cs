using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampaignApp.Api.Controllers;

[ApiController]
[Route("api/quest-locations")]
public class QuestLocationRelationshipsController : ControllerBase
{
    private readonly IQuestLocationService _questLocationService;

    public QuestLocationRelationshipsController(IQuestLocationService questLocationService)
    {
        _questLocationService = questLocationService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<QuestLocationDto>> GetById(Guid id)
    {
        var relationship = await _questLocationService.GetByIdAsync(id);
        return relationship is null ? NotFound() : Ok(relationship);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<QuestLocationDto>> Update(Guid id, [FromBody] QuestLocationUpdateRequestDto request)
    {
        var relationship = await _questLocationService.UpdateAsync(id, request);
        return relationship is null ? NotFound() : Ok(relationship);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _questLocationService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
