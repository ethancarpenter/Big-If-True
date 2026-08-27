import { CampaignForm } from "@/components/CampaignForm";

export default function NewCampaignPage() {
  return (
    <div className="flex flex-col gap-6">
      <h1 className="font-serif text-2xl font-semibold text-foreground">New Campaign</h1>
      <CampaignForm />
    </div>
  );
}
