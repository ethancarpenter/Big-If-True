using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampaignApp.Api.Controllers;

[ApiController]
[Route("api/campaigns")]
public class CampaignsController : ControllerBase
{
    private readonly ICampaignService _campaignService;

    public CampaignsController(ICampaignService campaignService)
    {
        _campaignService = campaignService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CampaignDto>>> GetAll()
    {
        var campaigns = await _campaignService.GetAllAsync();
        return Ok(campaigns);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CampaignDto>> GetById(Guid id)
    {
        var campaign = await _campaignService.GetByIdAsync(id);
        return campaign is null ? NotFound() : Ok(campaign);
    }

    [HttpPost]
    public async Task<ActionResult<CampaignDto>> Create([FromBody] CampaignRequestDto request)
    {
        var campaign = await _campaignService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = campaign.Id }, campaign);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CampaignDto>> Update(Guid id, [FromBody] CampaignRequestDto request)
    {
        var campaign = await _campaignService.UpdateAsync(id, request);
        return campaign is null ? NotFound() : Ok(campaign);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _campaignService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
