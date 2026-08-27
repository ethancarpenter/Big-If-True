using CampaignApp.Application.DTOs;
using CampaignApp.Application.Interfaces;
using CampaignApp.Domain.Entities;

namespace CampaignApp.Application.Services;

public class QuestObjectiveService : IQuestObjectiveService
{
    private readonly IQuestObjectiveRepository _objectiveRepository;
    private readonly IQuestRepository _questRepository;
    private readonly ICurrentUserProvider _currentUserProvider;

    public QuestObjectiveService(
        IQuestObjectiveRepository objectiveRepository,
        IQuestRepository questRepository,
        ICurrentUserProvider currentUserProvider)
    {
        _objectiveRepository = objectiveRepository;
        _questRepository = questRepository;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<List<QuestObjectiveDto>?> GetAllForQuestAsync(Guid questId)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var quest = await _questRepository.GetByIdAsync(questId);
        if (quest is null || quest.Campaign.UserId != userId)
        {
            return null;
        }

        var objectives = await _objectiveRepository.GetAllForQuestAsync(questId);
        return objectives.OrderBy(o => o.SortOrder).Select(ToDto).ToList();
    }

    public async Task<QuestObjectiveDto?> CreateAsync(Guid questId, QuestObjectiveCreateRequestDto request)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var quest = await _questRepository.GetByIdAsync(questId);
        if (quest is null || quest.Campaign.UserId != userId)
        {
            return null;
        }

        var existing = await _objectiveRepository.GetAllForQuestAsync(questId);
        var nextSortOrder = existing.Count == 0 ? 0 : existing.Max(o => o.SortOrder) + 1;

        var now = DateTime.UtcNow;
        var objective = new QuestObjective
        {
            Id = Guid.NewGuid(),
            QuestId = questId,
            Description = request.Description,
            IsCompleted = false,
            SortOrder = nextSortOrder,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _objectiveRepository.AddAsync(objective);
        await _objectiveRepository.SaveChangesAsync();

        return ToDto(objective);
    }

    public async Task<QuestObjectiveDto?> UpdateAsync(Guid id, QuestObjectiveUpdateRequestDto request)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var objective = await _objectiveRepository.GetByIdAsync(id);
        if (!IsOwnedByUser(objective, userId))
        {
            return null;
        }

        objective!.Description = request.Description;
        objective.IsCompleted = request.IsCompleted;
        objective.UpdatedAt = DateTime.UtcNow;

        _objectiveRepository.Update(objective);
        await _objectiveRepository.SaveChangesAsync();

        return ToDto(objective);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var objective = await _objectiveRepository.GetByIdAsync(id);
        if (!IsOwnedByUser(objective, userId))
        {
            return false;
        }

        var questId = objective!.QuestId;
        _objectiveRepository.Remove(objective);
        await _objectiveRepository.SaveChangesAsync();

        // Compact the remaining objectives' SortOrder to stay contiguous
        // (0..N-1) so gaps never accumulate over repeated add/delete cycles.
        var remaining = (await _objectiveRepository.GetAllForQuestAsync(questId))
            .OrderBy(o => o.SortOrder)
            .ToList();
        for (var index = 0; index < remaining.Count; index++)
        {
            if (remaining[index].SortOrder != index)
            {
                remaining[index].SortOrder = index;
                _objectiveRepository.Update(remaining[index]);
            }
        }

        if (remaining.Count > 0)
        {
            await _objectiveRepository.SaveChangesAsync();
        }

        return true;
    }

    public async Task<List<QuestObjectiveDto>?> ReorderAsync(Guid questId, QuestObjectiveReorderRequestDto request)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var quest = await _questRepository.GetByIdAsync(questId);
        if (quest is null || quest.Campaign.UserId != userId)
        {
            return null;
        }

        var existing = await _objectiveRepository.GetAllForQuestAsync(questId);

        // The request must specify exactly the quest's current objective ids
        // (no more, no fewer) - anything else is treated as "not found",
        // same idiom used everywhere else for a mismatched foreign reference.
        var existingIds = existing.Select(o => o.Id).ToHashSet();
        var requestedIds = request.ObjectiveIds.ToHashSet();
        if (existingIds.Count != requestedIds.Count || !existingIds.SetEquals(requestedIds))
        {
            return null;
        }

        var byId = existing.ToDictionary(o => o.Id);
        for (var index = 0; index < request.ObjectiveIds.Count; index++)
        {
            var objective = byId[request.ObjectiveIds[index]];
            if (objective.SortOrder != index)
            {
                objective.SortOrder = index;
                _objectiveRepository.Update(objective);
            }
        }

        await _objectiveRepository.SaveChangesAsync();

        return existing.OrderBy(o => o.SortOrder).Select(ToDto).ToList();
    }

    private static bool IsOwnedByUser(QuestObjective? objective, Guid userId) =>
        objective is not null && objective.Quest.Campaign.UserId == userId;

    private static QuestObjectiveDto ToDto(QuestObjective objective) => new()
    {
        Id = objective.Id,
        QuestId = objective.QuestId,
        Description = objective.Description,
        IsCompleted = objective.IsCompleted,
        SortOrder = objective.SortOrder,
    };
}
