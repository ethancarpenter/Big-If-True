using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampaignApp.Api.Controllers;

[ApiController]
[Route("api/locations/{locationId:guid}/npcs")]
public class LocationNpcsController : ControllerBase
{
    private readonly INpcLocationService _npcLocationService;

    public LocationNpcsController(INpcLocationService npcLocationService)
    {
        _npcLocationService = npcLocationService;
    }

    [HttpGet]
    public async Task<ActionResult<List<NpcLocationDto>>> GetAll(Guid locationId)
    {
        var relationships = await _npcLocationService.GetAllForLocationAsync(locationId);
        return relationships is null ? NotFound() : Ok(relationships);
    }
}
