import { redirect } from "next/navigation";
import { Sidebar } from "@/components/Sidebar";
import { TopNavigation } from "@/components/TopNavigation";
import { getCurrentUser } from "@/lib/server-api";

interface AppLayoutProps {
  children: React.ReactNode;
}

/**
 * Gates the whole authenticated shell behind a real session. This redirect
 * is a UX nicety only, not a security boundary - every page under here
 * still fetches its own data from the API, which independently enforces
 * authentication on every request regardless of what this layout does.
 */
export default async function AppLayout({ children }: AppLayoutProps) {
  const user = await getCurrentUser();
  if (!user) {
    redirect("/login");
  }

  return (
    <>
      <TopNavigation user={user} />
      <div className="flex flex-1">
        <Sidebar />
        <main className="flex-1 p-8">{children}</main>
      </div>
    </>
  );
}
