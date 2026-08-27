"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";
import { deleteCampaign } from "@/lib/api";
import { ConfirmationDialog } from "./ConfirmationDialog";

interface DeleteCampaignButtonProps {
  campaignId: string;
  campaignName: string;
}

export function DeleteCampaignButton({ campaignId, campaignName }: DeleteCampaignButtonProps) {
  const router = useRouter();
  const [open, setOpen] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);

  async function handleConfirm() {
    setIsDeleting(true);
    await deleteCampaign(campaignId);
    router.push("/campaigns");
    router.refresh();
  }

  return (
    <>
      <button
        onClick={() => setOpen(true)}
        className="rounded-md border border-red-500/40 px-4 py-2 text-sm font-medium text-red-400 hover:bg-red-500/10"
      >
        Delete Campaign
      </button>
      <ConfirmationDialog
        open={open}
        title="Delete campaign?"
        description={`This will permanently delete "${campaignName}". This cannot be undone.`}
        confirmLabel={isDeleting ? "Deleting..." : "Delete"}
        onConfirm={handleConfirm}
        onCancel={() => setOpen(false)}
      />
    </>
  );
}
