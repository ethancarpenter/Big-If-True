using CampaignApp.Application.DTOs;
using CampaignApp.Application.Interfaces;
using CampaignApp.Domain.Entities;
using CampaignApp.Domain.Enums;

namespace CampaignApp.Application.Services;

public class QuestConnectionService : IQuestConnectionService
{
    private readonly IQuestConnectionRepository _questConnectionRepository;
    private readonly IQuestRepository _questRepository;
    private readonly ICampaignRepository _campaignRepository;
    private readonly ICurrentUserProvider _currentUserProvider;

    public QuestConnectionService(
        IQuestConnectionRepository questConnectionRepository,
        IQuestRepository questRepository,
        ICampaignRepository campaignRepository,
        ICurrentUserProvider currentUserProvider)
    {
        _questConnectionRepository = questConnectionRepository;
        _questRepository = questRepository;
        _campaignRepository = campaignRepository;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<List<QuestConnectionDto>?> GetAllForCampaignAsync(Guid campaignId)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var campaign = await _campaignRepository.GetByIdForUserAsync(campaignId, userId);
        if (campaign is null)
        {
            return null;
        }

        var connections = await _questConnectionRepository.GetAllForCampaignAsync(campaignId);
        return connections.Select(ToDto).ToList();
    }

    public async Task<QuestConnectionDto?> GetByIdAsync(Guid id)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var connection = await _questConnectionRepository.GetByIdAsync(id);
        return IsOwnedByUser(connection, userId) ? ToDto(connection!) : null;
    }

    public async Task<CreateQuestConnectionResult> CreateAsync(Guid campaignId, QuestConnectionCreateRequestDto request)
    {
        var userId = _currentUserProvider.GetCurrentUserId();

        var campaign = await _campaignRepository.GetByIdForUserAsync(campaignId, userId);
        if (campaign is null)
        {
            return new CreateQuestConnectionResult(CreateQuestConnectionOutcome.NotFound, null);
        }

        var sourceQuest = await _questRepository.GetByIdAsync(request.SourceQuestId);
        if (sourceQuest is null || sourceQuest.CampaignId != campaignId)
        {
            return new CreateQuestConnectionResult(CreateQuestConnectionOutcome.NotFound, null);
        }

        var targetQuest = await _questRepository.GetByIdAsync(request.TargetQuestId);
        if (targetQuest is null || targetQuest.CampaignId != campaignId)
        {
            return new CreateQuestConnectionResult(CreateQuestConnectionOutcome.NotFound, null);
        }

        var connectionType = request.ConnectionType!.Value;

        var existing = await _questConnectionRepository.GetExistingAsync(request.SourceQuestId, request.TargetQuestId);
        if (existing is not null)
        {
            return new CreateQuestConnectionResult(CreateQuestConnectionOutcome.Duplicate, null);
        }

        if (connectionType == QuestConnectionType.Related)
        {
            var reverse = await _questConnectionRepository.GetExistingAsync(request.TargetQuestId, request.SourceQuestId);
            if (reverse is not null && reverse.ConnectionType == QuestConnectionType.Related)
            {
                return new CreateQuestConnectionResult(CreateQuestConnectionOutcome.Duplicate, null);
            }
        }

        if (await WouldCreateCycleAsync(campaignId, request.SourceQuestId, request.TargetQuestId, connectionType, excludeConnectionId: null))
        {
            return new CreateQuestConnectionResult(CreateQuestConnectionOutcome.Cycle, null);
        }

        var now = DateTime.UtcNow;
        var connection = new QuestConnection
        {
            Id = Guid.NewGuid(),
            SourceQuestId = request.SourceQuestId,
            SourceQuest = sourceQuest,
            TargetQuestId = request.TargetQuestId,
            TargetQuest = targetQuest,
            ConnectionType = connectionType,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _questConnectionRepository.AddAsync(connection);
        await _questConnectionRepository.SaveChangesAsync();

        return new CreateQuestConnectionResult(CreateQuestConnectionOutcome.Success, ToDto(connection));
    }

    public async Task<UpdateQuestConnectionResult> UpdateAsync(Guid id, QuestConnectionUpdateRequestDto request)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var connection = await _questConnectionRepository.GetByIdAsync(id);
        if (!IsOwnedByUser(connection, userId))
        {
            return new UpdateQuestConnectionResult(UpdateQuestConnectionOutcome.NotFound, null);
        }

        var newType = request.ConnectionType!.Value;
        var campaignId = connection!.SourceQuest.CampaignId;

        if (await WouldCreateCycleAsync(campaignId, connection.SourceQuestId, connection.TargetQuestId, newType, excludeConnectionId: connection.Id))
        {
            return new UpdateQuestConnectionResult(UpdateQuestConnectionOutcome.Cycle, null);
        }

        connection.ConnectionType = newType;
        connection.UpdatedAt = DateTime.UtcNow;

        _questConnectionRepository.Update(connection);
        await _questConnectionRepository.SaveChangesAsync();

        return new UpdateQuestConnectionResult(UpdateQuestConnectionOutcome.Success, ToDto(connection));
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var userId = _currentUserProvider.GetCurrentUserId();
        var connection = await _questConnectionRepository.GetByIdAsync(id);
        if (!IsOwnedByUser(connection, userId))
        {
            return false;
        }

        _questConnectionRepository.Remove(connection!);
        await _questConnectionRepository.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Normalizes a connection to its progression direction: Unlocks and FailureLeadsTo
    /// point Source -> Target (Source happens first); Requires is Source's inverse, so it
    /// normalizes to Target -> Source (Target is the prerequisite). Optional, AlternativePath,
    /// and Related don't assert a hard ordering and are excluded (null).
    /// </summary>
    private static (Guid From, Guid To)? NormalizeProgressionEdge(Guid sourceQuestId, Guid targetQuestId, QuestConnectionType type) =>
        type switch
        {
            QuestConnectionType.Unlocks => (sourceQuestId, targetQuestId),
            QuestConnectionType.FailureLeadsTo => (sourceQuestId, targetQuestId),
            QuestConnectionType.Requires => (targetQuestId, sourceQuestId),
            _ => null,
        };

    /// <summary>
    /// Checks whether adding/retyping an edge to (sourceQuestId, targetQuestId, type) would close
    /// a cycle among the campaign's progression-type connections (Unlocks/Requires/FailureLeadsTo,
    /// each normalized to its true "happens before" direction). Non-progression types never
    /// participate and always return false. BFS from the candidate edge's normalized destination,
    /// looking for its normalized origin: O(V+E) over the campaign's quests/progression-edges.
    /// </summary>
    private async Task<bool> WouldCreateCycleAsync(
        Guid campaignId, Guid sourceQuestId, Guid targetQuestId, QuestConnectionType type, Guid? excludeConnectionId)
    {
        var candidate = NormalizeProgressionEdge(sourceQuestId, targetQuestId, type);
        if (candidate is null)
        {
            return false;
        }

        var (from, to) = candidate.Value;

        var existingConnections = await _questConnectionRepository.GetAllForCampaignAsync(campaignId);

        var adjacency = new Dictionary<Guid, List<Guid>>();
        foreach (var connection in existingConnections)
        {
            if (excludeConnectionId is not null && connection.Id == excludeConnectionId)
            {
                continue;
            }

            var edge = NormalizeProgressionEdge(connection.SourceQuestId, connection.TargetQuestId, connection.ConnectionType);
            if (edge is null)
            {
                continue;
            }

            if (!adjacency.TryGetValue(edge.Value.From, out var neighbors))
            {
                neighbors = [];
                adjacency[edge.Value.From] = neighbors;
            }

            neighbors.Add(edge.Value.To);
        }

        // Would adding `from -> to` close a cycle? Equivalent to: can `to` already reach `from`?
        var visited = new HashSet<Guid> { to };
        var queue = new Queue<Guid>();
        queue.Enqueue(to);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == from)
            {
                return true;
            }

            if (!adjacency.TryGetValue(current, out var neighbors))
            {
                continue;
            }

            foreach (var neighbor in neighbors)
            {
                if (visited.Add(neighbor))
                {
                    queue.Enqueue(neighbor);
                }
            }
        }

        return false;
    }

    private static bool IsOwnedByUser(QuestConnection? connection, Guid userId) =>
        connection is not null && connection.SourceQuest.Campaign.UserId == userId;

    private static QuestConnectionDto ToDto(QuestConnection connection) => new()
    {
        Id = connection.Id,
        SourceQuestId = connection.SourceQuestId,
        SourceQuestName = connection.SourceQuest.Name,
        TargetQuestId = connection.TargetQuestId,
        TargetQuestName = connection.TargetQuest.Name,
        ConnectionType = connection.ConnectionType,
        CreatedAt = connection.CreatedAt,
    };
}
