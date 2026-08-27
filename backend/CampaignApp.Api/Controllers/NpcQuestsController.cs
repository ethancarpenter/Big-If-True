using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampaignApp.Api.Controllers;

[ApiController]
[Route("api/npcs/{npcId:guid}/quests")]
public class NpcQuestsController : ControllerBase
{
    private readonly IQuestNpcService _questNpcService;

    public NpcQuestsController(IQuestNpcService questNpcService)
    {
        _questNpcService = questNpcService;
    }

    [HttpGet]
    public async Task<ActionResult<List<QuestNpcDto>>> GetAll(Guid npcId)
    {
        var relationships = await _questNpcService.GetAllForNpcAsync(npcId);
        return relationships is null ? NotFound() : Ok(relationships);
    }
}
