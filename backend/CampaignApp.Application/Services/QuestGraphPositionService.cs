using CampaignApp.Application.DTOs;
using CampaignApp.Application.Interfaces;
using CampaignApp.Domain.Entities;

namespace CampaignApp.Application.Services;

public class QuestGraphPositionService : IQuestGraphPositionService
{
    private readonly IQuestGraphPositionRepository _positionRepository;
    private readonly IQuestRepository _questRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly ICurrentUserProvider _currentUserProvider;

    public QuestGraphPositionService(
        IQuestGraphPositionRepository positionRepository,
        IQuestRepository questRepository,
        ICampaignRepository campaignRepository,
        ICurrentUserProvider currentUserProvider)
    {
        _positionRepository = positionRepository;
        _questRepository = questRepository;
        _campaignRepository = campaignRepository;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<List<QuestGraphPositionDto>?> GetAllForCampaignAsync(Guid campaignId)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var campaign = await _campaignRepository.GetByIdForUserAsync(campaignId, userId);
        if (campaign is null)
        {
            return null;
        }

        var positions = await _positionRepository.GetAllForCampaignAsync(campaignId);
        return positions.Select(ToDto).ToList();
    }

    public async Task<QuestGraphPositionDto?> UpsertAsync(Guid questId, QuestGraphPositionUpdateRequestDto request)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var quest = await _questRepository.GetByIdAsync(questId);
        if (quest is null || quest.Campaign.UserId != userId)
        {
            return null;
        }

        var existing = await _positionRepository.GetByQuestIdAsync(questId);
        var now = DateTime.UtcNow;

        if (existing is null)
        {
            var position = new QuestGraphPosition
            {
                Id = Guid.NewGuid(),
                QuestId = questId,
                X = request.X!.Value,
                Y = request.Y!.Value,
                CreatedAt = now,
                UpdatedAt = now,
            };
            await _positionRepository.AddAsync(position);
            await _positionRepository.SaveChangesAsync();
            return ToDto(position);
        }

        existing.X = request.X!.Value;
        existing.Y = request.Y!.Value;
        existing.UpdatedAt = now;
        _positionRepository.Update(existing);
        await _positionRepository.SaveChangesAsync();
        return ToDto(existing);
    }

    private static QuestGraphPositionDto ToDto(QuestGraphPosition position) => new()
    {
        QuestId = position.QuestId,
        X = position.X,
        Y = position.Y,
    };
}
