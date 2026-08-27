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

export class ApiNotFoundError extends Error {
  constructor() {
    super("Not found");
  }
}

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    if (response.status === 404) {
      throw new ApiNotFoundError();
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
