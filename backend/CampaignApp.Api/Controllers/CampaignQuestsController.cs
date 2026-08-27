using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampaignApp.Api.Controllers;

[ApiController]
[Route("api/campaigns/{campaignId:guid}/quests")]
public class CampaignQuestsController : ControllerBase
{
    private readonly IQuestService _questService;

    public CampaignQuestsController(IQuestService questService)
    {
        _questService = questService;
    }

    [HttpGet]
    public async Task<ActionResult<List<QuestDto>>> GetAll(Guid campaignId)
    {
        var quests = await _questService.GetAllForCampaignAsync(campaignId);
        return quests is null ? NotFound() : Ok(quests);
    }

    [HttpPost]
    public async Task<ActionResult<QuestDto>> Create(Guid campaignId, [FromBody] QuestRequestDto request)
    {
        var quest = await _questService.CreateAsync(campaignId, request);
        if (quest is null)
        {
            return NotFound();
        }

        return CreatedAtAction(
            nameof(QuestsController.GetById),
            "Quests",
            new { id = quest.Id },
            quest);
    }
}
