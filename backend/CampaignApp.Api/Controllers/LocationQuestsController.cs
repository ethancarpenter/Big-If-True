using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampaignApp.Api.Controllers;

[ApiController]
[Route("api/locations/{locationId:guid}/quests")]
public class LocationQuestsController : ControllerBase
{
    private readonly IQuestLocationService _questLocationService;

    public LocationQuestsController(IQuestLocationService questLocationService)
    {
        _questLocationService = questLocationService;
    }

    [HttpGet]
    public async Task<ActionResult<List<QuestLocationDto>>> GetAll(Guid locationId)
    {
        var relationships = await _questLocationService.GetAllForLocationAsync(locationId);
        return relationships is null ? NotFound() : Ok(relationships);
    }
}
