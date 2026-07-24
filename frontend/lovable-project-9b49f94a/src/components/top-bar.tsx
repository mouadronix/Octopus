import { Bell, Search, ChevronDown, Cloud, Wind, Waves } from "lucide-react";
import { SidebarTrigger } from "@/components/ui/sidebar";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";

export function TopBar() {
  return (
    <header className="sticky top-0 z-30 flex h-14 items-center gap-3 border-b border-border bg-background/85 px-4 backdrop-blur">
      <SidebarTrigger className="text-muted-foreground" />
      <Separator orientation="vertical" className="h-5" />

      <div className="flex items-center gap-2 text-xs text-muted-foreground">
        <span className="font-mono uppercase tracking-[0.18em]">Port of Genoa</span>
        <span className="status-dot text-[color:var(--signal-ok)]" />
        <span className="font-medium text-foreground">Live</span>
      </div>

      <Separator orientation="vertical" className="mx-1 hidden h-5 md:block" />

      <div className="hidden items-center gap-4 text-xs text-muted-foreground md:flex">
        <span className="inline-flex items-center gap-1.5">
          <Cloud className="h-3.5 w-3.5" /> 18°C · Clear
        </span>
        <span className="inline-flex items-center gap-1.5">
          <Wind className="h-3.5 w-3.5" /> 12 kn NE
        </span>
        <span className="inline-flex items-center gap-1.5">
          <Waves className="h-3.5 w-3.5" /> Tide +1.2m
        </span>
      </div>

      <div className="ml-auto flex items-center gap-2">
        <div className="relative hidden md:block">
          <Search className="pointer-events-none absolute left-2.5 top-1/2 h-3.5 w-3.5 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search vessel, IMO, berth…"
            className="h-9 w-72 pl-8 font-mono text-xs"
          />
          <kbd className="pointer-events-none absolute right-2 top-1/2 hidden -translate-y-1/2 rounded border border-border bg-muted px-1.5 py-0.5 font-mono text-[10px] text-muted-foreground md:inline-block">
            ⌘K
          </kbd>
        </div>

        <Button variant="ghost" size="icon" className="relative h-9 w-9">
          <Bell className="h-4 w-4" />
          <span className="absolute right-1.5 top-1.5 h-1.5 w-1.5 rounded-full bg-[color:var(--signal-crit)]" />
        </Button>

        <div className="flex h-9 items-center gap-2 rounded-md border border-border bg-card px-2.5">
          <div className="grid h-6 w-6 place-items-center rounded-full bg-primary text-[10px] font-semibold text-primary-foreground">
            MR
          </div>
          <div className="hidden leading-tight lg:block">
            <div className="text-xs font-medium text-foreground">M. Ronix</div>
            <div className="text-[10px] text-muted-foreground">Operations · Genoa</div>
          </div>
          <ChevronDown className="h-3.5 w-3.5 text-muted-foreground" />
        </div>
      </div>
    </header>
  );
}
