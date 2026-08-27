using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampaignApp.Api.Controllers;

[ApiController]
[Route("api/campaigns/{campaignId:guid}/locations")]
public class CampaignLocationsController : ControllerBase
{
    private readonly ILocationService _locationService;

    public CampaignLocationsController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [HttpGet]
    public async Task<ActionResult<List<LocationDto>>> GetAll(Guid campaignId, [FromQuery] Guid? cityId)
    {
        var locations = await _locationService.GetAllForCampaignAsync(campaignId, cityId);
        return locations is null ? NotFound() : Ok(locations);
    }

    [HttpPost]
    public async Task<ActionResult<LocationDto>> Create(Guid campaignId, [FromBody] LocationRequestDto request)
    {
        var location = await _locationService.CreateAsync(campaignId, request);
        if (location is null)
        {
            return NotFound();
        }

        return CreatedAtAction(
            nameof(LocationsController.GetById),
            "Locations",
            new { id = location.Id },
            location);
    }
}
