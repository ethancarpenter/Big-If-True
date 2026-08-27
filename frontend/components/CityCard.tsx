import Link from "next/link";
import type { City } from "@/lib/api";

export function CityCard({ city }: { city: City }) {
  const subtitle = [city.region, city.government].filter(Boolean).join(" · ");

  return (
    <Link
      href={`/campaigns/${city.campaignId}/cities/${city.id}`}
      className="block rounded-lg border border-border bg-surface p-5 transition-colors hover:border-accent"
    >
      <h3 className="font-serif text-lg font-semibold text-foreground">{city.name}</h3>
      {subtitle && <p className="mt-1 text-xs text-muted">{subtitle}</p>}
      {city.description && (
        <p className="mt-2 line-clamp-2 text-sm text-muted">{city.description}</p>
      )}
    </Link>
  );
}
