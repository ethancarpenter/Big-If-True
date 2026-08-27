"use client";

import { Handle, Position, type Node, type NodeProps } from "@xyflow/react";
import { useRouter } from "next/navigation";
import { StatusBadge } from "./StatusBadge";
import { QUEST_STATUS_TONE } from "./quest-status-tone";
import type { Quest } from "@/lib/api";

export interface QuestGraphNodeData extends Record<string, unknown> {
  quest: Quest;
  campaignId: string;
}

export type QuestGraphNodeType = Node<QuestGraphNodeData, "quest">;

export function QuestGraphNode({ data }: NodeProps<QuestGraphNodeType>) {
  const router = useRouter();
  const { quest, campaignId } = data;

  return (
    <div
      onClick={() => router.push(`/campaigns/${campaignId}/quests/${quest.id}`)}
      className="w-56 cursor-pointer rounded-lg border border-border bg-surface px-4 py-3 shadow-sm transition-colors hover:border-accent"
    >
      <Handle type="target" position={Position.Left} className="!h-2.5 !w-2.5 !border-accent !bg-surface" />

      <p className="truncate font-serif text-sm font-semibold text-foreground">{quest.name}</p>
      <div className="mt-2">
        <StatusBadge label={quest.status} tone={QUEST_STATUS_TONE[quest.status]} />
      </div>

      <Handle type="source" position={Position.Right} className="!h-2.5 !w-2.5 !border-accent !bg-surface" />
    </div>
  );
}
