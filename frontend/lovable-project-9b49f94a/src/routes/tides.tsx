import { createFileRoute } from "@tanstack/react-router";
import { AppShell, Panel } from "@/components/app-shell";
import { Cloud, CloudRain, Wind, Waves, Sun, CloudSun, Droplets } from "lucide-react";
import { cn } from "@/lib/utils";

export const Route = createFileRoute("/tides")({
  head: () => ({
    meta: [
      { title: "Tides & Weather · Octopus Port Operations" },
      {
        name: "description",
        content:
          "Live tide levels, sea state, wind and weather forecast for safe berthing and pilot operations at the Port of Genoa.",
      },
      { property: "og:title", content: "Tides & Weather · Octopus Port Operations" },
      { property: "og:description", content: "Live tide, wind and weather for port operations." },
    ],
  }),
  component: TidesPage,
});

// Simple sinusoidal tide curve, 24h
const tidePoints = Array.from({ length: 25 }, (_, h) => {
  const y = 1.4 + Math.sin(((h - 3) / 24) * Math.PI * 4) * 0.9;
  return { h, y: parseFloat(y.toFixed(2)) };
});

const forecast = [
  { time: "Now",   icon: Sun,      temp: 18, wind: "12 kn NE", wave: 0.6, cond: "Clear" },
  { time: "15:00", icon: CloudSun, temp: 19, wind: "14 kn NE", wave: 0.7, cond: "Fair" },
  { time: "18:00", icon: Cloud,    temp: 17, wind: "16 kn E",  wave: 0.9, cond: "Cloudy" },
  { time: "21:00", icon: Cloud,    temp: 15, wind: "18 kn E",  wave: 1.1, cond: "Overcast" },
  { time: "00:00", icon: CloudRain,temp: 13, wind: "22 kn SE", wave: 1.6, cond: "Light rain" },
  { time: "03:00", icon: CloudRain,temp: 12, wind: "24 kn SE", wave: 1.9, cond: "Rain" },
  { time: "06:00", icon: Cloud,    temp: 13, wind: "20 kn S",  wave: 1.4, cond: "Cloudy" },
  { time: "09:00", icon: CloudSun, temp: 16, wind: "14 kn SW", wave: 0.8, cond: "Fair" },
];

function TideChart() {
  const w = 800;
  const h = 180;
  const maxY = 2.5;
  const points = tidePoints
    .map((p, i) => {
      const x = (i / (tidePoints.length - 1)) * w;
      const y = h - (p.y / maxY) * h;
      return `${x},${y}`;
    })
    .join(" ");
  const area = `0,${h} ${points} ${w},${h}`;

  return (
    <svg viewBox={`0 0 ${w} ${h}`} preserveAspectRatio="none" className="h-48 w-full">
      <defs>
        <linearGradient id="tideFill" x1="0" y1="0" x2="0" y2="1">
          <stop offset="0%" stopColor="var(--navy-700)" stopOpacity="0.35" />
          <stop offset="100%" stopColor="var(--navy-700)" stopOpacity="0" />
        </linearGradient>
      </defs>
      {[0.5, 1.0, 1.5, 2.0].map((t) => (
        <g key={t}>
          <line
            x1="0"
            x2={w}
            y1={h - (t / maxY) * h}
            y2={h - (t / maxY) * h}
            stroke="var(--grid-line)"
            strokeDasharray="3 4"
          />
        </g>
      ))}
      <polygon points={area} fill="url(#tideFill)" />
      <polyline
        points={points}
        fill="none"
        stroke="var(--navy-800)"
        strokeWidth="2"
        strokeLinecap="round"
        strokeLinejoin="round"
        vectorEffect="non-scaling-stroke"
      />
    </svg>
  );
}

