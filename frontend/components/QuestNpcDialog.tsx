"use client";

import { useRouter } from "next/navigation";
import { useState, type FormEvent } from "react";
import {
  createQuestNpc,
  updateQuestNpc,
  ApiConflictError,
  QUEST_NPC_ROLES,
  type Npc,
  type Quest,
  type QuestNpc,
  type QuestNpcRole,
} from "@/lib/api";
import { QUEST_NPC_ROLE_LABELS } from "./quest-npc-role-labels";

type QuestNpcDialogProps =
  | {
      open: boolean;
      mode: "create";
      quest: Quest;
      availableNpcs: Npc[];
      onClose: () => void;
    }
  | {
      open: boolean;
      mode: "edit";
      relationship: QuestNpc;
      onClose: () => void;
    };

export function QuestNpcDialog(props: QuestNpcDialogProps) {
  const router = useRouter();
  const isEdit = props.mode === "edit";

  const [npcId, setNpcId] = useState(isEdit ? "" : (props.availableNpcs[0]?.id ?? ""));
  const [role, setRole] = useState<QuestNpcRole>(isEdit ? props.relationship.role : QUEST_NPC_ROLES[0]);
  const [notes, setNotes] = useState(isEdit ? (props.relationship.notes ?? "") : "");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  if (!props.open) {
    return null;
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      if (props.mode === "edit") {
        await updateQuestNpc(props.relationship.id, { role, notes: notes.trim() || undefined });
      } else {
        await createQuestNpc(props.quest.id, { npcId, role, notes: notes.trim() || undefined });
      }
      router.refresh();
      props.onClose();
    } catch (err) {
      if (err instanceof ApiConflictError) {
        setError("This quest is already linked to that NPC.");
      } else {
        setError("Something went wrong saving this relationship. Please try again.");
      }
      setIsSubmitting(false);
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60">
      <div className="w-full max-w-sm rounded-lg border border-border bg-surface p-6">
        <h2 className="text-lg font-semibold text-foreground">
          {isEdit ? "Edit NPC Relationship" : "Add NPC"}
        </h2>

        <form onSubmit={handleSubmit} className="mt-4 flex flex-col gap-4">
          {props.mode === "create" && (
            <div className="flex flex-col gap-1.5">
              <label htmlFor="npcId" className="text-sm font-medium text-foreground">
                NPC
              </label>
              {props.availableNpcs.length === 0 ? (
                <p className="text-sm text-muted">
                  This quest is already linked to every NPC in this campaign.
                </p>
              ) : (
                <select
                  id="npcId"
                  value={npcId}
                  onChange={(e) => setNpcId(e.target.value)}
                  required
                  className="rounded-md border border-border bg-background px-3 py-2 text-foreground outline-none focus:border-accent"
                >
                  {props.availableNpcs.map((npc) => (
                    <option key={npc.id} value={npc.id}>
                      {npc.name}
                    </option>
                  ))}
                </select>
              )}
            </div>
          )}

          <div className="flex flex-col gap-1.5">
            <label htmlFor="role" className="text-sm font-medium text-foreground">
              Role
            </label>
            <select
              id="role"
              value={role}
              onChange={(e) => setRole(e.target.value as QuestNpcRole)}
              className="rounded-md border border-border bg-background px-3 py-2 text-foreground outline-none focus:border-accent"
            >
              {QUEST_NPC_ROLES.map((option) => (
                <option key={option} value={option}>
                  {QUEST_NPC_ROLE_LABELS[option]}
                </option>
              ))}
            </select>
          </div>

          <div className="flex flex-col gap-1.5">
            <label htmlFor="notes" className="text-sm font-medium text-foreground">
              Notes
            </label>
            <textarea
              id="notes"
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
              rows={3}
              className="rounded-md border border-border bg-background px-3 py-2 text-foreground outline-none focus:border-accent"
              placeholder="Optional context about this relationship"
            />
          </div>

          {error && <p className="text-sm text-red-400">{error}</p>}

          <div className="mt-2 flex justify-end gap-3">
            <button
              type="button"
              onClick={props.onClose}
              className="rounded-md px-4 py-2 text-sm font-medium text-foreground hover:bg-white/5"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={isSubmitting || (props.mode === "create" && props.availableNpcs.length === 0)}
              className="rounded-md bg-accent px-4 py-2 text-sm font-medium text-accent-foreground transition-opacity hover:opacity-90 disabled:opacity-50"
            >
              {isSubmitting ? "Saving..." : isEdit ? "Save Changes" : "Add"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
