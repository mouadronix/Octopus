import { cn } from "@/lib/utils";
import { Ship } from "lucide-react";

type Vessel = {
  imo: string;
  name: string;
  flag: string;
  type: "Container" | "Bulk" | "Tanker" | "Ro-Ro";
  loa: number;
  eta: string;
  berth: string;
  status: "On schedule" | "Delayed" | "Priority" | "Cleared";
};

const rows: Vessel[] = [
  { imo: "9812441", name: "ONE Aquila",       flag: "JP", type: "Container", loa: 330, eta: "14:20", berth: "B-01", status: "On schedule" },
  { imo: "9755320", name: "COSCO Riviera",    flag: "CN", type: "Container", loa: 350, eta: "16:05", berth: "C-02", status: "Priority" },
  { imo: "9702118", name: "Grimaldi Eurocargo",flag: "IT", type: "Ro-Ro",    loa: 240, eta: "17:40", berth: "A-03", status: "On schedule" },
  { imo: "9666204", name: "Nordic Amber",     flag: "NO", type: "Tanker",    loa: 275, eta: "19:15", berth: "—",    status: "Delayed" },
  { imo: "9598712", name: "Atlantic Ore",     flag: "LR", type: "Bulk",      loa: 292, eta: "22:00", berth: "—",    status: "On schedule" },
  { imo: "9843301", name: "MSC Ligura",       flag: "PA", type: "Container", loa: 366, eta: "Tomorrow 04:10", berth: "A-04", status: "Cleared" },
];

const statusStyles: Record<Vessel["status"], string> = {
  "On schedule": "bg-[color:var(--signal-ok)]/10 text-[color:var(--signal-ok)]",
  "Priority":    "bg-[color:var(--signal-info)]/10 text-[color:var(--signal-info)]",
  "Delayed":     "bg-[color:var(--signal-crit)]/10 text-[color:var(--signal-crit)]",
  "Cleared":     "bg-muted text-muted-foreground",
};

export function IncomingVessels() {
  return (
    <section className="surface-panel flex flex-col">
      <header className="flex items-center justify-between border-b border-border px-4 py-3">
        <div>
          <h2 className="font-display text-sm font-semibold tracking-tight text-foreground">
            Incoming Vessels
          </h2>
          <p className="text-[11px] text-muted-foreground">Next 24 hours · AIS-verified</p>
        </div>
        <button className="rounded-md border border-border bg-card px-2.5 py-1 text-[11px] font-medium text-foreground transition hover:bg-accent">
          Open manifest
        </button>
      </header>

      <div className="overflow-x-auto">
        <table className="w-full text-left text-xs">
          <thead className="text-[10px] uppercase tracking-[0.14em] text-muted-foreground">
            <tr className="border-b border-border">
              <th className="px-4 py-2 font-medium">Vessel</th>
              <th className="px-4 py-2 font-medium">Type</th>
              <th className="px-4 py-2 font-medium">LOA</th>
              <th className="px-4 py-2 font-medium">ETA</th>
              <th className="px-4 py-2 font-medium">Berth</th>
              <th className="px-4 py-2 font-medium">Status</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-border">
            {rows.map((v) => (
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
                <td className="px-4 py-2.5 font-mono text-muted-foreground">{v.loa}m</td>
                <td className="px-4 py-2.5 font-mono text-foreground">{v.eta}</td>
                <td className="px-4 py-2.5 font-mono text-foreground">{v.berth}</td>
                <td className="px-4 py-2.5">
                  <span className={cn("inline-flex rounded-full px-2 py-0.5 text-[10px] font-medium", statusStyles[v.status])}>
                    {v.status}
                  </span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  );
}
