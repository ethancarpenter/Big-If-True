using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampaignApp.Api.Controllers;

[ApiController]
[Route("api/cities")]
public class CitiesController : ControllerBase
{
    private readonly ICityService _cityService;

    public CitiesController(ICityService cityService)
    {
        _cityService = cityService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CityDto>> GetById(Guid id)
    {
        var city = await _cityService.GetByIdAsync(id);
        return city is null ? NotFound() : Ok(city);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CityDto>> Update(Guid id, [FromBody] CityRequestDto request)
    {
        var city = await _cityService.UpdateAsync(id, request);
        return city is null ? NotFound() : Ok(city);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _cityService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
