import Link from "next/link";
import { notFound } from "next/navigation";
import { Badge } from "@/components/Badge";
import { DeleteEntityButton } from "@/components/DeleteEntityButton";
import { QuestLocationsSection } from "@/components/QuestLocationsSection";
import { QuestNpcsSection } from "@/components/QuestNpcsSection";
import { QuestObjectivesChecklist } from "@/components/QuestObjectivesChecklist";
import { StatusBadge } from "@/components/StatusBadge";
import { QUEST_STATUS_TONE } from "@/components/quest-status-tone";
import { QUEST_TYPE_LABELS } from "@/components/quest-type-labels";
import { formatQuestLevelRange } from "@/lib/format-quest-level";
import {
  ApiNotFoundError,
  getLocationsForCampaign,
  getNpcsForCampaign,
  getQuest,
  getQuestLocationsForQuest,
  getQuestNpcsForQuest,
} from "@/lib/api";

interface QuestDetailPageProps {
  params: Promise<{ id: string; questId: string }>;
}

export default async function QuestDetailPage({ params }: QuestDetailPageProps) {
  const { id: campaignId, questId } = await params;

  let quest;
  try {
    quest = await getQuest(questId);
  } catch (error) {
    if (error instanceof ApiNotFoundError) {
      notFound();
    }
    throw error;
  }

  const [npcRelationships, locationRelationships, campaignNpcs, campaignLocations] = await Promise.all([
    getQuestNpcsForQuest(questId),
    getQuestLocationsForQuest(questId),
    getNpcsForCampaign(campaignId),
    getLocationsForCampaign(campaignId),
  ]);

  const levelRange = formatQuestLevelRange(quest.recommendedLevelMin, quest.recommendedLevelMax);

  return (
    <div className="flex max-w-2xl flex-col gap-6">
      <div className="flex items-center justify-between">
        <h2 className="font-serif text-2xl font-semibold text-foreground">{quest.name}</h2>
        <div className="flex gap-3">
          <Link
            href={`/campaigns/${campaignId}/quests/${quest.id}/edit`}
            className="rounded-md border border-border px-4 py-2 text-sm font-medium text-foreground hover:bg-white/5"
          >
            Edit
          </Link>
          <DeleteEntityButton
            kind="quest"
            id={quest.id}
            name={quest.name}
            redirectTo={`/campaigns/${campaignId}/quests`}
          />
        </div>
      </div>

      <div className="flex flex-wrap items-center gap-3">
        <StatusBadge label={quest.status} tone={QUEST_STATUS_TONE[quest.status]} />
        <Badge>{QUEST_TYPE_LABELS[quest.questType]}</Badge>
        {levelRange && <span className="text-sm text-muted">{levelRange}</span>}
      </div>

      {quest.description ? (
        <p className="text-muted">{quest.description}</p>
      ) : (
        <p className="text-muted italic">No description yet.</p>
      )}

      <QuestObjectivesChecklist questId={quest.id} objectives={quest.objectives} />

      <QuestNpcsSection
        quest={quest}
        campaignId={campaignId}
        relationships={npcRelationships}
        campaignNpcs={campaignNpcs}
      />

      <QuestLocationsSection
        quest={quest}
        campaignId={campaignId}
        relationships={locationRelationships}
        campaignLocations={campaignLocations}
      />

      {quest.dmNotes && (
        <div className="rounded-md border border-amber-500/30 bg-amber-500/10 p-4">
          <p className="text-xs font-semibold uppercase tracking-wide text-amber-400">DM Notes 🔒</p>
          <p className="mt-1 text-sm text-foreground">{quest.dmNotes}</p>
        </div>
      )}

      <p className="text-xs text-muted">
        Created {new Date(quest.createdAt).toLocaleString()} · Updated{" "}
        {new Date(quest.updatedAt).toLocaleString()}
      </p>
    </div>
  );
}
