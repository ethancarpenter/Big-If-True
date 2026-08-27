"use client";

import { useRouter } from "next/navigation";
import { useState, type FormEvent } from "react";
import { createCampaign, updateCampaign, type Campaign } from "@/lib/api";

interface CampaignFormProps {
  campaign?: Campaign;
}

export function CampaignForm({ campaign }: CampaignFormProps) {
  const router = useRouter();
  const [name, setName] = useState(campaign?.name ?? "");
  const [description, setDescription] = useState(campaign?.description ?? "");
  const [coverImageUrl, setCoverImageUrl] = useState(campaign?.coverImageUrl ?? "");
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
        coverImageUrl: coverImageUrl || undefined,
      };

      const result = campaign
        ? await updateCampaign(campaign.id, payload)
        : await createCampaign(payload);

      router.push(`/campaigns/${result.id}`);
      router.refresh();
    } catch {
      setError("Something went wrong saving this campaign. Please try again.");
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

      <div className="flex flex-col gap-1.5">
        <label htmlFor="coverImageUrl" className="text-sm font-medium text-foreground">
          Cover Image URL
        </label>
        <input
          id="coverImageUrl"
          value={coverImageUrl}
          onChange={(e) => setCoverImageUrl(e.target.value)}
          maxLength={500}
          type="url"
          className="rounded-md border border-border bg-surface px-3 py-2 text-foreground outline-none focus:border-accent"
        />
      </div>

      {error && <p className="text-sm text-red-400">{error}</p>}

      <button
        type="submit"
        disabled={isSubmitting}
        className="self-start rounded-md bg-accent px-5 py-2 font-medium text-accent-foreground transition-opacity hover:opacity-90 disabled:opacity-50"
      >
        {isSubmitting ? "Saving..." : campaign ? "Save Changes" : "Create Campaign"}
      </button>
    </form>
  );
}
