"use client";

import { useRouter } from "next/navigation";
import { useState, type FormEvent } from "react";
import {
  createQuest,
  updateQuest,
  QUEST_STATUSES,
  QUEST_TYPES,
  type Quest,
  type QuestStatus,
  type QuestType,
} from "@/lib/api";
import { QUEST_TYPE_LABELS } from "./quest-type-labels";

interface QuestFormProps {
  campaignId: string;
  quest?: Quest;
}

const inputClass =
  "rounded-md border border-border bg-surface px-3 py-2 text-foreground outline-none focus:border-accent";

export function QuestForm({ campaignId, quest }: QuestFormProps) {
  const router = useRouter();
  const [name, setName] = useState(quest?.name ?? "");
  const [description, setDescription] = useState(quest?.description ?? "");
  const [status, setStatus] = useState<QuestStatus>(quest?.status ?? "Planned");
  const [questType, setQuestType] = useState<QuestType>(quest?.questType ?? QUEST_TYPES[0]);
  const [levelMin, setLevelMin] = useState(quest?.recommendedLevelMin?.toString() ?? "");
  const [levelMax, setLevelMax] = useState(quest?.recommendedLevelMax?.toString() ?? "");
  const [dmNotes, setDmNotes] = useState(quest?.dmNotes ?? "");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);

    const min = levelMin ? Number(levelMin) : undefined;
    const max = levelMax ? Number(levelMax) : undefined;
    if (min !== undefined && max !== undefined && min > max) {
      setError("Minimum recommended level must be less than or equal to the maximum.");
      return;
    }

    setIsSubmitting(true);

    try {
      const payload = {
        name,
        description: description || undefined,
        status,
        questType,
        recommendedLevelMin: min,
        recommendedLevelMax: max,
        dmNotes: dmNotes || undefined,
      };

      const result = quest ? await updateQuest(quest.id, payload) : await createQuest(campaignId, payload);

      router.push(`/campaigns/${campaignId}/quests/${result.id}`);
      router.refresh();
    } catch {
      setError("Something went wrong saving this quest. Please try again.");
      setIsSubmitting(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} className="flex max-w-xl flex-col gap-6">
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
          className={inputClass}
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
          className={inputClass}
        />
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div className="flex flex-col gap-1.5">
          <label htmlFor="status" className="text-sm font-medium text-foreground">
            Status
          </label>
          <select
            id="status"
            value={status}
            onChange={(e) => setStatus(e.target.value as QuestStatus)}
            className={inputClass}
          >
            {QUEST_STATUSES.map((option) => (
              <option key={option} value={option}>
                {option}
              </option>
            ))}
          </select>
        </div>

        <div className="flex flex-col gap-1.5">
          <label htmlFor="questType" className="text-sm font-medium text-foreground">
            Type
          </label>
          <select
            id="questType"
            value={questType}
            onChange={(e) => setQuestType(e.target.value as QuestType)}
            className={inputClass}
          >
            {QUEST_TYPES.map((option) => (
              <option key={option} value={option}>
                {QUEST_TYPE_LABELS[option]}
              </option>
            ))}
          </select>
        </div>

        <div className="flex flex-col gap-1.5">
          <label htmlFor="levelMin" className="text-sm font-medium text-foreground">
            Recommended Level (Min)
          </label>
          <input
            id="levelMin"
            type="number"
            min={1}
            max={20}
            value={levelMin}
            onChange={(e) => setLevelMin(e.target.value)}
            className={inputClass}
          />
        </div>

        <div className="flex flex-col gap-1.5">
          <label htmlFor="levelMax" className="text-sm font-medium text-foreground">
            Recommended Level (Max)
          </label>
          <input
            id="levelMax"
            type="number"
            min={1}
            max={20}
            value={levelMax}
            onChange={(e) => setLevelMax(e.target.value)}
            className={inputClass}
          />
        </div>
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
          className={inputClass}
        />
      </div>

      {error && <p className="text-sm text-red-400">{error}</p>}

      <button
        type="submit"
        disabled={isSubmitting}
        className="self-start rounded-md bg-accent px-5 py-2 font-medium text-accent-foreground transition-opacity hover:opacity-90 disabled:opacity-50"
      >
        {isSubmitting ? "Saving..." : quest ? "Save Changes" : "Create Quest"}
      </button>
    </form>
  );
}
