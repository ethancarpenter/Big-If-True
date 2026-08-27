import Link from "next/link";

export default function NotFound() {
  return (
    <div className="flex max-w-2xl flex-col items-center gap-4 rounded-lg border border-dashed border-border p-12 text-center text-muted">
      <h2 className="font-serif text-2xl font-semibold text-foreground">Page not found</h2>
      <p>The page you&rsquo;re looking for doesn&rsquo;t exist, or you don&rsquo;t have access to it.</p>
      <Link
        href="/campaigns"
        className="rounded-md bg-accent px-4 py-2 text-sm font-medium text-accent-foreground hover:opacity-90"
      >
        Back to Campaigns
      </Link>
    </div>
  );
}
