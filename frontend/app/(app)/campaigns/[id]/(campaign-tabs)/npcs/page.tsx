import Link from "next/link";
import { NpcCard } from "@/components/NpcCard";
import { getNpcsForCampaign } from "@/lib/server-api";

interface NpcsPageProps {
  params: Promise<{ id: string }>;
}

export default async function NpcsPage({ params }: NpcsPageProps) {
  const { id: campaignId } = await params;
  const npcs = await getNpcsForCampaign(campaignId);

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <h2 className="text-lg font-semibold text-foreground">NPCs</h2>
        <Link
          href={`/campaigns/${campaignId}/npcs/new`}
          className="rounded-md bg-accent px-4 py-2 text-sm font-medium text-accent-foreground hover:opacity-90"
        >
          + New NPC
        </Link>
      </div>

      {npcs.length === 0 ? (
        <div className="rounded-lg border border-dashed border-border p-12 text-center text-muted">
          No NPCs yet. Create your first one to get started.
        </div>
      ) : (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {npcs.map((npc) => (
            <NpcCard key={npc.id} npc={npc} />
          ))}
        </div>
      )}
    </div>
  );
}
