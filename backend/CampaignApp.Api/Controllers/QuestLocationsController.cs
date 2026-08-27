using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampaignApp.Api.Controllers;

[ApiController]
[Route("api/quests/{questId:guid}/locations")]
public class QuestLocationsController : ControllerBase
{
    private readonly IQuestLocationService _questLocationService;

    public QuestLocationsController(IQuestLocationService questLocationService)
    {
        _questLocationService = questLocationService;
    }

    [HttpGet]
    public async Task<ActionResult<List<QuestLocationDto>>> GetAll(Guid questId)
    {
        var relationships = await _questLocationService.GetAllForQuestAsync(questId);
        return relationships is null ? NotFound() : Ok(relationships);
    }

    [HttpPost]
    public async Task<ActionResult<QuestLocationDto>> Create(Guid questId, [FromBody] QuestLocationCreateRequestDto request)
    {
        var result = await _questLocationService.CreateAsync(questId, request);

        return result.Outcome switch
        {
            CreateQuestLocationOutcome.Success => CreatedAtAction(
                nameof(QuestLocationRelationshipsController.GetById),
                "QuestLocationRelationships",
                new { id = result.Relationship!.Id },
                result.Relationship),
            CreateQuestLocationOutcome.Duplicate => Conflict(),
            _ => NotFound(),
        };
    }
}
