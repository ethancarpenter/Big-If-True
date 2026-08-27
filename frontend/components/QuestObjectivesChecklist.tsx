"use client";

import { useRouter } from "next/navigation";
import { useState, type FormEvent } from "react";
import { ConfirmationDialog } from "./ConfirmationDialog";
import {
  createQuestObjective,
  updateQuestObjective,
  deleteQuestObjective,
  reorderQuestObjectives,
  type QuestObjective,
} from "@/lib/api";

interface QuestObjectivesChecklistProps {
  questId: string;
  objectives: QuestObjective[];
}

export function QuestObjectivesChecklist({ questId, objectives }: QuestObjectivesChecklistProps) {
  const router = useRouter();
  const [newDescription, setNewDescription] = useState("");
  const [isAdding, setIsAdding] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editingText, setEditingText] = useState("");
  const [removing, setRemoving] = useState<QuestObjective | null>(null);
  const [isRemoving, setIsRemoving] = useState(false);
  const [isBusy, setIsBusy] = useState(false);

  const sorted = [...objectives].sort((a, b) => a.sortOrder - b.sortOrder);

  async function handleAdd(event: FormEvent) {
    event.preventDefault();
    const description = newDescription.trim();
    if (!description) {
      return;
    }
    setIsAdding(true);
    await createQuestObjective(questId, { description });
    setNewDescription("");
    setIsAdding(false);
    router.refresh();
  }

  async function handleToggle(objective: QuestObjective) {
    setIsBusy(true);
    await updateQuestObjective(questId, objective.id, {
      description: objective.description,
      isCompleted: !objective.isCompleted,
    });
    setIsBusy(false);
    router.refresh();
  }

  function startEditing(objective: QuestObjective) {
    setEditingId(objective.id);
    setEditingText(objective.description);
  }

  async function handleSaveEdit(objective: QuestObjective) {
    const description = editingText.trim();
    if (!description) {
      return;
    }
    setIsBusy(true);
    await updateQuestObjective(questId, objective.id, {
      description,
      isCompleted: objective.isCompleted,
    });
    setIsBusy(false);
    setEditingId(null);
    router.refresh();
  }

  async function handleRemoveConfirm() {
    if (!removing) {
      return;
    }
    setIsRemoving(true);
    await deleteQuestObjective(questId, removing.id);
    setIsRemoving(false);
    setRemoving(null);
    router.refresh();
  }

  async function handleMove(index: number, direction: -1 | 1) {
    const targetIndex = index + direction;
    if (targetIndex < 0 || targetIndex >= sorted.length) {
      return;
    }
    const newOrder = [...sorted];
    [newOrder[index], newOrder[targetIndex]] = [newOrder[targetIndex], newOrder[index]];
    setIsBusy(true);
    await reorderQuestObjectives(questId, { objectiveIds: newOrder.map((o) => o.id) });
    setIsBusy(false);
    router.refresh();
  }

  return (
    <div className="flex flex-col gap-3">
      <h3 className="text-sm font-semibold text-foreground">Objectives</h3>

      {sorted.length === 0 ? (
        <p className="text-sm italic text-muted">No objectives yet.</p>
      ) : (
        <ul className="flex flex-col gap-2">
          {sorted.map((objective, index) => (
            <li
              key={objective.id}
              className="flex items-center gap-3 rounded-md border border-border bg-surface px-3 py-2"
            >
              <input
                type="checkbox"
                checked={objective.isCompleted}
                onChange={() => handleToggle(objective)}
                disabled={isBusy}
                className="h-4 w-4 shrink-0 rounded border-border"
              />

              {editingId === objective.id ? (
                <div className="flex flex-1 items-center gap-2">
                  <input
                    value={editingText}
                    onChange={(e) => setEditingText(e.target.value)}
                    maxLength={500}
                    className="flex-1 rounded-md border border-border bg-background px-2 py-1 text-sm text-foreground outline-none focus:border-accent"
                  />
                  <button onClick={() => handleSaveEdit(objective)} className="text-sm text-accent hover:underline">
                    Save
                  </button>
                  <button onClick={() => setEditingId(null)} className="text-sm text-muted hover:text-foreground">
                    Cancel
                  </button>
                </div>
              ) : (
                <>
                  <span
                    className={`flex-1 text-sm ${objective.isCompleted ? "text-muted line-through" : "text-foreground"}`}
                  >
                    {objective.description}
                  </span>
                  <button
                    onClick={() => handleMove(index, -1)}
                    disabled={index === 0 || isBusy}
                    className="text-muted hover:text-foreground disabled:opacity-30"
                    aria-label="Move up"
                  >
                    ↑
                  </button>
                  <button
                    onClick={() => handleMove(index, 1)}
                    disabled={index === sorted.length - 1 || isBusy}
                    className="text-muted hover:text-foreground disabled:opacity-30"
                    aria-label="Move down"
                  >
                    ↓
                  </button>
                  <button onClick={() => startEditing(objective)} className="text-sm text-muted hover:text-foreground">
                    Edit
                  </button>
                  <button onClick={() => setRemoving(objective)} className="text-sm text-red-400 hover:text-red-300">
                    Delete
                  </button>
                </>
              )}
            </li>
          ))}
        </ul>
      )}

      <form onSubmit={handleAdd} className="flex gap-2">
        <input
          value={newDescription}
          onChange={(e) => setNewDescription(e.target.value)}
          placeholder="Add an objective..."
          maxLength={500}
          className="flex-1 rounded-md border border-border bg-surface px-3 py-2 text-sm text-foreground outline-none focus:border-accent"
        />
        <button
          type="submit"
          disabled={isAdding || !newDescription.trim()}
          className="rounded-md border border-border px-4 py-2 text-sm font-medium text-foreground hover:bg-white/5 disabled:opacity-50"
        >
          Add
        </button>
      </form>

      <ConfirmationDialog
        open={removing !== null}
        title="Delete objective?"
        description={
          removing ? `This will remove "${removing.description}" from the checklist. This cannot be undone.` : ""
        }
        confirmLabel={isRemoving ? "Deleting..." : "Delete"}
        onConfirm={handleRemoveConfirm}
        onCancel={() => setRemoving(null)}
      />
    </div>
  );
}
