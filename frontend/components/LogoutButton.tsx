"use client";

import { useState } from "react";
import { logoutUser } from "@/lib/api";

export function LogoutButton() {
  const [isLoggingOut, setIsLoggingOut] = useState(false);

  async function handleLogout() {
    setIsLoggingOut(true);
    try {
      await logoutUser();
    } finally {
      // Full reload, not a client-side navigation - guarantees no stale
      // client state (e.g. a cached CSRF token from the old session)
      // lingers past logout, same reasoning as the global 401 handling.
      // eslint-disable-next-line @next/next/no-location-assign-relative-destination -- deliberate full reload, see comment above
      window.location.href = "/login";
    }
  }

  return (
    <button
      onClick={handleLogout}
      disabled={isLoggingOut}
      className="rounded-md border border-border px-3 py-1.5 text-sm font-medium text-foreground transition-opacity hover:bg-white/5 disabled:opacity-50"
    >
      {isLoggingOut ? "Logging out..." : "Log out"}
    </button>
  );
}
