import Link from "next/link";
import { notFound } from "next/navigation";
import { DeleteEntityButton } from "@/components/DeleteEntityButton";
import { TabNavigation } from "@/components/TabNavigation";
import { ApiNotFoundError, getCampaign } from "@/lib/api";

interface CampaignLayoutProps {
  children: React.ReactNode;
  params: Promise<{ id: string }>;
}

export default async function CampaignLayout({ children, params }: CampaignLayoutProps) {
  const { id } = await params;

  let campaign;
  try {
    campaign = await getCampaign(id);
  } catch (error) {
    if (error instanceof ApiNotFoundError) {
      notFound();
    }
    throw error;
  }

  return (
    <div className="flex flex-col gap-6">
      <div className="flex items-center justify-between">
        <h1 className="font-serif text-3xl font-semibold text-foreground">{campaign.name}</h1>
        <div className="flex gap-3">
          <Link
            href={`/campaigns/${campaign.id}/edit`}
            className="rounded-md border border-border px-4 py-2 text-sm font-medium text-foreground hover:bg-white/5"
          >
            Edit
          </Link>
          <DeleteEntityButton kind="campaign" id={campaign.id} name={campaign.name} redirectTo="/campaigns" />
        </div>
      </div>

      <TabNavigation
        tabs={[
          { label: "Overview", href: `/campaigns/${campaign.id}` },
          { label: "Cities", href: `/campaigns/${campaign.id}/cities` },
        ]}
      />

      {children}
    </div>
  );
}
