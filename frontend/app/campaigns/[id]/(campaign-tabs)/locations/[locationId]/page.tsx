import Link from "next/link";
import { notFound } from "next/navigation";
import { Badge } from "@/components/Badge";
import { DeleteEntityButton } from "@/components/DeleteEntityButton";
import { RELATIONSHIP_TYPE_LABELS } from "@/components/npc-location-relationship-labels";
import { ApiNotFoundError, getLocation, getNpcLocationsForLocation } from "@/lib/api";

interface LocationDetailPageProps {
  params: Promise<{ id: string; locationId: string }>;
}

export default async function LocationDetailPage({ params }: LocationDetailPageProps) {
  const { id: campaignId, locationId } = await params;

  let location;
  try {
    location = await getLocation(locationId);
  } catch (error) {
    if (error instanceof ApiNotFoundError) {
      notFound();
    }
    throw error;
  }

  const npcRelationships = await getNpcLocationsForLocation(locationId);

  return (
    <div className="flex max-w-2xl flex-col gap-6">
      <div className="flex items-center justify-between">
        <h2 className="font-serif text-2xl font-semibold text-foreground">{location.name}</h2>
        <div className="flex gap-3">
          <Link
            href={`/campaigns/${campaignId}/locations/${location.id}/edit`}
            className="rounded-md border border-border px-4 py-2 text-sm font-medium text-foreground hover:bg-white/5"
          >
            Edit
          </Link>
          <DeleteEntityButton
            kind="location"
            id={location.id}
            name={location.name}
            redirectTo={`/campaigns/${campaignId}/locations`}
          />
        </div>
      </div>

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
        <div>
          <p className="text-xs text-muted">Type</p>
          <Badge className="mt-1">{location.type}</Badge>
        </div>
        <div>
          <p className="text-xs text-muted">City</p>
          <Link
            href={`/campaigns/${campaignId}/cities/${location.cityId}`}
            className="text-sm text-accent hover:underline"
          >
            {location.cityName}
          </Link>
        </div>
      </div>

      {location.description ? (
        <p className="text-muted">{location.description}</p>
      ) : (
        <p className="text-muted italic">No description yet.</p>
      )}

      {location.dmNotes && (
        <div className="rounded-md border border-amber-500/30 bg-amber-500/10 p-4">
          <p className="text-xs font-semibold uppercase tracking-wide text-amber-400">DM Notes 🔒</p>
          <p className="mt-1 text-sm text-foreground">{location.dmNotes}</p>
        </div>
      )}

      <div className="flex flex-col gap-3">
        <h3 className="text-sm font-semibold text-foreground">NPCs</h3>
        {npcRelationships.length === 0 ? (
          <p className="text-sm italic text-muted">No NPCs linked to this location yet.</p>
        ) : (
          <ul className="flex flex-col gap-2">
            {npcRelationships.map((relationship) => (
              <li
                key={relationship.id}
                className="flex items-center gap-3 rounded-md border border-border bg-surface px-4 py-3"
              >
                <Link
                  href={`/campaigns/${campaignId}/npcs/${relationship.npcId}`}
                  className="text-sm font-medium text-accent hover:underline"
                >
                  {relationship.npcName}
                </Link>
                <Badge>{RELATIONSHIP_TYPE_LABELS[relationship.relationshipType]}</Badge>
                {relationship.isPrimary && (
                  <span className="inline-flex items-center rounded-full border border-gold/40 bg-gold/10 px-3 py-1 text-xs font-medium text-gold">
                    ★ Primary
                  </span>
                )}
              </li>
            ))}
          </ul>
        )}
      </div>

      <p className="text-xs text-muted">
        Created {new Date(location.createdAt).toLocaleString()} · Updated{" "}
        {new Date(location.updatedAt).toLocaleString()}
      </p>
    </div>
  );
}
