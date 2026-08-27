import type { BadgeTone } from "./StatusBadge";
import type { NpcStatus } from "@/lib/api";

export const NPC_STATUS_TONE: Record<NpcStatus, BadgeTone> = {
  Alive: "positive",
  Dead: "negative",
  Missing: "warning",
  Unknown: "neutral",
};
