import { Ship } from './ship.model';

export interface Assignment {
  id: number;
  shipId: number;
  dockId: number;
  startDay: number;
  endDay: number;
  ship?: Ship;
}

export interface CreateAssignmentRequest {
  shipId: number;
  dockId: number;
}