function TidesPage() {
  return (
    <AppShell
      eyebrow="Environment"
      title="Tides & Weather"
      description="Sea state, tide window and forecast conditions affecting pilot access, berthing and cargo operations."
      actions={
        <>
          <button className="rounded-md border border-border bg-card px-3 py-1.5 font-medium text-foreground transition hover:bg-accent">
            Advisory feed
          </button>
          <button className="rounded-md bg-primary px-3 py-1.5 font-medium text-primary-foreground transition hover:bg-primary/90">
            Broadcast bulletin
          </button>
        </>
      }
    >
      <div className="grid grid-cols-2 gap-3 lg:grid-cols-4">
        <div className="surface-panel p-4">
          <div className="flex items-center justify-between">
            <span className="text-[11px] uppercase tracking-[0.14em] text-muted-foreground">Tide</span>
            <Waves className="h-3.5 w-3.5 text-muted-foreground" />
          </div>
          <div className="mt-1 font-display text-2xl font-semibold text-foreground">+1.2m</div>
          <div className="mt-1 text-[11px] text-muted-foreground">Rising · high 15:42</div>
        </div>
        <div className="surface-panel p-4">
          <div className="flex items-center justify-between">
            <span className="text-[11px] uppercase tracking-[0.14em] text-muted-foreground">Wind</span>
            <Wind className="h-3.5 w-3.5 text-muted-foreground" />
          </div>
          <div className="mt-1 font-display text-2xl font-semibold text-foreground">12 kn</div>
          <div className="mt-1 text-[11px] text-muted-foreground">NE · gust 18 kn</div>
        </div>
        <div className="surface-panel p-4">
          <div className="flex items-center justify-between">
            <span className="text-[11px] uppercase tracking-[0.14em] text-muted-foreground">Wave height</span>
            <Droplets className="h-3.5 w-3.5 text-muted-foreground" />
          </div>
          <div className="mt-1 font-display text-2xl font-semibold text-foreground">0.6m</div>
          <div className="mt-1 text-[11px] text-muted-foreground">Slight sea · SSE swell</div>
        </div>
        <div className="surface-panel p-4">
          <div className="flex items-center justify-between">
            <span className="text-[11px] uppercase tracking-[0.14em] text-muted-foreground">Visibility</span>
            <Sun className="h-3.5 w-3.5 text-muted-foreground" />
          </div>
          <div className="mt-1 font-display text-2xl font-semibold text-foreground">10 NM</div>
          <div className="mt-1 text-[11px] text-muted-foreground">Clear · dew point 11°C</div>
        </div>
      </div>

      <Panel
        title="Tide window · next 24h"
        subtitle="Reference datum: MSL Genoa · pilot boarding threshold 0.4m"
        actions={
          <span className="font-mono text-[10px] uppercase tracking-[0.14em] text-muted-foreground">
            Source: ISPRA / Port authority
          </span>
        }
      >
        <div className="p-4">
          <TideChart />
          <div className="mt-3 grid grid-cols-2 gap-3 sm:grid-cols-4">
            {[
              { label: "Low",  time: "09:14", v: "+0.3m" },
              { label: "High", time: "15:42", v: "+2.1m" },
              { label: "Low",  time: "21:58", v: "+0.4m" },
              { label: "High", time: "04:16", v: "+2.0m" },
            ].map((t) => (
              <div key={t.time} className="rounded-md border border-border bg-card p-3">
                <div className="text-[11px] uppercase tracking-[0.14em] text-muted-foreground">
                  {t.label} tide
                </div>
                <div className="mt-0.5 font-mono text-sm text-foreground">
                  {t.time} · <span className="font-semibold">{t.v}</span>
                </div>
              </div>
            ))}
          </div>
        </div>
      </Panel>

      <Panel title="Forecast" subtitle="3-hourly outlook · next 24 hours">
        <div className="overflow-x-auto">
          <div className="flex min-w-max gap-2 p-4">
            {forecast.map((f) => (
              <div
                key={f.time}
                className={cn(
                  "flex w-32 flex-col items-center gap-1 rounded-md border border-border bg-card p-3 text-center",
                )}
              >
                <div className="font-mono text-[10px] uppercase tracking-[0.14em] text-muted-foreground">
                  {f.time}
                </div>
                <f.icon className="h-6 w-6 text-[color:var(--navy-700)]" />
                <div className="font-display text-lg font-semibold text-foreground">{f.temp}°</div>
                <div className="text-[11px] text-muted-foreground">{f.cond}</div>
                <div className="mt-1 flex w-full flex-col gap-0.5 border-t border-border pt-2 font-mono text-[10px] text-muted-foreground">
                  <span>{f.wind}</span>
                  <span>wave {f.wave}m</span>
                </div>
              </div>
            ))}
          </div>
        </div>
      </Panel>
    </AppShell>
  );
}
