import { useEffect, useRef } from "react";

const FOCUSABLE_SELECTOR =
  'a[href], button:not([disabled]), textarea:not([disabled]), input:not([disabled]), select:not([disabled]), [tabindex]:not([tabindex="-1"])';

/**
 * Shared minimum-complete modal keyboard behavior for this app's six
 * dialog-shaped components. Adding `aria-modal="true"` without this would be
 * actively misleading - it asserts to assistive tech that focus can't leave
 * the dialog, which isn't true without an actual trap behind it.
 *
 * Handles, for as long as `open` is true:
 * - focus moves into the dialog when it opens (first focusable element,
 *   falling back to the dialog container itself)
 * - Tab/Shift+Tab stay within the dialog's focusable elements
 * - Escape calls `onClose`
 * - focus returns to whatever was focused right before the dialog opened
 *
 * For components without their own `open` prop (e.g. QuestConnectionDialog,
 * which is mounted/unmounted directly by its parent instead), pass a
 * constant `true` - the same cleanup-on-unmount path restores focus
 * correctly either way.
 */
export function useDialogFocus(open: boolean, onClose: () => void) {
  const containerRef = useRef<HTMLDivElement>(null);
  const onCloseRef = useRef(onClose);

  // Keep the latest onClose reachable from the keydown listener below without
  // making that effect re-run (and re-bind) every render. Synced in an effect
  // rather than during render - refs must not be written while rendering.
  useEffect(() => {
    onCloseRef.current = onClose;
  });

  useEffect(() => {
    if (!open) {
      return;
    }

    const triggerElement = document.activeElement as HTMLElement | null;
    const container = containerRef.current;

    const focusable = container?.querySelectorAll<HTMLElement>(FOCUSABLE_SELECTOR);
    const firstFocusable = focusable && focusable.length > 0 ? focusable[0] : container;
    firstFocusable?.focus();

    function handleKeyDown(event: KeyboardEvent) {
      if (event.key === "Escape") {
        event.preventDefault();
        onCloseRef.current();
        return;
      }

      if (event.key !== "Tab" || !container) {
        return;
      }

      // Recomputed on every Tab press rather than captured once, so
      // dynamically-changing dialog content (e.g. a select's option list
      // opening) doesn't stale-trap focus against an outdated node list.
      const nodes = container.querySelectorAll<HTMLElement>(FOCUSABLE_SELECTOR);
      if (nodes.length === 0) {
        event.preventDefault();
        return;
      }

      const first = nodes[0];
      const last = nodes[nodes.length - 1];

      if (event.shiftKey && document.activeElement === first) {
        event.preventDefault();
        last.focus();
      } else if (!event.shiftKey && document.activeElement === last) {
        event.preventDefault();
        first.focus();
      }
    }

    document.addEventListener("keydown", handleKeyDown);
    return () => {
      document.removeEventListener("keydown", handleKeyDown);
      triggerElement?.focus();
    };
  }, [open]);

  return containerRef;
}
