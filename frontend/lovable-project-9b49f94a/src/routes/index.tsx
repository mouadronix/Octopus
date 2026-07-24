import { createFileRoute } from "@tanstack/react-router";
import { AppShell } from "@/components/app-shell";
import { KpiRow } from "@/components/dashboard/kpi-row";
import { BerthMap } from "@/components/dashboard/berth-map";
import { IncomingVessels } from "@/components/dashboard/incoming-vessels";
import { ScheduleTimeline } from "@/components/dashboard/schedule-timeline";
import { OperationalAlerts } from "@/components/dashboard/operational-alerts";

export const Route = createFileRoute("/")({
  head: () => ({
    meta: [
      { title: "Control Tower · Octopus Port Operations" },
      {
        name: "description",
        content:
          "Live operational control tower for port authorities: vessel ETAs, berth allocation, scheduling and alerts in one enterprise-grade console.",
      },
      { property: "og:title", content: "Control Tower · Octopus Port Operations" },
      {
        property: "og:description",
        content:
          "Enterprise dashboard for port operations — vessels, berths, scheduling and real-time alerts.",
      },
    ],
  }),
  component: ControlTower,
});

function ControlTower() {
  return (
    <AppShell
      eyebrow="Operations · Shift B"
      title="Control Tower"
      description="Live status of vessels, berths and scheduling across the Port of Genoa."
      actions={
        <>
          <button className="rounded-md border border-border bg-card px-3 py-1.5 font-medium text-foreground transition hover:bg-accent">
            Export briefing
          </button>
          <button className="rounded-md bg-primary px-3 py-1.5 font-medium text-primary-foreground transition hover:bg-primary/90">
            New allocation
          </button>
        </>
      }
    >
      <KpiRow />

      <div className="grid grid-cols-1 gap-4 xl:grid-cols-3">
        <div className="xl:col-span-2">
          <BerthMap />
        </div>
        <OperationalAlerts />
      </div>

      <ScheduleTimeline />
      <IncomingVessels />
    </AppShell>
  );
}
