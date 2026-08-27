"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

export interface Tab {
  label: string;
  href: string;
}

export function TabNavigation({ tabs }: { tabs: Tab[] }) {
  const pathname = usePathname();

  // Longest-prefix match so a nested route (e.g. /campaigns/1/cities/2)
  // still highlights its parent tab (e.g. /campaigns/1/cities) without
  // getting matched by a shorter sibling tab like /campaigns/1.
  const activeHref = tabs
    .filter((tab) => pathname === tab.href || pathname.startsWith(`${tab.href}/`))
    .sort((a, b) => b.href.length - a.href.length)[0]?.href;

  return (
    <div className="flex gap-6 border-b border-border">
      {tabs.map((tab) => (
        <Link
          key={tab.href}
          href={tab.href}
          className={`border-b-2 px-1 pb-3 text-sm font-medium transition-colors ${
            tab.href === activeHref
              ? "border-accent text-foreground"
              : "border-transparent text-muted hover:text-foreground"
          }`}
        >
          {tab.label}
        </Link>
      ))}
    </div>
  );
}
