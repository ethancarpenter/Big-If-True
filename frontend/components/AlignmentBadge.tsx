import type { Alignment } from "@/lib/api";

const ALIGNMENT_LABELS: Record<Alignment, string> = {
  LawfulGood: "Lawful Good",
  NeutralGood: "Neutral Good",
  ChaoticGood: "Chaotic Good",
  LawfulNeutral: "Lawful Neutral",
  TrueNeutral: "True Neutral",
  ChaoticNeutral: "Chaotic Neutral",
  LawfulEvil: "Lawful Evil",
  NeutralEvil: "Neutral Evil",
  ChaoticEvil: "Chaotic Evil",
};

// Good/Neutral/Evil drives the badge's color family (spec: green/gold/red).
const ETHICS_TONE: Record<Alignment, "good" | "neutral" | "evil"> = {
  LawfulGood: "good",
  NeutralGood: "good",
  ChaoticGood: "good",
  LawfulNeutral: "neutral",
  TrueNeutral: "neutral",
  ChaoticNeutral: "neutral",
  LawfulEvil: "evil",
  NeutralEvil: "evil",
  ChaoticEvil: "evil",
};

// Lawful/Neutral/Chaotic slightly modifies the border treatment, per spec.
const LAW_AXIS: Record<Alignment, "lawful" | "neutral" | "chaotic"> = {
  LawfulGood: "lawful",
  LawfulNeutral: "lawful",
  LawfulEvil: "lawful",
  NeutralGood: "neutral",
  TrueNeutral: "neutral",
  NeutralEvil: "neutral",
  ChaoticGood: "chaotic",
  ChaoticNeutral: "chaotic",
  ChaoticEvil: "chaotic",
};

const TONE_CLASSES: Record<"good" | "neutral" | "evil", string> = {
  good: "bg-emerald-500/15 text-emerald-400 border-emerald-500/40",
  neutral: "bg-amber-500/15 text-amber-400 border-amber-500/40",
  evil: "bg-red-500/15 text-red-400 border-red-500/40",
};

const BORDER_STYLE: Record<"lawful" | "neutral" | "chaotic", string> = {
  lawful: "border-solid",
  neutral: "border-solid",
  chaotic: "border-dashed",
};

export function AlignmentBadge({ alignment }: { alignment: Alignment | null }) {
  if (!alignment) {
    return (
      <span className="inline-flex items-center rounded-full border border-border bg-surface px-3 py-1 text-xs font-medium text-muted">
        Alignment unknown
      </span>
    );
  }

  const tone = ETHICS_TONE[alignment];
  const law = LAW_AXIS[alignment];

  return (
    <span
      className={`inline-flex items-center rounded-full border px-3 py-1 text-xs font-medium ${TONE_CLASSES[tone]} ${BORDER_STYLE[law]}`}
    >
      {ALIGNMENT_LABELS[alignment]}
    </span>
  );
}
