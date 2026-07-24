import { createFileRoute } from "@tanstack/react-router";
import { Ship, Filter, Download, Search } from "lucide-react";
import { AppShell, Panel } from "@/components/app-shell";
import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";

export const Route = createFileRoute("/vessels")({
  head: () => ({
    meta: [
      { title: "Vessels · Octopus Port Operations" },
      {
        name: "description",
        content:
          "Fleet registry and live status of every vessel expected, at anchor or berthed at the Port of Genoa.",
      },
      { property: "og:title", content: "Vessels · Octopus Port Operations" },
      { property: "og:description", content: "Registry and live status of port vessels." },
    ],
  }),
  component: VesselsPage,
});

type Vessel = {
  imo: string;
  name: string;
  flag: string;
  type: "Container" | "Bulk" | "Tanker" | "Ro-Ro" | "Passenger";
  loa: number;
  dwt: number;
  eta: string;
  berth: string;
  status: "At berth" | "Anchored" | "Inbound" | "Departed" | "Delayed";
  agent: string;
};

const vessels: Vessel[] = [
  { imo: "9812441", name: "ONE Aquila",        flag: "JP", type: "Container", loa: 330, dwt: 140000, eta: "14:20", berth: "B-01", status: "Inbound",  agent: "Marittima SPA" },
  { imo: "9755320", name: "COSCO Riviera",     flag: "CN", type: "Container", loa: 350, dwt: 152000, eta: "16:05", berth: "C-02", status: "Inbound",  agent: "GNV Agencies" },
  { imo: "9702118", name: "Grimaldi Eurocargo",flag: "IT", type: "Ro-Ro",     loa: 240, dwt: 30000,  eta: "17:40", berth: "A-03", status: "Inbound",  agent: "Grimaldi Group" },
  { imo: "9666204", name: "Nordic Amber",      flag: "NO", type: "Tanker",    loa: 275, dwt: 115000, eta: "19:55", berth: "—",    status: "Delayed",  agent: "Nordic Shipping" },
  { imo: "9598712", name: "Atlantic Ore",      flag: "LR", type: "Bulk",      loa: 292, dwt: 180000, eta: "22:00", berth: "—",    status: "Anchored", agent: "OceanBulk Ltd" },
  { imo: "9843301", name: "MSC Ligura",        flag: "PA", type: "Container", loa: 366, dwt: 165000, eta: "04:10", berth: "A-04", status: "At berth", agent: "MSC Italia" },
  { imo: "9701221", name: "CMA CGM Ligure",    flag: "FR", type: "Container", loa: 340, dwt: 148000, eta: "—",     berth: "A-02", status: "At berth", agent: "CMA CGM" },
  { imo: "9588220", name: "Evergreen Elba",    flag: "TW", type: "Container", loa: 400, dwt: 210000, eta: "—",     berth: "B-03", status: "At berth", agent: "Evergreen" },
  { imo: "9420115", name: "Costa Firenze",     flag: "IT", type: "Passenger", loa: 323, dwt: 55000,  eta: "07:30", berth: "P-01", status: "Departed", agent: "Costa Crociere" },
];

const statusStyles: Record<Vessel["status"], string> = {
  "At berth": "bg-[color:var(--navy-700)]/10 text-[color:var(--navy-900)]",
  "Anchored": "bg-muted text-muted-foreground",
  "Inbound":  "bg-[color:var(--signal-info)]/10 text-[color:var(--signal-info)]",
  "Departed": "bg-[color:var(--signal-ok)]/10 text-[color:var(--signal-ok)]",
  "Delayed":  "bg-[color:var(--signal-crit)]/10 text-[color:var(--signal-crit)]",
};

const chips = ["All", "At berth", "Inbound", "Anchored", "Delayed", "Departed"] as const;

function StatCell({ label, value, hint }: { label: string; value: string; hint?: string }) {
  return (
    <div className="surface-panel p-4">
      <div className="text-[11px] font-medium uppercase tracking-[0.14em] text-muted-foreground">
        {label}
      </div>
      <div className="mt-1 font-display text-2xl font-semibold tracking-tight text-foreground">
        {value}
      </div>
      {hint && <div className="mt-1 text-[11px] text-muted-foreground">{hint}</div>}
    </div>
  );
}

