"use client";

import { useRouter } from "next/navigation";
import { useState, type FormEvent } from "react";
import {
  createNpc,
  updateNpc,
  NPC_CLASSES,
  ALIGNMENTS,
  NPC_STATUSES,
  type Npc,
  type NpcClass,
  type Alignment,
  type NpcStatus,
} from "@/lib/api";

const ALIGNMENT_LABELS: Record<Alignment, string> = {
  LawfulGood: "Lawful Good",
  NeutralGood: "Neutral Good",
  ChaoticGood: "Chaotic Good",
  LawfulNeutral: "Lawful Neutral",
  TrueNeutral: "True Neutral",
  ChaoticNeutral: "Chaotic Neutral",
  LawfulEvil: "Lawful Evil",
  NeutralEvil: "Neutral Evil",
  ChaoticEvil: "Chaotic Evil",
};

interface NpcFormProps {
  campaignId: string;
  npc?: Npc;
}

const inputClass =
  "rounded-md border border-border bg-surface px-3 py-2 text-foreground outline-none focus:border-accent";

export function NpcForm({ campaignId, npc }: NpcFormProps) {
  const router = useRouter();
  const [name, setName] = useState(npc?.name ?? "");
  const [species, setSpecies] = useState(npc?.species ?? "");
  const [gender, setGender] = useState(npc?.gender ?? "");
  const [age, setAge] = useState(npc?.age?.toString() ?? "");
  const [npcClass, setNpcClass] = useState<NpcClass | "">(npc?.class ?? "");
  const [alignment, setAlignment] = useState<Alignment | "">(npc?.alignment ?? "");
  const [occupation, setOccupation] = useState(npc?.occupation ?? "");
  const [disposition, setDisposition] = useState(npc?.disposition ?? "");
  const [status, setStatus] = useState<NpcStatus>(npc?.status ?? "Alive");
  const [description, setDescription] = useState(npc?.description ?? "");
  const [dmNotes, setDmNotes] = useState(npc?.dmNotes ?? "");
  const [portraitUrl, setPortraitUrl] = useState(npc?.portraitUrl ?? "");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      const payload = {
        name,
        species: species || undefined,
        gender: gender || undefined,
        age: age ? Number(age) : undefined,
        class: npcClass || undefined,
        alignment: alignment || undefined,
        occupation: occupation || undefined,
        disposition: disposition || undefined,
        status,
        description: description || undefined,
        dmNotes: dmNotes || undefined,
        portraitUrl: portraitUrl || undefined,
      };

      const result = npc ? await updateNpc(npc.id, payload) : await createNpc(campaignId, payload);

      router.push(`/campaigns/${campaignId}/npcs/${result.id}`);
      router.refresh();
    } catch {
      setError("Something went wrong saving this NPC. Please try again.");
      setIsSubmitting(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} className="flex max-w-xl flex-col gap-8">
      <fieldset className="flex flex-col gap-4">
        <legend className="text-sm font-semibold text-foreground">Identity</legend>

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

        <div className="grid grid-cols-3 gap-4">
          <div className="flex flex-col gap-1.5">
            <label htmlFor="species" className="text-sm font-medium text-foreground">
              Species
            </label>
            <input
              id="species"
              value={species}
              onChange={(e) => setSpecies(e.target.value)}
              maxLength={100}
              className={inputClass}
            />
          </div>

          <div className="flex flex-col gap-1.5">
            <label htmlFor="gender" className="text-sm font-medium text-foreground">
              Gender
            </label>
            <input
              id="gender"
              value={gender}
              onChange={(e) => setGender(e.target.value)}
              maxLength={100}
              className={inputClass}
            />
          </div>

          <div className="flex flex-col gap-1.5">
            <label htmlFor="age" className="text-sm font-medium text-foreground">
              Age
            </label>
            <input
              id="age"
              type="number"
              min={0}
              value={age}
              onChange={(e) => setAge(e.target.value)}
              className={inputClass}
            />
          </div>
        </div>
      </fieldset>

      <fieldset className="flex flex-col gap-4">
        <legend className="text-sm font-semibold text-foreground">Role</legend>

        <div className="grid grid-cols-2 gap-4">
          <div className="flex flex-col gap-1.5">
            <label htmlFor="class" className="text-sm font-medium text-foreground">
              Class
            </label>
            <select
              id="class"
              value={npcClass}
              onChange={(e) => setNpcClass(e.target.value as NpcClass | "")}
              className={inputClass}
            >
              <option value="">Not set</option>
              {NPC_CLASSES.map((option) => (
                <option key={option} value={option}>
                  {option}
                </option>
              ))}
            </select>
          </div>

          <div className="flex flex-col gap-1.5">
            <label htmlFor="alignment" className="text-sm font-medium text-foreground">
              Alignment
            </label>
            <select
              id="alignment"
              value={alignment}
              onChange={(e) => setAlignment(e.target.value as Alignment | "")}
              className={inputClass}
            >
              <option value="">Not set</option>
              {ALIGNMENTS.map((option) => (
                <option key={option} value={option}>
                  {ALIGNMENT_LABELS[option]}
                </option>
              ))}
            </select>
          </div>

          <div className="flex flex-col gap-1.5">
            <label htmlFor="occupation" className="text-sm font-medium text-foreground">
              Occupation
            </label>
            <input
              id="occupation"
              value={occupation}
              onChange={(e) => setOccupation(e.target.value)}
              maxLength={200}
              className={inputClass}
            />
          </div>

          <div className="flex flex-col gap-1.5">
            <label htmlFor="disposition" className="text-sm font-medium text-foreground">
              Disposition
            </label>
            <input
              id="disposition"
              value={disposition}
              onChange={(e) => setDisposition(e.target.value)}
              maxLength={100}
              placeholder="Friendly"
              className={inputClass}
            />
          </div>
        </div>

        <div className="flex flex-col gap-1.5">
          <label htmlFor="status" className="text-sm font-medium text-foreground">
            Status
          </label>
          <select
            id="status"
            value={status}
            onChange={(e) => setStatus(e.target.value as NpcStatus)}
            className={`${inputClass} max-w-xs`}
          >
            {NPC_STATUSES.map((option) => (
              <option key={option} value={option}>
                {option}
              </option>
            ))}
          </select>
        </div>
      </fieldset>

      <fieldset className="flex flex-col gap-4">
        <legend className="text-sm font-semibold text-foreground">Details</legend>

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

        <div className="flex flex-col gap-1.5">
          <label htmlFor="portraitUrl" className="text-sm font-medium text-foreground">
            Portrait URL
          </label>
          <input
            id="portraitUrl"
            value={portraitUrl}
            onChange={(e) => setPortraitUrl(e.target.value)}
            maxLength={500}
            type="url"
            className={inputClass}
          />
        </div>
      </fieldset>

      {error && <p className="text-sm text-red-400">{error}</p>}

      <button
        type="submit"
        disabled={isSubmitting}
        className="self-start rounded-md bg-accent px-5 py-2 font-medium text-accent-foreground transition-opacity hover:opacity-90 disabled:opacity-50"
      >
        {isSubmitting ? "Saving..." : npc ? "Save Changes" : "Create NPC"}
      </button>
    </form>
  );
}
