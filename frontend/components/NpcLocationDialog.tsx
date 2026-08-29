"use client";

import { useRouter } from "next/navigation";
import { useId, useState, type FormEvent } from "react";
import { useDialogFocus } from "./useDialogFocus";
import {
  createNpcLocation,
  updateNpcLocation,
  ApiConflictError,
  NPC_LOCATION_RELATIONSHIP_TYPES,
  type Location,
  type Npc,
  type NpcLocation,
  type NpcLocationRelationshipType,
} from "@/lib/api";
import { RELATIONSHIP_TYPE_LABELS } from "./npc-location-relationship-labels";

type NpcLocationDialogProps =
  | {
      open: boolean;
      mode: "create";
      npc: Npc;
      availableLocations: Location[];
      onClose: () => void;
    }
  | {
      open: boolean;
      mode: "edit";
      relationship: NpcLocation;
      onClose: () => void;
    };

export function NpcLocationDialog(props: NpcLocationDialogProps) {
  const router = useRouter();
  const isEdit = props.mode === "edit";

  const [locationId, setLocationId] = useState(isEdit ? "" : (props.availableLocations[0]?.id ?? ""));
  const [relationshipType, setRelationshipType] = useState<NpcLocationRelationshipType>(
    isEdit ? props.relationship.relationshipType : NPC_LOCATION_RELATIONSHIP_TYPES[0],
  );
  const [isPrimary, setIsPrimary] = useState(isEdit ? props.relationship.isPrimary : false);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const containerRef = useDialogFocus(props.open, props.onClose);
  const titleId = useId();

  if (!props.open) {
    return null;
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      if (props.mode === "edit") {
        await updateNpcLocation(props.relationship.id, { relationshipType, isPrimary });
      } else {
        await createNpcLocation(props.npc.id, { locationId, relationshipType, isPrimary });
      }
      router.refresh();
      props.onClose();
    } catch (err) {
      if (err instanceof ApiConflictError) {
        setError("This NPC is already linked to that location.");
      } else {
        setError("Something went wrong saving this relationship. Please try again.");
      }
      setIsSubmitting(false);
    }
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
        <h2 id={titleId} className="text-lg font-semibold text-foreground">
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
                  This NPC is already linked to every location in this campaign.
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
            <label htmlFor="relationshipType" className="text-sm font-medium text-foreground">
              Relationship
            </label>
            <select
              id="relationshipType"
              value={relationshipType}
              onChange={(e) => setRelationshipType(e.target.value as NpcLocationRelationshipType)}
              className="rounded-md border border-border bg-background px-3 py-2 text-foreground outline-none focus:border-accent"
            >
              {NPC_LOCATION_RELATIONSHIP_TYPES.map((option) => (
                <option key={option} value={option}>
                  {RELATIONSHIP_TYPE_LABELS[option]}
                </option>
              ))}
            </select>
          </div>

          <label className="flex items-center gap-2 text-sm text-foreground">
            <input
              type="checkbox"
              checked={isPrimary}
              onChange={(e) => setIsPrimary(e.target.checked)}
              className="h-4 w-4 rounded border-border"
            />
            Primary location
          </label>

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
