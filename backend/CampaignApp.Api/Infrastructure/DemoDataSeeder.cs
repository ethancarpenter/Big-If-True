using CampaignApp.Domain.Entities;
using CampaignApp.Domain.Enums;
using CampaignApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CampaignApp.Api.Infrastructure;

/// <summary>
/// Development-only, idempotent seeding of a self-contained demo@local.test
/// account and campaign, separate from the hand-curated dev@local.test data
/// (see Program.cs) so a reviewer can explore a populated campaign without
/// depending on or cluttering that account. Called from the same
/// IsDevelopment() block that seeds the dev user - structurally unreachable
/// in production, exactly like that seed.
/// </summary>
public static class DemoDataSeeder
{
    private static readonly Guid DemoUserId = Guid.Parse("00000000-0000-0000-0000-000000000002");

    public static async Task SeedAsync(AppDbContext dbContext, IPasswordHasher<User> hasher)
    {
        var now = DateTime.UtcNow;

        var demoUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == DemoUserId);
        if (demoUser is null)
        {
            demoUser = new User
            {
                Id = DemoUserId,
                Email = "demo@local.test",
                NormalizedEmail = "DEMO@LOCAL.TEST",
                CreatedAt = now,
                UpdatedAt = now,
            };
            demoUser.PasswordHash = hasher.HashPassword(demoUser, "DemoPassword123!");
            dbContext.Users.Add(demoUser);
        }
        else
        {
            demoUser.PasswordHash = hasher.HashPassword(demoUser, "DemoPassword123!");
            demoUser.UpdatedAt = now;
        }

        // Idempotency check is on the campaign, not just the user - if the
        // demo user already exists but somehow has no campaign yet, this
        // still seeds one; if the campaign already exists, this does nothing
        // further, so restarting the API repeatedly never duplicates data.
        var existingCampaign = await dbContext.Campaigns
            .FirstOrDefaultAsync(c => c.UserId == DemoUserId);
        if (existingCampaign is not null)
        {
            await dbContext.SaveChangesAsync();
            return;
        }

        var campaign = new Campaign
        {
            Id = Guid.NewGuid(),
            UserId = DemoUserId,
            Name = "The Sunken Lantern",
            Description = "A coastal trading city hides a smuggling ring beneath its docks, and a missing lighthouse keeper may be the key to unraveling it.",
            CreatedAt = now,
            UpdatedAt = now,
        };
        dbContext.Campaigns.Add(campaign);

        var portCity = new City
        {
            Id = Guid.NewGuid(),
            CampaignId = campaign.Id,
            Name = "Saltmere",
            Description = "A weather-beaten port city built on trade, fog, and things left unsaid.",
            Population = "~6,000",
            Government = "Merchant Council",
            Region = "The Grey Coast",
            Alignment = "Neutral",
            CreatedAt = now,
            UpdatedAt = now,
        };
        var innerCity = new City
        {
            Id = Guid.NewGuid(),
            CampaignId = campaign.Id,
            Name = "Highgarrow",
            Description = "An inland market town a day's ride from Saltmere, where the smuggled goods are said to end up.",
            Population = "~1,200",
            Government = "Town Elder",
            Region = "The Grey Coast",
            Alignment = "Lawful",
            CreatedAt = now,
            UpdatedAt = now,
        };
        dbContext.Cities.AddRange(portCity, innerCity);

        var lighthouse = new Location
        {
            Id = Guid.NewGuid(),
            CampaignId = campaign.Id,
            CityId = portCity.Id,
            Name = "Saltmere Lighthouse",
            Type = LocationType.Landmark,
            Description = "The tall stone lighthouse guarding the harbor mouth. Its keeper hasn't been seen in three days.",
            DmNotes = "A hidden trapdoor beneath the lamp room leads to the smugglers' tunnel network.",
            CreatedAt = now,
            UpdatedAt = now,
        };
        var tavern = new Location
        {
            Id = Guid.NewGuid(),
            CampaignId = campaign.Id,
            CityId = portCity.Id,
            Name = "The Drowned Bell",
            Type = LocationType.Tavern,
            Description = "A dockside tavern where sailors, smugglers, and off-duty guards drink side by side.",
            CreatedAt = now,
            UpdatedAt = now,
        };
        var warehouse = new Location
        {
            Id = Guid.NewGuid(),
            CampaignId = campaign.Id,
            CityId = portCity.Id,
            Name = "Old Customs Warehouse",
            Type = LocationType.Other,
            Description = "A disused warehouse near the docks, officially condemned.",
            DmNotes = "Actual staging point for smuggled cargo before it moves inland to Highgarrow.",
            CreatedAt = now,
            UpdatedAt = now,
        };
        var market = new Location
        {
            Id = Guid.NewGuid(),
            CampaignId = campaign.Id,
            CityId = innerCity.Id,
            Name = "Highgarrow Market Square",
            Type = LocationType.Landmark,
            Description = "The town's central market, busiest at dawn when farmers and traders arrive.",
            CreatedAt = now,
            UpdatedAt = now,
        };
        dbContext.Locations.AddRange(lighthouse, tavern, warehouse, market);

