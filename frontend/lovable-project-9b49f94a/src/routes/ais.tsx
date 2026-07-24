import { createFileRoute } from "@tanstack/react-router";
import { AppShell, Panel } from "@/components/app-shell";
import { Radio, Radar, Satellite } from "lucide-react";
import { cn } from "@/lib/utils";

export const Route = createFileRoute("/ais")({
  head: () => ({
    meta: [
      { title: "AIS Feed · Octopus Port Operations" },
      {
        name: "description",
        content:
          "Live AIS tracking of vessels within port approach: MMSI, position, course, speed, ETA and last signal.",
      },
      { property: "og:title", content: "AIS Feed · Octopus Port Operations" },
      { property: "og:description", content: "Live AIS tracking of vessels approaching the port." },
    ],
  }),
  component: AisPage,
});

type Ais = {
  mmsi: string;
  name: string;
  lat: string;
  lon: string;
  cog: number;   // course over ground
  sog: number;   // speed over ground (knots)
  dest: string;
  eta: string;
  signal: "strong" | "weak" | "lost";
};

const feed: Ais[] = [
  { mmsi: "247123456", name: "ONE Aquila",       lat: "44°21.4'N", lon: "8°55.7'E",  cog: 274, sog: 12.4, dest: "GENOA", eta: "14:20", signal: "strong" },
  { mmsi: "413678901", name: "COSCO Riviera",    lat: "44°15.9'N", lon: "9°02.1'E",  cog: 291, sog: 14.1, dest: "GENOA", eta: "16:05", signal: "strong" },
  { mmsi: "247456789", name: "Grimaldi Eurocargo",lat: "44°18.7'N",lon: "8°48.3'E",  cog: 265, sog: 10.8, dest: "GENOA", eta: "17:40", signal: "strong" },
  { mmsi: "257344210", name: "Nordic Amber",     lat: "43°58.1'N", lon: "8°22.4'E",  cog: 52,  sog: 6.2,  dest: "GENOA", eta: "19:55", signal: "weak" },
  { mmsi: "636019345", name: "Atlantic Ore",     lat: "43°42.6'N", lon: "8°10.8'E",  cog: 40,  sog: 11.7, dest: "GENOA", eta: "22:00", signal: "strong" },
  { mmsi: "352884001", name: "MSC Ligura",       lat: "44°24.1'N", lon: "8°55.2'E",  cog: 90,  sog: 0.1,  dest: "BERTH A-04", eta: "—", signal: "strong" },
  { mmsi: "227889444", name: "Le Havre Express", lat: "43°30.9'N", lon: "7°55.2'E",  cog: 65,  sog: 15.3, dest: "GENOA", eta: "Tomorrow 08:15", signal: "lost" },
];

const signalMeta: Record<Ais["signal"], { label: string; className: string }> = {
  strong: { label: "Strong", className: "bg-[color:var(--signal-ok)]/10 text-[color:var(--signal-ok)]" },
  weak:   { label: "Weak",   className: "bg-[color:var(--signal-warn)]/15 text-[color:var(--signal-warn)]" },
  lost:   { label: "Lost",   className: "bg-[color:var(--signal-crit)]/10 text-[color:var(--signal-crit)]" },
};

