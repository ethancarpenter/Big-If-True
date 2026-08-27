"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { Badge } from "./Badge";
import { ConfirmationDialog } from "./ConfirmationDialog";
import { QuestLocationDialog } from "./QuestLocationDialog";
import { QUEST_LOCATION_ROLE_LABELS } from "./quest-location-role-labels";
import { deleteQuestLocation, type Location, type Quest, type QuestLocation } from "@/lib/api";

interface QuestLocationsSectionProps {
  quest: Quest;
  campaignId: string;
  relationships: QuestLocation[];
  campaignLocations: Location[];
}

export function QuestLocationsSection({
  quest,
  campaignId,
  relationships,
  campaignLocations,
}: QuestLocationsSectionProps) {
  const router = useRouter();
  const [addOpen, setAddOpen] = useState(false);
  const [editing, setEditing] = useState<QuestLocation | null>(null);
  const [removing, setRemoving] = useState<QuestLocation | null>(null);
  const [isRemoving, setIsRemoving] = useState(false);

  const availableLocations = campaignLocations.filter(
    (location) => !relationships.some((r) => r.locationId === location.id),
  );

  async function handleRemoveConfirm() {
    if (!removing) {
      return;
    }
    setIsRemoving(true);
    await deleteQuestLocation(removing.id);
    setIsRemoving(false);
    setRemoving(null);
    router.refresh();
  }

  return (
    <div className="flex flex-col gap-4">
      <div className="flex items-center justify-between">
        <h3 className="text-sm font-semibold text-foreground">Locations</h3>
        <button
          onClick={() => setAddOpen(true)}
          className="rounded-md border border-border px-3 py-1.5 text-sm font-medium text-foreground hover:bg-white/5"
        >
          + Add Location
        </button>
      </div>

      {relationships.length === 0 ? (
        <p className="text-sm italic text-muted">Not linked to any locations yet.</p>
      ) : (
        <ul className="flex flex-col gap-2">
          {relationships.map((relationship) => (
            <li
              key={relationship.id}
              className="flex flex-wrap items-center justify-between gap-3 rounded-md border border-border bg-surface px-4 py-3"
            >
              <div className="flex flex-wrap items-center gap-3">
                <div>
                  <Link
                    href={`/campaigns/${campaignId}/locations/${relationship.locationId}`}
                    className="text-sm font-medium text-accent hover:underline"
                  >
                    {relationship.locationName}
                  </Link>
                  <p className="text-xs text-muted">{relationship.cityName}</p>
                  {relationship.notes && <p className="text-xs text-muted">{relationship.notes}</p>}
                </div>
                <Badge>{QUEST_LOCATION_ROLE_LABELS[relationship.role]}</Badge>
              </div>
              <div className="flex gap-3">
                <button
                  onClick={() => setEditing(relationship)}
                  className="text-sm text-muted hover:text-foreground"
                >
                  Edit
                </button>
                <button
                  onClick={() => setRemoving(relationship)}
                  className="text-sm text-red-400 hover:text-red-300"
                >
                  Remove
                </button>
              </div>
            </li>
          ))}
        </ul>
      )}

      {addOpen && (
        <QuestLocationDialog
          open
          mode="create"
          quest={quest}
          availableLocations={availableLocations}
          onClose={() => setAddOpen(false)}
        />
      )}

      {editing && (
        <QuestLocationDialog
          open={editing !== null}
          mode="edit"
          relationship={editing}
          onClose={() => setEditing(null)}
        />
      )}

      <ConfirmationDialog
        open={removing !== null}
        title="Remove location relationship?"
        description={
          removing
            ? `This will remove the link between "${quest.name}" and "${removing.locationName}". This cannot be undone.`
            : ""
        }
        confirmLabel={isRemoving ? "Removing..." : "Remove"}
        onConfirm={handleRemoveConfirm}
        onCancel={() => setRemoving(null)}
      />
    </div>
  );
}
