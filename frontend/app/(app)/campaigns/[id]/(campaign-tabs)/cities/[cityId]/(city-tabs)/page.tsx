import { notFound } from "next/navigation";
import { ApiNotFoundError, getCity } from "@/lib/server-api";

interface CityOverviewPageProps {
  params: Promise<{ id: string; cityId: string }>;
}

const infoFields: { label: string; key: "population" | "government" | "region" | "alignment" }[] = [
  { label: "Population", key: "population" },
  { label: "Government", key: "government" },
  { label: "Region", key: "region" },
  { label: "Alignment", key: "alignment" },
];

export default async function CityOverviewPage({ params }: CityOverviewPageProps) {
  const { cityId } = await params;

  let city;
  try {
    city = await getCity(cityId);
  } catch (error) {
    if (error instanceof ApiNotFoundError) {
      notFound();
    }
    throw error;
  }

  const presentFields = infoFields.filter((field) => city[field.key]);

  return (
    <div className="flex max-w-2xl flex-col gap-6">
      {presentFields.length > 0 && (
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-4">
          {presentFields.map((field) => (
            <div key={field.key}>
              <p className="text-xs text-muted">{field.label}</p>
              <p className="text-sm text-foreground">{city[field.key]}</p>
            </div>
          ))}
        </div>
      )}

      {city.description ? (
        <p className="text-muted">{city.description}</p>
      ) : (
        <p className="text-muted italic">No description yet.</p>
      )}

      <p className="text-xs text-muted">
        Created {new Date(city.createdAt).toLocaleString()} · Updated{" "}
        {new Date(city.updatedAt).toLocaleString()}
      </p>
    </div>
  );
}
