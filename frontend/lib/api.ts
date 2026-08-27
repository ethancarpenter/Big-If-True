const API_URL = process.env.NEXT_PUBLIC_API_URL;

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

export class ApiNotFoundError extends Error {
  constructor() {
    super("Not found");
  }
}

export class ApiConflictError extends Error {
  constructor() {
    super("Conflict");
  }
}

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    if (response.status === 404) {
      throw new ApiNotFoundError();
    }
    if (response.status === 409) {
      throw new ApiConflictError();
    }
    const body = await response.text();
    throw new Error(`API request failed (${response.status}): ${body}`);
  }
  if (response.status === 204) {
    return undefined as T;
  }
  return (await response.json()) as T;
}

export async function getCampaigns(): Promise<Campaign[]> {
  const response = await fetch(`${API_URL}/api/campaigns`, { cache: "no-store" });
  return handleResponse<Campaign[]>(response);
}

export async function getCampaign(id: string): Promise<Campaign> {
  const response = await fetch(`${API_URL}/api/campaigns/${id}`, { cache: "no-store" });
  return handleResponse<Campaign>(response);
}

export async function createCampaign(data: CampaignRequest): Promise<Campaign> {
  const response = await fetch(`${API_URL}/api/campaigns`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<Campaign>(response);
}

export async function updateCampaign(id: string, data: CampaignRequest): Promise<Campaign> {
  const response = await fetch(`${API_URL}/api/campaigns/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<Campaign>(response);
}

export async function deleteCampaign(id: string): Promise<void> {
  const response = await fetch(`${API_URL}/api/campaigns/${id}`, { method: "DELETE" });
  return handleResponse<void>(response);
}

export async function getCitiesForCampaign(campaignId: string): Promise<City[]> {
  const response = await fetch(`${API_URL}/api/campaigns/${campaignId}/cities`, { cache: "no-store" });
  return handleResponse<City[]>(response);
}

export async function getCity(id: string): Promise<City> {
  const response = await fetch(`${API_URL}/api/cities/${id}`, { cache: "no-store" });
  return handleResponse<City>(response);
}

export async function createCity(campaignId: string, data: CityRequest): Promise<City> {
  const response = await fetch(`${API_URL}/api/campaigns/${campaignId}/cities`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<City>(response);
}

export async function updateCity(id: string, data: CityRequest): Promise<City> {
  const response = await fetch(`${API_URL}/api/cities/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<City>(response);
}

export async function deleteCity(id: string): Promise<void> {
  const response = await fetch(`${API_URL}/api/cities/${id}`, { method: "DELETE" });
  return handleResponse<void>(response);
}

export async function getLocationsForCampaign(campaignId: string, cityId?: string): Promise<Location[]> {
  const query = cityId ? `?cityId=${cityId}` : "";
  const response = await fetch(`${API_URL}/api/campaigns/${campaignId}/locations${query}`, {
    cache: "no-store",
  });
  return handleResponse<Location[]>(response);
}

export async function getLocation(id: string): Promise<Location> {
  const response = await fetch(`${API_URL}/api/locations/${id}`, { cache: "no-store" });
  return handleResponse<Location>(response);
}

export async function createLocation(campaignId: string, data: LocationRequest): Promise<Location> {
  const response = await fetch(`${API_URL}/api/campaigns/${campaignId}/locations`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<Location>(response);
}

export async function updateLocation(id: string, data: LocationRequest): Promise<Location> {
  const response = await fetch(`${API_URL}/api/locations/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<Location>(response);
}

export async function deleteLocation(id: string): Promise<void> {
  const response = await fetch(`${API_URL}/api/locations/${id}`, { method: "DELETE" });
  return handleResponse<void>(response);
}

export async function getNpcsForCampaign(campaignId: string): Promise<Npc[]> {
  const response = await fetch(`${API_URL}/api/campaigns/${campaignId}/npcs`, { cache: "no-store" });
  return handleResponse<Npc[]>(response);
}

export async function getNpc(id: string): Promise<Npc> {
  const response = await fetch(`${API_URL}/api/npcs/${id}`, { cache: "no-store" });
  return handleResponse<Npc>(response);
}

export async function createNpc(campaignId: string, data: NpcRequest): Promise<Npc> {
  const response = await fetch(`${API_URL}/api/campaigns/${campaignId}/npcs`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<Npc>(response);
}

export async function updateNpc(id: string, data: NpcRequest): Promise<Npc> {
  const response = await fetch(`${API_URL}/api/npcs/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<Npc>(response);
}

export async function deleteNpc(id: string): Promise<void> {
  const response = await fetch(`${API_URL}/api/npcs/${id}`, { method: "DELETE" });
  return handleResponse<void>(response);
}

export async function getNpcLocationsForNpc(npcId: string): Promise<NpcLocation[]> {
  const response = await fetch(`${API_URL}/api/npcs/${npcId}/locations`, { cache: "no-store" });
  return handleResponse<NpcLocation[]>(response);
}

export async function getNpcLocationsForLocation(locationId: string): Promise<NpcLocation[]> {
  const response = await fetch(`${API_URL}/api/locations/${locationId}/npcs`, { cache: "no-store" });
  return handleResponse<NpcLocation[]>(response);
}

export async function createNpcLocation(npcId: string, data: CreateNpcLocationRequest): Promise<NpcLocation> {
  const response = await fetch(`${API_URL}/api/npcs/${npcId}/locations`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<NpcLocation>(response);
}

export async function updateNpcLocation(id: string, data: UpdateNpcLocationRequest): Promise<NpcLocation> {
  const response = await fetch(`${API_URL}/api/npc-locations/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<NpcLocation>(response);
}

export async function deleteNpcLocation(id: string): Promise<void> {
  const response = await fetch(`${API_URL}/api/npc-locations/${id}`, { method: "DELETE" });
  return handleResponse<void>(response);
}

export async function getQuestsForCampaign(campaignId: string): Promise<Quest[]> {
  const response = await fetch(`${API_URL}/api/campaigns/${campaignId}/quests`, { cache: "no-store" });
  return handleResponse<Quest[]>(response);
}

export async function getQuest(id: string): Promise<Quest> {
  const response = await fetch(`${API_URL}/api/quests/${id}`, { cache: "no-store" });
  return handleResponse<Quest>(response);
}

export async function createQuest(campaignId: string, data: QuestRequest): Promise<Quest> {
  const response = await fetch(`${API_URL}/api/campaigns/${campaignId}/quests`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<Quest>(response);
}

export async function updateQuest(id: string, data: QuestRequest): Promise<Quest> {
  const response = await fetch(`${API_URL}/api/quests/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<Quest>(response);
}

export async function deleteQuest(id: string): Promise<void> {
  const response = await fetch(`${API_URL}/api/quests/${id}`, { method: "DELETE" });
  return handleResponse<void>(response);
}

export async function createQuestObjective(questId: string, data: CreateQuestObjectiveRequest): Promise<QuestObjective> {
  const response = await fetch(`${API_URL}/api/quests/${questId}/objectives`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<QuestObjective>(response);
}

export async function updateQuestObjective(
  questId: string,
  objectiveId: string,
  data: UpdateQuestObjectiveRequest,
): Promise<QuestObjective> {
  const response = await fetch(`${API_URL}/api/quests/${questId}/objectives/${objectiveId}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<QuestObjective>(response);
}

export async function deleteQuestObjective(questId: string, objectiveId: string): Promise<void> {
  const response = await fetch(`${API_URL}/api/quests/${questId}/objectives/${objectiveId}`, { method: "DELETE" });
  return handleResponse<void>(response);
}

export async function reorderQuestObjectives(
  questId: string,
  data: ReorderQuestObjectivesRequest,
): Promise<QuestObjective[]> {
  const response = await fetch(`${API_URL}/api/quests/${questId}/objectives/reorder`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<QuestObjective[]>(response);
}

export async function getQuestNpcsForQuest(questId: string): Promise<QuestNpc[]> {
  const response = await fetch(`${API_URL}/api/quests/${questId}/npcs`, { cache: "no-store" });
  return handleResponse<QuestNpc[]>(response);
}

export async function getQuestNpcsForNpc(npcId: string): Promise<QuestNpc[]> {
  const response = await fetch(`${API_URL}/api/npcs/${npcId}/quests`, { cache: "no-store" });
  return handleResponse<QuestNpc[]>(response);
}

export async function createQuestNpc(questId: string, data: CreateQuestNpcRequest): Promise<QuestNpc> {
  const response = await fetch(`${API_URL}/api/quests/${questId}/npcs`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<QuestNpc>(response);
}

export async function updateQuestNpc(id: string, data: UpdateQuestNpcRequest): Promise<QuestNpc> {
  const response = await fetch(`${API_URL}/api/quest-npcs/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<QuestNpc>(response);
}

export async function deleteQuestNpc(id: string): Promise<void> {
  const response = await fetch(`${API_URL}/api/quest-npcs/${id}`, { method: "DELETE" });
  return handleResponse<void>(response);
}

export async function getQuestLocationsForQuest(questId: string): Promise<QuestLocation[]> {
  const response = await fetch(`${API_URL}/api/quests/${questId}/locations`, { cache: "no-store" });
  return handleResponse<QuestLocation[]>(response);
}

export async function getQuestLocationsForLocation(locationId: string): Promise<QuestLocation[]> {
  const response = await fetch(`${API_URL}/api/locations/${locationId}/quests`, { cache: "no-store" });
  return handleResponse<QuestLocation[]>(response);
}

export async function createQuestLocation(questId: string, data: CreateQuestLocationRequest): Promise<QuestLocation> {
  const response = await fetch(`${API_URL}/api/quests/${questId}/locations`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<QuestLocation>(response);
}

export async function updateQuestLocation(id: string, data: UpdateQuestLocationRequest): Promise<QuestLocation> {
  const response = await fetch(`${API_URL}/api/quest-locations/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<QuestLocation>(response);
}

export async function deleteQuestLocation(id: string): Promise<void> {
  const response = await fetch(`${API_URL}/api/quest-locations/${id}`, { method: "DELETE" });
  return handleResponse<void>(response);
}
