import { notFound } from "next/navigation";
import { ApiNotFoundError, getCampaign } from "@/lib/api";

interface CampaignOverviewPageProps {
  params: Promise<{ id: string }>;
}

export default async function CampaignOverviewPage({ params }: CampaignOverviewPageProps) {
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
      {campaign.description ? (
        <p className="text-muted">{campaign.description}</p>
      ) : (
        <p className="text-muted italic">No description yet.</p>
      )}

      <p className="text-xs text-muted">
        Created {new Date(campaign.createdAt).toLocaleString()} · Updated{" "}
        {new Date(campaign.updatedAt).toLocaleString()}
      </p>
    </div>
  );
}