        var keeper = new Npc
        {
            Id = Guid.NewGuid(),
            CampaignId = campaign.Id,
            Name = "Old Corrin",
            Species = "Human",
            Gender = "Male",
            Age = 61,
            Alignment = Alignment.LawfulGood,
            Occupation = "Lighthouse Keeper",
            Disposition = "Missing",
            Description = "The lighthouse keeper of thirty years. Vanished without a trace three nights ago.",
            Status = NpcStatus.Missing,
            CreatedAt = now,
            UpdatedAt = now,
        };
        var barkeep = new Npc
        {
            Id = Guid.NewGuid(),
            CampaignId = campaign.Id,
            Name = "Meris Talvane",
            Species = "Human",
            Gender = "Female",
            Age = 44,
            Alignment = Alignment.TrueNeutral,
            Occupation = "Tavernkeeper",
            Disposition = "Guarded but fair",
            Description = "Runs the Drowned Bell. Hears everything, tells only what's worth trading for.",
            Status = NpcStatus.Alive,
            CreatedAt = now,
            UpdatedAt = now,
        };
        var smuggler = new Npc
        {
            Id = Guid.NewGuid(),
            CampaignId = campaign.Id,
            Name = "Dask Corwin",
            Species = "Half-Elf",
            Gender = "Male",
            Age = 33,
            Class = NpcClass.Rogue,
            Alignment = Alignment.ChaoticNeutral,
            Occupation = "Smuggler",
            Disposition = "Nervous, quick to bargain",
            Description = "A low-ranking member of the smuggling ring, more scared of his bosses than of the party.",
            DmNotes = "Knows the warehouse is the staging point and can be turned with the right pressure.",
            Status = NpcStatus.Alive,
            CreatedAt = now,
            UpdatedAt = now,
        };
        dbContext.Npcs.AddRange(keeper, barkeep, smuggler);

        dbContext.NpcLocations.AddRange(
            new NpcLocation
            {
                Id = Guid.NewGuid(),
                NpcId = keeper.Id,
                LocationId = lighthouse.Id,
                RelationshipType = NpcLocationRelationshipType.LivesAt,
                IsPrimary = true,
                CreatedAt = now,
                UpdatedAt = now,
            },
            new NpcLocation
            {
                Id = Guid.NewGuid(),
                NpcId = barkeep.Id,
                LocationId = tavern.Id,
                RelationshipType = NpcLocationRelationshipType.WorksAt,
                IsPrimary = true,
                CreatedAt = now,
                UpdatedAt = now,
            },
            new NpcLocation
            {
                Id = Guid.NewGuid(),
                NpcId = smuggler.Id,
                LocationId = warehouse.Id,
                RelationshipType = NpcLocationRelationshipType.FrequentlyVisits,
                IsPrimary = false,
                CreatedAt = now,
                UpdatedAt = now,
            });

        var findKeeper = new Quest
        {
            Id = Guid.NewGuid(),
            CampaignId = campaign.Id,
            Name = "The Missing Keeper",
            Description = "Old Corrin, the lighthouse keeper, has vanished. Find out what happened to him.",
            Status = QuestStatus.Available,
            QuestType = QuestType.MainQuest,
            RecommendedLevelMin = 1,
            RecommendedLevelMax = 3,
            Objectives =
            [
                new QuestObjective { Id = Guid.NewGuid(), Description = "Search the lighthouse for clues", SortOrder = 0, CreatedAt = now, UpdatedAt = now },
                new QuestObjective { Id = Guid.NewGuid(), Description = "Ask around the Drowned Bell about Corrin's disappearance", SortOrder = 1, CreatedAt = now, UpdatedAt = now },
                new QuestObjective { Id = Guid.NewGuid(), Description = "Find the hidden trapdoor beneath the lamp room", SortOrder = 2, CreatedAt = now, UpdatedAt = now },
            ],
            CreatedAt = now,
            UpdatedAt = now,
        };
        var smugglingRing = new Quest
        {
            Id = Guid.NewGuid(),
            CampaignId = campaign.Id,
            Name = "Cargo of the Old Customs Warehouse",
            Description = "The trapdoor leads to a smuggling operation staged out of the old warehouse. Uncover who's running it.",
            Status = QuestStatus.Planned,
            QuestType = QuestType.MainQuest,
            RecommendedLevelMin = 2,
            RecommendedLevelMax = 4,
            DmNotes = "Dask Corwin can be turned into an informant if approached without violence.",
            Objectives =
            [
                new QuestObjective { Id = Guid.NewGuid(), Description = "Investigate the Old Customs Warehouse", SortOrder = 0, CreatedAt = now, UpdatedAt = now },
                new QuestObjective { Id = Guid.NewGuid(), Description = "Identify who's running the smuggling ring", SortOrder = 1, CreatedAt = now, UpdatedAt = now },
            ],
            CreatedAt = now,
            UpdatedAt = now,
        };
        var marketRumors = new Quest
        {
            Id = Guid.NewGuid(),
            CampaignId = campaign.Id,
            Name = "Rumors from Highgarrow",
            Description = "Merchants in Highgarrow's market square whisper about unusually cheap goods arriving from the coast.",
            Status = QuestStatus.Planned,
            QuestType = QuestType.SideQuest,
            RecommendedLevelMin = 1,
            RecommendedLevelMax = 3,
            Objectives =
            [
                new QuestObjective { Id = Guid.NewGuid(), Description = "Talk to traders at Highgarrow Market Square", SortOrder = 0, CreatedAt = now, UpdatedAt = now },
            ],
            CreatedAt = now,
            UpdatedAt = now,
        };
        dbContext.Quests.AddRange(findKeeper, smugglingRing, marketRumors);