function ApproachRadar() {
  // Decorative radar-style plot
  const cx = 200;
  const cy = 200;
  const dots = feed.map((v, i) => {
    const angle = (i / feed.length) * Math.PI * 2;
    const r = 40 + ((v.sog / 16) * 140);
    return {
      key: v.mmsi,
      x: cx + Math.cos(angle) * r,
      y: cy + Math.sin(angle) * r,
      name: v.name,
      strong: v.signal,
    };
  });
  return (
    <svg viewBox="0 0 400 400" className="h-72 w-full">
      <defs>
        <radialGradient id="radarGlow" cx="50%" cy="50%" r="50%">
          <stop offset="0%" stopColor="var(--navy-800)" stopOpacity="0.15" />
          <stop offset="100%" stopColor="var(--navy-800)" stopOpacity="0" />
        </radialGradient>
      </defs>
      <circle cx={cx} cy={cy} r="180" fill="url(#radarGlow)" />
      {[60, 120, 180].map((r) => (
        <circle key={r} cx={cx} cy={cy} r={r} fill="none" stroke="var(--grid-line)" strokeDasharray="3 4" />
      ))}
      <line x1={cx} y1={20} x2={cx} y2={380} stroke="var(--grid-line)" strokeDasharray="3 4" />
      <line x1={20} y1={cy} x2={380} y2={cy} stroke="var(--grid-line)" strokeDasharray="3 4" />
      <text x={cx} y="18" textAnchor="middle" className="fill-[color:var(--muted-foreground)] font-mono text-[10px]">N</text>
      <text x={cx} y="392" textAnchor="middle" className="fill-[color:var(--muted-foreground)] font-mono text-[10px]">S</text>
      <text x="8" y={cy + 3} className="fill-[color:var(--muted-foreground)] font-mono text-[10px]">W</text>
      <text x="386" y={cy + 3} className="fill-[color:var(--muted-foreground)] font-mono text-[10px]">E</text>

      {dots.map((d) => (
        <g key={d.key}>
          <circle
            cx={d.x}
            cy={d.y}
            r={d.strong === "lost" ? 3 : 4}
            fill={
              d.strong === "strong"
                ? "var(--navy-800)"
                : d.strong === "weak"
                ? "var(--signal-warn)"
                : "var(--signal-crit)"
            }
            opacity={d.strong === "lost" ? 0.5 : 1}
          />
          <text
            x={d.x + 8}
            y={d.y + 3}
            className="fill-[color:var(--foreground)] font-mono text-[9px]"
          >
            {d.name}
          </text>
        </g>
      ))}
      <circle cx={cx} cy={cy} r="4" fill="var(--signal-info)" />
      <text x={cx + 8} y={cy + 3} className="fill-[color:var(--signal-info)] font-mono text-[9px]">
        Port of Genoa
      </text>
    </svg>
  );
}

