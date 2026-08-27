"use client";

import { BaseEdge, EdgeLabelRenderer, getBezierPath, useReactFlow, type Edge, type EdgeProps } from "@xyflow/react";
import { QUEST_CONNECTION_TYPE_LABELS } from "./quest-connection-type-labels";
import { deleteQuestConnection, type QuestConnectionType } from "@/lib/api";

export interface QuestConnectionEdgeData extends Record<string, unknown> {
  connectionType: QuestConnectionType;
}

export type QuestConnectionEdgeType = Edge<QuestConnectionEdgeData, "questConnection">;

export function QuestConnectionEdge({
  id,
  sourceX,
  sourceY,
  targetX,
  targetY,
  sourcePosition,
  targetPosition,
  markerEnd,
  data,
}: EdgeProps<QuestConnectionEdgeType>) {
  const { setEdges } = useReactFlow();

  const [edgePath, labelX, labelY] = getBezierPath({
    sourceX,
    sourceY,
    sourcePosition,
    targetX,
    targetY,
    targetPosition,
  });

  function handleDelete(event: React.MouseEvent) {
    event.stopPropagation();
    setEdges((eds) => eds.filter((e) => e.id !== id));
    void deleteQuestConnection(id);
  }

  if (!data) {
    return null;
  }

  return (
    <>
      <BaseEdge id={id} path={edgePath} markerEnd={markerEnd} style={{ stroke: "var(--border)", strokeWidth: 1.5 }} />
      <EdgeLabelRenderer>
        <div
          style={{
            position: "absolute",
            transform: `translate(-50%, -50%) translate(${labelX}px, ${labelY}px)`,
            pointerEvents: "all",
          }}
          className="flex items-center gap-1.5 rounded-full border border-border bg-surface px-2.5 py-1 text-xs font-medium text-foreground shadow-sm"
        >
          {QUEST_CONNECTION_TYPE_LABELS[data.connectionType]}
          <button onClick={handleDelete} className="text-muted hover:text-red-400" aria-label="Delete connection">
            ×
          </button>
        </div>
      </EdgeLabelRenderer>
    </>
  );
}
