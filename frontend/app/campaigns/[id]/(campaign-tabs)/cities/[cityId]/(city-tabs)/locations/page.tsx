import Link from "next/link";
import { LocationCard } from "@/components/LocationCard";
import { getLocationsForCampaign } from "@/lib/api";

interface CityLocationsPageProps {
  params: Promise<{ id: string; cityId: string }>;
}

export default async function CityLocationsPage({ params }: CityLocationsPageProps) {
  const { id: campaignId, cityId } = await params;
  const locations = await getLocationsForCampaign(campaignId, cityId);

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-semibold text-foreground">Locations</h3>
        <Link
          href={`/campaigns/${campaignId}/locations/new?cityId=${cityId}`}
          className="rounded-md bg-accent px-4 py-2 text-sm font-medium text-accent-foreground hover:opacity-90"
        >
          + Add Location
        </Link>
      </div>

      {locations.length === 0 ? (
        <div className="rounded-lg border border-dashed border-border p-12 text-center text-muted">
          No locations yet. Add your first one to get started.
        </div>
      ) : (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {locations.map((location) => (
            <LocationCard key={location.id} location={location} />
          ))}
        </div>
      )}
    </div>
  );
}