function AisPage() {
  return (
    <AppShell
      eyebrow="Realtime"
      title="AIS Feed"
      description="Live AIS positions of vessels within 40NM of the port approach, refreshed every 12 seconds."
      actions={
        <>
          <span className="inline-flex items-center gap-1.5 rounded-md border border-border bg-card px-2.5 py-1.5 font-mono text-[11px] uppercase tracking-[0.14em] text-muted-foreground">
            <span className="status-dot text-[color:var(--signal-ok)]" />
            Stream live
          </span>
          <button className="rounded-md bg-primary px-3 py-1.5 font-medium text-primary-foreground transition hover:bg-primary/90">
            Configure receiver
          </button>
        </>
      }
    >
      <div className="grid grid-cols-2 gap-3 lg:grid-cols-4">
        <div className="surface-panel p-4">
          <div className="flex items-center justify-between">
            <span className="text-[11px] uppercase tracking-[0.14em] text-muted-foreground">Vessels tracked</span>
            <Radio className="h-3.5 w-3.5 text-muted-foreground" />
          </div>
          <div className="mt-1 font-display text-2xl font-semibold text-foreground">57</div>
          <div className="mt-1 text-[11px] text-muted-foreground">within 40NM</div>
        </div>
        <div className="surface-panel p-4">
          <div className="flex items-center justify-between">
            <span className="text-[11px] uppercase tracking-[0.14em] text-muted-foreground">Signal quality</span>
            <Radar className="h-3.5 w-3.5 text-muted-foreground" />
          </div>
          <div className="mt-1 font-display text-2xl font-semibold text-[color:var(--signal-ok)]">97%</div>
          <div className="mt-1 text-[11px] text-muted-foreground">avg SNR 14dB</div>
        </div>
        <div className="surface-panel p-4">
          <div className="flex items-center justify-between">
            <span className="text-[11px] uppercase tracking-[0.14em] text-muted-foreground">Antennae</span>
            <Satellite className="h-3.5 w-3.5 text-muted-foreground" />
          </div>
          <div className="mt-1 font-display text-2xl font-semibold text-foreground">4 / 4</div>
          <div className="mt-1 text-[11px] text-muted-foreground">Genoa · Portofino · Voltri · Savona</div>
        </div>
        <div className="surface-panel p-4">
          <div className="flex items-center justify-between">
            <span className="text-[11px] uppercase tracking-[0.14em] text-muted-foreground">Latency</span>
            <Radio className="h-3.5 w-3.5 text-muted-foreground" />
          </div>
          <div className="mt-1 font-display text-2xl font-semibold text-foreground">1.4s</div>
          <div className="mt-1 text-[11px] text-muted-foreground">end-to-end median</div>
        </div>
      </div>

      <div className="grid grid-cols-1 gap-4 xl:grid-cols-3">
        <Panel
          className="xl:col-span-2"
          title="Approach chart"
          subtitle="Radar-style plot · 40NM range"
        >
          <div className="grid-bg p-4">
            <ApproachRadar />
          </div>
        </Panel>

        <Panel title="Receiver stations" subtitle="Coastal antennae · uptime 30d">
          <ul className="divide-y divide-border">
            {[
              { name: "Genoa Molo",    up: 99.98, snr: 15 },
              { name: "Portofino",     up: 99.62, snr: 12 },
              { name: "Voltri",        up: 99.90, snr: 14 },
              { name: "Savona",        up: 98.71, snr: 11 },
            ].map((r) => (
              <li key={r.name} className="flex items-center justify-between px-4 py-3">
                <div className="flex items-center gap-2.5">
                  <div className="grid h-7 w-7 place-items-center rounded-md bg-[color:var(--navy-50)] text-[color:var(--navy-800)]">
                    <Satellite className="h-3.5 w-3.5" />
                  </div>
                  <div>
                    <div className="text-xs font-semibold text-foreground">{r.name}</div>
                    <div className="font-mono text-[10px] text-muted-foreground">SNR {r.snr}dB</div>
                  </div>
                </div>
                <div className="text-right">
                  <div className="font-mono text-xs font-semibold text-foreground">{r.up}%</div>
                  <div className="font-mono text-[10px] text-muted-foreground">uptime</div>
                </div>
              </li>
            ))}
          </ul>
        </Panel>
      </div>

      <Panel title="Live message feed" subtitle="AIS class A positions · latest first">
        <div className="overflow-x-auto">
          <table className="w-full text-left text-xs">
            <thead className="text-[10px] uppercase tracking-[0.14em] text-muted-foreground">
              <tr className="border-b border-border">
                <th className="px-4 py-2 font-medium">MMSI</th>
                <th className="px-4 py-2 font-medium">Vessel</th>
                <th className="px-4 py-2 font-medium">Position</th>
                <th className="px-4 py-2 font-medium">COG</th>
                <th className="px-4 py-2 font-medium">SOG</th>
                <th className="px-4 py-2 font-medium">Destination</th>
                <th className="px-4 py-2 font-medium">ETA</th>
                <th className="px-4 py-2 font-medium">Signal</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border font-mono">
              {feed.map((r) => (
                <tr key={r.mmsi} className="transition hover:bg-muted/40">
                  <td className="px-4 py-2.5 text-muted-foreground">{r.mmsi}</td>
                  <td className="px-4 py-2.5 font-sans font-medium text-foreground">{r.name}</td>
                  <td className="px-4 py-2.5 text-muted-foreground">{r.lat} · {r.lon}</td>
                  <td className="px-4 py-2.5 text-foreground">{String(r.cog).padStart(3, "0")}°</td>
                  <td className="px-4 py-2.5 text-foreground">{r.sog.toFixed(1)} kn</td>
                  <td className="px-4 py-2.5 font-sans text-muted-foreground">{r.dest}</td>
                  <td className="px-4 py-2.5 text-foreground">{r.eta}</td>
                  <td className="px-4 py-2.5">
                    <span className={cn("inline-flex rounded-full px-2 py-0.5 font-sans text-[10px] font-medium", signalMeta[r.signal].className)}>
                      {signalMeta[r.signal].label}
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
