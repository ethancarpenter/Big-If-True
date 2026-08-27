# Campaign Management App — V0.1 Product Specification

## 1. Product Goal

Build a campaign-management workspace for tabletop RPG Dungeon Masters.

The application should allow a DM to organize a campaign around interconnected:

* Campaigns
* Cities
* Locations
* NPCs
* Quests

The main design principle is:

**Campaign information should be connected rather than stored as isolated notes.**

A DM should be able to move naturally between an NPC, the location they occupy, the city containing that location, and the quests involving that NPC.

V0.1 is focused on establishing this data model and a polished user experience. Advanced features such as quest graphs, world-map drawing, encounters, battle maps, campaign timelines, and player access will come later.

---

# 2. V0.1 Success Criteria

V0.1 is complete when a user can:

1. Create an account.
2. Sign in.
3. Create a campaign.
4. Open the campaign dashboard.
5. Create a city.
6. Create locations within that city.
7. Create an NPC.
8. Assign that NPC to a location.
9. Give the NPC a class and alignment.
10. Create a quest.
11. Associate NPCs with the quest.
12. Associate locations with the quest.
13. Navigate between related entities.
14. Edit and delete the records they created.

The primary demo flow should be:

**Create Campaign → Create City → Create Location → Create NPC → Link NPC to Location → Create Quest → Link NPC and Location to Quest**

---

# 3. Technology Stack

## Frontend

* Next.js
* React
* TypeScript

## Backend

* C#
* ASP.NET Core Web API
* Entity Framework Core

## Database

* PostgreSQL

## Testing

* xUnit for backend tests
* Playwright for important UI flows

## Development

* Docker
* Git
* GitHub
* Claude Code

---

# 4. Application Structure

```text
Campaign
│
├── Cities
│    │
│    └── Locations
│
├── NPCs
│
└── Quests
```

Relationships connect these systems:

```text
NPC ───── located at ─────► Location

NPC ───── involved in ────► Quest

Quest ─── takes place at ─► Location

Location ─ belongs to ────► City
```

Later versions can expand this graph substantially.

---

# 5. Primary Navigation

Persistent left sidebar:

```text
Dashboard

Campaigns

NPCs

Quests

Locations

Cities

────────────

Settings
```

Global search should appear in the top navigation bar.

For V0.1, search can support:

* Campaign name
* NPC name
* Quest name
* City name
* Location name

Selecting a result opens that entity.

---

# 6. Authentication

## Required

Users must be able to:

* Register
* Sign in
* Sign out

Each campaign must belong to a user.

Users must not be able to access another user's campaign through the API by manually changing IDs.

Full campaign sharing and player roles are not part of V0.1.

---

# 7. Campaign Dashboard

## Purpose

Provide the user with a high-level overview of their campaigns.

The dashboard should follow the visual direction of the approved mockup.

Do not include:

* Recently viewed
* Quick actions

## Header

```text
Welcome back,
Dungeon Master

Plan your next adventure.
```

Include:

**+ New Campaign**

---

## Campaign Cards

Each campaign card should contain:

* Campaign name
* Optional cover image
* Last updated date
* NPC count
* Location count
* Quest count

Example:

```text
The Northern Reach

Updated 2 days ago

NPCs        25
Locations   16
Quests       9
```

Clicking the card opens the campaign.

---

## Dashboard Overview

Display:

* Total Campaigns
* Total NPCs
* Total Locations
* Active Quests

---

## Campaign Summary Table

```text
Campaign              Sessions   NPCs   Locations   Quests

The Northern Reach       8        25       16         9
Shadows of Black Coast   6        16        9         6
The Lost Mines           6         9        7         5
```

Sessions can remain informational or omitted from the database until the future Session system is implemented.

---

# 8. Campaign Page

Route:

```text
/campaigns/{campaignId}
```

The campaign page should act as the main workspace.

Header:

```text
The Northern Reach
```

Tabs or navigation:

```text
Overview
Cities
Locations
NPCs
Quests
```

Overview should display:

* Campaign description
* Number of cities
* Number of locations
* Number of NPCs
* Number of quests
* Active quest count

---

# 9. City System

## City Entity

Required fields:

```text
Id
CampaignId
Name
Description
Population
Government
Region
Alignment
CreatedAt
UpdatedAt
```

Most descriptive fields should be optional.

---

# 10. City Page

Route:

```text
/campaigns/{campaignId}/cities/{cityId}
```

Example:

```text
Stonehaven
```

Header information:

