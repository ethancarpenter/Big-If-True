import { redirect } from "next/navigation";
import { getCurrentUser } from "@/lib/server-api";

interface AuthLayoutProps {
  children: React.ReactNode;
}

export default async function AuthLayout({ children }: AuthLayoutProps) {
  const user = await getCurrentUser();
  if (user) {
    redirect("/campaigns");
  }

  return <div className="flex flex-1 items-center justify-center p-8">{children}</div>;
}
