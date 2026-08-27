import { TabNavigation } from "@/components/TabNavigation";

interface QuestViewsLayoutProps {
  children: React.ReactNode;
  params: Promise<{ id: string }>;
}

export default async function QuestViewsLayout({ children, params }: QuestViewsLayoutProps) {
  const { id: campaignId } = await params;

  return (
    <div className="flex flex-col gap-6">
      <TabNavigation
        tabs={[
          { label: "List", href: `/campaigns/${campaignId}/quests` },
          { label: "Graph", href: `/campaigns/${campaignId}/quests/graph` },
        ]}
      />

      {children}
    </div>
  );
}
