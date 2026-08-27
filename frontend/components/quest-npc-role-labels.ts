import type { QuestNpcRole } from "@/lib/api";

export const QUEST_NPC_ROLE_LABELS: Record<QuestNpcRole, string> = {
  QuestGiver: "Quest Giver",
  Ally: "Ally",
  Enemy: "Enemy",
  Victim: "Victim",
  Contact: "Contact",
  Target: "Target",
  Witness: "Witness",
  Participant: "Participant",
  Other: "Other",
};
