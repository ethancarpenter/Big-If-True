using CampaignApp.Application.DTOs;
using CampaignApp.Application.Interfaces;
using CampaignApp.Domain.Entities;

namespace CampaignApp.Application.Services;

public class QuestNpcService : IQuestNpcService
{
    private readonly IQuestNpcRepository _questNpcRepository;
    private readonly IQuestRepository _questRepository;
    private readonly INpcRepository _npcRepository;
    private readonly ICurrentUserProvider _currentUserProvider;

    public QuestNpcService(
        IQuestNpcRepository questNpcRepository,
        IQuestRepository questRepository,
        INpcRepository npcRepository,
        ICurrentUserProvider currentUserProvider)
    {
        _questNpcRepository = questNpcRepository;
        _questRepository = questRepository;
        _npcRepository = npcRepository;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<List<QuestNpcDto>?> GetAllForQuestAsync(Guid questId)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var quest = await _questRepository.GetByIdAsync(questId);
        if (quest is null || quest.Campaign.UserId != userId)
        {
            return null;
        }

        var relationships = await _questNpcRepository.GetAllForQuestAsync(questId);
        return relationships.Select(ToDto).ToList();
    }

    public async Task<List<QuestNpcDto>?> GetAllForNpcAsync(Guid npcId)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var npc = await _npcRepository.GetByIdAsync(npcId);
        if (npc is null || npc.Campaign.UserId != userId)
        {
            return null;
        }

        var relationships = await _questNpcRepository.GetAllForNpcAsync(npcId);
        return relationships.Select(ToDto).ToList();
    }

    public async Task<QuestNpcDto?> GetByIdAsync(Guid id)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var questNpc = await _questNpcRepository.GetByIdAsync(id);
        return IsOwnedByUser(questNpc, userId) ? ToDto(questNpc!) : null;
    }

    public async Task<CreateQuestNpcResult> CreateAsync(Guid questId, QuestNpcCreateRequestDto request)
    {
        var userId = _currentUserProvider.GetCurrentUserId();

        var quest = await _questRepository.GetByIdAsync(questId);
        if (quest is null || quest.Campaign.UserId != userId)
        {
            return new CreateQuestNpcResult(CreateQuestNpcOutcome.NotFound, null);
        }

        var npc = await _npcRepository.GetByIdAsync(request.NpcId);
        if (npc is null || npc.CampaignId != quest.CampaignId)
        {
            // The NPC doesn't exist, or belongs to a different campaign than
            // the quest - treated as "not found" rather than confirming some
            // other campaign's NPC exists. Ownership of the NPC is
            // transitively confirmed here too: it's already known to share a
            // CampaignId with a quest we just verified the current user owns.
            return new CreateQuestNpcResult(CreateQuestNpcOutcome.NotFound, null);
        }

        var existing = await _questNpcRepository.GetExistingAsync(questId, request.NpcId);
        if (existing is not null)
        {
            return new CreateQuestNpcResult(CreateQuestNpcOutcome.Duplicate, null);
        }

        var now = DateTime.UtcNow;
        var questNpc = new QuestNpc
        {
            Id = Guid.NewGuid(),
            QuestId = questId,
            Quest = quest,
            NpcId = request.NpcId,
            Npc = npc,
            Role = request.Role!.Value,
            Notes = request.Notes,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _questNpcRepository.AddAsync(questNpc);
        await _questNpcRepository.SaveChangesAsync();

        return new CreateQuestNpcResult(CreateQuestNpcOutcome.Success, ToDto(questNpc));
    }

    public async Task<QuestNpcDto?> UpdateAsync(Guid id, QuestNpcUpdateRequestDto request)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var questNpc = await _questNpcRepository.GetByIdAsync(id);
        if (!IsOwnedByUser(questNpc, userId))
        {
            return null;
        }

        questNpc!.Role = request.Role!.Value;
        questNpc.Notes = request.Notes;
        questNpc.UpdatedAt = DateTime.UtcNow;

        _questNpcRepository.Update(questNpc);
        await _questNpcRepository.SaveChangesAsync();

        return ToDto(questNpc);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var questNpc = await _questNpcRepository.GetByIdAsync(id);
        if (!IsOwnedByUser(questNpc, userId))
        {
            return false;
        }

        _questNpcRepository.Remove(questNpc!);
        await _questNpcRepository.SaveChangesAsync();
        return true;
    }

    private static bool IsOwnedByUser(QuestNpc? questNpc, Guid userId) =>
        questNpc is not null && questNpc.Quest.Campaign.UserId == userId;

    private static QuestNpcDto ToDto(QuestNpc questNpc) => new()
    {
        Id = questNpc.Id,
        QuestId = questNpc.QuestId,
        QuestName = questNpc.Quest.Name,
        NpcId = questNpc.NpcId,
        NpcName = questNpc.Npc.Name,
        Role = questNpc.Role,
        Notes = questNpc.Notes,
        CreatedAt = questNpc.CreatedAt,
    };
}
