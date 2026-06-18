import { ShipSize } from './ship.model';
import { Assignment } from './assignment.model';

export interface Berth {
  id: number;
  name: string;
  size: ShipSize;
  assignments?: Assignment[];
}

export interface CreateBerthRequest {
  name: string;
  size: ShipSize;
}