```text
Population
~8,000

Government
Merchant Council

Region
Northern Reach

Alignment
Neutral
```

Tabs:

```text
Overview
Locations
People
Notes
```

---

## Locations Section

Display locations as a list.

Example:

```text
Ironforge Inn
Tavern

A bustling inn known for its hearty meals
and warm ale.


Temple of Moradin
Temple

A dwarven temple dedicated to the great smith.
```

Each row should be clickable.

Include:

**+ Add Location**

---

## People Section

Display NPCs associated with locations in the city.

Example:

```text
Eldrin Vale
Innkeeper
Ironforge Inn

Captain Mara
City Watch
Castle Stonehaven
```

---

# 11. Location System

## Location Entity

```text
Id
CampaignId
CityId
Name
Type
Description
DmNotes
CreatedAt
UpdatedAt
```

Possible location types:

```text
Tavern
Temple
Shop
Government
Residence
Dungeon
Landmark
Wilderness
Other
```

Use a selectable value rather than unrestricted text where reasonable.

---

# 12. Location Page

Route:

```text
/campaigns/{campaignId}/locations/{locationId}
```

Display:

* Name
* Type
* City
* Description
* DM Notes
* Associated NPCs
* Associated Quests

Example:

```text
IRONFORGE INN

Type
Tavern

City
Stonehaven

NPCs

Eldrin Vale
Innkeeper

RELATED QUESTS

Missing Caravan
The Black Coin
```

All linked entities should be clickable.

---

# 13. NPC System

NPCs are one of the most important entities in the application.

## NPC Entity

```text
Id
CampaignId
Name
Species
Gender
Age
Class
Alignment
Occupation
Disposition
Description
DmNotes
Status
CreatedAt
UpdatedAt
```

Optional:

```text
PortraitUrl
```

---

# 14. NPC Classes

Provide class options.

Initial list:

```text
Barbarian
Bard
Cleric
Druid
Fighter
Monk
Paladin
Ranger
Rogue
Sorcerer
Warlock
Wizard
Artificer
Commoner
Other
```

Important:

NPCs do not have to follow full player-character rules.

The class represents their primary archetype.

---

# 15. NPC Alignment

Alignment should be selectable from the traditional nine alignments:

```text
Lawful Good
Neutral Good
Chaotic Good

Lawful Neutral
True Neutral
Chaotic Neutral

Lawful Evil
Neutral Evil
Chaotic Evil
```

Alignment should be visually color coded throughout the interface.

Suggested visual grouping:

```text
GOOD
Green family

NEUTRAL
Gold / amber family

EVIL
Red family
```

Lawful / Neutral / Chaotic can slightly modify shade or border treatment.

The text label must always remain visible so color is not the only way alignment is communicated.

Example:

```text
[ Chaotic Good ]
```

Green badge.

```text
[ True Neutral ]
```

Gold/gray badge.

```text
[ Lawful Evil ]
```

Red badge.

---

# 16. NPC Status

Initial statuses:

```text
Alive
Dead
Missing
Unknown
```

Later versions can expand this.

---

# 17. NPC Page

Route:

```text
/campaigns/{campaignId}/npcs/{npcId}
```

Header:

```text
Eldrin Vale
```

Profile section:

```text
Species       Human
Gender        Male
Age           45
Class         Ranger
Alignment     Chaotic Good
Occupation    Innkeeper
Status        Alive
Disposition   Friendly
```

Class and alignment should appear as visual badges.

---

## NPC Page Tabs

```text
Overview
Connections
Quests
Notes
```

---

## Overview

Display:

### Description

Public/general character description.

### DM Notes

Clearly distinguished visually.

Example:

```text
DM NOTES 🔒

Eldrin secretly works with the Black Hand.
```

The lock is currently visual only.

Actual DM/player information permissions will be implemented later.

---

## Connections

Example:

```text
LOCATION

Ironforge Inn
Stonehaven
```

Clicking the location opens it.

---

## Quests

Example:

```text
Missing Caravan
Role: Quest Giver

The Black Coin
Role: Contact
```

---

# 18. Quest System

## Quest Entity

```text
Id
CampaignId
Name
Description
Status
QuestType
RecommendedLevelMin
RecommendedLevelMax
DmNotes
CreatedAt
UpdatedAt
```

---

# 19. Quest Status

```text
Planned
Available
Active
Completed
Failed
Abandoned
```

The interface should visually distinguish status.

Example:

```text
● Active
```

---

# 20. Quest Types

Initial values:

```text
Main Quest
Side Quest
Personal Quest
Faction Quest
Hidden Quest
Other
```

