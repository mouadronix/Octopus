import { createFileRoute } from "@tanstack/react-router";
import { AppShell, Panel } from "@/components/app-shell";
import { cn } from "@/lib/utils";
import { Anchor } from "lucide-react";

export const Route = createFileRoute("/berths")({
  head: () => ({
    meta: [
      { title: "Berths · Octopus Port Operations" },
      {
        name: "description",
        content:
          "Quays and berths across the port: LOA capacity, draft, current occupancy, utilization history and maintenance windows.",
      },
      { property: "og:title", content: "Berths · Octopus Port Operations" },
      {
        property: "og:description",
        content: "Quays, berths, capacity and utilization at the Port of Genoa.",
      },
    ],
  }),
  component: BerthsPage,
});

type Berth = {
  id: string;
  quay: "A" | "B" | "C" | "P";
  length: number;
  draft: number;
  vessel?: string;
  status: "occupied" | "arriving" | "free" | "maintenance";
  utilization: number;
  eta?: string;
};

const berths: Berth[] = [
  { id: "A-01", quay: "A", length: 320, draft: 14.5, vessel: "MSC Ambra",       status: "occupied",    utilization: 92, eta: "—" },
  { id: "A-02", quay: "A", length: 340, draft: 15.0, vessel: "CMA CGM Ligure",  status: "occupied",    utilization: 88 },
  { id: "A-03", quay: "A", length: 280, draft: 12.5,                            status: "free",        utilization: 62 },
  { id: "A-04", quay: "A", length: 366, draft: 16.5, vessel: "Maersk Genoa",    status: "occupied",    utilization: 94 },
  { id: "B-01", quay: "B", length: 300, draft: 13.5, vessel: "ONE Aquila",      status: "arriving",    utilization: 74, eta: "14:20" },
  { id: "B-02", quay: "B", length: 260, draft: 12.0,                            status: "maintenance", utilization: 0 },
  { id: "B-03", quay: "B", length: 400, draft: 17.0, vessel: "Evergreen Elba",  status: "occupied",    utilization: 96 },
  { id: "B-04", quay: "B", length: 240, draft: 11.5,                            status: "free",        utilization: 48 },
  { id: "C-01", quay: "C", length: 335, draft: 15.0, vessel: "Hapag Portofino", status: "occupied",    utilization: 81 },
  { id: "C-02", quay: "C", length: 350, draft: 15.5, vessel: "COSCO Riviera",   status: "arriving",    utilization: 79, eta: "16:05" },
  { id: "C-03", quay: "C", length: 220, draft: 10.5,                            status: "free",        utilization: 55 },
  { id: "C-04", quay: "C", length: 310, draft: 14.0, vessel: "ZIM Tigullio",    status: "occupied",    utilization: 86 },
];

const statusMeta: Record<
  Berth["status"],
  { label: string; bg: string; fg: string; dot: string }
> = {
  occupied:    { label: "Occupied",    bg: "bg-[color:var(--navy-700)]/10",       fg: "text-[color:var(--navy-900)]",    dot: "bg-[color:var(--navy-700)]" },
  arriving:    { label: "Arriving",    bg: "bg-[color:var(--signal-info)]/10",    fg: "text-[color:var(--signal-info)]", dot: "bg-[color:var(--signal-info)]" },
  free:        { label: "Available",   bg: "bg-[color:var(--signal-ok)]/10",      fg: "text-[color:var(--signal-ok)]",   dot: "bg-[color:var(--signal-ok)]" },
  maintenance: { label: "Maintenance", bg: "bg-[color:var(--signal-warn)]/15",    fg: "text-[color:var(--signal-warn)]", dot: "bg-[color:var(--signal-warn)]" },
};

