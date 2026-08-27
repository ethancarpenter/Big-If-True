import Link from "next/link";
import { Badge } from "./Badge";
import { StatusBadge } from "./StatusBadge";
import { QUEST_STATUS_TONE } from "./quest-status-tone";
import { QUEST_TYPE_LABELS } from "./quest-type-labels";
import { formatQuestLevelRange } from "@/lib/format-quest-level";
import type { Quest } from "@/lib/api";

export function QuestCard({ quest }: { quest: Quest }) {
  const levelRange = formatQuestLevelRange(quest.recommendedLevelMin, quest.recommendedLevelMax);

  return (
    <Link
      href={`/campaigns/${quest.campaignId}/quests/${quest.id}`}
      className="block rounded-lg border border-border bg-surface p-5 transition-colors hover:border-accent"
    >
      <div className="flex items-start justify-between gap-2">
        <h3 className="font-serif text-lg font-semibold text-foreground">{quest.name}</h3>
        <Badge>{QUEST_TYPE_LABELS[quest.questType]}</Badge>
      </div>

      <div className="mt-2 flex flex-wrap items-center gap-2">
        <StatusBadge label={quest.status} tone={QUEST_STATUS_TONE[quest.status]} />
        {levelRange && <span className="text-xs text-muted">{levelRange}</span>}
      </div>

      {quest.description && (
        <p className="mt-2 line-clamp-2 text-sm text-muted">{quest.description}</p>
      )}
    </Link>
  );
}
