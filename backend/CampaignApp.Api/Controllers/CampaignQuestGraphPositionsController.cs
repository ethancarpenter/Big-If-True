using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampaignApp.Api.Controllers;

[ApiController]
[Route("api/campaigns/{campaignId:guid}/quest-graph-positions")]
public class CampaignQuestGraphPositionsController : ControllerBase
{
    private readonly IQuestGraphPositionService _questGraphPositionService;

    public CampaignQuestGraphPositionsController(IQuestGraphPositionService questGraphPositionService)
    {
        _questGraphPositionService = questGraphPositionService;
    }

    [HttpGet]
    public async Task<ActionResult<List<QuestGraphPositionDto>>> GetAll(Guid campaignId)
    {
        var positions = await _questGraphPositionService.GetAllForCampaignAsync(campaignId);
        return positions is null ? NotFound() : Ok(positions);
    }
}
