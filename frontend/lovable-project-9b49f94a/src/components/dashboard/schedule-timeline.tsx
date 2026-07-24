import { cn } from "@/lib/utils";

const hours = Array.from({ length: 12 }, (_, i) => i + 8); // 08 → 19

type Block = {
  berth: string;
  vessel: string;
  start: number; // hour
  end: number;
  tone: "primary" | "info" | "warn" | "muted";
};

const rowsData: Block[] = [
  { berth: "A-01", vessel: "MSC Ambra",         start: 8,  end: 13, tone: "primary" },
  { berth: "A-01", vessel: "Maersk Genoa",      start: 14, end: 19, tone: "primary" },
  { berth: "A-02", vessel: "CMA CGM Ligure",    start: 9,  end: 16, tone: "primary" },
  { berth: "B-01", vessel: "ONE Aquila",        start: 14, end: 18, tone: "info" },
  { berth: "B-02", vessel: "Maintenance",       start: 8,  end: 12, tone: "warn" },
  { berth: "B-03", vessel: "Evergreen Elba",    start: 10, end: 17, tone: "primary" },
  { berth: "C-01", vessel: "Hapag Portofino",   start: 8,  end: 14, tone: "primary" },
  { berth: "C-02", vessel: "COSCO Riviera",     start: 16, end: 20, tone: "info" },
  { berth: "C-04", vessel: "ZIM Tigullio",      start: 11, end: 18, tone: "primary" },
];

const toneStyles: Record<Block["tone"], string> = {
  primary: "bg-[color:var(--navy-800)] text-white",
  info:    "bg-[color:var(--signal-info)]/85 text-white",
  warn:    "bg-[color:var(--signal-warn)]/25 text-[color:var(--navy-900)] border border-[color:var(--signal-warn)]/60",
  muted:   "bg-muted text-muted-foreground",
};

export function ScheduleTimeline() {
  const berths = Array.from(new Set(rowsData.map((b) => b.berth))).sort();
  const start = hours[0];
  const end = hours[hours.length - 1] + 1;
  const span = end - start;

  return (
    <section className="surface-panel">
      <header className="flex items-center justify-between border-b border-border px-4 py-3">
        <div>
          <h2 className="font-display text-sm font-semibold tracking-tight text-foreground">
            Today's Berth Schedule
          </h2>
          <p className="text-[11px] text-muted-foreground">Gantt view · 08:00 – 20:00 CET</p>
        </div>
        <div className="flex gap-1 rounded-md border border-border bg-card p-0.5 text-[11px]">
          {["Day", "Week", "Month"].map((t, i) => (
            <button
              key={t}
              className={cn(
                "rounded px-2 py-1 font-medium transition",
                i === 0
                  ? "bg-primary text-primary-foreground"
                  : "text-muted-foreground hover:bg-accent",
              )}
            >
              {t}
            </button>
          ))}
        </div>
      </header>

      <div className="overflow-x-auto p-4">
        <div className="min-w-[720px]">
          {/* Hour ruler */}
          <div className="ml-16 grid" style={{ gridTemplateColumns: `repeat(${span}, minmax(0,1fr))` }}>
            {hours.map((h) => (
              <div
                key={h}
                className="border-l border-border/70 pl-1 font-mono text-[10px] text-muted-foreground"
              >
                {String(h).padStart(2, "0")}:00
              </div>
            ))}
          </div>

          {/* Rows */}
          <div className="mt-2 space-y-1.5">
            {berths.map((b) => {
              const blocks = rowsData.filter((r) => r.berth === b);
              return (
                <div key={b} className="flex items-center gap-2">
                  <div className="w-14 shrink-0 font-mono text-[11px] font-medium text-muted-foreground">
                    {b}
                  </div>
                  <div className="relative h-8 flex-1 rounded-md bg-muted/50 ring-1 ring-inset ring-border">
                    {/* hour ticks */}
                    <div
                      className="absolute inset-0 grid opacity-60"
                      style={{ gridTemplateColumns: `repeat(${span}, minmax(0,1fr))` }}
                    >
                      {hours.map((h) => (
                        <div key={h} className="border-l border-border/60" />
                      ))}
                    </div>
                    {blocks.map((blk, i) => {
                      const left = ((blk.start - start) / span) * 100;
                      const width = ((blk.end - blk.start) / span) * 100;
                      return (
                        <div
                          key={i}
                          className={cn(
                            "absolute inset-y-1 flex items-center gap-1.5 rounded px-2 text-[11px] font-medium shadow-sm",
                            toneStyles[blk.tone],
                          )}
                          style={{ left: `${left}%`, width: `${width}%` }}
                        >
                          <span className="truncate">{blk.vessel}</span>
                          <span className="ml-auto shrink-0 font-mono text-[10px] opacity-80">
                            {String(blk.start).padStart(2, "0")}–{String(blk.end).padStart(2, "0")}
                          </span>
                        </div>
                      );
                    })}
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      </div>
    </section>
  );
}
