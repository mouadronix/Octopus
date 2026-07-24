import { Link, useRouterState } from "@tanstack/react-router";
import {
  Anchor,
  LayoutDashboard,
  Ship,
  CalendarClock,
  Waves,
  Radio,
  Users,
  FileBarChart,
  Settings,
  LifeBuoy,
} from "lucide-react";

import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  useSidebar,
} from "@/components/ui/sidebar";

const operations = [
  { title: "Control Tower", url: "/", icon: LayoutDashboard },
  { title: "Vessels", url: "/vessels", icon: Ship },
  { title: "Berths", url: "/berths", icon: Anchor },
  { title: "Scheduling", url: "/scheduling", icon: CalendarClock },
  { title: "Tides & Weather", url: "/tides", icon: Waves },
  { title: "AIS Feed", url: "/ais", icon: Radio },
];

const admin = [
  { title: "Operators", url: "/operators", icon: Users },
  { title: "Reports", url: "/reports", icon: FileBarChart },
  { title: "Settings", url: "/settings", icon: Settings },
];

export function AppSidebar() {
  const { state } = useSidebar();
  const collapsed = state === "collapsed";
  const currentPath = useRouterState({
    select: (r) => r.location.pathname,
  });
  const isActive = (path: string) => currentPath === path;

  return (
    <Sidebar collapsible="icon" className="border-r border-sidebar-border">
      <SidebarHeader className="border-b border-sidebar-border/60">
        <div className="flex items-center gap-2.5 px-2 py-2">
          <div className="grid h-8 w-8 place-items-center rounded-md bg-white/10 ring-1 ring-white/15">
            <span className="font-display text-sm font-bold text-sidebar-primary">O</span>
          </div>
          {!collapsed && (
            <div className="flex flex-col leading-tight">
              <span className="font-display text-sm font-semibold tracking-tight text-sidebar-primary">
                OCTOPUS
              </span>
              <span className="text-[10px] uppercase tracking-[0.18em] text-sidebar-foreground/60">
                Port Operations
              </span>
            </div>
          )}
        </div>
      </SidebarHeader>

      <SidebarContent>
        <SidebarGroup>
          <SidebarGroupLabel className="text-[10px] font-medium uppercase tracking-[0.18em] text-sidebar-foreground/50">
            Operations
          </SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu>
              {operations.map((item) => (
                <SidebarMenuItem key={item.title}>
                  <SidebarMenuButton
                    asChild
                    isActive={isActive(item.url)}
                    tooltip={item.title}
                  >
                    <Link to={item.url} className="flex items-center gap-2.5">
                      <item.icon className="h-4 w-4" />
                      {!collapsed && <span>{item.title}</span>}
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              ))}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>

        <SidebarGroup>
          <SidebarGroupLabel className="text-[10px] font-medium uppercase tracking-[0.18em] text-sidebar-foreground/50">
            Administration
          </SidebarGroupLabel>
          <SidebarGroupContent>
            <SidebarMenu>
              {admin.map((item) => (
                <SidebarMenuItem key={item.title}>
                  <SidebarMenuButton asChild isActive={isActive(item.url)} tooltip={item.title}>
                    <Link to={item.url} className="flex items-center gap-2.5">
                      <item.icon className="h-4 w-4" />
                      {!collapsed && <span>{item.title}</span>}
                    </Link>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              ))}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      </SidebarContent>

      <SidebarFooter className="border-t border-sidebar-border/60">
        {!collapsed ? (
          <div className="mx-1 my-1 rounded-md bg-white/[0.04] p-3 ring-1 ring-white/10">
            <div className="flex items-center gap-2 text-xs text-sidebar-foreground/80">
              <LifeBuoy className="h-3.5 w-3.5" />
              24/7 Operations Support
            </div>
            <p className="mt-1 font-mono text-[11px] text-sidebar-foreground/60">
              +39 010 998 4412
            </p>
          </div>
        ) : (
          <div className="grid h-8 w-8 place-items-center">
            <LifeBuoy className="h-4 w-4 text-sidebar-foreground/70" />
          </div>
        )}
      </SidebarFooter>
    </Sidebar>
  );
}
