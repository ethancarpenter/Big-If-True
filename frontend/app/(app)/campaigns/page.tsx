import Link from "next/link";
import { CampaignCard } from "@/components/CampaignCard";
import { getCampaigns } from "@/lib/server-api";

export default async function CampaignsPage() {
  const campaigns = await getCampaigns();

  return (
    <div className="flex flex-col gap-8">
      <div className="flex items-center justify-between">
        <div>
          <p className="text-sm text-muted">Welcome back,</p>
          <h1 className="font-serif text-3xl font-semibold text-foreground">Dungeon Master</h1>
          <p className="mt-1 text-muted">Plan your next adventure.</p>
        </div>
        <Link
          href="/campaigns/new"
          className="rounded-md bg-accent px-4 py-2 font-medium text-accent-foreground hover:opacity-90"
        >
          + New Campaign
        </Link>
      </div>

      {campaigns.length === 0 ? (
        <div className="rounded-lg border border-dashed border-border p-12 text-center text-muted">
          No campaigns yet. Create your first one to get started.
        </div>
      ) : (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {campaigns.map((campaign) => (
            <CampaignCard key={campaign.id} campaign={campaign} />
          ))}
        </div>
      )}
    </div>
  );
}
