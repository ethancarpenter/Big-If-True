import Link from "next/link";
import { notFound } from "next/navigation";
import { DeleteEntityButton } from "@/components/DeleteEntityButton";
import { TabNavigation } from "@/components/TabNavigation";
import { ApiNotFoundError, getCity } from "@/lib/server-api";

interface CityLayoutProps {
  children: React.ReactNode;
  params: Promise<{ id: string; cityId: string }>;
}

export default async function CityLayout({ children, params }: CityLayoutProps) {
  const { id: campaignId, cityId } = await params;

  let city;
  try {
    city = await getCity(cityId);
  } catch (error) {
    if (error instanceof ApiNotFoundError) {
      notFound();
    }
    throw error;
  }

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <h2 className="font-serif text-2xl font-semibold text-foreground">{city.name}</h2>
        <div className="flex gap-3">
          <Link
            href={`/campaigns/${campaignId}/cities/${city.id}/edit`}
            className="rounded-md border border-border px-4 py-2 text-sm font-medium text-foreground hover:bg-white/5"
          >
            Edit
          </Link>
          <DeleteEntityButton
            kind="city"
            id={city.id}
            name={city.name}
            redirectTo={`/campaigns/${campaignId}/cities`}
          />
        </div>
      </div>

      <TabNavigation
        tabs={[
          { label: "Overview", href: `/campaigns/${campaignId}/cities/${city.id}` },
          { label: "Locations", href: `/campaigns/${campaignId}/cities/${city.id}/locations` },
        ]}
      />

      {children}
    </div>
  );
}
