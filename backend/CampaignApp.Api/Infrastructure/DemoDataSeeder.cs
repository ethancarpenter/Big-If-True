using CampaignApp.Domain.Entities;
using CampaignApp.Domain.Enums;
using CampaignApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CampaignApp.Api.Infrastructure;

/// <summary>
/// Idempotent seeding (and, separately, resetting) of a self-contained demo
/// account and campaign, kept separate from the hand-curated dev@local.test
/// data (see Program.cs) so a reviewer or recruiter can explore a populated
/// campaign without depending on or cluttering that account.
///
/// This seeder itself has no notion of "environment" - it just creates or
/// refreshes whatever account it's told to, under a fixed reserved user id
/// that normal registration (which always assigns Guid.NewGuid()) can never
/// collide with. Program.cs is what decides *when* SeedAsync runs at startup
/// and *which* credentials it uses:
///   - in Development, always, with the DefaultDevelopmentEmail/Password
///     constants below, for local-dev convenience;
///   - outside Development, only when DemoSeed:Enabled is true, using the
///     DemoSeed:Email / DemoSeed:Password configuration values.
/// ResetAsync is separate again: it never touches the User row at all (see
/// DemoLoginResetService for when it runs - on successful demo login, gated
/// by DemoSeed:ResetOnLogin).
/// </summary>
public static class DemoDataSeeder
{
    /// <summary>
    /// Convenience defaults used only by Development's unconditional demo
    /// seeding step in Program.cs. Never used outside Development, and never
    /// used as a fallback for production's DemoSeed:* configuration.
    /// </summary>
    public const string DefaultDevelopmentEmail = "demo@local.test";
    public const string DefaultDevelopmentPassword = "DemoPassword123!";

    /// <summary>
    /// The reserved demo account's user id - fixed and public so every part
    /// of the app (seeding, reset-on-login, tests) identifies the demo
    /// account the same, single way. Never used as an email-based lookup:
    /// email is configurable, this id is not.
    /// </summary>
    public static readonly Guid DemoUserId = Guid.Parse("00000000-0000-0000-0000-000000000002");

    /// <summary>
    /// Arbitrary but fixed application-specific key for a PostgreSQL
    /// transaction-scoped advisory lock (see ResetAsync). Only needs to be
    /// unique within this application - there is no other advisory lock use
    /// here - and must stay stable across deploys.
    /// </summary>
    private const long ResetAdvisoryLockKey = 725_904_831;

    public static async Task SeedAsync(AppDbContext dbContext, IPasswordHasher<User> hasher, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Demo account email and password must both be provided.");
        }

        var now = DateTime.UtcNow;
        var normalizedEmail = email.Trim().ToUpperInvariant();

