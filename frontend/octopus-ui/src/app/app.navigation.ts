export interface AppNavItem {
  label: string;
  path: string;
  description: string;
}

export const APP_NAVIGATION: AppNavItem[] = [
  {
    label: 'Home',
    path: '/home',
    description: 'BlueHarbor overview and navigation'
  },
  {
    label: 'Operator',
    path: '/operator',
    description: 'Ship registration and terminal operations'
  },
  {
    label: 'Scheduler',
    path: '/scheduler',
    description: 'Pending ships and berth planning'
  },
  {
    label: 'Ships',
    path: '/ships',
    description: 'Ship registry and statuses'
  },
  {
    label: 'Berths',
    path: '/berths',
    description: 'Berth availability and assignments'
  }
];
