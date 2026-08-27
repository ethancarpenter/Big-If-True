using CampaignApp.Domain.Entities;

namespace CampaignApp.Application.Interfaces;

public interface IQuestGraphPositionRepository
{
    Task<List<QuestGraphPosition>> GetAllForCampaignAsync(Guid campaignId);
    Task<QuestGraphPosition?> GetByQuestIdAsync(Guid questId);
    Task AddAsync(QuestGraphPosition position);
    void Update(QuestGraphPosition position);
    Task SaveChangesAsync();
}
