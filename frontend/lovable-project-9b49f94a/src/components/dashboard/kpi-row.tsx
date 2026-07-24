import { ArrowUpRight, ArrowDownRight } from "lucide-react";
import { cn } from "@/lib/utils";

type Kpi = {
  label: string;
  value: string;
  unit?: string;
  delta: number;
  hint: string;
  spark: number[];
};

const kpis: Kpi[] = [
  {
    label: "Vessels in Port",
    value: "42",
    delta: 4.8,
    hint: "vs. 7-day avg",
    spark: [22, 28, 26, 31, 29, 34, 36, 33, 38, 40, 39, 42],
  },
  {
    label: "Berth Utilization",
    value: "78",
    unit: "%",
    delta: -2.1,
    hint: "target ≥ 82%",
    spark: [80, 82, 79, 81, 84, 83, 80, 78, 77, 79, 78, 78],
  },
  {
    label: "On-time Departures",
    value: "94.2",
    unit: "%",
    delta: 1.6,
    hint: "last 24h",
    spark: [88, 90, 91, 89, 92, 93, 91, 92, 94, 93, 94, 94],
  },
  {
    label: "TEU Throughput",
    value: "18,420",
    delta: 6.4,
    hint: "containers today",
    spark: [12, 14, 13, 16, 17, 16, 18, 19, 18, 20, 19, 18],
  },
];

function Sparkline({ data, positive }: { data: number[]; positive: boolean }) {
  const max = Math.max(...data);
  const min = Math.min(...data);
  const range = Math.max(1, max - min);
  const pts = data
    .map((v, i) => {
      const x = (i / (data.length - 1)) * 100;
      const y = 100 - ((v - min) / range) * 100;
      return `${x},${y}`;
    })
    .join(" ");
  const stroke = positive ? "var(--signal-ok)" : "var(--signal-crit)";
  return (
    <svg
      viewBox="0 0 100 100"
      preserveAspectRatio="none"
      className="h-10 w-full"
      aria-hidden
    >
      <polyline
        fill="none"
        stroke={stroke}
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="round"
        vectorEffect="non-scaling-stroke"
        points={pts}
      />
    </svg>
  );
}

export function KpiRow() {
  return (
    <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 xl:grid-cols-4">
      {kpis.map((k) => {
        const positive = k.delta >= 0;
        return (
          <div
            key={k.label}
            className="surface-panel relative overflow-hidden p-4"
          >
            <div className="flex items-start justify-between">
              <span className="text-[11px] font-medium uppercase tracking-[0.14em] text-muted-foreground">
                {k.label}
              </span>
              <span
                className={cn(
                  "inline-flex items-center gap-0.5 rounded-md px-1.5 py-0.5 font-mono text-[11px]",
                  positive
                    ? "bg-[color:var(--signal-ok)]/10 text-[color:var(--signal-ok)]"
                    : "bg-[color:var(--signal-crit)]/10 text-[color:var(--signal-crit)]",
                )}
              >
                {positive ? (
                  <ArrowUpRight className="h-3 w-3" />
                ) : (
                  <ArrowDownRight className="h-3 w-3" />
                )}
                {positive ? "+" : ""}
                {k.delta}%
              </span>
            </div>
            <div className="mt-3 flex items-baseline gap-1">
              <span className="font-display text-3xl font-semibold tracking-tight text-foreground">
                {k.value}
              </span>
              {k.unit && (
                <span className="font-display text-lg text-muted-foreground">
                  {k.unit}
                </span>
              )}
            </div>
            <div className="mt-1 text-[11px] text-muted-foreground">{k.hint}</div>
            <div className="mt-2">
              <Sparkline data={k.spark} positive={positive} />
            </div>
          </div>
        );
      })}
    </div>
  );
}
