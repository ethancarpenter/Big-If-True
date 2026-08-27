import { notFound } from "next/navigation";
import { CampaignForm } from "@/components/CampaignForm";
import { ApiNotFoundError, getCampaign } from "@/lib/api";

interface EditCampaignPageProps {
  params: Promise<{ id: string }>;
}

export default async function EditCampaignPage({ params }: EditCampaignPageProps) {
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
    <div className="flex flex-col gap-6">
      <h1 className="font-serif text-2xl font-semibold text-foreground">Edit {campaign.name}</h1>
      <CampaignForm campaign={campaign} />
    </div>
  );
}
