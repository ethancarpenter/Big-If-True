import Link from "next/link";
import { AlignmentBadge } from "./AlignmentBadge";
import { StatusBadge } from "./StatusBadge";
import { NPC_STATUS_TONE } from "./npc-status-tone";
import type { Npc } from "@/lib/api";

export function NpcCard({ npc }: { npc: Npc }) {
  const subtitle = [npc.species, npc.occupation].filter(Boolean).join(" · ");

  return (
    <Link
      href={`/campaigns/${npc.campaignId}/npcs/${npc.id}`}
      className="block rounded-lg border border-border bg-surface p-5 transition-colors hover:border-accent"
    >
      <div className="flex items-start gap-3">
        {npc.portraitUrl ? (
          // eslint-disable-next-line @next/next/no-img-element -- external, unpredictable-origin URLs; not worth Next's Image config for a small avatar
          <img
            src={npc.portraitUrl}
            alt={npc.name}
            className="h-10 w-10 shrink-0 rounded-full border border-border object-cover"
          />
        ) : (
          <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-full border border-border bg-background text-sm font-semibold text-muted">
            {npc.name.charAt(0).toUpperCase()}
          </div>
        )}
        <div className="min-w-0 flex-1">
          <h3 className="truncate font-serif text-lg font-semibold text-foreground">{npc.name}</h3>
          {subtitle && <p className="truncate text-xs text-muted">{subtitle}</p>}
        </div>
      </div>

      <div className="mt-3 flex flex-wrap gap-2">
        <AlignmentBadge alignment={npc.alignment} />
        <StatusBadge label={npc.status} tone={NPC_STATUS_TONE[npc.status]} />
      </div>

      {npc.description && (
        <p className="mt-3 line-clamp-2 text-sm text-muted">{npc.description}</p>
      )}
    </Link>
  );
}
