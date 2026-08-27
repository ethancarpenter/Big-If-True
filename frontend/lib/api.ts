import { API_URL, handleResponse } from "./api-shared";
import type {
  Campaign,
  CampaignRequest,
  City,
  CityRequest,
  CreateNpcLocationRequest,
  CreateQuestConnectionRequest,
  CreateQuestLocationRequest,
  CreateQuestNpcRequest,
  CreateQuestObjectiveRequest,
  CurrentUser,
  Location,
  LocationRequest,
  LoginRequest,
  Npc,
  NpcLocation,
  NpcRequest,
  Quest,
  QuestConnection,
  QuestGraphPosition,
  QuestLocation,
  QuestNpc,
  QuestObjective,
  QuestRequest,
  RegisterRequest,
  ReorderQuestObjectivesRequest,
  UpdateNpcLocationRequest,
  UpdateQuestConnectionRequest,
  UpdateQuestGraphPositionRequest,
  UpdateQuestLocationRequest,
  UpdateQuestNpcRequest,
  UpdateQuestObjectiveRequest,
} from "./api-types";

export * from "./api-types";
export * from "./api-shared";

// The current session's CSRF request token, cached for reuse across
// mutations - session-global state, the same category of thing API_URL
// already is in this file, not per-component state. ASP.NET Core's default
// antiforgery token generator binds a token to the request's identity at
// issue time, so it's invalidated after login/register/logout and lazily
// refetched on the next mutation.
let cachedCsrfToken: string | null = null;
let pendingCsrfFetch: Promise<string> | null = null;

async function ensureCsrfToken(): Promise<string> {
  if (cachedCsrfToken) {
    return cachedCsrfToken;
  }
  if (!pendingCsrfFetch) {
    pendingCsrfFetch = fetch(`${API_URL}/api/auth/csrf`, { credentials: "include" })
      .then((response) => handleResponse<{ token: string }>(response))
      .then(({ token }) => {
        cachedCsrfToken = token;
        return token;
      })
      .finally(() => {
        pendingCsrfFetch = null;
      });
  }
  return pendingCsrfFetch;
}

function clearCsrfToken() {
  cachedCsrfToken = null;
}

async function apiFetch(path: string, init: RequestInit = {}): Promise<Response> {
  const method = (init.method ?? "GET").toUpperCase();
  const headers = new Headers(init.headers);
  if (method !== "GET" && method !== "HEAD") {
    headers.set("X-CSRF-TOKEN", await ensureCsrfToken());
  }
  return fetch(`${API_URL}${path}`, { ...init, headers, credentials: "include" });
}

