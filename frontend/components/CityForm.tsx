"use client";

import { useRouter } from "next/navigation";
import { useState, type FormEvent } from "react";
import { createCity, updateCity, type City } from "@/lib/api";

interface CityFormProps {
  campaignId: string;
  city?: City;
}

export function CityForm({ campaignId, city }: CityFormProps) {
  const router = useRouter();
  const [name, setName] = useState(city?.name ?? "");
  const [description, setDescription] = useState(city?.description ?? "");
  const [population, setPopulation] = useState(city?.population ?? "");
  const [government, setGovernment] = useState(city?.government ?? "");
  const [region, setRegion] = useState(city?.region ?? "");
  const [alignment, setAlignment] = useState(city?.alignment ?? "");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      const payload = {
        name,
        description: description || undefined,
        population: population || undefined,
        government: government || undefined,
        region: region || undefined,
        alignment: alignment || undefined,
      };

      const result = city ? await updateCity(city.id, payload) : await createCity(campaignId, payload);

      router.push(`/campaigns/${campaignId}/cities/${result.id}`);
      router.refresh();
    } catch {
      setError("Something went wrong saving this city. Please try again.");
      setIsSubmitting(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} className="flex max-w-xl flex-col gap-5">
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

      <div className="grid grid-cols-2 gap-4">
        <div className="flex flex-col gap-1.5">
          <label htmlFor="population" className="text-sm font-medium text-foreground">
            Population
          </label>
          <input
            id="population"
            value={population}
            onChange={(e) => setPopulation(e.target.value)}
            maxLength={100}
            placeholder="~8,000"
            className="rounded-md border border-border bg-surface px-3 py-2 text-foreground outline-none focus:border-accent"
          />
        </div>

        <div className="flex flex-col gap-1.5">
          <label htmlFor="government" className="text-sm font-medium text-foreground">
            Government
          </label>
          <input
            id="government"
            value={government}
            onChange={(e) => setGovernment(e.target.value)}
            maxLength={200}
            className="rounded-md border border-border bg-surface px-3 py-2 text-foreground outline-none focus:border-accent"
          />
        </div>

        <div className="flex flex-col gap-1.5">
          <label htmlFor="region" className="text-sm font-medium text-foreground">
            Region
          </label>
          <input
            id="region"
            value={region}
            onChange={(e) => setRegion(e.target.value)}
            maxLength={200}
            className="rounded-md border border-border bg-surface px-3 py-2 text-foreground outline-none focus:border-accent"
          />
        </div>

        <div className="flex flex-col gap-1.5">
          <label htmlFor="alignment" className="text-sm font-medium text-foreground">
            Alignment
          </label>
          <input
            id="alignment"
            value={alignment}
            onChange={(e) => setAlignment(e.target.value)}
            maxLength={100}
            placeholder="Neutral"
            className="rounded-md border border-border bg-surface px-3 py-2 text-foreground outline-none focus:border-accent"
          />
        </div>
      </div>

      {error && <p className="text-sm text-red-400">{error}</p>}

      <button
        type="submit"
        disabled={isSubmitting}
        className="self-start rounded-md bg-accent px-5 py-2 font-medium text-accent-foreground transition-opacity hover:opacity-90 disabled:opacity-50"
      >
        {isSubmitting ? "Saving..." : city ? "Save Changes" : "Create City"}
      </button>
    </form>
  );
}