        dbContext.QuestNpcs.AddRange(
            new QuestNpc { Id = Guid.NewGuid(), QuestId = findKeeper.Id, NpcId = keeper.Id, Role = QuestNpcRole.Victim, CreatedAt = now, UpdatedAt = now },
            new QuestNpc { Id = Guid.NewGuid(), QuestId = findKeeper.Id, NpcId = barkeep.Id, Role = QuestNpcRole.Contact, Notes = "Overheard Corrin arguing with a stranger the night before he vanished.", CreatedAt = now, UpdatedAt = now },
            new QuestNpc { Id = Guid.NewGuid(), QuestId = smugglingRing.Id, NpcId = smuggler.Id, Role = QuestNpcRole.Target, CreatedAt = now, UpdatedAt = now });

        dbContext.QuestLocations.AddRange(
            new QuestLocation { Id = Guid.NewGuid(), QuestId = findKeeper.Id, LocationId = lighthouse.Id, Role = QuestLocationRole.StartingLocation, CreatedAt = now, UpdatedAt = now },
            new QuestLocation { Id = Guid.NewGuid(), QuestId = findKeeper.Id, LocationId = tavern.Id, Role = QuestLocationRole.ObjectiveLocation, CreatedAt = now, UpdatedAt = now },
            new QuestLocation { Id = Guid.NewGuid(), QuestId = smugglingRing.Id, LocationId = warehouse.Id, Role = QuestLocationRole.Destination, CreatedAt = now, UpdatedAt = now },
            new QuestLocation { Id = Guid.NewGuid(), QuestId = marketRumors.Id, LocationId = market.Id, Role = QuestLocationRole.StartingLocation, CreatedAt = now, UpdatedAt = now });

        // A small acyclic graph: finding the keeper unlocks the smuggling-ring
        // quest, and the market rumors are a loosely related side thread -
        // exercises both an "Unlocks" progression edge and a non-progression
        // "Related" edge without creating a cycle.
        dbContext.QuestConnections.AddRange(
            new QuestConnection { Id = Guid.NewGuid(), SourceQuestId = findKeeper.Id, TargetQuestId = smugglingRing.Id, ConnectionType = QuestConnectionType.Unlocks, CreatedAt = now, UpdatedAt = now },
            new QuestConnection { Id = Guid.NewGuid(), SourceQuestId = smugglingRing.Id, TargetQuestId = marketRumors.Id, ConnectionType = QuestConnectionType.Related, CreatedAt = now, UpdatedAt = now });

        // Positions set explicitly so the graph opens already laid out left
        // to right in quest order, rather than React Flow's default stacked
        // grid on first render.
        dbContext.QuestGraphPositions.AddRange(
            new QuestGraphPosition { Id = Guid.NewGuid(), QuestId = findKeeper.Id, X = 0, Y = 0, CreatedAt = now, UpdatedAt = now },
            new QuestGraphPosition { Id = Guid.NewGuid(), QuestId = smugglingRing.Id, X = 300, Y = 0, CreatedAt = now, UpdatedAt = now },
            new QuestGraphPosition { Id = Guid.NewGuid(), QuestId = marketRumors.Id, X = 300, Y = 180, CreatedAt = now, UpdatedAt = now });

        await dbContext.SaveChangesAsync();
    }
}