function BerthsPage() {
  const quays = ["A", "B", "C"] as const;

  return (
    <AppShell
      eyebrow="Infrastructure"
      title="Berths & Quays"
      description="Capacity, draft, live occupancy and maintenance status across every berth of the port."
      actions={
        <>
          <button className="rounded-md border border-border bg-card px-3 py-1.5 font-medium text-foreground transition hover:bg-accent">
            Export layout
          </button>
          <button className="rounded-md bg-primary px-3 py-1.5 font-medium text-primary-foreground transition hover:bg-primary/90">
            Plan maintenance
          </button>
        </>
      }
    >
      <div className="grid grid-cols-2 gap-3 lg:grid-cols-4">
        <div className="surface-panel p-4">
          <div className="text-[11px] uppercase tracking-[0.14em] text-muted-foreground">Total berths</div>
          <div className="mt-1 font-display text-2xl font-semibold text-foreground">{berths.length}</div>
          <div className="mt-1 text-[11px] text-muted-foreground">across 3 quays</div>
        </div>
        <div className="surface-panel p-4">
          <div className="text-[11px] uppercase tracking-[0.14em] text-muted-foreground">Occupancy</div>
          <div className="mt-1 font-display text-2xl font-semibold text-foreground">78%</div>
          <div className="mt-1 text-[11px] text-muted-foreground">rolling 24h</div>
        </div>
        <div className="surface-panel p-4">
          <div className="text-[11px] uppercase tracking-[0.14em] text-muted-foreground">Max LOA</div>
          <div className="mt-1 font-display text-2xl font-semibold text-foreground">400m</div>
          <div className="mt-1 text-[11px] text-muted-foreground">B-03 · deep-water</div>
        </div>
        <div className="surface-panel p-4">
          <div className="text-[11px] uppercase tracking-[0.14em] text-muted-foreground">Maintenance</div>
          <div className="mt-1 font-display text-2xl font-semibold text-[color:var(--signal-warn)]">1</div>
          <div className="mt-1 text-[11px] text-muted-foreground">B-02 · fender repair</div>
        </div>
      </div>

      {quays.map((q) => (
        <Panel
          key={q}
          title={`Quay ${q}`}
          subtitle={`${berths.filter((b) => b.quay === q).length} berths · deep-water access`}
          actions={
            <span className="font-mono text-[10px] uppercase tracking-[0.14em] text-muted-foreground">
              North terminal · Genoa
            </span>
          }
        >
          <div className="grid-bg p-4">
            <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4">
              {berths.filter((b) => b.quay === q).map((b) => {
                const meta = statusMeta[b.status];
                return (
                  <div
                    key={b.id}
                    className="rounded-md border border-border bg-card p-3 transition hover:shadow-sm"
                  >
                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-2">
                        <div className="grid h-7 w-7 place-items-center rounded-md bg-[color:var(--navy-50)] text-[color:var(--navy-800)]">
                          <Anchor className="h-3.5 w-3.5" />
                        </div>
                        <div className="font-mono text-xs font-medium text-foreground">{b.id}</div>
                      </div>
                      <span className={cn("inline-flex items-center gap-1 rounded-full px-1.5 py-0.5 text-[10px] font-medium", meta.bg, meta.fg)}>
                        <span className={cn("h-1.5 w-1.5 rounded-full", meta.dot)} />
                        {meta.label}
                      </span>
                    </div>

                    <div className="mt-2 min-h-[36px]">
                      {b.vessel ? (
                        <div className="font-display text-sm font-semibold text-foreground">{b.vessel}</div>
                      ) : (
                        <div className="text-xs italic text-muted-foreground">
                          {b.status === "maintenance" ? "Scheduled maintenance" : "No vessel scheduled"}
                        </div>
                      )}
                      <div className="mt-0.5 flex flex-wrap gap-x-2 font-mono text-[10px] text-muted-foreground">
                        <span>LOA {b.length}m</span>
                        <span>· draft {b.draft}m</span>
                        {b.eta && <span>· ETA {b.eta}</span>}
                      </div>
                    </div>

                    <div className="mt-2">
                      <div className="flex items-center justify-between font-mono text-[10px] text-muted-foreground">
                        <span>Utilization 7d</span>
                        <span>{b.utilization}%</span>
                      </div>
                      <div className="mt-1 h-1 overflow-hidden rounded-full bg-muted">
                        <div
                          className={cn("h-full rounded-full", meta.dot)}
                          style={{ width: `${b.utilization}%` }}
                        />
                      </div>
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        </Panel>
      ))}
    </AppShell>
  );
}
