"use client";

import { useId } from "react";
import { useDialogFocus } from "./useDialogFocus";

interface ConfirmationDialogProps {
  open: boolean;
  title: string;
  description: string;
  confirmLabel?: string;
  onConfirm: () => void;
  onCancel: () => void;
}

export function ConfirmationDialog({
  open,
  title,
  description,
  confirmLabel = "Confirm",
  onConfirm,
  onCancel,
}: ConfirmationDialogProps) {
  const containerRef = useDialogFocus(open, onCancel);
  const titleId = useId();

  if (!open) {
    return null;
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60">
      <div
        ref={containerRef}
        role="dialog"
        aria-modal="true"
        aria-labelledby={titleId}
        className="w-full max-w-sm rounded-lg border border-border bg-surface p-6"
      >
        <h2 id={titleId} className="text-lg font-semibold text-foreground">{title}</h2>
        <p className="mt-2 text-sm text-muted">{description}</p>
        <div className="mt-6 flex justify-end gap-3">
          <button
            onClick={onCancel}
            className="rounded-md px-4 py-2 text-sm font-medium text-foreground hover:bg-white/5"
          >
            Cancel
          </button>
          <button
            onClick={onConfirm}
            className="rounded-md bg-red-600 px-4 py-2 text-sm font-medium text-white hover:bg-red-500"
          >
            {confirmLabel}
          </button>
        </div>
      </div>
    </div>
  );
}
