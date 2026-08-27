import Link from "next/link";
import { Badge } from "./Badge";
import type { Location } from "@/lib/api";

export function LocationCard({ location, showCityName = false }: { location: Location; showCityName?: boolean }) {
  return (
    <Link
      href={`/campaigns/${location.campaignId}/locations/${location.id}`}
      className="block rounded-lg border border-border bg-surface p-5 transition-colors hover:border-accent"
    >
      <div className="flex items-start justify-between gap-2">
        <h3 className="font-serif text-lg font-semibold text-foreground">{location.name}</h3>
        <Badge>{location.type}</Badge>
      </div>
      {showCityName && <p className="mt-1 text-xs text-muted">{location.cityName}</p>}
      {location.description && (
        <p className="mt-2 line-clamp-2 text-sm text-muted">{location.description}</p>
      )}
    </Link>
  );
}