export async function registerUser(data: RegisterRequest): Promise<CurrentUser> {
  const response = await apiFetch("/api/auth/register", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  const user = await handleResponse<CurrentUser>(response);
  clearCsrfToken();
  return user;
}

export async function loginUser(data: LoginRequest): Promise<CurrentUser> {
  const response = await apiFetch("/api/auth/login", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  const user = await handleResponse<CurrentUser>(response);
  clearCsrfToken();
  return user;
}

export async function logoutUser(): Promise<void> {
  const response = await apiFetch("/api/auth/logout", { method: "POST" });
  await handleResponse<void>(response);
  clearCsrfToken();
}

export async function createCampaign(data: CampaignRequest): Promise<Campaign> {
  const response = await apiFetch("/api/campaigns", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<Campaign>(response);
}

export async function updateCampaign(id: string, data: CampaignRequest): Promise<Campaign> {
  const response = await apiFetch(`/api/campaigns/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<Campaign>(response);
}

export async function deleteCampaign(id: string): Promise<void> {
  const response = await apiFetch(`/api/campaigns/${id}`, { method: "DELETE" });
  return handleResponse<void>(response);
}

export async function createCity(campaignId: string, data: CityRequest): Promise<City> {
  const response = await apiFetch(`/api/campaigns/${campaignId}/cities`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<City>(response);
}

export async function updateCity(id: string, data: CityRequest): Promise<City> {
  const response = await apiFetch(`/api/cities/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<City>(response);
}

export async function deleteCity(id: string): Promise<void> {
  const response = await apiFetch(`/api/cities/${id}`, { method: "DELETE" });
  return handleResponse<void>(response);
}

export async function createLocation(campaignId: string, data: LocationRequest): Promise<Location> {
  const response = await apiFetch(`/api/campaigns/${campaignId}/locations`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<Location>(response);
}

export async function updateLocation(id: string, data: LocationRequest): Promise<Location> {
  const response = await apiFetch(`/api/locations/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<Location>(response);
}

export async function deleteLocation(id: string): Promise<void> {
  const response = await apiFetch(`/api/locations/${id}`, { method: "DELETE" });
  return handleResponse<void>(response);
}

export async function createNpc(campaignId: string, data: NpcRequest): Promise<Npc> {
  const response = await apiFetch(`/api/campaigns/${campaignId}/npcs`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<Npc>(response);
}

export async function updateNpc(id: string, data: NpcRequest): Promise<Npc> {
  const response = await apiFetch(`/api/npcs/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<Npc>(response);
}

export async function deleteNpc(id: string): Promise<void> {
  const response = await apiFetch(`/api/npcs/${id}`, { method: "DELETE" });
  return handleResponse<void>(response);
}

export async function createNpcLocation(npcId: string, data: CreateNpcLocationRequest): Promise<NpcLocation> {
  const response = await apiFetch(`/api/npcs/${npcId}/locations`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<NpcLocation>(response);
}

export async function updateNpcLocation(id: string, data: UpdateNpcLocationRequest): Promise<NpcLocation> {
  const response = await apiFetch(`/api/npc-locations/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<NpcLocation>(response);
}

export async function deleteNpcLocation(id: string): Promise<void> {
  const response = await apiFetch(`/api/npc-locations/${id}`, { method: "DELETE" });
  return handleResponse<void>(response);
}

export async function createQuest(campaignId: string, data: QuestRequest): Promise<Quest> {
  const response = await apiFetch(`/api/campaigns/${campaignId}/quests`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<Quest>(response);
}

export async function updateQuest(id: string, data: QuestRequest): Promise<Quest> {
  const response = await apiFetch(`/api/quests/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<Quest>(response);
}

export async function deleteQuest(id: string): Promise<void> {
  const response = await apiFetch(`/api/quests/${id}`, { method: "DELETE" });
  return handleResponse<void>(response);
}

export async function createQuestObjective(questId: string, data: CreateQuestObjectiveRequest): Promise<QuestObjective> {
  const response = await apiFetch(`/api/quests/${questId}/objectives`, {
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
  const response = await apiFetch(`/api/quests/${questId}/objectives/${objectiveId}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<QuestObjective>(response);
}

export async function deleteQuestObjective(questId: string, objectiveId: string): Promise<void> {
  const response = await apiFetch(`/api/quests/${questId}/objectives/${objectiveId}`, { method: "DELETE" });
  return handleResponse<void>(response);
}

export async function reorderQuestObjectives(
  questId: string,
  data: ReorderQuestObjectivesRequest,
): Promise<QuestObjective[]> {
  const response = await apiFetch(`/api/quests/${questId}/objectives/reorder`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<QuestObjective[]>(response);
}

export async function createQuestNpc(questId: string, data: CreateQuestNpcRequest): Promise<QuestNpc> {
  const response = await apiFetch(`/api/quests/${questId}/npcs`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<QuestNpc>(response);
}

export async function updateQuestNpc(id: string, data: UpdateQuestNpcRequest): Promise<QuestNpc> {
  const response = await apiFetch(`/api/quest-npcs/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<QuestNpc>(response);
}

export async function deleteQuestNpc(id: string): Promise<void> {
  const response = await apiFetch(`/api/quest-npcs/${id}`, { method: "DELETE" });
  return handleResponse<void>(response);
}

export async function createQuestLocation(questId: string, data: CreateQuestLocationRequest): Promise<QuestLocation> {
  const response = await apiFetch(`/api/quests/${questId}/locations`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<QuestLocation>(response);
}

export async function updateQuestLocation(id: string, data: UpdateQuestLocationRequest): Promise<QuestLocation> {
  const response = await apiFetch(`/api/quest-locations/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<QuestLocation>(response);
}

export async function deleteQuestLocation(id: string): Promise<void> {
  const response = await apiFetch(`/api/quest-locations/${id}`, { method: "DELETE" });
  return handleResponse<void>(response);
}

export async function createQuestConnection(
  campaignId: string,
  data: CreateQuestConnectionRequest,
): Promise<QuestConnection> {
  const response = await apiFetch(`/api/campaigns/${campaignId}/quest-connections`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<QuestConnection>(response);
}

export async function updateQuestConnection(id: string, data: UpdateQuestConnectionRequest): Promise<QuestConnection> {
  const response = await apiFetch(`/api/quest-connections/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<QuestConnection>(response);
}

export async function deleteQuestConnection(id: string): Promise<void> {
  const response = await apiFetch(`/api/quest-connections/${id}`, { method: "DELETE" });
  return handleResponse<void>(response);
}

export async function updateQuestGraphPosition(
  questId: string,
  data: UpdateQuestGraphPositionRequest,
): Promise<QuestGraphPosition> {
  const response = await apiFetch(`/api/quests/${questId}/graph-position`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(data),
  });
  return handleResponse<QuestGraphPosition>(response);
}
