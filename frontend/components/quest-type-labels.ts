import type { QuestType } from "@/lib/api";

export const QUEST_TYPE_LABELS: Record<QuestType, string> = {
  MainQuest: "Main Quest",
  SideQuest: "Side Quest",
  PersonalQuest: "Personal Quest",
  FactionQuest: "Faction Quest",
  HiddenQuest: "Hidden Quest",
  Other: "Other",
};
