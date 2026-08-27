import type { QuestLocationRole } from "@/lib/api";

export const QUEST_LOCATION_ROLE_LABELS: Record<QuestLocationRole, string> = {
  StartingLocation: "Starting Location",
  ObjectiveLocation: "Objective Location",
  EncounterLocation: "Encounter Location",
  Destination: "Destination",
  RelatedLocation: "Related Location",
  Other: "Other",
};