function VesselsPage() {
  return (
    <AppShell
      eyebrow="Fleet · Registry"
      title="Vessels"
      description="Every vessel expected, anchored, berthed or departed within the last 24 hours."
      actions={
        <>
          <button className="inline-flex items-center gap-1.5 rounded-md border border-border bg-card px-3 py-1.5 font-medium text-foreground transition hover:bg-accent">
            <Download className="h-3.5 w-3.5" /> CSV
          </button>
          <button className="rounded-md bg-primary px-3 py-1.5 font-medium text-primary-foreground transition hover:bg-primary/90">
            Register vessel
          </button>
        </>
      }
    >
      <div className="grid grid-cols-2 gap-3 lg:grid-cols-4">
        <StatCell label="Registered" value="128" hint="fleet in system" />
        <StatCell label="At berth" value="42" hint="all quays" />
        <StatCell label="Inbound 24h" value="17" hint="AIS-tracked" />
        <StatCell label="Anchored" value="6" hint="waiting slot" />
      </div>

      <Panel
        title="Fleet"
        subtitle="Filter, search and drill down"
        actions={
          <div className="flex items-center gap-2">
            <div className="relative">
              <Search className="pointer-events-none absolute left-2.5 top-1/2 h-3.5 w-3.5 -translate-y-1/2 text-muted-foreground" />
              <Input placeholder="IMO, name or flag…" className="h-8 w-56 pl-8 font-mono text-xs" />
            </div>
            <button className="inline-flex items-center gap-1.5 rounded-md border border-border bg-card px-2.5 py-1 text-[11px] font-medium text-foreground hover:bg-accent">
              <Filter className="h-3 w-3" /> Filters
            </button>
          </div>
        }
      >
        <div className="flex flex-wrap gap-1.5 border-b border-border px-4 py-2.5">
          {chips.map((c, i) => (
            <button
              key={c}
              className={cn(
                "rounded-full px-2.5 py-1 text-[11px] font-medium transition",
                i === 0
                  ? "bg-primary text-primary-foreground"
                  : "bg-muted text-muted-foreground hover:bg-accent",
              )}
            >
              {c}
            </button>
          ))}
        </div>

        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs">
            <thead className="text-[10px] uppercase tracking-[0.14em] text-muted-foreground">
              <tr className="border-b border-border">
                <th className="px-4 py-2 font-medium">Vessel</th>
                <th className="px-4 py-2 font-medium">Type</th>
                <th className="px-4 py-2 font-medium">LOA / DWT</th>
                <th className="px-4 py-2 font-medium">Agent</th>
                <th className="px-4 py-2 font-medium">Berth</th>
                <th className="px-4 py-2 font-medium">ETA</th>
                <th className="px-4 py-2 font-medium">Status</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {vessels.map((v) => (
                <tr key={v.imo} className="transition hover:bg-muted/40">
                  <td className="px-4 py-2.5">
                    <div className="flex items-center gap-2.5">
                      <div className="grid h-7 w-7 place-items-center rounded-md bg-[color:var(--navy-50)] text-[color:var(--navy-800)]">
                        <Ship className="h-3.5 w-3.5" />
                      </div>
                      <div className="leading-tight">
                        <div className="font-medium text-foreground">{v.name}</div>
                        <div className="font-mono text-[10px] text-muted-foreground">
                          IMO {v.imo} · {v.flag}
                        </div>
                      </div>
                    </div>
                  </td>
                  <td className="px-4 py-2.5 text-muted-foreground">{v.type}</td>
                  <td className="px-4 py-2.5 font-mono text-muted-foreground">
                    {v.loa}m · {(v.dwt / 1000).toFixed(0)}k dwt
                  </td>
                  <td className="px-4 py-2.5 text-muted-foreground">{v.agent}</td>
                  <td className="px-4 py-2.5 font-mono text-foreground">{v.berth}</td>
                  <td className="px-4 py-2.5 font-mono text-foreground">{v.eta}</td>
                  <td className="px-4 py-2.5">
                    <span
                      className={cn(
                        "inline-flex rounded-full px-2 py-0.5 text-[10px] font-medium",
                        statusStyles[v.status],
                      )}
                    >
                      {v.status}
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
