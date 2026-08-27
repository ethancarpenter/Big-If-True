export interface NavItem {
  label: string;
  href: string;
}

// Add a nav item here once its feature exists. Deliberately not listing
// NPCs/Quests/Locations/Cities/Settings yet — those routes don't exist
// until their milestone lands.
//
// "Dashboard" used to sit alongside "Campaigns" here, pointing at the exact
// same URL — a leftover from Milestone 1's placeholder nav, before any
// dashboard-specific page existed. Dropped as redundant; add a real
// Dashboard entry back only if a distinct dashboard page is ever built.
export const navItems: NavItem[] = [
  { label: "Campaigns", href: "/campaigns" },
];
