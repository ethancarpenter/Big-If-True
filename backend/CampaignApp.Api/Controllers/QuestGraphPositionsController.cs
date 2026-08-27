using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampaignApp.Api.Controllers;

[ApiController]
[Route("api/quests/{questId:guid}/graph-position")]
public class QuestGraphPositionsController : ControllerBase
{
    private readonly IQuestGraphPositionService _questGraphPositionService;

    public QuestGraphPositionsController(IQuestGraphPositionService questGraphPositionService)
    {
        _questGraphPositionService = questGraphPositionService;
    }

    [HttpPut]
    public async Task<ActionResult<QuestGraphPositionDto>> Upsert(Guid questId, [FromBody] QuestGraphPositionUpdateRequestDto request)
    {
        var position = await _questGraphPositionService.UpsertAsync(questId, request);
        return position is null ? NotFound() : Ok(position);
    }
}
