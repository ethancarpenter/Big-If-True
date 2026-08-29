import Link from "next/link";
import { GlobalSearch } from "./GlobalSearch";
import { LogoutButton } from "./LogoutButton";
import { MobileNav } from "./MobileNav";
import type { CurrentUser } from "@/lib/api-types";

export function TopNavigation({ user }: { user: CurrentUser }) {
  return (
    <header className="flex h-14 shrink-0 items-center justify-between border-b border-border bg-surface px-6">
      <div className="flex items-center gap-2">
        <MobileNav />
        <Link href="/campaigns" className="font-serif text-lg font-semibold text-foreground hover:text-accent">
          Big If True
        </Link>
      </div>
      <div className="flex items-center gap-4">
        <GlobalSearch />
        <span className="hidden text-sm text-muted sm:inline">{user.email}</span>
        <LogoutButton />
      </div>
    </header>
  );
}
