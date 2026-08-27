export type BadgeTone = "positive" | "negative" | "warning" | "neutral";

const TONE_CLASSES: Record<BadgeTone, string> = {
  positive: "bg-emerald-500/15 text-emerald-400 border-emerald-500/40",
  negative: "bg-red-500/15 text-red-400 border-red-500/40",
  warning: "bg-amber-500/15 text-amber-400 border-amber-500/40",
  neutral: "bg-surface text-muted border-border",
};

// Domain-agnostic on purpose: NPC status supplies its own label/tone mapping
// today, and Quest status (a later milestone) can reuse this component with
// its own mapping instead of needing a near-identical one built from scratch.
export function StatusBadge({ label, tone }: { label: string; tone: BadgeTone }) {
  return (
    <span
      className={`inline-flex items-center gap-1.5 rounded-full border px-3 py-1 text-xs font-medium ${TONE_CLASSES[tone]}`}
    >
      <span className="h-1.5 w-1.5 rounded-full bg-current" />
      {label}
    </span>
  );
}
