import Link from "next/link";
import { notFound } from "next/navigation";
import { DeleteCampaignButton } from "@/components/DeleteCampaignButton";
import { ApiNotFoundError, getCampaign } from "@/lib/api";

interface CampaignDetailPageProps {
  params: Promise<{ id: string }>;
}

export default async function CampaignDetailPage({ params }: CampaignDetailPageProps) {
  const { id } = await params;

  let campaign;
  try {
    campaign = await getCampaign(id);
  } catch (error) {
    if (error instanceof ApiNotFoundError) {
      notFound();
    }
    throw error;
  }

  return (
    <div className="flex max-w-2xl flex-col gap-6">
      <div className="flex items-center justify-between">
        <h1 className="font-serif text-3xl font-semibold text-foreground">{campaign.name}</h1>
        <div className="flex gap-3">
          <Link
            href={`/campaigns/${campaign.id}/edit`}
            className="rounded-md border border-border px-4 py-2 text-sm font-medium text-foreground hover:bg-white/5"
          >
            Edit
          </Link>
          <DeleteCampaignButton campaignId={campaign.id} campaignName={campaign.name} />
        </div>
      </div>

      {campaign.description && <p className="text-muted">{campaign.description}</p>}

      <p className="text-xs text-muted">
        Created {new Date(campaign.createdAt).toLocaleString()} · Updated{" "}
        {new Date(campaign.updatedAt).toLocaleString()}
      </p>
    </div>
  );
}
