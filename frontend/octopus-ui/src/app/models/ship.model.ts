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
}

export interface CreateShipRequest {
  name: string;
  notes?: string;
}
