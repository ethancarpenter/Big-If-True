"use client";

import { useEffect, useId, useState } from "react";
import { NavLinks } from "./NavLinks";
import { useDialogFocus } from "./useDialogFocus";

/**
 * Below `md`, the desktop `Sidebar` is hidden - this hamburger + slide-out
 * drawer is the primary nav in its place. Reuses `NavLinks` (single nav
 * definition) and `useDialogFocus` (focus-in, Tab trap, Escape-to-close,
 * focus-return-to-hamburger). Selecting a link, clicking the backdrop, or
 * pressing Escape all close it. Rendered inside `TopNavigation`.
 */
export function MobileNav() {
  const [open, setOpen] = useState(false);
  const drawerId = useId();
  const close = () => setOpen(false);
  const containerRef = useDialogFocus(open, close);

  // Stop the page behind the drawer from scrolling while it's open. Local to
  // this component - restores whatever `overflow` was there before on close
  // or unmount.
  useEffect(() => {
    if (!open) {
      return;
    }
    const previousOverflow = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    return () => {
      document.body.style.overflow = previousOverflow;
    };
  }, [open]);

  return (
    <div className="md:hidden">
      <button
        type="button"
        onClick={() => setOpen(true)}
        aria-label="Open navigation"
        aria-expanded={open}
        aria-controls={drawerId}
        className="flex h-9 w-9 items-center justify-center rounded-md text-foreground hover:bg-white/5"
      >
        <svg
          aria-hidden="true"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          strokeWidth="2"
          strokeLinecap="round"
          className="h-5 w-5"
        >
          <line x1="3" y1="6" x2="21" y2="6" />
          <line x1="3" y1="12" x2="21" y2="12" />
          <line x1="3" y1="18" x2="21" y2="18" />
        </svg>
      </button>

      {open && (
        <div className="fixed inset-0 z-50 bg-black/60" onClick={close}>
          <div
            ref={containerRef}
            id={drawerId}
            role="dialog"
            aria-modal="true"
            aria-label="Navigation"
            onClick={(e) => e.stopPropagation()}
            className="h-full w-64 max-w-[80%] border-r border-border bg-surface p-4"
          >
            <div className="mb-4 flex items-center justify-between">
              <span className="font-serif text-lg font-semibold text-foreground">Big If True</span>
              <button
                type="button"
                onClick={close}
                aria-label="Close navigation"
                className="flex h-9 w-9 items-center justify-center rounded-md text-foreground hover:bg-white/5"
              >
                <svg
                  aria-hidden="true"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2"
                  strokeLinecap="round"
                  className="h-5 w-5"
                >
                  <line x1="6" y1="6" x2="18" y2="18" />
                  <line x1="6" y1="18" x2="18" y2="6" />
                </svg>
              </button>
            </div>
            <NavLinks onNavigate={close} />
          </div>
        </div>
      )}
    </div>
  );
}
