export interface Assignment {
  id: number;
  shipId: number;
  berthId: number;
  startsAt: string;
  endsAt?: string;
  status: string;
}