---

# 21. Quest Page

Route:

```text
/campaigns/{campaignId}/quests/{questId}
```

Example:

```text
Missing Caravan
```

Header information:

```text
Status
Active

Type
Main Quest

Level
3–5
```

Tabs:

```text
Overview
Connections
NPCs
Locations
Notes
```

---

## Description

General quest description.

---

## Objectives

V0.1 may support simple checklist objectives.

Example:

```text
☑ Talk to Eldrin Vale

☑ Travel to the last known location

☐ Search the area for clues

☐ Find out what happened to the caravan
```

Objective entity:

```text
Id
QuestId
Description
IsCompleted
SortOrder
```

---

# 22. Quest ↔ NPC Relationship

Create an explicit relationship.

```text
QuestNpc

Id
QuestId
NpcId
Role
Notes
```

Possible roles:

```text
Quest Giver
Ally
Enemy
Victim
Contact
Target
Witness
Participant
Other
```

Example:

```text
Missing Caravan

Eldrin Vale
Quest Giver

Captain Mara
Ally

Roth Blackhand
Antagonist
```

---

# 23. Quest ↔ Location Relationship

```text
QuestLocation

Id
QuestId
LocationId
Role
Notes
```

Suggested roles:

```text
Starting Location
Objective Location
Encounter Location
Destination
Related Location
Other
```

---

# 24. NPC ↔ Location Relationship

Initially, each NPC may have one primary location.

However, the database should eventually support multiple associations.

Recommended structure:

```text
NpcLocation

Id
NpcId
LocationId
RelationshipType
IsPrimary
```

Example relationship types:

```text
Lives At
Works At
Frequently Visits
Owns
Guards
Other
```

This avoids having to redesign the database later.

---

# 25. Global Search

Search field:

```text
Search anything...
```

Results grouped by type.

Example:

```text
NPCs

Eldrin Vale
Captain Mara


Locations

Ironforge Inn


Cities

Stonehaven


Quests

Missing Caravan
```

Selecting a result navigates directly to the entity.

Keyboard shortcut can come later.

---

# 26. Visual Design Direction

Use the approved mockup as the reference.

## Style

Modern SaaS interface with subtle fantasy influence.

Avoid:

* parchment backgrounds
* overly ornate medieval fonts
* fake scrolls
* excessive fantasy ornamentation

The application should still look like professional software.

---

## Color Direction

Primary application background:

Dark charcoal / navy.

Primary accent:

Purple.

Secondary accents:

Gold.

Entity colors may eventually follow categories:

```text
NPC        Purple
Location   Blue
City       Gold
Quest      Green
```

These colors will later help visualize relationship graphs.

---

# 27. Typography

Use a highly readable sans-serif font for UI text.

A serif font may be used sparingly for:

* Page titles
* Campaign titles
* Major headings

Do not use decorative fantasy fonts for body text.

---

# 28. Reusable Components

The frontend should prioritize reusable components.

Examples:

```text
Sidebar

TopNavigation

SearchBar

EntityCard

EntityBadge

AlignmentBadge

StatusBadge

PageHeader

TabNavigation

DetailPanel

RelationshipRow

EmptyState

Modal

ConfirmationDialog
```

Avoid page-specific duplicated components where a shared component makes sense.

---

# 29. API Design

Initial API structure:

```text
/api/auth

/api/campaigns

/api/campaigns/{campaignId}

/api/campaigns/{campaignId}/cities

/api/cities/{cityId}

/api/campaigns/{campaignId}/locations

/api/locations/{locationId}

/api/campaigns/{campaignId}/npcs

/api/npcs/{npcId}

/api/campaigns/{campaignId}/quests

/api/quests/{questId}
```

Relationship endpoints can follow:

```text
/api/quests/{questId}/npcs

/api/quests/{questId}/locations

/api/npcs/{npcId}/locations
```

Exact endpoint structure can evolve as the API design becomes clearer.

---

# 30. Backend Architecture

Suggested solution:

```text
backend/

CampaignApp.Api

CampaignApp.Application

CampaignApp.Domain

CampaignApp.Infrastructure

CampaignApp.Tests
```

## Domain

Contains:

* Entities
* Enums
* Domain rules

## Application

Contains:

* Use cases
* Services
* DTOs
* Interfaces

## Infrastructure

Contains:

* Entity Framework
* PostgreSQL
* Repositories
* External infrastructure

## API

Contains:

* Controllers / endpoints
* Authentication
* Request handling
* API configuration

---

