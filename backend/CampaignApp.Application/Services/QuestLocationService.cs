using CampaignApp.Application.DTOs;
using CampaignApp.Application.Interfaces;
using CampaignApp.Domain.Entities;

namespace CampaignApp.Application.Services;

public class QuestLocationService : IQuestLocationService
{
    private readonly IQuestLocationRepository _questLocationRepository;
    private readonly IQuestRepository _questRepository;
    private readonly ILocationRepository _locationRepository;
    private readonly ICurrentUserProvider _currentUserProvider;

    public QuestLocationService(
        IQuestLocationRepository questLocationRepository,
        IQuestRepository questRepository,
        ILocationRepository locationRepository,
        ICurrentUserProvider currentUserProvider)
    {
        _questLocationRepository = questLocationRepository;
        _questRepository = questRepository;
        _locationRepository = locationRepository;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<List<QuestLocationDto>?> GetAllForQuestAsync(Guid questId)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var quest = await _questRepository.GetByIdAsync(questId);
        if (quest is null || quest.Campaign.UserId != userId)
        {
            return null;
        }

        var relationships = await _questLocationRepository.GetAllForQuestAsync(questId);
        return relationships.Select(ToDto).ToList();
    }

    public async Task<List<QuestLocationDto>?> GetAllForLocationAsync(Guid locationId)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var location = await _locationRepository.GetByIdAsync(locationId);
        if (location is null || location.Campaign.UserId != userId)
        {
            return null;
        }

        var relationships = await _questLocationRepository.GetAllForLocationAsync(locationId);
        return relationships.Select(ToDto).ToList();
    }

    public async Task<QuestLocationDto?> GetByIdAsync(Guid id)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var questLocation = await _questLocationRepository.GetByIdAsync(id);
        return IsOwnedByUser(questLocation, userId) ? ToDto(questLocation!) : null;
    }

    public async Task<CreateQuestLocationResult> CreateAsync(Guid questId, QuestLocationCreateRequestDto request)
    {
        var userId = _currentUserProvider.GetCurrentUserId();

        var quest = await _questRepository.GetByIdAsync(questId);
        if (quest is null || quest.Campaign.UserId != userId)
        {
            return new CreateQuestLocationResult(CreateQuestLocationOutcome.NotFound, null);
        }

        var location = await _locationRepository.GetByIdAsync(request.LocationId);
        if (location is null || location.CampaignId != quest.CampaignId)
        {
            // The location doesn't exist, or belongs to a different campaign
            // than the quest - treated as "not found" rather than confirming
            // some other campaign's location exists. Ownership of the
            // location is transitively confirmed here too: it's already
            // known to share a CampaignId with a quest we just verified the
            // current user owns.
            return new CreateQuestLocationResult(CreateQuestLocationOutcome.NotFound, null);
        }

        var existing = await _questLocationRepository.GetExistingAsync(questId, request.LocationId);
        if (existing is not null)
        {
            return new CreateQuestLocationResult(CreateQuestLocationOutcome.Duplicate, null);
        }

        var now = DateTime.UtcNow;
        var questLocation = new QuestLocation
        {
            Id = Guid.NewGuid(),
            QuestId = questId,
            Quest = quest,
            LocationId = request.LocationId,
            Location = location,
            Role = request.Role!.Value,
            Notes = request.Notes,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _questLocationRepository.AddAsync(questLocation);
        await _questLocationRepository.SaveChangesAsync();

        return new CreateQuestLocationResult(CreateQuestLocationOutcome.Success, ToDto(questLocation));
    }

    public async Task<QuestLocationDto?> UpdateAsync(Guid id, QuestLocationUpdateRequestDto request)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var questLocation = await _questLocationRepository.GetByIdAsync(id);
        if (!IsOwnedByUser(questLocation, userId))
        {
            return null;
        }

        questLocation!.Role = request.Role!.Value;
        questLocation.Notes = request.Notes;
        questLocation.UpdatedAt = DateTime.UtcNow;

        _questLocationRepository.Update(questLocation);
        await _questLocationRepository.SaveChangesAsync();

        return ToDto(questLocation);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var questLocation = await _questLocationRepository.GetByIdAsync(id);
        if (!IsOwnedByUser(questLocation, userId))
        {
            return false;
        }

        _questLocationRepository.Remove(questLocation!);
        await _questLocationRepository.SaveChangesAsync();
        return true;
    }

    private static bool IsOwnedByUser(QuestLocation? questLocation, Guid userId) =>
        questLocation is not null && questLocation.Quest.Campaign.UserId == userId;

    private static QuestLocationDto ToDto(QuestLocation questLocation) => new()
    {
        Id = questLocation.Id,
        QuestId = questLocation.QuestId,
        QuestName = questLocation.Quest.Name,
        LocationId = questLocation.LocationId,
        LocationName = questLocation.Location.Name,
        CityName = questLocation.Location.City.Name,
        Role = questLocation.Role,
        Notes = questLocation.Notes,
        CreatedAt = questLocation.CreatedAt,
    };
}
