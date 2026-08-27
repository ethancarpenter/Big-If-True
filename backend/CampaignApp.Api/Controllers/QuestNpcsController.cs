using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampaignApp.Api.Controllers;

[ApiController]
[Route("api/quests/{questId:guid}/npcs")]
public class QuestNpcsController : ControllerBase
{
    private readonly IQuestNpcService _questNpcService;

    public QuestNpcsController(IQuestNpcService questNpcService)
    {
        _questNpcService = questNpcService;
    }

    [HttpGet]
    public async Task<ActionResult<List<QuestNpcDto>>> GetAll(Guid questId)
    {
        var relationships = await _questNpcService.GetAllForQuestAsync(questId);
        return relationships is null ? NotFound() : Ok(relationships);
    }

    [HttpPost]
    public async Task<ActionResult<QuestNpcDto>> Create(Guid questId, [FromBody] QuestNpcCreateRequestDto request)
    {
        var result = await _questNpcService.CreateAsync(questId, request);

        return result.Outcome switch
        {
            CreateQuestNpcOutcome.Success => CreatedAtAction(
                nameof(QuestNpcRelationshipsController.GetById),
                "QuestNpcRelationships",
                new { id = result.Relationship!.Id },
                result.Relationship),
            CreateQuestNpcOutcome.Duplicate => Conflict(),
            _ => NotFound(),
        };
    }
}
