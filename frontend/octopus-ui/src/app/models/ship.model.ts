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
  berthName?: string | null;
  assignmentId?: number | null;
  assignmentStartDay?: number | null;
  assignmentEndDay?: number | null;
  imageUrl?: string | null;
}

export interface CreateShipRequest {
  name: string;
  notes?: string;
  size: Exclude<ShipSize, number>;
  arrivalDay: number;
  duration: number;
  imageUrl?: string;
}
