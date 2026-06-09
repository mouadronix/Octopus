export interface Assignment {
  id: number;
  shipId: number;
  dockId: number;
  startDay: number;
  endDay: number;
}

export interface CreateAssignmentRequest {
  shipId: number;
  dockId: number;
}
