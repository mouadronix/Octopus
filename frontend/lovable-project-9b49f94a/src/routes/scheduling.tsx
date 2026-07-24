import { createFileRoute } from "@tanstack/react-router";
import { AppShell, Panel } from "@/components/app-shell";
import { ScheduleTimeline } from "@/components/dashboard/schedule-timeline";
import { CalendarClock, Clock, Users } from "lucide-react";
import { cn } from "@/lib/utils";

export const Route = createFileRoute("/scheduling")({
  head: () => ({
    meta: [
      { title: "Scheduling · Octopus Port Operations" },
      {
        name: "description",
        content:
          "Berth allocation planner and shift scheduling for pilots, tugs and stevedore crews across every quay.",
      },
      { property: "og:title", content: "Scheduling · Octopus Port Operations" },
      { property: "og:description", content: "Berth planner and crew scheduling for port operations." },
    ],
  }),
  component: SchedulingPage,
});

type Slot = {
  time: string;
  vessel: string;
  operation: "Pilot in" | "Tug assist" | "Cargo ops" | "Bunkering" | "Departure";
  crew: string;
  status: "confirmed" | "pending" | "conflict";
};

const slots: Slot[] = [
  { time: "13:40", vessel: "ONE Aquila",        operation: "Pilot in",   crew: "Pilot Rossi",     status: "confirmed" },
  { time: "14:00", vessel: "ONE Aquila",        operation: "Tug assist", crew: "Tug Ausonia",     status: "confirmed" },
  { time: "14:30", vessel: "MSC Ambra",         operation: "Cargo ops",  crew: "Gang A · 12 pax", status: "confirmed" },
  { time: "15:20", vessel: "Maersk Genoa",      operation: "Bunkering",  crew: "Bunker Boat 3",   status: "pending" },
  { time: "16:00", vessel: "COSCO Riviera",     operation: "Pilot in",   crew: "Pilot Conte",     status: "conflict" },
  { time: "17:15", vessel: "Grimaldi Eurocargo",operation: "Cargo ops",  crew: "Gang C · 8 pax",  status: "confirmed" },
  { time: "18:40", vessel: "CMA CGM Ligure",    operation: "Departure",  crew: "Pilot Rossi",     status: "pending" },
];

const statusStyles: Record<Slot["status"], string> = {
  confirmed: "bg-[color:var(--signal-ok)]/10 text-[color:var(--signal-ok)]",
  pending:   "bg-[color:var(--signal-warn)]/15 text-[color:var(--signal-warn)]",
  conflict:  "bg-[color:var(--signal-crit)]/10 text-[color:var(--signal-crit)]",
};

function SchedulingPage() {
  return (
    <AppShell
      eyebrow="Planner"
      title="Scheduling"
      description="Berth allocation, pilot rotations, tug assignments and stevedore shifts — all in one timeline."
      actions={
        <>
          <button className="rounded-md border border-border bg-card px-3 py-1.5 font-medium text-foreground transition hover:bg-accent">
            Print plan
          </button>
          <button className="rounded-md bg-primary px-3 py-1.5 font-medium text-primary-foreground transition hover:bg-primary/90">
            New slot
          </button>
        </>
      }
    >
      <div className="grid grid-cols-2 gap-3 lg:grid-cols-4">
        <div className="surface-panel p-4">
          <div className="flex items-center justify-between">
            <span className="text-[11px] uppercase tracking-[0.14em] text-muted-foreground">Slots today</span>
            <CalendarClock className="h-3.5 w-3.5 text-muted-foreground" />
          </div>
          <div className="mt-1 font-display text-2xl font-semibold text-foreground">63</div>
          <div className="mt-1 text-[11px] text-muted-foreground">48 confirmed · 12 pending · 3 conflict</div>
        </div>
        <div className="surface-panel p-4">
          <div className="flex items-center justify-between">
            <span className="text-[11px] uppercase tracking-[0.14em] text-muted-foreground">Avg. turnaround</span>
            <Clock className="h-3.5 w-3.5 text-muted-foreground" />
          </div>
          <div className="mt-1 font-display text-2xl font-semibold text-foreground">18.4h</div>
          <div className="mt-1 text-[11px] text-muted-foreground">−1.2h vs. last week</div>
        </div>
        <div className="surface-panel p-4">
          <div className="flex items-center justify-between">
            <span className="text-[11px] uppercase tracking-[0.14em] text-muted-foreground">Pilots on shift</span>
            <Users className="h-3.5 w-3.5 text-muted-foreground" />
          </div>
          <div className="mt-1 font-display text-2xl font-semibold text-foreground">6 / 8</div>
          <div className="mt-1 text-[11px] text-muted-foreground">2 on standby</div>
        </div>
        <div className="surface-panel p-4">
          <div className="flex items-center justify-between">
            <span className="text-[11px] uppercase tracking-[0.14em] text-muted-foreground">Gang deployment</span>
            <Users className="h-3.5 w-3.5 text-muted-foreground" />
          </div>
          <div className="mt-1 font-display text-2xl font-semibold text-foreground">84%</div>
          <div className="mt-1 text-[11px] text-muted-foreground">stevedores allocated</div>
        </div>
      </div>

      <ScheduleTimeline />

      <Panel title="Next operations" subtitle="Pilot, tug and cargo slots · next 6 hours">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs">
            <thead className="text-[10px] uppercase tracking-[0.14em] text-muted-foreground">
              <tr className="border-b border-border">
                <th className="px-4 py-2 font-medium">Time</th>
                <th className="px-4 py-2 font-medium">Vessel</th>
                <th className="px-4 py-2 font-medium">Operation</th>
                <th className="px-4 py-2 font-medium">Crew / Asset</th>
                <th className="px-4 py-2 font-medium">Status</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {slots.map((s, i) => (
                <tr key={i} className="transition hover:bg-muted/40">
                  <td className="px-4 py-2.5 font-mono text-foreground">{s.time}</td>
                  <td className="px-4 py-2.5 font-medium text-foreground">{s.vessel}</td>
                  <td className="px-4 py-2.5 text-muted-foreground">{s.operation}</td>
                  <td className="px-4 py-2.5 text-muted-foreground">{s.crew}</td>
                  <td className="px-4 py-2.5">
                    <span className={cn("inline-flex rounded-full px-2 py-0.5 text-[10px] font-medium capitalize", statusStyles[s.status])}>
                      {s.status}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </Panel>
    </AppShell>
  );
}
