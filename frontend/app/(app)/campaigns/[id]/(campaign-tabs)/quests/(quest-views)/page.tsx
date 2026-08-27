import Link from "next/link";
import { QuestCard } from "@/components/QuestCard";
import { getQuestsForCampaign } from "@/lib/server-api";

interface QuestsPageProps {
  params: Promise<{ id: string }>;
}

export default async function QuestsPage({ params }: QuestsPageProps) {
  const { id: campaignId } = await params;
  const quests = await getQuestsForCampaign(campaignId);

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <h2 className="text-lg font-semibold text-foreground">Quests</h2>
        <Link
          href={`/campaigns/${campaignId}/quests/new`}
          className="rounded-md bg-accent px-4 py-2 text-sm font-medium text-accent-foreground hover:opacity-90"
        >
          + New Quest
        </Link>
      </div>

      {quests.length === 0 ? (
        <div className="rounded-lg border border-dashed border-border p-12 text-center text-muted">
          No quests yet. Create your first one to get started.
        </div>
      ) : (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {quests.map((quest) => (
            <QuestCard key={quest.id} quest={quest} />
          ))}
        </div>
      )}
    </div>
  );
}
