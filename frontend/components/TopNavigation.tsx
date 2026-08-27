import Link from "next/link";
import { GlobalSearch } from "./GlobalSearch";
import { LogoutButton } from "./LogoutButton";
import type { CurrentUser } from "@/lib/api-types";

export function TopNavigation({ user }: { user: CurrentUser }) {
  return (
    <header className="flex h-14 shrink-0 items-center justify-between border-b border-border bg-surface px-6">
      <Link href="/campaigns" className="font-serif text-lg font-semibold text-foreground hover:text-accent">
        Big If True
      </Link>
      <div className="flex items-center gap-4">
        <GlobalSearch />
        <span className="text-sm text-muted">{user.email}</span>
        <LogoutButton />
      </div>
    </header>
  );
}