# 31. Important Engineering Rules

## Rule 1 — Campaign ownership must be enforced server-side

Never trust a campaign ID supplied by the frontend.

---

## Rule 2 — Do not expose database entities directly

Use request and response DTOs.

---

## Rule 3 — Relationships should be real relational records

Avoid storing related IDs inside JSON arrays.

---

## Rule 4 — Business logic belongs outside controllers

Controllers should remain thin.

---

## Rule 5 — Do not build future systems prematurely

V0.1 does not require:

* quest graph
* world map
* battle maps
* encounters
* CR calculation
* player character sheets
* compendium
* factions
* calendars
* live collaboration
* AI features

The architecture should allow future expansion without implementing those features now.

---

# 32. V0.1 Implementation Order

## Milestone 1 — Project Foundation

Create:

* Git repository
* Next.js frontend
* ASP.NET Core solution
* PostgreSQL database
* Docker configuration
* Local environment configuration

Goal:

Frontend and backend can both run locally.

---

## Milestone 2 — Campaigns

Implement:

* Campaign entity
* EF mapping
* Migration
* Campaign API
* Campaign creation UI
* Campaign dashboard
* Campaign details page

Goal:

User can create and open campaigns.

---

## Milestone 3 — Cities

Implement:

* City entity
* Campaign relationship
* CRUD API
* City list
* City page

Goal:

User can create Stonehaven inside The Northern Reach.

---

## Milestone 4 — Locations

Implement:

* Location entity
* City relationship
* CRUD API
* Location list
* Location page

Goal:

User can create Ironforge Inn inside Stonehaven.

---

## Milestone 5 — NPCs

Implement:

* NPC entity
* Class enum/options
* Alignment enum/options
* Status
* CRUD API
* NPC page
* Alignment badge styling

Goal:

User can create Eldrin Vale.

---

## Milestone 6 — NPC Location Relationships

Implement:

* NpcLocation entity
* Relationship type
* API
* UI

Goal:

User can assign Eldrin Vale to the Ironforge Inn.

Both pages should show the relationship.

---

## Milestone 7 — Quests

Implement:

* Quest entity
* Quest statuses
* Quest types
* Objectives
* CRUD API
* Quest page

Goal:

User can create Missing Caravan.

---

## Milestone 8 — Quest Relationships

Implement:

* QuestNpc
* QuestLocation
* Relationship roles
* UI for adding/removing associations

Goal:

Missing Caravan can reference:

```text
Eldrin Vale
Quest Giver

Ironforge Inn
Starting Location
```

---

## Milestone 9 — Global Search

Implement search across:

* NPCs
* Quests
* Cities
* Locations

Goal:

Typing:

```text
Eldrin
```

immediately allows navigation to his page.

---

## Milestone 10 — Polish & Tests

Add:

* Backend unit tests
* Integration tests where appropriate
* Primary Playwright workflow
* Validation
* Error handling
* Empty states
* Loading states
* Confirmation dialogs
* Responsive behavior

---

# 33. V0.1 End-to-End Acceptance Test

The application's primary automated or manual test should reproduce this scenario:

```text
1. User signs in.

2. User creates:
   The Northern Reach

3. User opens the campaign.

4. User creates:
   Stonehaven

5. Inside Stonehaven, user creates:
   Ironforge Inn

6. User creates:
   Eldrin Vale

7. NPC details:
   Human
   Ranger
   Chaotic Good
   Innkeeper
   Alive

8. User links:
   Eldrin Vale → Ironforge Inn

9. User creates:
   Missing Caravan

10. User links:
    Eldrin Vale → Missing Caravan
    Role: Quest Giver

11. User links:
    Ironforge Inn → Missing Caravan
    Role: Starting Location

12. User opens Eldrin Vale.

13. NPC page shows:
    Ironforge Inn
    Missing Caravan

14. User clicks Ironforge Inn.

15. Location page shows:
    Eldrin Vale
    Missing Caravan

16. User clicks Missing Caravan.

17. Quest page shows:
    Eldrin Vale
    Ironforge Inn
```

If that entire workflow feels polished, V0.1 has succeeded.

---

# 34. Next Major Version

Once V0.1 is stable:

## V0.2 — Quest Graph

Introduce:

```text
QuestConnection

SourceQuestId
TargetQuestId
ConnectionType
```

Possible relationships:

```text
Unlocks
Requires
Optional
Alternative Path
Failure Leads To
Related
```

Build an interactive node graph showing quest chains.

That should be the first major feature after the V0.1 foundation.
