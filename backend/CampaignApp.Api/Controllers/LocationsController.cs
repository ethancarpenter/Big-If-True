using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampaignApp.Api.Controllers;

[ApiController]
[Route("api/locations")]
public class LocationsController : ControllerBase
{
    private readonly ILocationService _locationService;

    public LocationsController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LocationDto>> GetById(Guid id)
    {
        var location = await _locationService.GetByIdAsync(id);
        return location is null ? NotFound() : Ok(location);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<LocationDto>> Update(Guid id, [FromBody] LocationRequestDto request)
    {
        var location = await _locationService.UpdateAsync(id, request);
        return location is null ? NotFound() : Ok(location);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _locationService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
