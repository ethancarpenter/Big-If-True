export interface NavItem {
  label: string;
  href: string;
}

// Add a nav item here once its feature exists. Deliberately not listing
// NPCs/Quests/Locations/Cities/Settings yet — those routes don't exist
// until their milestone lands.
export const navItems: NavItem[] = [
  { label: "Dashboard", href: "/campaigns" },
  { label: "Campaigns", href: "/campaigns" },
];
