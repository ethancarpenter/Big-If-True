export interface Campaign {
  id: string;
  name: string;
  description: string | null;
  coverImageUrl: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CampaignRequest {
  name: string;
  description?: string;
  coverImageUrl?: string;
}

export interface City {
  id: string;
  campaignId: string;
  name: string;
  description: string | null;
  population: string | null;
  government: string | null;
  region: string | null;
  alignment: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CityRequest {
  name: string;
  description?: string;
  population?: string;
  government?: string;
  region?: string;
  alignment?: string;
}

export const LOCATION_TYPES = [
  "Tavern",
  "Temple",
  "Shop",
  "Government",
  "Residence",
  "Dungeon",
  "Landmark",
  "Wilderness",
  "Other",
] as const;

export type LocationType = (typeof LOCATION_TYPES)[number];

export interface Location {
  id: string;
  campaignId: string;
  cityId: string;
  cityName: string;
  name: string;
  type: LocationType;
  description: string | null;
  dmNotes: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface LocationRequest {
  cityId: string;
  name: string;
  type: LocationType;
  description?: string;
  dmNotes?: string;
}

export const NPC_CLASSES = [
  "Barbarian",
  "Bard",
  "Cleric",
  "Druid",
  "Fighter",
  "Monk",
  "Paladin",
  "Ranger",
  "Rogue",
  "Sorcerer",
  "Warlock",
  "Wizard",
  "Artificer",
  "Commoner",
  "Other",
] as const;

export type NpcClass = (typeof NPC_CLASSES)[number];

export const ALIGNMENTS = [
  "LawfulGood",
  "NeutralGood",
  "ChaoticGood",
  "LawfulNeutral",
  "TrueNeutral",
  "ChaoticNeutral",
  "LawfulEvil",
  "NeutralEvil",
  "ChaoticEvil",
] as const;

export type Alignment = (typeof ALIGNMENTS)[number];

export const NPC_STATUSES = ["Alive", "Dead", "Missing", "Unknown"] as const;

export type NpcStatus = (typeof NPC_STATUSES)[number];

export interface Npc {
  id: string;
  campaignId: string;
  name: string;
  species: string | null;
  gender: string | null;
  age: number | null;
  class: NpcClass | null;
  alignment: Alignment | null;
  occupation: string | null;
  disposition: string | null;
  description: string | null;
  dmNotes: string | null;
  status: NpcStatus;
  portraitUrl: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface NpcRequest {
  name: string;
  species?: string;
  gender?: string;
  age?: number;
  class?: NpcClass;
  alignment?: Alignment;
  occupation?: string;
  disposition?: string;
  description?: string;
  dmNotes?: string;
  status: NpcStatus;
  portraitUrl?: string;
}

export const NPC_LOCATION_RELATIONSHIP_TYPES = [
  "LivesAt",
  "WorksAt",
  "FrequentlyVisits",
  "Owns",
  "Guards",
  "Other",
] as const;

export type NpcLocationRelationshipType = (typeof NPC_LOCATION_RELATIONSHIP_TYPES)[number];

export interface NpcLocation {
  id: string;
  npcId: string;
  npcName: string;
  locationId: string;
  locationName: string;
  cityName: string;
  relationshipType: NpcLocationRelationshipType;
  isPrimary: boolean;
  createdAt: string;
}

export interface CreateNpcLocationRequest {
  locationId: string;
  relationshipType: NpcLocationRelationshipType;
  isPrimary: boolean;
}

export interface UpdateNpcLocationRequest {
  relationshipType: NpcLocationRelationshipType;
  isPrimary: boolean;
}

export const QUEST_STATUSES = ["Planned", "Available", "Active", "Completed", "Failed", "Abandoned"] as const;

export type QuestStatus = (typeof QUEST_STATUSES)[number];

export const QUEST_TYPES = [
  "MainQuest",
  "SideQuest",
  "PersonalQuest",
  "FactionQuest",
  "HiddenQuest",
  "Other",
] as const;

export type QuestType = (typeof QUEST_TYPES)[number];

export interface QuestObjective {
  id: string;
  questId: string;
  description: string;
  isCompleted: boolean;
  sortOrder: number;
}

export interface Quest {
  id: string;
  campaignId: string;
  name: string;
  description: string | null;
  status: QuestStatus;
  questType: QuestType;
  recommendedLevelMin: number | null;
  recommendedLevelMax: number | null;
  dmNotes: string | null;
  objectives: QuestObjective[];
  createdAt: string;
  updatedAt: string;
}

export interface QuestRequest {
  name: string;
  description?: string;
  status: QuestStatus;
  questType: QuestType;
  recommendedLevelMin?: number;
  recommendedLevelMax?: number;
  dmNotes?: string;
}

export interface CreateQuestObjectiveRequest {
  description: string;
}

export interface UpdateQuestObjectiveRequest {
  description: string;
  isCompleted: boolean;
}

export interface ReorderQuestObjectivesRequest {
  objectiveIds: string[];
}

export const QUEST_NPC_ROLES = [
  "QuestGiver",
  "Ally",
  "Enemy",
  "Victim",
  "Contact",
  "Target",
  "Witness",
  "Participant",
  "Other",
] as const;

export type QuestNpcRole = (typeof QUEST_NPC_ROLES)[number];

export interface QuestNpc {
  id: string;
  questId: string;
  questName: string;
  npcId: string;
  npcName: string;
  role: QuestNpcRole;
  notes: string | null;
  createdAt: string;
}

export interface CreateQuestNpcRequest {
  npcId: string;
  role: QuestNpcRole;
  notes?: string;
}

export interface UpdateQuestNpcRequest {
  role: QuestNpcRole;
  notes?: string;
}

export const QUEST_LOCATION_ROLES = [
  "StartingLocation",
  "ObjectiveLocation",
  "EncounterLocation",
  "Destination",
  "RelatedLocation",
  "Other",
] as const;

export type QuestLocationRole = (typeof QUEST_LOCATION_ROLES)[number];

export interface QuestLocation {
  id: string;
  questId: string;
  questName: string;
  locationId: string;
  locationName: string;
  cityName: string;
  role: QuestLocationRole;
  notes: string | null;
  createdAt: string;
}

export interface CreateQuestLocationRequest {
  locationId: string;
  role: QuestLocationRole;
  notes?: string;
}

export interface UpdateQuestLocationRequest {
  role: QuestLocationRole;
  notes?: string;
}

export const QUEST_CONNECTION_TYPES = [
  "Unlocks",
  "Requires",
  "Optional",
  "AlternativePath",
  "FailureLeadsTo",
  "Related",
] as const;

export type QuestConnectionType = (typeof QUEST_CONNECTION_TYPES)[number];

export interface QuestConnection {
  id: string;
  sourceQuestId: string;
  sourceQuestName: string;
  targetQuestId: string;
  targetQuestName: string;
  connectionType: QuestConnectionType;
  createdAt: string;
}

export interface CreateQuestConnectionRequest {
  sourceQuestId: string;
  targetQuestId: string;
  connectionType: QuestConnectionType;
}

export interface UpdateQuestConnectionRequest {
  connectionType: QuestConnectionType;
}

export interface QuestGraphPosition {
  questId: string;
  x: number;
  y: number;
}

export interface UpdateQuestGraphPositionRequest {
  x: number;
  y: number;
}

export interface CurrentUser {
  id: string;
  email: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}
