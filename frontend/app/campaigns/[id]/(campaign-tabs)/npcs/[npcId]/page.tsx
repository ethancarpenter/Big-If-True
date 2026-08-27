import Link from "next/link";
import { notFound } from "next/navigation";
import { AlignmentBadge } from "@/components/AlignmentBadge";
import { Badge } from "@/components/Badge";
import { DeleteEntityButton } from "@/components/DeleteEntityButton";
import { NpcLocationsSection } from "@/components/NpcLocationsSection";
import { StatusBadge } from "@/components/StatusBadge";
import { NPC_STATUS_TONE } from "@/components/npc-status-tone";
import { ApiNotFoundError, getLocationsForCampaign, getNpc, getNpcLocationsForNpc } from "@/lib/api";

interface NpcDetailPageProps {
  params: Promise<{ id: string; npcId: string }>;
}

const profileFields: { label: string; key: "species" | "gender" | "age" | "occupation" | "disposition" }[] = [
  { label: "Species", key: "species" },
  { label: "Gender", key: "gender" },
  { label: "Age", key: "age" },
  { label: "Occupation", key: "occupation" },
  { label: "Disposition", key: "disposition" },
];

export default async function NpcDetailPage({ params }: NpcDetailPageProps) {
  const { id: campaignId, npcId } = await params;

  let npc;
  try {
    npc = await getNpc(npcId);
  } catch (error) {
    if (error instanceof ApiNotFoundError) {
      notFound();
    }
    throw error;
  }

  const presentFields = profileFields.filter((field) => npc[field.key] !== null && npc[field.key] !== "");
  const [relationships, campaignLocations] = await Promise.all([
    getNpcLocationsForNpc(npcId),
    getLocationsForCampaign(campaignId),
  ]);

  return (
    <div className="flex max-w-2xl flex-col gap-6">
      <div className="flex items-center gap-4">
        {npc.portraitUrl ? (
          // eslint-disable-next-line @next/next/no-img-element -- external, unpredictable-origin URLs
          <img
            src={npc.portraitUrl}
            alt={npc.name}
            className="h-16 w-16 shrink-0 rounded-full border border-border object-cover"
          />
        ) : (
          <div className="flex h-16 w-16 shrink-0 items-center justify-center rounded-full border border-border bg-surface text-xl font-semibold text-muted">
            {npc.name.charAt(0).toUpperCase()}
          </div>
        )}
        <div className="flex-1">
          <h2 className="font-serif text-2xl font-semibold text-foreground">{npc.name}</h2>
          <div className="mt-2 flex flex-wrap gap-2">
            {npc.class && <Badge>{npc.class}</Badge>}
            <AlignmentBadge alignment={npc.alignment} />
            <StatusBadge label={npc.status} tone={NPC_STATUS_TONE[npc.status]} />
          </div>
        </div>
        <div className="flex gap-3">
          <Link
            href={`/campaigns/${campaignId}/npcs/${npc.id}/edit`}
            className="rounded-md border border-border px-4 py-2 text-sm font-medium text-foreground hover:bg-white/5"
          >
            Edit
          </Link>
          <DeleteEntityButton
            kind="npc"
            id={npc.id}
            name={npc.name}
            redirectTo={`/campaigns/${campaignId}/npcs`}
          />
        </div>
      </div>

      {presentFields.length > 0 && (
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-3">
          {presentFields.map((field) => (
            <div key={field.key}>
              <p className="text-xs text-muted">{field.label}</p>
              <p className="text-sm text-foreground">{npc[field.key]}</p>
            </div>
          ))}
        </div>
      )}

      {npc.description ? (
        <p className="text-muted">{npc.description}</p>
      ) : (
        <p className="text-muted italic">No description yet.</p>
      )}

      {npc.dmNotes && (
        <div className="rounded-md border border-amber-500/30 bg-amber-500/10 p-4">
          <p className="text-xs font-semibold uppercase tracking-wide text-amber-400">DM Notes 🔒</p>
          <p className="mt-1 text-sm text-foreground">{npc.dmNotes}</p>
        </div>
      )}

      <NpcLocationsSection
        npc={npc}
        campaignId={campaignId}
        relationships={relationships}
        campaignLocations={campaignLocations}
      />

      <p className="text-xs text-muted">
        Created {new Date(npc.createdAt).toLocaleString()} · Updated{" "}
        {new Date(npc.updatedAt).toLocaleString()}
      </p>
    </div>
  );
}
