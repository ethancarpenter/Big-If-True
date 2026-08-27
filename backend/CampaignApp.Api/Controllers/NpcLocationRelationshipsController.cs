using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampaignApp.Api.Controllers;

[ApiController]
[Route("api/npc-locations")]
public class NpcLocationRelationshipsController : ControllerBase
{
    private readonly INpcLocationService _npcLocationService;

    public NpcLocationRelationshipsController(INpcLocationService npcLocationService)
    {
        _npcLocationService = npcLocationService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NpcLocationDto>> GetById(Guid id)
    {
        var relationship = await _npcLocationService.GetByIdAsync(id);
        return relationship is null ? NotFound() : Ok(relationship);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<NpcLocationDto>> Update(Guid id, [FromBody] NpcLocationUpdateRequestDto request)
    {
        var relationship = await _npcLocationService.UpdateAsync(id, request);
        return relationship is null ? NotFound() : Ok(relationship);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _npcLocationService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
