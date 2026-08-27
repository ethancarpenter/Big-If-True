import { CityForm } from "@/components/CityForm";

interface NewCityPageProps {
  params: Promise<{ id: string }>;
}

export default async function NewCityPage({ params }: NewCityPageProps) {
  const { id: campaignId } = await params;

  return (
    <div className="flex flex-col gap-6">
      <h2 className="text-lg font-semibold text-foreground">New City</h2>
      <CityForm campaignId={campaignId} />
    </div>
  );
}
