import type { QuestConnectionType } from "@/lib/api";

export const QUEST_CONNECTION_TYPE_LABELS: Record<QuestConnectionType, string> = {
  Unlocks: "Unlocks",
  Requires: "Requires",
  Optional: "Optional",
  AlternativePath: "Alternative Path",
  FailureLeadsTo: "Failure Leads To",
  Related: "Related",
};
