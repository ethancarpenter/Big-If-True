import type { NpcLocationRelationshipType } from "@/lib/api";

export const RELATIONSHIP_TYPE_LABELS: Record<NpcLocationRelationshipType, string> = {
  LivesAt: "Lives At",
  WorksAt: "Works At",
  FrequentlyVisits: "Frequently Visits",
  Owns: "Owns",
  Guards: "Guards",
  Other: "Other",
};