        var demoUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == DemoUserId);
        if (demoUser is null)
        {
            // Guard against the configured demo email already belonging to a
            // real, independently-registered user - never take over or
            // collide with another account's row.
            var emailBelongsToAnotherUser = await dbContext.Users
                .AnyAsync(u => u.NormalizedEmail == normalizedEmail && u.Id != DemoUserId);
            if (emailBelongsToAnotherUser)
            {
                throw new InvalidOperationException(
                    "Demo seeding aborted: the configured demo email is already registered to a different, non-demo user.");
            }

            demoUser = new User
            {
                Id = DemoUserId,
                Email = email.Trim(),
                NormalizedEmail = normalizedEmail,
                CreatedAt = now,
                UpdatedAt = now,
            };
            demoUser.PasswordHash = hasher.HashPassword(demoUser, password);
            dbContext.Users.Add(demoUser);
        }
        else
        {
            demoUser.Email = email.Trim();
            demoUser.NormalizedEmail = normalizedEmail;
            demoUser.PasswordHash = hasher.HashPassword(demoUser, password);
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

        CreateSunkenLanternCampaign(dbContext, now);

        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Resets the demo account's DATA ONLY back to the canonical "The Sunken
    /// Lantern" campaign: deletes everything currently owned by the reserved
    /// demo user id (whatever a recruiter left behind - edits, deletions,
    /// or brand-new entities) and recreates the seed content from scratch.
    ///
    /// Deliberately does not touch the User row at all - no id, email,
    /// password hash, or timestamp change - so this can run on every demo
    /// login without needing a password hasher or re-establishing the
    /// account's identity.
    ///
    /// Safety:
    ///   - every delete query below is scoped by ids traced from
    ///     `WHERE UserId = DemoUserId`, never a table-wide delete, and never
    ///     touches another user's rows;
    ///   - delete + recreate happen in ONE SaveChangesAsync call, which EF
    ///     Core wraps in its own implicit transaction even without the
    ///     explicit one below - so on any relational provider this step
    ///     alone is already all-or-nothing;
    ///   - on a relational provider, the whole operation additionally runs
    ///     inside an explicit transaction so a Postgres advisory lock (see
    ///     ResetAdvisoryLockKey) can be held across it, serializing
    ///     concurrent resets from simultaneous demo logins so two requests
    ///     can never both recreate the campaign and produce a duplicate;
    ///     the lock is transaction-scoped and is released automatically on
    ///     commit or rollback;
    ///   - on failure, the transaction is rolled back (relational
    ///     providers) so the demo account is never left half-empty; the
    ///     exception propagates to the caller (DemoLoginResetService), which
    ///     fails the login cleanly instead of signing the user into
    ///     partially reset data.
    ///
    /// The in-memory provider used by tests does not support real
    /// transactions or advisory locks, so both are skipped there - the
    /// single-SaveChangesAsync atomicity guarantee still applies.
    /// </summary>
    public static async Task ResetAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var isRelational = dbContext.Database.IsRelational();
        var transaction = isRelational
            ? await dbContext.Database.BeginTransactionAsync(cancellationToken)
            : null;
        try
        {
            if (isRelational && dbContext.Database.IsNpgsql())
            {
                await dbContext.Database.ExecuteSqlInterpolatedAsync(
                    $"SELECT pg_advisory_xact_lock({ResetAdvisoryLockKey})", cancellationToken);
            }

            await DeleteDemoCampaignDataAsync(dbContext, cancellationToken);
            CreateSunkenLanternCampaign(dbContext, DateTime.UtcNow);
            await dbContext.SaveChangesAsync(cancellationToken);

            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            if (transaction is not null)
            {
                await transaction.RollbackAsync(CancellationToken.None);
            }
            throw;
        }
        finally
        {
            if (transaction is not null)
            {
                await transaction.DisposeAsync();
            }
        }
    }

    /// <summary>
    /// Deletes every entity reachable from the reserved demo user's
    /// campaigns. Every query below is filtered by an id list traced back to
    /// `Campaigns.UserId == DemoUserId` - nothing here can reach another
    /// user's rows. Explicit per-table deletes are used instead of relying
    /// on the database's own ON DELETE CASCADE (which IS configured - see
    /// the *Configuration classes - Cascade throughout) so this behaves
    /// identically on the in-memory provider used by tests, which has no
    /// real foreign keys to cascade through.
    /// </summary>
    private static async Task DeleteDemoCampaignDataAsync(AppDbContext dbContext, CancellationToken cancellationToken)
    {
        var campaigns = await dbContext.Campaigns
            .Where(c => c.UserId == DemoUserId)
            .ToListAsync(cancellationToken);
        if (campaigns.Count == 0)
        {
            return;
        }

        var campaignIds = campaigns.Select(c => c.Id).ToList();

        var quests = await dbContext.Quests.Where(q => campaignIds.Contains(q.CampaignId)).ToListAsync(cancellationToken);
        var questIds = quests.Select(q => q.Id).ToList();
        var locations = await dbContext.Locations.Where(l => campaignIds.Contains(l.CampaignId)).ToListAsync(cancellationToken);
        var npcs = await dbContext.Npcs.Where(n => campaignIds.Contains(n.CampaignId)).ToListAsync(cancellationToken);
        var npcIds = npcs.Select(n => n.Id).ToList();
        var cities = await dbContext.Cities.Where(c => campaignIds.Contains(c.CampaignId)).ToListAsync(cancellationToken);

        var questObjectives = await dbContext.QuestObjectives.Where(o => questIds.Contains(o.QuestId)).ToListAsync(cancellationToken);
        var questNpcs = await dbContext.QuestNpcs.Where(qn => questIds.Contains(qn.QuestId)).ToListAsync(cancellationToken);
        var questLocations = await dbContext.QuestLocations.Where(ql => questIds.Contains(ql.QuestId)).ToListAsync(cancellationToken);
        var questConnections = await dbContext.QuestConnections
            .Where(qc => questIds.Contains(qc.SourceQuestId) || questIds.Contains(qc.TargetQuestId))
            .ToListAsync(cancellationToken);
        var questGraphPositions = await dbContext.QuestGraphPositions.Where(p => questIds.Contains(p.QuestId)).ToListAsync(cancellationToken);
        var npcLocations = await dbContext.NpcLocations.Where(nl => npcIds.Contains(nl.NpcId)).ToListAsync(cancellationToken);

        dbContext.QuestObjectives.RemoveRange(questObjectives);
        dbContext.QuestNpcs.RemoveRange(questNpcs);
        dbContext.QuestLocations.RemoveRange(questLocations);
        dbContext.QuestConnections.RemoveRange(questConnections);
        dbContext.QuestGraphPositions.RemoveRange(questGraphPositions);
        dbContext.NpcLocations.RemoveRange(npcLocations);
        dbContext.Quests.RemoveRange(quests);
        dbContext.Locations.RemoveRange(locations);
        dbContext.Npcs.RemoveRange(npcs);
        dbContext.Cities.RemoveRange(cities);
        dbContext.Campaigns.RemoveRange(campaigns);
    }

    /// <summary>
    /// Adds the canonical "The Sunken Lantern" campaign and its content to
    /// the context under the reserved demo user id. Does not call
    /// SaveChangesAsync - the caller controls when (and with what else) that
    /// happens, so both SeedAsync (fresh install) and ResetAsync (recurring
    /// reset) can share this without duplicating the dataset definition.
    /// </summary>
    private static void CreateSunkenLanternCampaign(AppDbContext dbContext, DateTime now)
    {
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
    }
}
