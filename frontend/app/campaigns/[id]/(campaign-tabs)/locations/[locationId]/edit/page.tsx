import { notFound } from "next/navigation";
import { LocationForm } from "@/components/LocationForm";
import { ApiNotFoundError, getCitiesForCampaign, getLocation } from "@/lib/api";

interface EditLocationPageProps {
  params: Promise<{ id: string; locationId: string }>;
}

export default async function EditLocationPage({ params }: EditLocationPageProps) {
  const { id: campaignId, locationId } = await params;

  let location;
  try {
    location = await getLocation(locationId);
  } catch (error) {
    if (error instanceof ApiNotFoundError) {
      notFound();
    }
    throw error;
  }

  const cities = await getCitiesForCampaign(campaignId);

  return (
    <div className="flex flex-col gap-6">
      <h2 className="text-lg font-semibold text-foreground">Edit {location.name}</h2>
      <LocationForm campaignId={campaignId} cities={cities} location={location} />
    </div>
  );
}
