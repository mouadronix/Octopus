import { createFileRoute } from "@tanstack/react-router";
import { AppShell, Panel } from "@/components/app-shell";
import { Input } from "@/components/ui/input";
import { Switch } from "@/components/ui/switch";
import { Building2, KeyRound, Bell, ShieldCheck, Plug } from "lucide-react";

export const Route = createFileRoute("/settings")({
  head: () => ({
    meta: [
      { title: "Settings · Octopus Port Operations" },
      {
        name: "description",
        content:
          "Manage terminal profile, API integrations, notifications, security and access policies for Octopus.",
      },
      { property: "og:title", content: "Settings · Octopus Port Operations" },
      { property: "og:description", content: "Terminal, integrations and security settings." },
    ],
  }),
  component: SettingsPage,
});

function Field({
  label,
  value,
  hint,
  readOnly,
}: {
  label: string;
  value: string;
  hint?: string;
  readOnly?: boolean;
}) {
  return (
    <label className="block">
      <div className="mb-1 text-[11px] font-medium uppercase tracking-[0.14em] text-muted-foreground">
        {label}
      </div>
      <Input
        defaultValue={value}
        readOnly={readOnly}
        className="h-9 font-mono text-xs"
      />
      {hint && <div className="mt-1 text-[10px] text-muted-foreground">{hint}</div>}
    </label>
  );
}

function Toggle({
  title,
  detail,
  defaultOn = false,
}: {
  title: string;
  detail: string;
  defaultOn?: boolean;
}) {
  return (
    <li className="flex items-center justify-between gap-3 px-4 py-3">
      <div className="min-w-0">
        <div className="text-sm font-semibold text-foreground">{title}</div>
        <div className="text-[11px] text-muted-foreground">{detail}</div>
      </div>
      <Switch defaultChecked={defaultOn} />
    </li>
  );
}

