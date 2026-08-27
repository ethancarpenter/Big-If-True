using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampaignApp.Api.Controllers;

[ApiController]
[Route("api/quests")]
public class QuestsController : ControllerBase
{
    private readonly IQuestService _questService;

    public QuestsController(IQuestService questService)
    {
        _questService = questService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<QuestDto>> GetById(Guid id)
    {
        var quest = await _questService.GetByIdAsync(id);
        return quest is null ? NotFound() : Ok(quest);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<QuestDto>> Update(Guid id, [FromBody] QuestRequestDto request)
    {
        var quest = await _questService.UpdateAsync(id, request);
        return quest is null ? NotFound() : Ok(quest);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _questService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
