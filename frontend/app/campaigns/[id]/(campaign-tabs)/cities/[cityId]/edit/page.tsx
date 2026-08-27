import { notFound } from "next/navigation";
import { CityForm } from "@/components/CityForm";
import { ApiNotFoundError, getCity } from "@/lib/api";

interface EditCityPageProps {
  params: Promise<{ id: string; cityId: string }>;
}

export default async function EditCityPage({ params }: EditCityPageProps) {
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
      <h2 className="text-lg font-semibold text-foreground">Edit {city.name}</h2>
      <CityForm campaignId={campaignId} city={city} />
    </div>
  );
}
