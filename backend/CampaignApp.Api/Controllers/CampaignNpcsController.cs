using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampaignApp.Api.Controllers;

[ApiController]
[Route("api/campaigns/{campaignId:guid}/npcs")]
public class CampaignNpcsController : ControllerBase
{
    private readonly INpcService _npcService;

    public CampaignNpcsController(INpcService npcService)
    {
        _npcService = npcService;
    }

    [HttpGet]
    public async Task<ActionResult<List<NpcDto>>> GetAll(Guid campaignId)
    {
        var npcs = await _npcService.GetAllForCampaignAsync(campaignId);
        return npcs is null ? NotFound() : Ok(npcs);
    }

    [HttpPost]
    public async Task<ActionResult<NpcDto>> Create(Guid campaignId, [FromBody] NpcRequestDto request)
    {
        var npc = await _npcService.CreateAsync(campaignId, request);
        if (npc is null)
        {
            return NotFound();
        }

        return CreatedAtAction(
            nameof(NpcsController.GetById),
            "Npcs",
            new { id = npc.Id },
            npc);
    }
}
