"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { navItems } from "./nav-items";

interface NavLinksProps {
  /** Fired after a nav link is clicked - lets the mobile drawer close itself. */
  onNavigate?: () => void;
}

/**
 * The app's primary navigation link list, shared by the desktop `Sidebar`
 * and the mobile drawer in `MobileNav`. Nav definition lives in
 * `nav-items.ts` - this component only renders it and owns active-route
 * highlighting.
 */
export function NavLinks({ onNavigate }: NavLinksProps) {
  const pathname = usePathname();

  return (
    <nav className="flex flex-col gap-1">
      {navItems.map((item) => {
        const isActive = pathname === item.href || pathname.startsWith(`${item.href}/`);
        return (
          <Link
            key={item.label}
            href={item.href}
            onClick={onNavigate}
            className={`rounded-md px-3 py-2 text-sm font-medium transition-colors ${
              isActive
                ? "bg-accent/20 text-accent"
                : "text-muted hover:bg-white/5 hover:text-foreground"
            }`}
          >
            {item.label}
          </Link>
        );
      })}
    </nav>
  );
}
