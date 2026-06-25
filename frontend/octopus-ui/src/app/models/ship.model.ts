export type ShipSize = 'XL' | 'L' | 'M' | 'S' | number;
export type ShipStatus = 'Pending' | 'Assigned' | 'Departed' | number;

export interface Ship {
  id: number;
  name: string;
  notes: string;
  size: ShipSize;
  status: ShipStatus;
  arrivalDay: number;
  duration: number;
  imageUrl: string;
  berthName?: string | null;
  assignmentId?: number | null;
  assignmentStartDay?: number | null;
  assignmentEndDay?: number | null;
}

export interface CreateShipRequest {
  name: string;
  notes?: string;
  size: Exclude<ShipSize, number>;
  arrivalDay: number;
  duration: number;
  imageUrl?: string;
}

export interface CompatibleBerth {
  dockId: number;
  dockName: string;
  size: string;
  startDay: number;
  available: boolean;
}

export interface SuggestionResponse {
  dockId: number;
  dockName: string;
  startDay: number;
  message: string;
  compatibleBerths: CompatibleBerth[];
}
