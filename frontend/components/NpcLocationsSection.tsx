"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { Badge } from "./Badge";
import { ConfirmationDialog } from "./ConfirmationDialog";
import { NpcLocationDialog } from "./NpcLocationDialog";
import { RELATIONSHIP_TYPE_LABELS } from "./npc-location-relationship-labels";
import { deleteNpcLocation, type Location, type Npc, type NpcLocation } from "@/lib/api";

interface NpcLocationsSectionProps {
  npc: Npc;
  campaignId: string;
  relationships: NpcLocation[];
  campaignLocations: Location[];
}

export function NpcLocationsSection({ npc, campaignId, relationships, campaignLocations }: NpcLocationsSectionProps) {
  const router = useRouter();
  const [addOpen, setAddOpen] = useState(false);
  const [editing, setEditing] = useState<NpcLocation | null>(null);
  const [removing, setRemoving] = useState<NpcLocation | null>(null);
  const [isRemoving, setIsRemoving] = useState(false);

  const availableLocations = campaignLocations.filter(
    (location) => !relationships.some((r) => r.locationId === location.id),
  );

  async function handleRemoveConfirm() {
    if (!removing) {
      return;
    }
    setIsRemoving(true);
    await deleteNpcLocation(removing.id);
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
                </div>
                <Badge>{RELATIONSHIP_TYPE_LABELS[relationship.relationshipType]}</Badge>
                {relationship.isPrimary && (
                  <span className="inline-flex items-center rounded-full border border-gold/40 bg-gold/10 px-3 py-1 text-xs font-medium text-gold">
                    ★ Primary
                  </span>
                )}
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
        <NpcLocationDialog
          open
          mode="create"
          npc={npc}
          availableLocations={availableLocations}
          onClose={() => setAddOpen(false)}
        />
      )}

      {editing && (
        <NpcLocationDialog
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
            ? `This will remove the link between ${npc.name} and "${removing.locationName}". This cannot be undone.`
            : ""
        }
        confirmLabel={isRemoving ? "Removing..." : "Remove"}
        onConfirm={handleRemoveConfirm}
        onCancel={() => setRemoving(null)}
      />
    </div>
  );
}
