"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { Badge } from "./Badge";
import { ConfirmationDialog } from "./ConfirmationDialog";
import { QuestNpcDialog } from "./QuestNpcDialog";
import { QUEST_NPC_ROLE_LABELS } from "./quest-npc-role-labels";
import { deleteQuestNpc, type Npc, type Quest, type QuestNpc } from "@/lib/api";

interface QuestNpcsSectionProps {
  quest: Quest;
  campaignId: string;
  relationships: QuestNpc[];
  campaignNpcs: Npc[];
}

export function QuestNpcsSection({ quest, campaignId, relationships, campaignNpcs }: QuestNpcsSectionProps) {
  const router = useRouter();
  const [addOpen, setAddOpen] = useState(false);
  const [editing, setEditing] = useState<QuestNpc | null>(null);
  const [removing, setRemoving] = useState<QuestNpc | null>(null);
  const [isRemoving, setIsRemoving] = useState(false);

  const availableNpcs = campaignNpcs.filter((npc) => !relationships.some((r) => r.npcId === npc.id));

  async function handleRemoveConfirm() {
    if (!removing) {
      return;
    }
    setIsRemoving(true);
    await deleteQuestNpc(removing.id);
    setIsRemoving(false);
    setRemoving(null);
    router.refresh();
  }

  return (
    <div className="flex flex-col gap-4">
      <div className="flex items-center justify-between">
        <h3 className="text-sm font-semibold text-foreground">NPCs</h3>
        <button
          onClick={() => setAddOpen(true)}
          className="rounded-md border border-border px-3 py-1.5 text-sm font-medium text-foreground hover:bg-white/5"
        >
          + Add NPC
        </button>
      </div>

      {relationships.length === 0 ? (
        <p className="text-sm italic text-muted">Not linked to any NPCs yet.</p>
      ) : (
        <ul className="flex flex-col gap-2">
          {relationships.map((relationship) => (
            <li
              key={relationship.id}
              className="flex flex-wrap items-center justify-between gap-3 rounded-md border border-border bg-surface px-4 py-3"
            >
              <div className="flex flex-wrap items-center gap-3">
                <div>
                  <Link
                    href={`/campaigns/${campaignId}/npcs/${relationship.npcId}`}
                    className="text-sm font-medium text-accent hover:underline"
                  >
                    {relationship.npcName}
                  </Link>
                  {relationship.notes && <p className="text-xs text-muted">{relationship.notes}</p>}
                </div>
                <Badge>{QUEST_NPC_ROLE_LABELS[relationship.role]}</Badge>
              </div>
              <div className="flex gap-3">
                <button
                  onClick={() => setEditing(relationship)}
                  className="text-sm text-muted hover:text-foreground"
                >
                  Edit
                </button>
                <button
                  onClick={() => setRemoving(relationship)}
                  className="text-sm text-red-400 hover:text-red-300"
                >
                  Remove
                </button>
              </div>
            </li>
          ))}
        </ul>
      )}

      {addOpen && (
        <QuestNpcDialog
          open
          mode="create"
          quest={quest}
          availableNpcs={availableNpcs}
          onClose={() => setAddOpen(false)}
        />
      )}

      {editing && (
        <QuestNpcDialog open={editing !== null} mode="edit" relationship={editing} onClose={() => setEditing(null)} />
      )}

      <ConfirmationDialog
        open={removing !== null}
        title="Remove NPC relationship?"
        description={
          removing
            ? `This will remove the link between "${quest.name}" and ${removing.npcName}. This cannot be undone.`
            : ""
        }
        confirmLabel={isRemoving ? "Removing..." : "Remove"}
        onConfirm={handleRemoveConfirm}
        onCancel={() => setRemoving(null)}
      />
    </div>
  );
}
