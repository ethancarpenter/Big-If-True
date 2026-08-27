using CampaignApp.Application.DTOs;
using CampaignApp.Application.Interfaces;
using CampaignApp.Domain.Entities;

namespace CampaignApp.Application.Services;

public class NpcService : INpcService
{
    private readonly INpcRepository _npcRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly ICurrentUserProvider _currentUserProvider;

    public NpcService(
        INpcRepository npcRepository,
        ICampaignRepository campaignRepository,
        ICurrentUserProvider currentUserProvider)
    {
        _npcRepository = npcRepository;
        _campaignRepository = campaignRepository;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<List<NpcDto>?> GetAllForCampaignAsync(Guid campaignId)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var campaign = await _campaignRepository.GetByIdForUserAsync(campaignId, userId);
        if (campaign is null)
        {
            return null;
        }

        var npcs = await _npcRepository.GetAllForCampaignAsync(campaignId);
        return npcs.Select(ToDto).ToList();
    }

    public async Task<NpcDto?> GetByIdAsync(Guid id)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var npc = await _npcRepository.GetByIdAsync(id);
        return IsOwnedByUser(npc, userId) ? ToDto(npc!) : null;
    }

    public async Task<NpcDto?> CreateAsync(Guid campaignId, NpcRequestDto request)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var campaign = await _campaignRepository.GetByIdForUserAsync(campaignId, userId);
        if (campaign is null)
        {
            return null;
        }

        var now = DateTime.UtcNow;
        var npc = new Npc
        {
            Id = Guid.NewGuid(),
            CampaignId = campaignId,
            Name = request.Name,
            Species = request.Species,
            Gender = request.Gender,
            Age = request.Age,
            Class = request.Class,
            Alignment = request.Alignment,
            Occupation = request.Occupation,
            Disposition = request.Disposition,
            Description = request.Description,
            DmNotes = request.DmNotes,
            Status = request.Status!.Value,
            PortraitUrl = request.PortraitUrl,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _npcRepository.AddAsync(npc);
        await _npcRepository.SaveChangesAsync();

        return ToDto(npc);
    }

    public async Task<NpcDto?> UpdateAsync(Guid id, NpcRequestDto request)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var npc = await _npcRepository.GetByIdAsync(id);
        if (!IsOwnedByUser(npc, userId))
        {
            return null;
        }

        npc!.Name = request.Name;
        npc.Species = request.Species;
        npc.Gender = request.Gender;
        npc.Age = request.Age;
        npc.Class = request.Class;
        npc.Alignment = request.Alignment;
        npc.Occupation = request.Occupation;
        npc.Disposition = request.Disposition;
        npc.Description = request.Description;
        npc.DmNotes = request.DmNotes;
        npc.Status = request.Status!.Value;
        npc.PortraitUrl = request.PortraitUrl;
        npc.UpdatedAt = DateTime.UtcNow;

        _npcRepository.Update(npc);
        await _npcRepository.SaveChangesAsync();

        return ToDto(npc);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var npc = await _npcRepository.GetByIdAsync(id);
        if (!IsOwnedByUser(npc, userId))
        {
            return false;
        }

        _npcRepository.Remove(npc!);
        await _npcRepository.SaveChangesAsync();
        return true;
    }

    private static bool IsOwnedByUser(Npc? npc, Guid userId) =>
        npc is not null && npc.Campaign.UserId == userId;

    private static NpcDto ToDto(Npc npc) => new()
    {
        Id = npc.Id,
        CampaignId = npc.CampaignId,
        Name = npc.Name,
        Species = npc.Species,
        Gender = npc.Gender,
        Age = npc.Age,
        Class = npc.Class,
        Alignment = npc.Alignment,
        Occupation = npc.Occupation,
        Disposition = npc.Disposition,
        Description = npc.Description,
        DmNotes = npc.DmNotes,
        Status = npc.Status,
        PortraitUrl = npc.PortraitUrl,
        CreatedAt = npc.CreatedAt,
        UpdatedAt = npc.UpdatedAt,
    };
}
