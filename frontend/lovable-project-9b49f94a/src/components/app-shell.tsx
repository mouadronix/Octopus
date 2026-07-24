import { type ReactNode } from "react";
import { SidebarProvider, SidebarInset } from "@/components/ui/sidebar";
import { AppSidebar } from "@/components/app-sidebar";
import { TopBar } from "@/components/top-bar";

type Props = {
  eyebrow?: string;
  title: string;
  description?: string;
  actions?: ReactNode;
  children: ReactNode;
};

export function AppShell({ eyebrow, title, description, actions, children }: Props) {
  return (
    <SidebarProvider>
      <div className="flex min-h-screen w-full bg-background">
        <AppSidebar />
        <SidebarInset className="flex min-w-0 flex-1 flex-col">
          <TopBar />
          <main className="flex-1 space-y-4 p-4 lg:p-6">
            <div className="flex flex-wrap items-end justify-between gap-3">
              <div>
                {eyebrow && (
                  <div className="font-mono text-[11px] uppercase tracking-[0.18em] text-muted-foreground">
                    {eyebrow}
                  </div>
                )}
                <h1 className="font-display text-2xl font-semibold tracking-tight text-foreground">
                  {title}
                </h1>
                {description && (
                  <p className="mt-1 max-w-2xl text-sm text-muted-foreground">{description}</p>
                )}
              </div>
              {actions && <div className="flex items-center gap-2 text-xs">{actions}</div>}
            </div>

            {children}

            <footer className="pt-2 pb-6 text-center font-mono text-[10px] uppercase tracking-[0.2em] text-muted-foreground">
              Octopus · Port Operations Suite · v2.4 · Genoa Terminal
            </footer>
          </main>
        </SidebarInset>
      </div>
    </SidebarProvider>
  );
}

export function Panel({
  title,
  subtitle,
  actions,
  children,
  className,
}: {
  title?: string;
  subtitle?: string;
  actions?: ReactNode;
  children: ReactNode;
  className?: string;
}) {
  return (
    <section className={`surface-panel flex flex-col ${className ?? ""}`}>
      {(title || actions) && (
        <header className="flex items-center justify-between border-b border-border px-4 py-3">
          <div>
            {title && (
              <h2 className="font-display text-sm font-semibold tracking-tight text-foreground">
                {title}
              </h2>
            )}
            {subtitle && <p className="text-[11px] text-muted-foreground">{subtitle}</p>}
          </div>
          {actions}
        </header>
      )}
      {children}
    </section>
  );
}