function SettingsPage() {
  return (
    <AppShell
      eyebrow="Administration"
      title="Settings"
      description="Terminal profile, integrations, notifications and access policies."
      actions={
        <>
          <button className="rounded-md border border-border bg-card px-3 py-1.5 font-medium text-foreground transition hover:bg-accent">
            Discard
          </button>
          <button className="rounded-md bg-primary px-3 py-1.5 font-medium text-primary-foreground transition hover:bg-primary/90">
            Save changes
          </button>
        </>
      }
    >
      <div className="grid grid-cols-1 gap-4 xl:grid-cols-3">
        <Panel
          className="xl:col-span-2"
          title="Terminal profile"
          subtitle="Identity used across dispatch, manifests and reports"
          actions={
            <span className="inline-flex items-center gap-1.5 font-mono text-[10px] uppercase tracking-[0.14em] text-muted-foreground">
              <Building2 className="h-3.5 w-3.5" /> UN/LOCODE IT GOA
            </span>
          }
        >
          <div className="grid grid-cols-1 gap-4 p-4 md:grid-cols-2">
            <Field label="Terminal name" value="Port of Genoa · North Terminal" />
            <Field label="Operator" value="Autorità di Sistema Portuale MLO" />
            <Field label="Timezone" value="Europe / Rome (CET)" />
            <Field label="Reference datum" value="MSL Genoa" hint="Used for tide levels and draft calculations" />
            <Field label="Primary contact" value="operations@port-genoa.it" />
            <Field label="Emergency channel" value="VHF 12 · +39 010 998 4412" />
          </div>
        </Panel>

        <Panel title="Security" subtitle="Access & authentication">
          <ul className="divide-y divide-border">
            <Toggle title="Enforce SSO" detail="SAML 2.0 · corporate IdP" defaultOn />
            <Toggle title="Two-factor for dispatchers" detail="TOTP required on every login" defaultOn />
            <Toggle title="IP allowlist" detail="Restrict admin to VPN ranges" />
            <Toggle title="Session hard timeout" detail="Force re-auth every 8 hours" defaultOn />
          </ul>
          <div className="border-t border-border p-4">
            <div className="flex items-center gap-2 text-xs text-muted-foreground">
              <ShieldCheck className="h-3.5 w-3.5 text-[color:var(--signal-ok)]" />
              Last security audit: <span className="font-mono text-foreground">Nov 12, 2024</span>
            </div>
          </div>
        </Panel>
      </div>

      <Panel
        title="Integrations"
        subtitle="Data sources and downstream systems"
        actions={
          <button className="rounded-md border border-border bg-card px-2.5 py-1 text-[11px] font-medium text-foreground hover:bg-accent">
            Manage
          </button>
        }
      >
        <ul className="divide-y divide-border">
          {[
            { name: "AIS receiver network",    detail: "4 stations · MMSI feed",  status: "Connected", ok: true },
            { name: "Customs · Dogane",         detail: "Manifest clearance API",  status: "Connected", ok: true },
            { name: "Weather · ISPRA",          detail: "Tide + forecast feed",    status: "Connected", ok: true },
            { name: "TOS · Navis N4",           detail: "Terminal ops system",     status: "Sync warning", ok: false },
            { name: "SAP ERP",                  detail: "Billing & payroll",       status: "Connected", ok: true },
          ].map((i) => (
            <li key={i.name} className="flex items-center justify-between gap-3 px-4 py-3">
              <div className="flex items-center gap-3">
                <div className="grid h-8 w-8 place-items-center rounded-md bg-[color:var(--navy-50)] text-[color:var(--navy-800)]">
                  <Plug className="h-3.5 w-3.5" />
                </div>
                <div className="leading-tight">
                  <div className="text-sm font-semibold text-foreground">{i.name}</div>
                  <div className="font-mono text-[10px] text-muted-foreground">{i.detail}</div>
                </div>
              </div>
              <span
                className={
                  "inline-flex items-center gap-1.5 rounded-full px-2 py-0.5 text-[10px] font-medium " +
                  (i.ok
                    ? "bg-[color:var(--signal-ok)]/10 text-[color:var(--signal-ok)]"
                    : "bg-[color:var(--signal-warn)]/15 text-[color:var(--signal-warn)]")
                }
              >
                <span className={"h-1.5 w-1.5 rounded-full " + (i.ok ? "bg-[color:var(--signal-ok)]" : "bg-[color:var(--signal-warn)]")} />
                {i.status}
              </span>
            </li>
          ))}
        </ul>
      </Panel>

      <div className="grid grid-cols-1 gap-4 xl:grid-cols-2">
        <Panel title="Notifications" subtitle="Channels & thresholds" actions={<Bell className="h-3.5 w-3.5 text-muted-foreground" />}>
          <ul className="divide-y divide-border">
            <Toggle title="Critical alerts by SMS" detail="Draft conflicts, ETA slips ≥ 60m" defaultOn />
            <Toggle title="Shift handover email"   detail="Auto-sent 15 minutes before shift end" defaultOn />
            <Toggle title="Push to VTS dispatchers" detail="In-app · sound on critical" defaultOn />
            <Toggle title="Weekly executive digest" detail="KPIs to port authority board" />
          </ul>
        </Panel>

        <Panel title="API keys" subtitle="Programmatic access to Octopus" actions={<KeyRound className="h-3.5 w-3.5 text-muted-foreground" />}>
          <div className="grid grid-cols-1 gap-4 p-4">
            <Field
              label="Publishable key"
              value="oct_pub_9F3A_18b2c740e2f14…"
              hint="Safe to embed in client applications"
              readOnly
            />
            <Field
              label="Secret key · production"
              value="oct_sk_prod_••••••••••••••••"
              hint="Rotate at least every 90 days"
              readOnly
            />
            <div className="flex items-center gap-2">
              <button className="rounded-md border border-border bg-card px-3 py-1.5 text-xs font-medium text-foreground hover:bg-accent">
                Generate new key
              </button>
              <button className="rounded-md border border-border bg-card px-3 py-1.5 text-xs font-medium text-[color:var(--signal-crit)] hover:bg-accent">
                Revoke all
              </button>
            </div>
          </div>
        </Panel>
      </div>
    </AppShell>
  );
}
