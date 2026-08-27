import type { BadgeTone } from "./StatusBadge";
import type { QuestStatus } from "@/lib/api";

export const QUEST_STATUS_TONE: Record<QuestStatus, BadgeTone> = {
  Planned: "neutral",
  Available: "warning",
  Active: "positive",
  Completed: "positive",
  Failed: "negative",
  Abandoned: "negative",
};
