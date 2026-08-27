import { notFound } from "next/navigation";
import { QuestForm } from "@/components/QuestForm";
import { ApiNotFoundError, getQuest } from "@/lib/api";

interface EditQuestPageProps {
  params: Promise<{ id: string; questId: string }>;
}

export default async function EditQuestPage({ params }: EditQuestPageProps) {
  const { id: campaignId, questId } = await params;

  let quest;
  try {
    quest = await getQuest(questId);
  } catch (error) {
    if (error instanceof ApiNotFoundError) {
      notFound();
    }
    throw error;
  }

  return (
    <div className="flex flex-col gap-6">
      <h2 className="text-lg font-semibold text-foreground">Edit {quest.name}</h2>
      <QuestForm campaignId={campaignId} quest={quest} />
    </div>
  );
}
