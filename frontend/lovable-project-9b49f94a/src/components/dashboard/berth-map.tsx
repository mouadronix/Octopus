import { cn } from "@/lib/utils";

type BerthStatus = "occupied" | "arriving" | "free" | "maintenance";

type Berth = {
  id: string;
  vessel?: string;
  length: number; // meters
  used?: number;
  status: BerthStatus;
  eta?: string;
};

const berths: Berth[] = [
  { id: "A-01", vessel: "MSC Ambra",        length: 320, used: 298, status: "occupied" },
  { id: "A-02", vessel: "CMA CGM Ligure",   length: 340, used: 312, status: "occupied" },
  { id: "A-03", length: 280, status: "free" },
  { id: "A-04", vessel: "Maersk Genoa",     length: 366, used: 340, status: "occupied" },
  { id: "B-01", vessel: "ONE Aquila",       length: 300, status: "arriving", eta: "14:20" },
  { id: "B-02", length: 260, status: "maintenance" },
  { id: "B-03", vessel: "Evergreen Elba",   length: 400, used: 372, status: "occupied" },
  { id: "B-04", length: 240, status: "free" },
  { id: "C-01", vessel: "Hapag Portofino",  length: 335, used: 300, status: "occupied" },
  { id: "C-02", vessel: "COSCO Riviera",    length: 350, status: "arriving", eta: "16:05" },
  { id: "C-03", length: 220, status: "free" },
  { id: "C-04", vessel: "ZIM Tigullio",     length: 310, used: 288, status: "occupied" },
];

const statusMeta: Record<BerthStatus, { label: string; dot: string; ring: string; fg: string }> = {
  occupied:    { label: "Occupied",    dot: "bg-[color:var(--navy-700)]",       ring: "ring-[color:var(--navy-700)]/40",       fg: "text-[color:var(--navy-900)]" },
  arriving:    { label: "Arriving",    dot: "bg-[color:var(--signal-info)]",    ring: "ring-[color:var(--signal-info)]/40",    fg: "text-[color:var(--signal-info)]" },
  free:        { label: "Available",   dot: "bg-[color:var(--signal-ok)]",      ring: "ring-[color:var(--signal-ok)]/40",      fg: "text-[color:var(--signal-ok)]" },
  maintenance: { label: "Maintenance", dot: "bg-[color:var(--signal-warn)]",    ring: "ring-[color:var(--signal-warn)]/40",    fg: "text-[color:var(--signal-warn)]" },
};

export function BerthMap() {
  return (
    <section className="surface-panel flex flex-col">
      <header className="flex items-center justify-between border-b border-border px-4 py-3">
        <div>
          <h2 className="font-display text-sm font-semibold tracking-tight text-foreground">
            Berth Allocation
          </h2>
          <p className="text-[11px] text-muted-foreground">
            Quays A · B · C — live occupancy grid
          </p>
        </div>
        <div className="flex flex-wrap gap-3">
          {(Object.keys(statusMeta) as BerthStatus[]).map((s) => (
            <span key={s} className="inline-flex items-center gap-1.5 text-[11px] text-muted-foreground">
              <span className={cn("h-2 w-2 rounded-full", statusMeta[s].dot)} />
              {statusMeta[s].label}
            </span>
          ))}
        </div>
      </header>

      <div className="grid-bg relative flex-1 p-4">
        <div className="grid grid-cols-2 gap-3 md:grid-cols-3 xl:grid-cols-4">
          {berths.map((b) => {
            const meta = statusMeta[b.status];
            const pct = b.used && b.length ? Math.round((b.used / b.length) * 100) : 0;
            return (
              <div
                key={b.id}
                className={cn(
                  "group relative rounded-md border border-border bg-card p-3 ring-1 ring-inset ring-transparent transition",
                  "hover:ring-1 hover:shadow-sm",
                  meta.ring,
                )}
              >
                <div className="flex items-center justify-between">
                  <span className="font-mono text-[11px] font-medium text-muted-foreground">
                    BERTH {b.id}
                  </span>
                  <span className={cn("inline-flex items-center gap-1 text-[10px] uppercase tracking-wider", meta.fg)}>
                    <span className={cn("h-1.5 w-1.5 rounded-full", meta.dot)} />
                    {meta.label}
                  </span>
                </div>

                <div className="mt-2 min-h-[36px]">
                  {b.vessel ? (
                    <div className="font-display text-sm font-semibold text-foreground">
                      {b.vessel}
                    </div>
                  ) : (
                    <div className="text-xs italic text-muted-foreground">
                      {b.status === "maintenance" ? "Scheduled maintenance" : "No vessel scheduled"}
                    </div>
                  )}
                  <div className="mt-0.5 flex items-center gap-2 font-mono text-[10px] text-muted-foreground">
                    <span>LOA cap. {b.length}m</span>
                    {b.eta && <span>· ETA {b.eta}</span>}
                    {b.used && <span>· {pct}% used</span>}
                  </div>
                </div>

                {b.used && (
                  <div className="mt-2 h-1 overflow-hidden rounded-full bg-muted">
                    <div
                      className="h-full rounded-full bg-[color:var(--navy-700)]"
                      style={{ width: `${pct}%` }}
                    />
                  </div>
                )}
              </div>
            );
          })}
        </div>
      </div>
    </section>
  );
}
