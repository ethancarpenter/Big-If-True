using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampaignApp.Api.Controllers;

[ApiController]
[Route("api/campaigns/{campaignId:guid}/cities")]
public class CampaignCitiesController : ControllerBase
{
    private readonly ICityService _cityService;

    public CampaignCitiesController(ICityService cityService)
    {
        _cityService = cityService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CityDto>>> GetAll(Guid campaignId)
    {
        var cities = await _cityService.GetAllForCampaignAsync(campaignId);
        return cities is null ? NotFound() : Ok(cities);
    }

    [HttpPost]
    public async Task<ActionResult<CityDto>> Create(Guid campaignId, [FromBody] CityRequestDto request)
    {
        var city = await _cityService.CreateAsync(campaignId, request);
        if (city is null)
        {
            return NotFound();
        }

        return CreatedAtAction(
            nameof(CitiesController.GetById),
            "Cities",
            new { id = city.Id },
            city);
    }
}
