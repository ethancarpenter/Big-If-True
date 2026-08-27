"use client";

import { useRouter } from "next/navigation";
import { useState, type FormEvent } from "react";
import {
  createQuestLocation,
  updateQuestLocation,
  ApiConflictError,
  QUEST_LOCATION_ROLES,
  type Location,
  type Quest,
  type QuestLocation,
  type QuestLocationRole,
} from "@/lib/api";
import { QUEST_LOCATION_ROLE_LABELS } from "./quest-location-role-labels";

type QuestLocationDialogProps =
  | {
      open: boolean;
      mode: "create";
      quest: Quest;
      availableLocations: Location[];
      onClose: () => void;
    }
  | {
      open: boolean;
      mode: "edit";
      relationship: QuestLocation;
      onClose: () => void;
    };

export function QuestLocationDialog(props: QuestLocationDialogProps) {
  const router = useRouter();
  const isEdit = props.mode === "edit";

  const [locationId, setLocationId] = useState(isEdit ? "" : (props.availableLocations[0]?.id ?? ""));
  const [role, setRole] = useState<QuestLocationRole>(
    isEdit ? props.relationship.role : QUEST_LOCATION_ROLES[0],
  );
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
        await updateQuestLocation(props.relationship.id, { role, notes: notes.trim() || undefined });
      } else {
        await createQuestLocation(props.quest.id, { locationId, role, notes: notes.trim() || undefined });
      }
      router.refresh();
      props.onClose();
    } catch (err) {
      if (err instanceof ApiConflictError) {
        setError("This quest is already linked to that location.");
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
          {isEdit ? "Edit Location Relationship" : "Add Location"}
        </h2>

        <form onSubmit={handleSubmit} className="mt-4 flex flex-col gap-4">
          {props.mode === "create" && (
            <div className="flex flex-col gap-1.5">
              <label htmlFor="locationId" className="text-sm font-medium text-foreground">
                Location
              </label>
              {props.availableLocations.length === 0 ? (
                <p className="text-sm text-muted">
                  This quest is already linked to every location in this campaign.
                </p>
              ) : (
                <select
                  id="locationId"
                  value={locationId}
                  onChange={(e) => setLocationId(e.target.value)}
                  required
                  className="rounded-md border border-border bg-background px-3 py-2 text-foreground outline-none focus:border-accent"
                >
                  {props.availableLocations.map((location) => (
                    <option key={location.id} value={location.id}>
                      {location.name} ({location.cityName})
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
              onChange={(e) => setRole(e.target.value as QuestLocationRole)}
              className="rounded-md border border-border bg-background px-3 py-2 text-foreground outline-none focus:border-accent"
            >
              {QUEST_LOCATION_ROLES.map((option) => (
                <option key={option} value={option}>
                  {QUEST_LOCATION_ROLE_LABELS[option]}
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
              disabled={isSubmitting || (props.mode === "create" && props.availableLocations.length === 0)}
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
