import { NavLinks } from "./NavLinks";

export function Sidebar() {
  return (
    <aside className="hidden w-56 shrink-0 border-r border-border bg-surface p-4 md:block">
      <NavLinks />
    </aside>
  );
}
