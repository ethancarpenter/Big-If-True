using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampaignApp.Api.Controllers;

[ApiController]
[Route("api/campaigns/{campaignId:guid}/quest-connections")]
public class CampaignQuestConnectionsController : ControllerBase
{
    private readonly IQuestConnectionService _questConnectionService;

    public CampaignQuestConnectionsController(IQuestConnectionService questConnectionService)
    {
        _questConnectionService = questConnectionService;
    }

    [HttpGet]
    public async Task<ActionResult<List<QuestConnectionDto>>> GetAll(Guid campaignId)
    {
        var connections = await _questConnectionService.GetAllForCampaignAsync(campaignId);
        return connections is null ? NotFound() : Ok(connections);
    }

    [HttpPost]
    public async Task<ActionResult<QuestConnectionDto>> Create(Guid campaignId, [FromBody] QuestConnectionCreateRequestDto request)
    {
        var result = await _questConnectionService.CreateAsync(campaignId, request);

        return result.Outcome switch
        {
            CreateQuestConnectionOutcome.Success => CreatedAtAction(
                nameof(QuestConnectionsController.GetById),
                "QuestConnections",
                new { id = result.Connection!.Id },
                result.Connection),
            CreateQuestConnectionOutcome.Duplicate => Conflict(new { reason = "Duplicate" }),
            CreateQuestConnectionOutcome.Cycle => Conflict(new { reason = "Cycle" }),
            _ => NotFound(),
        };
    }
}
