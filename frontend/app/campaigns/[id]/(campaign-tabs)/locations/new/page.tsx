import { LocationForm } from "@/components/LocationForm";
import { getCitiesForCampaign } from "@/lib/api";

interface NewLocationPageProps {
  params: Promise<{ id: string }>;
  searchParams: Promise<{ cityId?: string }>;
}

export default async function NewLocationPage({ params, searchParams }: NewLocationPageProps) {
  const { id: campaignId } = await params;
  const { cityId } = await searchParams;
  const cities = await getCitiesForCampaign(campaignId);

  return (
    <div className="flex flex-col gap-6">
      <h2 className="text-lg font-semibold text-foreground">New Location</h2>
      <LocationForm campaignId={campaignId} cities={cities} defaultCityId={cityId} />
    </div>
  );
}
