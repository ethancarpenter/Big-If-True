import { QuestGraph } from "@/components/QuestGraph";
import {
  getQuestConnectionsForCampaign,
  getQuestGraphPositionsForCampaign,
  getQuestsForCampaign,
} from "@/lib/api";

interface QuestGraphPageProps {
  params: Promise<{ id: string }>;
}

export default async function QuestGraphPage({ params }: QuestGraphPageProps) {
  const { id: campaignId } = await params;

  const [quests, connections, positions] = await Promise.all([
    getQuestsForCampaign(campaignId),
    getQuestConnectionsForCampaign(campaignId),
    getQuestGraphPositionsForCampaign(campaignId),
  ]);

  if (quests.length === 0) {
    return (
      <div className="rounded-lg border border-dashed border-border p-12 text-center text-muted">
        No quests yet. Create a few quests first, then connect them here.
      </div>
    );
  }

  return (
    <QuestGraph campaignId={campaignId} quests={quests} connections={connections} positions={positions} />
  );
}
