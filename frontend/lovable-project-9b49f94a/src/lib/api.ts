// API Service client for Octopus ASP.NET Core Backend

export interface ApiShip {
  id: number;
  name: string;
  imoNumber: string;
  cargoType: string;
  estimatedArrival: string;
  status: string;
}

export interface ApiBerth {
  id: number;
  name: string;
  maxDraftMeters: number;
  isAvailable: boolean;
}

export interface ApiAssignment {
  id: number;
  shipId: number;
  berthId: number;
  startsAt: string;
  endsAt?: string | null;
  status: string;
}

export interface ApiSystemState {
  environment: string;
  serverTimeUtc: string;
  shipCount: number;
  berthCount: number;
  activeAssignmentCount: number;
}

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "/api";

async function fetchJson<T>(url: string): Promise<T | null> {
  try {
    const res = await fetch(`${API_BASE_URL}${url}`, {
      headers: { "Content-Type": "application/json" },
    });
    if (!res.ok) throw new Error(`HTTP error! status: ${res.status}`);
    return (await res.json()) as T;
  } catch (err) {
    console.warn(`[Octopus API] Failed to fetch ${url}, using fallback data:`, err);
    return null;
  }
}

// Fallback Mock Data
const MOCK_SHIPS: ApiShip[] = [
  { id: 1, name: "MSC Liguria", imoNumber: "9843301", cargoType: "Container", estimatedArrival: new Date().toISOString(), status: "Berth" },
  { id: 2, name: "CMA CGM Ligure", imoNumber: "9701221", cargoType: "Container", estimatedArrival: new Date(Date.now() + 3600000).toISOString(), status: "Waiting" },
  { id: 3, name: "ONE Aquila", imoNumber: "9812441", cargoType: "Container", estimatedArrival: new Date(Date.now() + 7200000).toISOString(), status: "Inbound" },
];

const MOCK_BERTHS: ApiBerth[] = [
  { id: 1, name: "Berth A1 - Container Terminal", maxDraftMeters: 14.5, isAvailable: false },
  { id: 2, name: "Berth A2 - Container Terminal", maxDraftMeters: 14.5, isAvailable: true },
  { id: 3, name: "Berth B1 - Bulk Terminal", maxDraftMeters: 12.0, isAvailable: true },
];

const MOCK_ASSIGNMENTS: ApiAssignment[] = [
  { id: 1, shipId: 1, berthId: 1, startsAt: new Date().toISOString(), status: "Active" },
];

export const octopusApi = {
  async getShips(): Promise<ApiShip[]> {
    const data = await fetchJson<ApiShip[]>("/ships");
    return data ?? MOCK_SHIPS;
  },

  async getBerths(): Promise<ApiBerth[]> {
    const data = await fetchJson<ApiBerth[]>("/berths");
    return data ?? MOCK_BERTHS;
  },

  async getAssignments(): Promise<ApiAssignment[]> {
    const data = await fetchJson<ApiAssignment[]>("/assignments");
    return data ?? MOCK_ASSIGNMENTS;
  },

  async getSystemState(): Promise<ApiSystemState | null> {
    const data = await fetchJson<ApiSystemState>("/system/state");
    return data ?? {
      environment: "Development (Offline / Fallback)",
      serverTimeUtc: new Date().toISOString(),
      shipCount: MOCK_SHIPS.length,
      berthCount: MOCK_BERTHS.length,
      activeAssignmentCount: MOCK_ASSIGNMENTS.length,
    };
  },
};
