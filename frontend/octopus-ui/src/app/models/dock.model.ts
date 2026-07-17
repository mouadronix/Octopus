import { ShipSize } from './ship.model';
import { Assignment } from './assignment.model';

export interface Dock {
  id: number;
  name: string;
  size: ShipSize;
  assignments?: Assignment[];
}

export interface CreateDockRequest {
  name: string;
  size: ShipSize;
}
