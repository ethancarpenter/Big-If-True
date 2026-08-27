import { QuestForm } from "@/components/QuestForm";

interface NewQuestPageProps {
  params: Promise<{ id: string }>;
}

export default async function NewQuestPage({ params }: NewQuestPageProps) {
  const { id: campaignId } = await params;

  return (
    <div className="flex flex-col gap-6">
      <h2 className="text-lg font-semibold text-foreground">New Quest</h2>
      <QuestForm campaignId={campaignId} />
    </div>
  );
}
