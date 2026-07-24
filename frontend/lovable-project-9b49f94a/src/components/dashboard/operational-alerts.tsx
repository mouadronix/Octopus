import { AlertTriangle, Info, ShieldCheck, Radio } from "lucide-react";
import { cn } from "@/lib/utils";

type Alert = {
  level: "critical" | "warning" | "info" | "ok";
  title: string;
  detail: string;
  time: string;
};

const alerts: Alert[] = [
  { level: "critical", title: "Draft conflict · Berth A-04", detail: "Maersk Genoa draft 15.8m exceeds low-tide margin (T-45m).", time: "12:04" },
  { level: "warning",  title: "ETA slip · Nordic Amber",     detail: "AIS reports −40m velocity; new ETA 19:55.",             time: "11:52" },
  { level: "info",     title: "Pilot rotation",              detail: "Pilot Rossi assigned to inbound C-02 window.",         time: "11:20" },
  { level: "ok",       title: "Customs cleared",             detail: "MSC Ligura manifest approved by Dogane.",              time: "10:47" },
];

const meta: Record<Alert["level"], { icon: typeof AlertTriangle; tone: string; ring: string }> = {
  critical: { icon: AlertTriangle, tone: "text-[color:var(--signal-crit)]", ring: "bg-[color:var(--signal-crit)]/10" },
  warning:  { icon: Radio,         tone: "text-[color:var(--signal-warn)]", ring: "bg-[color:var(--signal-warn)]/15" },
  info:     { icon: Info,          tone: "text-[color:var(--signal-info)]", ring: "bg-[color:var(--signal-info)]/10" },
  ok:       { icon: ShieldCheck,   tone: "text-[color:var(--signal-ok)]",   ring: "bg-[color:var(--signal-ok)]/10" },
};

export function OperationalAlerts() {
  return (
    <section className="surface-panel flex flex-col">
      <header className="flex items-center justify-between border-b border-border px-4 py-3">
        <div>
          <h2 className="font-display text-sm font-semibold tracking-tight text-foreground">
            Operational Alerts
          </h2>
          <p className="text-[11px] text-muted-foreground">Signal feed · last 2 hours</p>
        </div>
        <span className="inline-flex items-center gap-1.5 font-mono text-[10px] uppercase tracking-[0.14em] text-muted-foreground">
          <span className="status-dot text-[color:var(--signal-crit)]" />
          1 critical
        </span>
      </header>

      <ul className="divide-y divide-border">
        {alerts.map((a) => {
          const m = meta[a.level];
          const Icon = m.icon;
          return (
            <li key={a.title} className="flex gap-3 px-4 py-3">
              <div className={cn("mt-0.5 grid h-7 w-7 shrink-0 place-items-center rounded-md", m.ring)}>
                <Icon className={cn("h-3.5 w-3.5", m.tone)} />
              </div>
              <div className="min-w-0 flex-1">
                <div className="flex items-baseline justify-between gap-3">
                  <p className="truncate text-xs font-semibold text-foreground">{a.title}</p>
                  <span className="font-mono text-[10px] text-muted-foreground">{a.time}</span>
                </div>
                <p className="mt-0.5 text-[11px] leading-relaxed text-muted-foreground">
                  {a.detail}
                </p>
              </div>
            </li>
          );
        })}
      </ul>
    </section>
  );
}
