using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampaignApp.Api.Controllers;

[ApiController]
[Route("api/quests/{questId:guid}/objectives")]
public class QuestObjectivesController : ControllerBase
{
    private readonly IQuestObjectiveService _objectiveService;

    public QuestObjectivesController(IQuestObjectiveService objectiveService)
    {
        _objectiveService = objectiveService;
    }

    [HttpGet]
    public async Task<ActionResult<List<QuestObjectiveDto>>> GetAll(Guid questId)
    {
        var objectives = await _objectiveService.GetAllForQuestAsync(questId);
        return objectives is null ? NotFound() : Ok(objectives);
    }

    [HttpPost]
    public async Task<ActionResult<QuestObjectiveDto>> Create(Guid questId, [FromBody] QuestObjectiveCreateRequestDto request)
    {
        var objective = await _objectiveService.CreateAsync(questId, request);
        return objective is null ? NotFound() : StatusCode(StatusCodes.Status201Created, objective);
    }

    [HttpPut("{objectiveId:guid}")]
    public async Task<ActionResult<QuestObjectiveDto>> Update(Guid questId, Guid objectiveId, [FromBody] QuestObjectiveUpdateRequestDto request)
    {
        var objective = await _objectiveService.UpdateAsync(objectiveId, request);
        return objective is null ? NotFound() : Ok(objective);
    }

    [HttpDelete("{objectiveId:guid}")]
    public async Task<IActionResult> Delete(Guid questId, Guid objectiveId)
    {
        var deleted = await _objectiveService.DeleteAsync(objectiveId);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPut("reorder")]
    public async Task<ActionResult<List<QuestObjectiveDto>>> Reorder(Guid questId, [FromBody] QuestObjectiveReorderRequestDto request)
    {
        var objectives = await _objectiveService.ReorderAsync(questId, request);
        return objectives is null ? NotFound() : Ok(objectives);
    }
}
