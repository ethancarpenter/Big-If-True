import { notFound } from "next/navigation";
import { NpcForm } from "@/components/NpcForm";
import { ApiNotFoundError, getNpc } from "@/lib/server-api";

interface EditNpcPageProps {
  params: Promise<{ id: string; npcId: string }>;
}

export default async function EditNpcPage({ params }: EditNpcPageProps) {
  const { id: campaignId, npcId } = await params;

  let npc;
  try {
    npc = await getNpc(npcId);
  } catch (error) {
    if (error instanceof ApiNotFoundError) {
      notFound();
    }
    throw error;
  }

  return (
    <div className="flex flex-col gap-6">
      <h2 className="text-lg font-semibold text-foreground">Edit {npc.name}</h2>
      <NpcForm campaignId={campaignId} npc={npc} />
    </div>
  );
}
