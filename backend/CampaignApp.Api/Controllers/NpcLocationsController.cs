using CampaignApp.Application.DTOs;
using CampaignApp.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace CampaignApp.Api.Controllers;

[ApiController]
[Route("api/npcs/{npcId:guid}/locations")]
public class NpcLocationsController : ControllerBase
{
    private readonly INpcLocationService _npcLocationService;

    public NpcLocationsController(INpcLocationService npcLocationService)
    {
        _npcLocationService = npcLocationService;
    }

    [HttpGet]
    public async Task<ActionResult<List<NpcLocationDto>>> GetAll(Guid npcId)
    {
        var relationships = await _npcLocationService.GetAllForNpcAsync(npcId);
        return relationships is null ? NotFound() : Ok(relationships);
    }

    [HttpPost]
    public async Task<ActionResult<NpcLocationDto>> Create(Guid npcId, [FromBody] NpcLocationCreateRequestDto request)
    {
        var result = await _npcLocationService.CreateAsync(npcId, request);

        return result.Outcome switch
        {
            CreateNpcLocationOutcome.Success => CreatedAtAction(
                nameof(NpcLocationRelationshipsController.GetById),
                "NpcLocationRelationships",
                new { id = result.Relationship!.Id },
                result.Relationship),
            CreateNpcLocationOutcome.Duplicate => Conflict(),
            _ => NotFound(),
        };
    }
}
