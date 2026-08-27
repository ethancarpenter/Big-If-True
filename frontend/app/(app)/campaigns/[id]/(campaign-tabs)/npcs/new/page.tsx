import { NpcForm } from "@/components/NpcForm";

interface NewNpcPageProps {
  params: Promise<{ id: string }>;
}

export default async function NewNpcPage({ params }: NewNpcPageProps) {
  const { id: campaignId } = await params;

  return (
    <div className="flex flex-col gap-6">
      <h2 className="text-lg font-semibold text-foreground">New NPC</h2>
      <NpcForm campaignId={campaignId} />
    </div>
  );
}
