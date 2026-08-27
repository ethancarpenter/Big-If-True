using CampaignApp.Domain.Entities;

namespace CampaignApp.Application.Interfaces;

public interface IQuestConnectionRepository
{
    Task<List<QuestConnection>> GetAllForCampaignAsync(Guid campaignId);
    Task<QuestConnection?> GetByIdAsync(Guid id);
    Task<QuestConnection?> GetExistingAsync(Guid sourceQuestId, Guid targetQuestId);
    Task AddAsync(QuestConnection connection);
    void Update(QuestConnection connection);
    void Remove(QuestConnection connection);
    Task SaveChangesAsync();
}
