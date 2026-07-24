import { createFileRoute } from "@tanstack/react-router";
import { AppShell, Panel } from "@/components/app-shell";
import { FileBarChart, Download, TrendingUp } from "lucide-react";

export const Route = createFileRoute("/reports")({
  head: () => ({
    meta: [
      { title: "Reports · Octopus Port Operations" },
      {
        name: "description",
        content:
          "Operational reports and KPIs: throughput, turnaround time, dwell, on-time performance and safety incidents.",
      },
      { property: "og:title", content: "Reports · Octopus Port Operations" },
      { property: "og:description", content: "Operational reports and KPIs for port authorities." },
    ],
  }),
  component: ReportsPage,
});

// Bar-chart data — monthly throughput (TEU × 1000)
const throughput = [
  { m: "Jan", teu: 148 },
  { m: "Feb", teu: 156 },
  { m: "Mar", teu: 172 },
  { m: "Apr", teu: 168 },
  { m: "May", teu: 182 },
  { m: "Jun", teu: 195 },
  { m: "Jul", teu: 208 },
  { m: "Aug", teu: 192 },
  { m: "Sep", teu: 214 },
  { m: "Oct", teu: 226 },
  { m: "Nov", teu: 218 },
  { m: "Dec", teu: 231 },
];

const reports = [
  { title: "Monthly throughput", type: "PDF · 24 pages",  updated: "2 hours ago",  scope: "Nov 2024" },
  { title: "Turnaround by vessel type", type: "Excel · 6 sheets", updated: "Yesterday",   scope: "Q4 rolling" },
  { title: "Safety & incidents log",   type: "PDF · 12 pages",  updated: "Nov 22, 09:14",scope: "YTD 2024" },
  { title: "Berth utilization audit",  type: "PDF · 18 pages",  updated: "Nov 20, 16:22",scope: "Oct 2024" },
  { title: "Customs & clearance SLA",  type: "Excel · 3 sheets",updated: "Nov 18",       scope: "Q4 rolling" },
];

function BarChart() {
  const max = Math.max(...throughput.map((d) => d.teu));
  return (
    <div className="p-4">
      <div className="flex h-48 items-end gap-2">
        {throughput.map((d) => {
          const h = (d.teu / max) * 100;
          return (
            <div key={d.m} className="group flex flex-1 flex-col items-center gap-1.5">
              <div className="relative flex h-full w-full items-end">
                <div
                  className="w-full rounded-t bg-[color:var(--navy-800)] transition group-hover:bg-[color:var(--navy-700)]"
                  style={{ height: `${h}%` }}
                />
              </div>
              <span className="font-mono text-[10px] text-muted-foreground">{d.m}</span>
            </div>
          );
        })}
      </div>
    </div>
  );
}

function ReportsPage() {
  return (
    <AppShell
      eyebrow="Analytics"
      title="Reports"
      description="Operational KPIs, throughput analytics and compliance reports for port authorities and terminal operators."
      actions={
        <>
          <button className="rounded-md border border-border bg-card px-3 py-1.5 font-medium text-foreground transition hover:bg-accent">
            Schedule report
          </button>
          <button className="inline-flex items-center gap-1.5 rounded-md bg-primary px-3 py-1.5 font-medium text-primary-foreground transition hover:bg-primary/90">
            <FileBarChart className="h-3.5 w-3.5" /> New report
          </button>
        </>
      }
    >
      <div className="grid grid-cols-2 gap-3 lg:grid-cols-4">
        {[
          { l: "TEU · YTD",         v: "2.31M", d: "+8.4% YoY" },
          { l: "Avg. turnaround",   v: "18.4h", d: "−1.2h vs. plan" },
          { l: "On-time %",         v: "94.2%", d: "+1.6 pts" },
          { l: "Safety incidents",  v: "3",     d: "YTD · target ≤ 5" },
        ].map((k) => (
          <div key={k.l} className="surface-panel p-4">
            <div className="flex items-center justify-between">
              <span className="text-[11px] uppercase tracking-[0.14em] text-muted-foreground">{k.l}</span>
              <TrendingUp className="h-3.5 w-3.5 text-[color:var(--signal-ok)]" />
            </div>
            <div className="mt-1 font-display text-2xl font-semibold text-foreground">{k.v}</div>
            <div className="mt-1 text-[11px] text-muted-foreground">{k.d}</div>
          </div>
        ))}
      </div>

      <Panel
        title="Container throughput"
        subtitle="TEU × 1000 · monthly · 12-month rolling"
        actions={
          <button className="inline-flex items-center gap-1.5 rounded-md border border-border bg-card px-2.5 py-1 text-[11px] font-medium text-foreground hover:bg-accent">
            <Download className="h-3 w-3" /> Export
          </button>
        }
      >
        <BarChart />
      </Panel>

      <Panel title="Recent reports" subtitle="Downloadable operational reports">
        <ul className="divide-y divide-border">
          {reports.map((r) => (
            <li key={r.title} className="flex items-center justify-between gap-3 px-4 py-3">
              <div className="flex items-center gap-3">
                <div className="grid h-9 w-9 place-items-center rounded-md bg-[color:var(--navy-50)] text-[color:var(--navy-800)]">
                  <FileBarChart className="h-4 w-4" />
                </div>
                <div className="leading-tight">
                  <div className="text-sm font-semibold text-foreground">{r.title}</div>
                  <div className="font-mono text-[10px] text-muted-foreground">
                    {r.type} · scope {r.scope} · updated {r.updated}
                  </div>
                </div>
              </div>
              <div className="flex items-center gap-2">
                <button className="rounded-md border border-border bg-card px-2.5 py-1 text-[11px] font-medium text-foreground hover:bg-accent">
                  Open
                </button>
                <button className="inline-flex items-center gap-1 rounded-md bg-primary px-2.5 py-1 text-[11px] font-medium text-primary-foreground hover:bg-primary/90">
                  <Download className="h-3 w-3" /> Download
                </button>
              </div>
            </li>
          ))}
        </ul>
      </Panel>
    </AppShell>
  );
}
