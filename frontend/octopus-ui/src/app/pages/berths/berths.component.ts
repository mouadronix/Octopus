import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { Assignment } from '../../models/assignment.model';
import { Dock } from '../../models/dock.model';
import { ShipSize, ShipStatus } from '../../models/ship.model';
import { AssignmentService } from '../../services/assignment.service';
import { DockService } from '../../services/dock.service';
import { SystemService } from '../../services/system.service';

type SizeFilter = 'All' | 'XL' | 'L' | 'M' | 'S';
type StatusFilter = 'All' | 'Assigned' | 'Pending' | 'Departed' | 'Available';

interface DockGroup {
  size: Exclude<SizeFilter, 'All'>;
  docks: Dock[];
}

@Component({
  selector: 'app-docks',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './berths.component.html',
  styleUrl: './berths.component.scss'
})
export class DocksComponent implements OnInit {
  readonly sizeOptions: SizeFilter[] = ['All', 'XL', 'L', 'M', 'S'];
  readonly statusOptions: StatusFilter[] = ['All', 'Assigned', 'Pending', 'Departed', 'Available'];
  readonly weekDays = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];
  // monthDays is now a dynamic getter below

  assignments: Assignment[] = [];
  docks: Dock[] = [];
  dockFilter = 'All';
  currentDay = 12;
  errorMessage = '';
  groupBy = 'Dock Size';
  isLoading = true;
  selectedStatus: StatusFilter = 'All';
  selectedSize: SizeFilter = 'All';
  viewDays = 7;

  constructor(
    private readonly assignmentService: AssignmentService,
    private readonly dockService: DockService,
    private readonly systemService: SystemService
  ) {}

  ngOnInit(): void {
    this.loadCalendar();
  }

  get days(): number[] {
    return Array.from({ length: this.viewDays }, (_unused, index) => this.currentDay + index);
  }

  get visibleDocks(): Dock[] {
    return this.docks
      .filter((dock) => this.selectedSize === 'All' || this.normalizeSize(dock.size) === this.selectedSize)
      .filter((dock) => this.dockFilter === 'All' || dock.name === this.dockFilter)
      .sort((left, right) => {
        const rankDelta = this.sizeRank(left.size) - this.sizeRank(right.size);
        return rankDelta || left.name.localeCompare(right.name, undefined, { numeric: true });
      });
  }

  get dockGroups(): DockGroup[] {
    if (this.groupBy === 'None') {
      return this.visibleDocks.length > 0
        ? [{ size: 'All Docks' as Exclude<SizeFilter, 'All'>, docks: this.visibleDocks }]
        : [];
    }
    if (this.groupBy === 'Status') {
      const groups: DockGroup[] = [];
      const occupied = this.visibleDocks.filter((d) => this.rowAssignments(d).length > 0);
      const available = this.visibleDocks.filter((d) => this.rowAssignments(d).length === 0);
      if (occupied.length) groups.push({ size: 'Occupied' as Exclude<SizeFilter, 'All'>, docks: occupied });
      if (available.length) groups.push({ size: 'Available' as Exclude<SizeFilter, 'All'>, docks: available });
      return groups;
    }
    // Default: group by dock size
    return (['XL', 'L', 'M', 'S'] as const)
      .map((size) => ({
        size,
        docks: this.visibleDocks.filter((dock) => this.normalizeSize(dock.size) === size)
      }))
      .filter((group) => group.docks.length > 0);
  }

  get visibleAssignments(): Assignment[] {
    const visibleDockIds = new Set(this.visibleDocks.map((dock) => dock.id));
    return this.assignments.filter((assignment) => {
      const status = this.normalizeStatus(assignment.ship?.status ?? 'Assigned');
      return visibleDockIds.has(assignment.dockId)
        && this.overlapsVisibleRange(assignment)
        && (this.selectedStatus === 'All' || this.selectedStatus === status);
    });
  }

  get totalAssignments(): number {
    return this.visibleAssignments.length;
  }

  get occupiedDays(): number {
    return this.visibleAssignments.reduce((total, assignment) => total + this.assignmentSpan(assignment), 0);
  }

  get availableDays(): number {
    return Math.max(this.visibleDocks.length * this.viewDays - this.occupiedDays, 0);
  }

  get utilization(): number {
    const totalDays = this.visibleDocks.length * this.viewDays;
    return totalDays === 0 ? 0 : Math.round((this.occupiedDays / totalDays) * 1000) / 10;
  }

  loadCalendar(): void {
    this.isLoading = true;
    this.errorMessage = '';

    forkJoin({
      assignments: this.assignmentService.getAssignments(),
      docks: this.dockService.getDocks(),
      state: this.systemService.getState()
    }).subscribe({
      next: ({ assignments, docks, state }) => {
        this.assignments = assignments;
        this.docks = docks;
        this.currentDay = state.currentDay || this.currentDay;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Unable to load planning calendar data from the server.';
        this.isLoading = false;
      }
    });
  }

  goToday(): void {
    this.systemService.getState().subscribe({
      next: (state) => {
        this.currentDay = state.currentDay || this.currentDay;
      }
    });
  }

  previousWeek(): void {
    this.currentDay = Math.max(1, this.currentDay - this.viewDays);
  }

  nextWeek(): void {
    this.currentDay += this.viewDays;
  }

  resetFilters(): void {
    this.selectedSize = 'All';
    this.dockFilter = 'All';
    this.selectedStatus = 'All';
  }

  exportCalendar(): void {
    const lines = [
      'BEGIN:VCALENDAR',
      'VERSION:2.0',
      'PRODID:-//OCTOPUS//Planning Calendar//EN',
      'CALSCALE:GREGORIAN',
      'METHOD:PUBLISH',
      `X-WR-CALNAME:${this.escapeIcsText(`OCTOPUS Planning Day ${this.days[0]}-${this.days[this.days.length - 1]}`)}`
    ];

    this.visibleAssignments.forEach((assignment) => {
      const dock = this.docks.find((item) => item.id === assignment.dockId);
      const status = this.getShipStatus(assignment).toUpperCase();
      const startDate = this.formatIcsDate(assignment.startDay);
      const endDate = this.formatIcsDate(assignment.endDay + 1);
      const summary = `${this.getShipName(assignment)} (${this.getShipSize(assignment)}) - ${dock?.name ?? 'Unassigned dock'}`;
      const description = [
        `Ship: ${this.getShipName(assignment)}`,
        `Dock: ${dock?.name ?? 'Unknown'}`,
        `Size: ${this.getShipSize(assignment)}`,
        `Status: ${this.getShipStatus(assignment)}`,
        `Simulation range: ${this.getRangeLabel(assignment)}`
      ].join('\\n');

      lines.push(
        'BEGIN:VEVENT',
        `UID:octopus-assignment-${assignment.id}@octopus.local`,
        `DTSTAMP:${this.formatIcsDateTime(new Date())}`,
        `DTSTART;VALUE=DATE:${startDate}`,
        `DTEND;VALUE=DATE:${endDate}`,
        `SUMMARY:${this.escapeIcsText(summary)}`,
        `DESCRIPTION:${this.escapeIcsText(description)}`,
        `LOCATION:${this.escapeIcsText(dock?.name ?? 'Unknown dock')}`,
        `STATUS:${status === 'DEPARTED' ? 'CANCELLED' : 'CONFIRMED'}`,
        'END:VEVENT'
      );
    });

    lines.push('END:VCALENDAR');

    const fileName = `octopus-planning-day-${this.days[0]}-${this.days[this.days.length - 1]}.ics`;
    this.downloadFile(fileName, lines.join('\r\n'), 'text/calendar;charset=utf-8');
  }

  rowAssignments(dock: Dock): Assignment[] {
    return this.visibleAssignments
      .filter((assignment) => assignment.dockId === dock.id)
      .sort((left, right) => left.startDay - right.startDay);
  }

  hasAssignments(dock: Dock): boolean {
    return this.rowAssignments(dock).length > 0;
  }

  getGridColumn(assignment: Assignment): string {
    const firstDay = this.days[0];
    const lastDay = this.days[this.days.length - 1];
    const start = Math.max(assignment.startDay, firstDay);
    const end = Math.min(assignment.endDay, lastDay);
    const column = start - firstDay + 1;
    const span = Math.max(end - start + 1, 1);
    return `${column} / span ${span}`;
  }

  getAssignmentClass(assignment: Assignment): string {
    const size = this.normalizeSize(assignment.ship?.size ?? 'S');
    const status = this.normalizeStatus(assignment.ship?.status ?? 'Assigned');
    return `${size.toLowerCase()} ${status.toLowerCase()}`;
  }

  getDockAssignmentsCount(dock: Dock): number {
    return this.rowAssignments(dock).length;
  }

  getMonthLabel(): string {
    return this.dayToDate(this.currentDay).toLocaleDateString('en-US', { month: 'long', year: 'numeric' });
  }

  getDayLabel(index: number): string {
    return this.weekDays[index % this.weekDays.length];
  }

  getShipName(assignment: Assignment): string {
    return assignment.ship?.name || `Ship #${assignment.shipId}`;
  }

  getShipSize(assignment: Assignment): string {
    return this.normalizeSize(assignment.ship?.size ?? 'S');
  }

  getShipStatus(assignment: Assignment): string {
    return this.normalizeStatus(assignment.ship?.status ?? 'Assigned');
  }

  getRangeLabel(assignment: Assignment): string {
    return `Day ${assignment.startDay} -> Day ${assignment.endDay}`;
  }

  trackByDockId(_index: number, dock: Dock): number {
    return dock.id;
  }

  trackByAssignmentId(_index: number, assignment: Assignment): number {
    return assignment.id;
  }

  trackByGroupSize(_index: number, group: DockGroup): string {
    return group.size;
  }

  normalizeSize(size: ShipSize | undefined): Exclude<SizeFilter, 'All'> {
    if (size === 0 || size === 'XL') {
      return 'XL';
    }
    if (size === 1 || size === 'L') {
      return 'L';
    }
    if (size === 2 || size === 'M') {
      return 'M';
    }
    return 'S';
  }

  get monthDays(): number[] {
    const date = this.dayToDate(this.currentDay);
    const year = date.getFullYear();
    const month = date.getMonth();
    const daysInMonth = new Date(year, month + 1, 0).getDate();
    return Array.from({ length: daysInMonth }, (_, i) => i + 1);
  }

  prevMonth(): void {
    const date = this.dayToDate(this.currentDay);
    date.setMonth(date.getMonth() - 1);
    const base = new Date(2025, 4, 1);
    this.currentDay = Math.max(1, Math.round((date.getTime() - base.getTime()) / 86400000) + 1);
  }

  nextMonth(): void {
    const date = this.dayToDate(this.currentDay);
    date.setMonth(date.getMonth() + 1);
    const base = new Date(2025, 4, 1);
    this.currentDay = Math.max(1, Math.round((date.getTime() - base.getTime()) / 86400000) + 1);
  }

  normalizeStatus(status: ShipStatus | undefined): Exclude<StatusFilter, 'All' | 'Available'> {
    if (status === 0 || status === 'Pending') {
      return 'Pending';
    }
    if (status === 2 || status === 'Departed') {
      return 'Departed';
    }
    return 'Assigned';
  }

  private overlapsVisibleRange(assignment: Assignment): boolean {
    const firstDay = this.days[0];
    const lastDay = this.days[this.days.length - 1];
    return assignment.startDay <= lastDay && assignment.endDay >= firstDay;
  }

  private assignmentSpan(assignment: Assignment): number {
    const firstDay = this.days[0];
    const lastDay = this.days[this.days.length - 1];
    const start = Math.max(assignment.startDay, firstDay);
    const end = Math.min(assignment.endDay, lastDay);
    return Math.max(end - start + 1, 0);
  }

  private sizeRank(size: ShipSize): number {
    return { XL: 0, L: 1, M: 2, S: 3 }[this.normalizeSize(size)];
  }

  private downloadFile(fileName: string, content: string, mimeType: string): void {
    const blob = new Blob([content], { type: mimeType });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    link.remove();
    window.setTimeout(() => URL.revokeObjectURL(url), 0);
  }

  private escapeIcsText(value: string): string {
    return value
      .replace(/\\/g, '\\\\')
      .replace(/\n/g, '\\n')
      .replace(/,/g, '\\,')
      .replace(/;/g, '\\;');
  }

  private formatIcsDate(day: number): string {
    const date = new Date(Date.UTC(2025, 4, day));
    return [
      date.getUTCFullYear(),
      String(date.getUTCMonth() + 1).padStart(2, '0'),
      String(date.getUTCDate()).padStart(2, '0')
    ].join('');
  }

  private formatIcsDateTime(date: Date): string {
    return [
      date.getUTCFullYear(),
      String(date.getUTCMonth() + 1).padStart(2, '0'),
      String(date.getUTCDate()).padStart(2, '0'),
      'T',
      String(date.getUTCHours()).padStart(2, '0'),
      String(date.getUTCMinutes()).padStart(2, '0'),
      String(date.getUTCSeconds()).padStart(2, '0'),
      'Z'
    ].join('');
  }

  private dayToDate(day: number): Date {
    return new Date(2025, 4, day);
  }
}
