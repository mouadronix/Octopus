import { createFileRoute } from "@tanstack/react-router";
import { AppShell, Panel } from "@/components/app-shell";
import { UserPlus, Search } from "lucide-react";
import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";

export const Route = createFileRoute("/operators")({
  head: () => ({
    meta: [
      { title: "Operators · Octopus Port Operations" },
      {
        name: "description",
        content:
          "Team roster: pilots, tugmasters, dispatchers and stevedore leads with certifications, shifts and status.",
      },
      { property: "og:title", content: "Operators · Octopus Port Operations" },
      { property: "og:description", content: "Roster of pilots, tug crews and dispatchers." },
    ],
  }),
  component: OperatorsPage,
});

type Operator = {
  id: string;
  name: string;
  role: "Pilot" | "Tugmaster" | "Dispatcher" | "Stevedore Lead" | "Harbour Master";
  cert: string;
  shift: "Morning" | "Afternoon" | "Night" | "On call";
  status: "On duty" | "Standby" | "Off" | "On call";
  ops: number;
};

const roster: Operator[] = [
  { id: "P-014", name: "Marco Rossi",    role: "Pilot",           cert: "Class A · Deep-water", shift: "Afternoon", status: "On duty",  ops: 412 },
  { id: "P-021", name: "Elena Conte",    role: "Pilot",           cert: "Class A · Deep-water", shift: "Afternoon", status: "On duty",  ops: 366 },
  { id: "P-032", name: "Alessia Ferri",  role: "Pilot",           cert: "Class B",              shift: "Night",     status: "Standby",  ops: 128 },
  { id: "T-007", name: "Giovanni Bosco", role: "Tugmaster",       cert: "Ausonia · 65t bollard",shift: "Afternoon", status: "On duty",  ops: 890 },
  { id: "T-011", name: "Paolo Marino",   role: "Tugmaster",       cert: "Sirius · 75t bollard", shift: "Night",     status: "Standby",  ops: 640 },
  { id: "D-003", name: "Chiara De Luca", role: "Dispatcher",      cert: "Level 3 · VTS",        shift: "Morning",   status: "Off",      ops: 1204 },
  { id: "D-005", name: "Luca Bianchi",   role: "Dispatcher",      cert: "Level 3 · VTS",        shift: "Afternoon", status: "On duty",  ops: 940 },
  { id: "S-018", name: "Fatima Haddad",  role: "Stevedore Lead",  cert: "Gang A · 12 pax",      shift: "Afternoon", status: "On duty",  ops: 512 },
  { id: "S-024", name: "Roberto Greco",  role: "Stevedore Lead",  cert: "Gang C · 8 pax",       shift: "Night",     status: "On call",  ops: 388 },
  { id: "H-001", name: "Comm. Sanna",    role: "Harbour Master",  cert: "Port Authority",       shift: "On call",   status: "On duty",  ops: 3021 },
];

const statusStyles: Record<Operator["status"], string> = {
  "On duty": "bg-[color:var(--signal-ok)]/10 text-[color:var(--signal-ok)]",
  "Standby": "bg-[color:var(--signal-warn)]/15 text-[color:var(--signal-warn)]",
  "Off":     "bg-muted text-muted-foreground",
  "On call": "bg-[color:var(--signal-info)]/10 text-[color:var(--signal-info)]",
};

function initials(n: string) {
  return n
    .split(" ")
    .map((p) => p[0])
    .slice(0, 2)
    .join("")
    .toUpperCase();
}

function OperatorsPage() {
  return (
    <AppShell
      eyebrow="Workforce"
      title="Operators"
      description="Certified personnel across pilotage, tug fleet, dispatch and stevedoring — with live shift and duty status."
      actions={
        <>
          <button className="rounded-md border border-border bg-card px-3 py-1.5 font-medium text-foreground transition hover:bg-accent">
            Shift roster
          </button>
          <button className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3 py-1.5 font-medium text-primary-foreground transition hover:bg-primary/90">
            <UserPlus className="h-3.5 w-3.5" /> Add operator
          </button>
        </>
      }
    >
      <div className="grid grid-cols-2 gap-3 lg:grid-cols-4">
        <div className="surface-panel p-4">
          <div className="text-[11px] uppercase tracking-[0.14em] text-muted-foreground">Total staff</div>
          <div className="mt-1 font-display text-2xl font-semibold text-foreground">84</div>
          <div className="mt-1 text-[11px] text-muted-foreground">certified & active</div>
        </div>
        <div className="surface-panel p-4">
          <div className="text-[11px] uppercase tracking-[0.14em] text-muted-foreground">On duty now</div>
          <div className="mt-1 font-display text-2xl font-semibold text-[color:var(--signal-ok)]">32</div>
          <div className="mt-1 text-[11px] text-muted-foreground">Shift B · Afternoon</div>
        </div>
        <div className="surface-panel p-4">
          <div className="text-[11px] uppercase tracking-[0.14em] text-muted-foreground">Pilots available</div>
          <div className="mt-1 font-display text-2xl font-semibold text-foreground">6 / 8</div>
          <div className="mt-1 text-[11px] text-muted-foreground">2 on standby</div>
        </div>
        <div className="surface-panel p-4">
          <div className="text-[11px] uppercase tracking-[0.14em] text-muted-foreground">Certifications expiring</div>
          <div className="mt-1 font-display text-2xl font-semibold text-[color:var(--signal-warn)]">3</div>
          <div className="mt-1 text-[11px] text-muted-foreground">within 30 days</div>
        </div>
      </div>

      <Panel
        title="Roster"
        subtitle="All operators · sortable"
        actions={
          <div className="relative">
            <Search className="pointer-events-none absolute left-2.5 top-1/2 h-3.5 w-3.5 -translate-y-1/2 text-muted-foreground" />
            <Input placeholder="Name, role, ID…" className="h-8 w-56 pl-8 font-mono text-xs" />
          </div>
        }
      >
        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs">
            <thead className="text-[10px] uppercase tracking-[0.14em] text-muted-foreground">
              <tr className="border-b border-border">
                <th className="px-4 py-2 font-medium">Operator</th>
                <th className="px-4 py-2 font-medium">Role</th>
                <th className="px-4 py-2 font-medium">Certification</th>
                <th className="px-4 py-2 font-medium">Shift</th>
                <th className="px-4 py-2 font-medium">Ops (12m)</th>
                <th className="px-4 py-2 font-medium">Status</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {roster.map((o) => (
                <tr key={o.id} className="transition hover:bg-muted/40">
                  <td className="px-4 py-2.5">
                    <div className="flex items-center gap-2.5">
                      <div className="grid h-8 w-8 place-items-center rounded-full bg-[color:var(--navy-800)] text-[10px] font-semibold text-white">
                        {initials(o.name)}
                      </div>
                      <div className="leading-tight">
                        <div className="font-medium text-foreground">{o.name}</div>
                        <div className="font-mono text-[10px] text-muted-foreground">{o.id}</div>
                      </div>
                    </div>
                  </td>
                  <td className="px-4 py-2.5 text-muted-foreground">{o.role}</td>
                  <td className="px-4 py-2.5 text-muted-foreground">{o.cert}</td>
                  <td className="px-4 py-2.5 font-mono text-foreground">{o.shift}</td>
                  <td className="px-4 py-2.5 font-mono text-foreground">{o.ops}</td>
                  <td className="px-4 py-2.5">
                    <span className={cn("inline-flex rounded-full px-2 py-0.5 text-[10px] font-medium", statusStyles[o.status])}>
                      {o.status}
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
