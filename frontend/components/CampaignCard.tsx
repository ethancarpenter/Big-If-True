import Link from "next/link";
import type { Campaign } from "@/lib/api";

export function CampaignCard({ campaign }: { campaign: Campaign }) {
  return (
    <Link
      href={`/campaigns/${campaign.id}`}
      className="block rounded-lg border border-border bg-surface p-5 transition-colors hover:border-accent"
    >
      <h3 className="font-serif text-lg font-semibold text-foreground">{campaign.name}</h3>
      {campaign.description && (
        <p className="mt-2 line-clamp-2 text-sm text-muted">{campaign.description}</p>
      )}
      <p className="mt-4 text-xs text-muted">
        Updated {new Date(campaign.updatedAt).toLocaleDateString()}
      </p>
    </Link>
  );
}
