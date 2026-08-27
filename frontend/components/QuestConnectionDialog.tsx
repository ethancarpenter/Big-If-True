"use client";

import { useState, type FormEvent } from "react";
import { QUEST_CONNECTION_TYPES, type QuestConnectionType } from "@/lib/api";
import { QUEST_CONNECTION_TYPE_LABELS } from "./quest-connection-type-labels";

interface QuestConnectionDialogProps {
  sourceQuestName: string;
  targetQuestName: string;
  onConfirm: (connectionType: QuestConnectionType) => Promise<void>;
  onCancel: () => void;
}

export function QuestConnectionDialog({
  sourceQuestName,
  targetQuestName,
  onConfirm,
  onCancel,
}: QuestConnectionDialogProps) {
  const [connectionType, setConnectionType] = useState<QuestConnectionType>(QUEST_CONNECTION_TYPES[0]);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);
    try {
      await onConfirm(connectionType);
    } catch (err) {
      setError(err instanceof Error && err.message ? err.message : "Something went wrong. Please try again.");
      setIsSubmitting(false);
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60">
      <div className="w-full max-w-sm rounded-lg border border-border bg-surface p-6">
        <h2 className="text-lg font-semibold text-foreground">New Connection</h2>
        <p className="mt-1 text-sm text-muted">
          {sourceQuestName} <span className="text-accent">→</span> {targetQuestName}
        </p>

        <form onSubmit={handleSubmit} className="mt-4 flex flex-col gap-4">
          <div className="flex flex-col gap-1.5">
            <label htmlFor="connectionType" className="text-sm font-medium text-foreground">
              Connection Type
            </label>
            <select
              id="connectionType"
              value={connectionType}
              onChange={(e) => setConnectionType(e.target.value as QuestConnectionType)}
              className="rounded-md border border-border bg-background px-3 py-2 text-foreground outline-none focus:border-accent"
            >
              {QUEST_CONNECTION_TYPES.map((option) => (
                <option key={option} value={option}>
                  {QUEST_CONNECTION_TYPE_LABELS[option]}
                </option>
              ))}
            </select>
          </div>

          {error && <p className="text-sm text-red-400">{error}</p>}

          <div className="mt-2 flex justify-end gap-3">
            <button
              type="button"
              onClick={onCancel}
              className="rounded-md px-4 py-2 text-sm font-medium text-foreground hover:bg-white/5"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className="rounded-md bg-accent px-4 py-2 text-sm font-medium text-accent-foreground transition-opacity hover:opacity-90 disabled:opacity-50"
            >
              {isSubmitting ? "Saving..." : "Add"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
