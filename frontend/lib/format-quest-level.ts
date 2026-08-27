export function formatQuestLevelRange(min: number | null, max: number | null): string | null {
  if (min !== null && max !== null) {
    return `Level ${min}–${max}`;
  }
  if (min !== null) {
    return `Level ${min}+`;
  }
  if (max !== null) {
    return `Level ≤${max}`;
  }
  return null;
}
