"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";
import { deleteCampaign, deleteCity } from "@/lib/api";
import { ConfirmationDialog } from "./ConfirmationDialog";

// Functions can't be passed from Server Components into Client Component
// props, so this switches on `kind` and calls the matching API function
// itself rather than accepting an onDelete callback. Add a case here as
// each future entity (Location, NPC, Quest) gets a delete action.
type DeleteEntityButtonProps =
  | { kind: "campaign"; id: string; name: string; redirectTo: string }
  | { kind: "city"; id: string; name: string; redirectTo: string };

const labels = {
  campaign: "Campaign",
  city: "City",
} as const;

export function DeleteEntityButton(props: DeleteEntityButtonProps) {
  const router = useRouter();
  const [open, setOpen] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);

  const label = labels[props.kind];

  async function handleConfirm() {
    setIsDeleting(true);
    if (props.kind === "campaign") {
      await deleteCampaign(props.id);
    } else {
      await deleteCity(props.id);
    }
    router.push(props.redirectTo);
    router.refresh();
  }

  return (
    <>
      <button
        onClick={() => setOpen(true)}
        className="rounded-md border border-red-500/40 px-4 py-2 text-sm font-medium text-red-400 hover:bg-red-500/10"
      >
        Delete {label}
      </button>
      <ConfirmationDialog
        open={open}
        title={`Delete ${label.toLowerCase()}?`}
        description={`This will permanently delete "${props.name}". This cannot be undone.`}
        confirmLabel={isDeleting ? "Deleting..." : "Delete"}
        onConfirm={handleConfirm}
        onCancel={() => setOpen(false)}
      />
    </>
  );
}
