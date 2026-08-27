using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampaignApp.Api.Controllers;

[ApiController]
[Route("api/npcs")]
public class NpcsController : ControllerBase
{
    private readonly INpcService _npcService;

    public NpcsController(INpcService npcService)
    {
        _npcService = npcService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<NpcDto>> GetById(Guid id)
    {
        var npc = await _npcService.GetByIdAsync(id);
        return npc is null ? NotFound() : Ok(npc);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<NpcDto>> Update(Guid id, [FromBody] NpcRequestDto request)
    {
        var npc = await _npcService.UpdateAsync(id, request);
        return npc is null ? NotFound() : Ok(npc);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _npcService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
