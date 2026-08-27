using CampaignApp.Application.DTOs;
using CampaignApp.Application.Interfaces;
using CampaignApp.Domain.Entities;

namespace CampaignApp.Application.Services;

public class QuestService : IQuestService
{
    private readonly IQuestRepository _questRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly ICurrentUserProvider _currentUserProvider;

    public QuestService(
        IQuestRepository questRepository,
        ICampaignRepository campaignRepository,
        ICurrentUserProvider currentUserProvider)
    {
        _questRepository = questRepository;
        _campaignRepository = campaignRepository;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<List<QuestDto>?> GetAllForCampaignAsync(Guid campaignId)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var campaign = await _campaignRepository.GetByIdForUserAsync(campaignId, userId);
        if (campaign is null)
        {
            return null;
        }

        var quests = await _questRepository.GetAllForCampaignAsync(campaignId);
        return quests.Select(ToDto).ToList();
    }

    public async Task<QuestDto?> GetByIdAsync(Guid id)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var quest = await _questRepository.GetByIdAsync(id);
        return IsOwnedByUser(quest, userId) ? ToDto(quest!) : null;
    }

    public async Task<QuestDto?> CreateAsync(Guid campaignId, QuestRequestDto request)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var campaign = await _campaignRepository.GetByIdForUserAsync(campaignId, userId);
        if (campaign is null)
        {
            return null;
        }

        var now = DateTime.UtcNow;
        var quest = new Quest
        {
            Id = Guid.NewGuid(),
            CampaignId = campaignId,
            Name = request.Name,
            Description = request.Description,
            Status = request.Status!.Value,
            QuestType = request.QuestType!.Value,
            RecommendedLevelMin = request.RecommendedLevelMin,
            RecommendedLevelMax = request.RecommendedLevelMax,
            DmNotes = request.DmNotes,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _questRepository.AddAsync(quest);
        await _questRepository.SaveChangesAsync();

        return ToDto(quest);
    }

    public async Task<QuestDto?> UpdateAsync(Guid id, QuestRequestDto request)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var quest = await _questRepository.GetByIdAsync(id);
        if (!IsOwnedByUser(quest, userId))
        {
            return null;
        }

        quest!.Name = request.Name;
        quest.Description = request.Description;
        quest.Status = request.Status!.Value;
        quest.QuestType = request.QuestType!.Value;
        quest.RecommendedLevelMin = request.RecommendedLevelMin;
        quest.RecommendedLevelMax = request.RecommendedLevelMax;
        quest.DmNotes = request.DmNotes;
        quest.UpdatedAt = DateTime.UtcNow;

        _questRepository.Update(quest);
        await _questRepository.SaveChangesAsync();

        return ToDto(quest);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var quest = await _questRepository.GetByIdAsync(id);
        if (!IsOwnedByUser(quest, userId))
        {
            return false;
        }

        _questRepository.Remove(quest!);
        await _questRepository.SaveChangesAsync();
        return true;
    }

    private static bool IsOwnedByUser(Quest? quest, Guid userId) =>
        quest is not null && quest.Campaign.UserId == userId;

    private static QuestDto ToDto(Quest quest) => new()
    {
        Id = quest.Id,
        CampaignId = quest.CampaignId,
        Name = quest.Name,
        Description = quest.Description,
        Status = quest.Status,
        QuestType = quest.QuestType,
        RecommendedLevelMin = quest.RecommendedLevelMin,
        RecommendedLevelMax = quest.RecommendedLevelMax,
        DmNotes = quest.DmNotes,
        Objectives = quest.Objectives
            .OrderBy(o => o.SortOrder)
            .Select(o => new QuestObjectiveDto
            {
                Id = o.Id,
                QuestId = o.QuestId,
                Description = o.Description,
                IsCompleted = o.IsCompleted,
                SortOrder = o.SortOrder,
            })
            .ToList(),
        CreatedAt = quest.CreatedAt,
        UpdatedAt = quest.UpdatedAt,
    };
}
