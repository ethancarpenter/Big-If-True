import Link from "next/link";
import { notFound } from "next/navigation";
import { DeleteEntityButton } from "@/components/DeleteEntityButton";
import { ApiNotFoundError, getCity } from "@/lib/api";

interface CityDetailPageProps {
  params: Promise<{ id: string; cityId: string }>;
}

const infoFields: { label: string; key: "population" | "government" | "region" | "alignment" }[] = [
  { label: "Population", key: "population" },
  { label: "Government", key: "government" },
  { label: "Region", key: "region" },
  { label: "Alignment", key: "alignment" },
];

export default async function CityDetailPage({ params }: CityDetailPageProps) {
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

  const presentFields = infoFields.filter((field) => city[field.key]);

  return (
    <div className="flex max-w-2xl flex-col gap-6">
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

      {city.description && <p className="text-muted">{city.description}</p>}

      <p className="text-xs text-muted">
        Created {new Date(city.createdAt).toLocaleString()} · Updated{" "}
        {new Date(city.updatedAt).toLocaleString()}
      </p>
    </div>
  );
}
