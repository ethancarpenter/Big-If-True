"use client";

import "@xyflow/react/dist/style.css";
import {
  Background,
  Controls,
  MarkerType,
  MiniMap,
  ReactFlow,
  ReactFlowProvider,
  useEdgesState,
  useNodesState,
  type Connection,
} from "@xyflow/react";
import { useCallback, useMemo, useState } from "react";
import { QuestConnectionDialog } from "./QuestConnectionDialog";
import { QuestConnectionEdge, type QuestConnectionEdgeData, type QuestConnectionEdgeType } from "./QuestConnectionEdge";
import { QuestGraphNode, type QuestGraphNodeData, type QuestGraphNodeType } from "./QuestGraphNode";
import {
  ApiConflictError,
  createQuestConnection,
  deleteQuestConnection,
  updateQuestGraphPosition,
  type Quest,
  type QuestConnection,
  type QuestConnectionType,
  type QuestGraphPosition,
} from "@/lib/api";

const nodeTypes = { quest: QuestGraphNode };
const edgeTypes = { questConnection: QuestConnectionEdge };

const GRID_COLUMNS = 4;
const GRID_SPACING_X = 260;
const GRID_SPACING_Y = 140;

interface QuestGraphProps {
  campaignId: string;
  quests: Quest[];
  connections: QuestConnection[];
  positions: QuestGraphPosition[];
}

function buildEdge(connection: QuestConnection): QuestConnectionEdgeType {
  return {
    id: connection.id,
    source: connection.sourceQuestId,
    target: connection.targetQuestId,
    type: "questConnection",
    markerEnd:
      connection.connectionType === "Related" ? undefined : { type: MarkerType.ArrowClosed, color: "var(--accent)" },
    data: { connectionType: connection.connectionType } satisfies QuestConnectionEdgeData,
  };
}

function buildInitialNodes(quests: Quest[], positions: QuestGraphPosition[], campaignId: string): QuestGraphNodeType[] {
  const positionByQuestId = new Map(positions.map((p) => [p.questId, p]));
  return quests.map((quest, index) => {
    const saved = positionByQuestId.get(quest.id);
    const position = saved
      ? { x: saved.x, y: saved.y }
      : { x: (index % GRID_COLUMNS) * GRID_SPACING_X, y: Math.floor(index / GRID_COLUMNS) * GRID_SPACING_Y };
    return {
      id: quest.id,
      type: "quest",
      position,
      data: { quest, campaignId } satisfies QuestGraphNodeData,
    };
  });
}

function QuestGraphInner({ campaignId, quests, connections, positions }: QuestGraphProps) {
  const questsById = useMemo(() => new Map(quests.map((q) => [q.id, q])), [quests]);

  // Seeded once from server props (React state only ever reads the first
  // value passed here, same as useState); local state owns every update
  // from here — no router.refresh() after mutations, see the note in the
  // Milestone 9 plan about why this graph deliberately deviates from that
  // pattern (it would visibly reset pan/zoom on every drag or edge edit).
  const [nodes, , onNodesChange] = useNodesState(buildInitialNodes(quests, positions, campaignId));
  const [edges, setEdges, onEdgesChange] = useEdgesState(connections.map(buildEdge));
  const [pendingConnection, setPendingConnection] = useState<Connection | null>(null);

  const handleConnect = useCallback((connection: Connection) => {
    if (!connection.source || !connection.target) {
      return;
    }
    setPendingConnection(connection);
  }, []);

  async function handleConfirmConnection(connectionType: QuestConnectionType) {
    if (!pendingConnection?.source || !pendingConnection.target) {
      return;
    }

    try {
      const created = await createQuestConnection(campaignId, {
        sourceQuestId: pendingConnection.source,
        targetQuestId: pendingConnection.target,
        connectionType,
      });
      setEdges((eds) => [...eds, buildEdge(created)]);
      setPendingConnection(null);
    } catch (err) {
      if (err instanceof ApiConflictError) {
        throw new Error(
          err.reason === "Cycle"
            ? "This would create a cycle in the quest progression graph."
            : "This connection already exists.",
        );
      }
      throw new Error("Something went wrong creating this connection. Please try again.");
    }
  }

  const handleNodeDragStop = useCallback((_event: unknown, node: QuestGraphNodeType) => {
    void updateQuestGraphPosition(node.id, { x: node.position.x, y: node.position.y });
  }, []);

  const handleEdgesDelete = useCallback((deleted: QuestConnectionEdgeType[]) => {
    for (const edge of deleted) {
      void deleteQuestConnection(edge.id);
    }
  }, []);

  const pendingSourceName = pendingConnection?.source ? questsById.get(pendingConnection.source)?.name : undefined;
  const pendingTargetName = pendingConnection?.target ? questsById.get(pendingConnection.target)?.name : undefined;

  return (
    <div className="h-[70vh] w-full overflow-hidden rounded-lg border border-border bg-background">
      <ReactFlow
        nodes={nodes}
        edges={edges}
        onNodesChange={onNodesChange}
        onEdgesChange={onEdgesChange}
        onConnect={handleConnect}
        onNodeDragStop={handleNodeDragStop}
        onEdgesDelete={handleEdgesDelete}
        nodeTypes={nodeTypes}
        edgeTypes={edgeTypes}
        colorMode="dark"
        fitView
      >
        <Background gap={20} color="var(--border)" />
        <Controls showInteractive={false} />
        <MiniMap pannable zoomable nodeColor="var(--surface)" maskColor="rgba(15, 14, 23, 0.75)" />
      </ReactFlow>

      {pendingConnection && pendingSourceName && pendingTargetName && (
        <QuestConnectionDialog
          sourceQuestName={pendingSourceName}
          targetQuestName={pendingTargetName}
          onConfirm={handleConfirmConnection}
          onCancel={() => setPendingConnection(null)}
        />
      )}
    </div>
  );
}

export function QuestGraph(props: QuestGraphProps) {
  return (
    <ReactFlowProvider>
      <QuestGraphInner {...props} />
    </ReactFlowProvider>
  );
}
