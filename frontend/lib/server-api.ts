import "server-only";
import { cookies } from "next/headers";
import { API_URL, handleResponse } from "./api-shared";
import type {
  Campaign,
  City,
  CurrentUser,
  Location,
  Npc,
  NpcLocation,
  Quest,
  QuestConnection,
  QuestGraphPosition,
  QuestLocation,
  QuestNpc,
} from "./api-types";

export * from "./api-types";
export * from "./api-shared";

/**
 * Server Component data-fetching runs in the Next.js Node process, not the
 * browser - a fresh outbound fetch() from here does not automatically carry
 * anything from the inbound browser request. The auth cookie has to be read
 * from the current request (via next/headers, valid here because every
 * caller is a Server Component/layout render) and forwarded explicitly.
 */
async function serverFetch(path: string, init: RequestInit = {}): Promise<Response> {
  const cookieHeader = (await cookies()).toString();
  const headers = new Headers(init.headers);
  if (cookieHeader) {
    headers.set("Cookie", cookieHeader);
  }
  return fetch(`${API_URL}${path}`, { ...init, headers, cache: init.cache ?? "no-store" });
}

export async function getCurrentUser(): Promise<CurrentUser | null> {
  const response = await serverFetch("/api/auth/me");
  if (response.status === 401) {
    return null;
  }
  return handleResponse<CurrentUser>(response);
}

export async function getCampaigns(): Promise<Campaign[]> {
  const response = await serverFetch("/api/campaigns");
  return handleResponse<Campaign[]>(response);
}

export async function getCampaign(id: string): Promise<Campaign> {
  const response = await serverFetch(`/api/campaigns/${id}`);
  return handleResponse<Campaign>(response);
}

export async function getCitiesForCampaign(campaignId: string): Promise<City[]> {
  const response = await serverFetch(`/api/campaigns/${campaignId}/cities`);
  return handleResponse<City[]>(response);
}

export async function getCity(id: string): Promise<City> {
  const response = await serverFetch(`/api/cities/${id}`);
  return handleResponse<City>(response);
}

export async function getLocationsForCampaign(campaignId: string, cityId?: string): Promise<Location[]> {
  const query = cityId ? `?cityId=${cityId}` : "";
  const response = await serverFetch(`/api/campaigns/${campaignId}/locations${query}`);
  return handleResponse<Location[]>(response);
}

export async function getLocation(id: string): Promise<Location> {
  const response = await serverFetch(`/api/locations/${id}`);
  return handleResponse<Location>(response);
}

export async function getNpcsForCampaign(campaignId: string): Promise<Npc[]> {
  const response = await serverFetch(`/api/campaigns/${campaignId}/npcs`);
  return handleResponse<Npc[]>(response);
}

export async function getNpc(id: string): Promise<Npc> {
  const response = await serverFetch(`/api/npcs/${id}`);
  return handleResponse<Npc>(response);
}

export async function getNpcLocationsForNpc(npcId: string): Promise<NpcLocation[]> {
  const response = await serverFetch(`/api/npcs/${npcId}/locations`);
  return handleResponse<NpcLocation[]>(response);
}

export async function getNpcLocationsForLocation(locationId: string): Promise<NpcLocation[]> {
  const response = await serverFetch(`/api/locations/${locationId}/npcs`);
  return handleResponse<NpcLocation[]>(response);
}

export async function getQuestsForCampaign(campaignId: string): Promise<Quest[]> {
  const response = await serverFetch(`/api/campaigns/${campaignId}/quests`);
  return handleResponse<Quest[]>(response);
}

export async function getQuest(id: string): Promise<Quest> {
  const response = await serverFetch(`/api/quests/${id}`);
  return handleResponse<Quest>(response);
}

export async function getQuestNpcsForQuest(questId: string): Promise<QuestNpc[]> {
  const response = await serverFetch(`/api/quests/${questId}/npcs`);
  return handleResponse<QuestNpc[]>(response);
}

export async function getQuestNpcsForNpc(npcId: string): Promise<QuestNpc[]> {
  const response = await serverFetch(`/api/npcs/${npcId}/quests`);
  return handleResponse<QuestNpc[]>(response);
}

export async function getQuestLocationsForQuest(questId: string): Promise<QuestLocation[]> {
  const response = await serverFetch(`/api/quests/${questId}/locations`);
  return handleResponse<QuestLocation[]>(response);
}

export async function getQuestLocationsForLocation(locationId: string): Promise<QuestLocation[]> {
  const response = await serverFetch(`/api/locations/${locationId}/quests`);
  return handleResponse<QuestLocation[]>(response);
}

export async function getQuestConnectionsForCampaign(campaignId: string): Promise<QuestConnection[]> {
  const response = await serverFetch(`/api/campaigns/${campaignId}/quest-connections`);
  return handleResponse<QuestConnection[]>(response);
}

export async function getQuestGraphPositionsForCampaign(campaignId: string): Promise<QuestGraphPosition[]> {
  const response = await serverFetch(`/api/campaigns/${campaignId}/quest-graph-positions`);
  return handleResponse<QuestGraphPosition[]>(response);
}
