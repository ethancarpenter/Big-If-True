"use client";

import { useRouter } from "next/navigation";
import { useState, type FormEvent } from "react";
import {
  createLocation,
  updateLocation,
  LOCATION_TYPES,
  type City,
  type Location,
  type LocationType,
} from "@/lib/api";

interface LocationFormProps {
  campaignId: string;
  cities: City[];
  defaultCityId?: string;
  location?: Location;
}

export function LocationForm({ campaignId, cities, defaultCityId, location }: LocationFormProps) {
  const router = useRouter();
  const [cityId, setCityId] = useState(location?.cityId ?? defaultCityId ?? "");
  const [name, setName] = useState(location?.name ?? "");
  const [type, setType] = useState<LocationType>(location?.type ?? LOCATION_TYPES[0]);
  const [description, setDescription] = useState(location?.description ?? "");
  const [dmNotes, setDmNotes] = useState(location?.dmNotes ?? "");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      const payload = {
        cityId,
        name,
        type,
        description: description || undefined,
        dmNotes: dmNotes || undefined,
      };

      const result = location
        ? await updateLocation(location.id, payload)
        : await createLocation(campaignId, payload);

      router.push(`/campaigns/${campaignId}/locations/${result.id}`);
      router.refresh();
    } catch {
      setError("Something went wrong saving this location. Please try again.");
      setIsSubmitting(false);
    }
  }

  if (cities.length === 0) {
    return (
      <p className="text-muted">
        You need to create a city before you can add a location.
      </p>
    );
  }

  return (
    <form onSubmit={handleSubmit} className="flex max-w-xl flex-col gap-5">
      <div className="flex flex-col gap-1.5">
        <label htmlFor="cityId" className="text-sm font-medium text-foreground">
          City
        </label>
        <select
          id="cityId"
          value={cityId}
          onChange={(e) => setCityId(e.target.value)}
          required
          className="rounded-md border border-border bg-surface px-3 py-2 text-foreground outline-none focus:border-accent"
        >
          <option value="" disabled>
            Select a city&hellip;
          </option>
          {cities.map((city) => (
            <option key={city.id} value={city.id}>
              {city.name}
            </option>
          ))}
        </select>
      </div>

      <div className="flex flex-col gap-1.5">
        <label htmlFor="name" className="text-sm font-medium text-foreground">
          Name
        </label>
        <input
          id="name"
          value={name}
          onChange={(e) => setName(e.target.value)}
          required
          maxLength={200}
          className="rounded-md border border-border bg-surface px-3 py-2 text-foreground outline-none focus:border-accent"
        />
      </div>

      <div className="flex flex-col gap-1.5">
        <label htmlFor="type" className="text-sm font-medium text-foreground">
          Type
        </label>
        <select
          id="type"
          value={type}
          onChange={(e) => setType(e.target.value as LocationType)}
          className="rounded-md border border-border bg-surface px-3 py-2 text-foreground outline-none focus:border-accent"
        >
          {LOCATION_TYPES.map((option) => (
            <option key={option} value={option}>
              {option}
            </option>
          ))}
        </select>
      </div>

      <div className="flex flex-col gap-1.5">
        <label htmlFor="description" className="text-sm font-medium text-foreground">
          Description
        </label>
        <textarea
          id="description"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          maxLength={2000}
          rows={4}
          className="rounded-md border border-border bg-surface px-3 py-2 text-foreground outline-none focus:border-accent"
        />
      </div>

      <div className="flex flex-col gap-1.5">
        <label htmlFor="dmNotes" className="text-sm font-medium text-foreground">
          DM Notes 🔒
        </label>
        <textarea
          id="dmNotes"
          value={dmNotes}
          onChange={(e) => setDmNotes(e.target.value)}
          maxLength={2000}
          rows={3}
          className="rounded-md border border-border bg-surface px-3 py-2 text-foreground outline-none focus:border-accent"
        />
      </div>

      {error && <p className="text-sm text-red-400">{error}</p>}

      <button
        type="submit"
        disabled={isSubmitting}
        className="self-start rounded-md bg-accent px-5 py-2 font-medium text-accent-foreground transition-opacity hover:opacity-90 disabled:opacity-50"
      >
        {isSubmitting ? "Saving..." : location ? "Save Changes" : "Create Location"}
      </button>
    </form>
  );
}
