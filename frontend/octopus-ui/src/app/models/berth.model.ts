import { ShipSize } from './ship.model';

export interface Berth {
  id: number;
  name: string;
  size: ShipSize;
}

export interface CreateBerthRequest {
  name: string;
  size: ShipSize;
}
