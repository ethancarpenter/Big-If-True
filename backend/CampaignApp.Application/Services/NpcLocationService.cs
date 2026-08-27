using CampaignApp.Application.DTOs;
using CampaignApp.Application.Interfaces;
using CampaignApp.Domain.Entities;

namespace CampaignApp.Application.Services;

public class NpcLocationService : INpcLocationService
{
    private readonly INpcLocationRepository _npcLocationRepository;
    private readonly INpcRepository _npcRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly ICurrentUserProvider _currentUserProvider;

    public NpcLocationService(
        INpcLocationRepository npcLocationRepository,
        INpcRepository npcRepository,
        ILocationRepository locationRepository,
        ICurrentUserProvider currentUserProvider)
    {
        _npcLocationRepository = npcLocationRepository;
        _npcRepository = npcRepository;
        _locationRepository = locationRepository;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<List<NpcLocationDto>?> GetAllForNpcAsync(Guid npcId)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var npc = await _npcRepository.GetByIdAsync(npcId);
        if (npc is null || npc.Campaign.UserId != userId)
        {
            return null;
        }

        var relationships = await _npcLocationRepository.GetAllForNpcAsync(npcId);
        return relationships.Select(ToDto).ToList();
    }

    public async Task<List<NpcLocationDto>?> GetAllForLocationAsync(Guid locationId)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var location = await _locationRepository.GetByIdAsync(locationId);
        if (location is null || location.Campaign.UserId != userId)
        {
            return null;
        }

        var relationships = await _npcLocationRepository.GetAllForLocationAsync(locationId);
        return relationships.Select(ToDto).ToList();
    }

    public async Task<NpcLocationDto?> GetByIdAsync(Guid id)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var npcLocation = await _npcLocationRepository.GetByIdAsync(id);
        return IsOwnedByUser(npcLocation, userId) ? ToDto(npcLocation!) : null;
    }

    public async Task<CreateNpcLocationResult> CreateAsync(Guid npcId, NpcLocationCreateRequestDto request)
    {
        var userId = _currentUserProvider.GetCurrentUserId();

        var npc = await _npcRepository.GetByIdAsync(npcId);
        if (npc is null || npc.Campaign.UserId != userId)
        {
            return new CreateNpcLocationResult(CreateNpcLocationOutcome.NotFound, null);
        }

        var location = await _locationRepository.GetByIdAsync(request.LocationId);
        if (location is null || location.CampaignId != npc.CampaignId)
        {
            // The location doesn't exist, or belongs to a different campaign
            // than the NPC - treated as "not found" rather than confirming
            // some other campaign's location exists. Ownership of the
            // location is transitively confirmed here too: it's already
            // known to share a CampaignId with an NPC we just verified the
            // current user owns.
            return new CreateNpcLocationResult(CreateNpcLocationOutcome.NotFound, null);
        }

        var existing = await _npcLocationRepository.GetExistingAsync(npcId, request.LocationId);
        if (existing is not null)
        {
            return new CreateNpcLocationResult(CreateNpcLocationOutcome.Duplicate, null);
        }

        var now = DateTime.UtcNow;
        var npcLocation = new NpcLocation
        {
            Id = Guid.NewGuid(),
            NpcId = npcId,
            Npc = npc,
            LocationId = request.LocationId,
            Location = location,
            RelationshipType = request.RelationshipType!.Value,
            IsPrimary = false, // set correctly below, either directly or via the primary switch
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _npcLocationRepository.AddAsync(npcLocation);

        if (request.IsPrimary)
        {
            var currentPrimary = await _npcLocationRepository.GetPrimaryForNpcAsync(npcId);
            await _npcLocationRepository.SavePrimarySwitchAsync(currentPrimary, npcLocation);
        }
        else
        {
            await _npcLocationRepository.SaveChangesAsync();
        }

        return new CreateNpcLocationResult(CreateNpcLocationOutcome.Success, ToDto(npcLocation));
    }

    public async Task<NpcLocationDto?> UpdateAsync(Guid id, NpcLocationUpdateRequestDto request)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var npcLocation = await _npcLocationRepository.GetByIdAsync(id);
        if (!IsOwnedByUser(npcLocation, userId))
        {
            return null;
        }

        npcLocation!.RelationshipType = request.RelationshipType!.Value;
        npcLocation.UpdatedAt = DateTime.UtcNow;
        _npcLocationRepository.Update(npcLocation);

        if (request.IsPrimary && !npcLocation.IsPrimary)
        {
            var currentPrimary = await _npcLocationRepository.GetPrimaryForNpcAsync(npcLocation.NpcId);
            await _npcLocationRepository.SavePrimarySwitchAsync(currentPrimary, npcLocation);
        }
        else if (!request.IsPrimary && npcLocation.IsPrimary)
        {
            // Only ever removes a `true` - never risks a second primary, so
            // no ordering/transaction concern here.
            npcLocation.IsPrimary = false;
            await _npcLocationRepository.SaveChangesAsync();
        }
        else
        {
            // IsPrimary unchanged either way; just persist RelationshipType.
            await _npcLocationRepository.SaveChangesAsync();
        }

        return ToDto(npcLocation);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var npcLocation = await _npcLocationRepository.GetByIdAsync(id);
        if (!IsOwnedByUser(npcLocation, userId))
        {
            return false;
        }

        _npcLocationRepository.Remove(npcLocation!);
        await _npcLocationRepository.SaveChangesAsync();
        return true;
    }

    private static bool IsOwnedByUser(NpcLocation? npcLocation, Guid userId) =>
        npcLocation is not null && npcLocation.Npc.Campaign.UserId == userId;

    private static NpcLocationDto ToDto(NpcLocation npcLocation) => new()
    {
        Id = npcLocation.Id,
        NpcId = npcLocation.NpcId,
        NpcName = npcLocation.Npc.Name,
        LocationId = npcLocation.LocationId,
        LocationName = npcLocation.Location.Name,
        CityName = npcLocation.Location.City.Name,
        RelationshipType = npcLocation.RelationshipType,
        IsPrimary = npcLocation.IsPrimary,
        CreatedAt = npcLocation.CreatedAt,
    };
}
