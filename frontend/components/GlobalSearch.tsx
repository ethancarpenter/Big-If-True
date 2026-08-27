"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useMemo, useRef, useState, type KeyboardEvent } from "react";
import { search, type SearchResult, type SearchResultType } from "@/lib/api";

const TYPE_ORDER: SearchResultType[] = ["Campaign", "City", "Location", "Npc", "Quest"];

const TYPE_LABELS: Record<SearchResultType, string> = {
  Campaign: "Campaigns",
  City: "Cities",
  Location: "Locations",
  Npc: "NPCs",
  Quest: "Quests",
};

const DEBOUNCE_MS = 200;

export function GlobalSearch() {
  const router = useRouter();
  const [open, setOpen] = useState(false);
  const [query, setQuery] = useState("");
  const [results, setResults] = useState<SearchResult[]>([]);
  const [status, setStatus] = useState<"idle" | "loading" | "error">("idle");
  const [highlightedIndex, setHighlightedIndex] = useState(0);
  const inputRef = useRef<HTMLInputElement>(null);

  function openSearch() {
    setOpen(true);
  }

  // Resetting query/results/status here (an event handler, not an effect
  // body) rather than reactively watching `open` - the overlay is
  // conditionally unmounted while closed anyway, so there's nothing to
  // "synchronize"; this just makes the next open start from a clean slate.
  function closeSearch() {
    setOpen(false);
    setQuery("");
    setResults([]);
    setStatus("idle");
    setHighlightedIndex(0);
  }

  function navigateTo(url: string) {
    closeSearch();
    router.push(url);
  }

  // Ctrl/Cmd+K opens from anywhere - it's a modifier chord, not a bare
  // character, so it can't collide with normal typing in any of this app's
  // text fields the way a bare "/" shortcut would.
  useEffect(() => {
    function handleKeyDown(event: globalThis.KeyboardEvent) {
      if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === "k") {
        event.preventDefault();
        openSearch();
      } else if (event.key === "Escape") {
        closeSearch();
      }
    }
    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, []);

  // Focusing the input is a genuine side effect (imperatively touching the
  // DOM in response to becoming visible), unlike the state resets above.
  useEffect(() => {
    if (open) {
      inputRef.current?.focus();
    }
  }, [open]);

  // Debounced, abortable search. Every effect run's cleanup cancels its own
  // still-pending timeout and, if a request already started, aborts it -
  // whether because `query` changed again or the component unmounted -
  // so a slower earlier response can never overwrite a faster later one.
  // Deliberately does *not* reset results/status when the query goes back
  // to empty: the render below always checks the empty-query case first,
  // so stale results/status can never be shown regardless.
  useEffect(() => {
    if (!query.trim()) {
      return;
    }

    const controller = new AbortController();
    const timeoutId = setTimeout(async () => {
      setStatus("loading");
      try {
        const data = await search(query, controller.signal);
        setResults(data);
        setStatus("idle");
        setHighlightedIndex(0);
      } catch {
        if (!controller.signal.aborted) {
          setStatus("error");
        }
      }
    }, DEBOUNCE_MS);

    return () => {
      clearTimeout(timeoutId);
      controller.abort();
    };
  }, [query]);

  const groupedResults = useMemo(
    () =>
      TYPE_ORDER.map((type) => ({ type, items: results.filter((r) => r.type === type) })).filter(
        (group) => group.items.length > 0,
      ),
    [results],
  );

  function handleInputKeyDown(event: KeyboardEvent<HTMLInputElement>) {
    if (event.key === "ArrowDown") {
      event.preventDefault();
      setHighlightedIndex((i) => Math.min(i + 1, results.length - 1));
    } else if (event.key === "ArrowUp") {
      event.preventDefault();
      setHighlightedIndex((i) => Math.max(i - 1, 0));
    } else if (event.key === "Enter") {
      const highlighted = results[highlightedIndex];
      if (highlighted) {
        event.preventDefault();
        navigateTo(highlighted.url);
      }
    }
  }

  return (
    <>
      <button
        onClick={openSearch}
        className="flex items-center gap-2 rounded-md border border-border bg-background px-3 py-1.5 text-sm text-muted transition-colors hover:border-accent hover:text-foreground"
      >
        Search...
        <span className="rounded border border-border px-1.5 py-0.5 text-xs">Ctrl K</span>
      </button>

      {open && (
        <div className="fixed inset-0 z-50 flex items-start justify-center bg-black/60 pt-24" onClick={closeSearch}>
          <div
            className="w-full max-w-lg overflow-hidden rounded-lg border border-border bg-surface shadow-xl"
            onClick={(e) => e.stopPropagation()}
          >
            <input
              ref={inputRef}
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              onKeyDown={handleInputKeyDown}
              placeholder="Search campaigns, cities, locations, NPCs, and quests..."
              className="w-full border-b border-border bg-transparent px-4 py-3 text-foreground outline-none placeholder:text-muted"
            />

            <div className="max-h-96 overflow-y-auto p-2">
              {query.trim() === "" ? (
                <p className="px-3 py-2 text-sm text-muted">
                  Search campaigns, cities, locations, NPCs, and quests.
                </p>
              ) : status === "loading" ? (
                <p className="px-3 py-2 text-sm text-muted">Searching...</p>
              ) : status === "error" ? (
                <p className="px-3 py-2 text-sm text-red-400">
                  Something went wrong searching. Please try again.
                </p>
              ) : results.length === 0 ? (
                <p className="px-3 py-2 text-sm text-muted">No results for &ldquo;{query}&rdquo;.</p>
              ) : (
                groupedResults.map((group) => (
                  <div key={group.type} className="mb-2 last:mb-0">
                    <p className="px-3 py-1 text-xs font-semibold uppercase tracking-wide text-muted">
                      {TYPE_LABELS[group.type]}
                    </p>
                    {group.items.map((item) => {
                      const flatIndex = results.indexOf(item);
                      return (
                        <Link
                          key={`${item.type}-${item.id}`}
                          href={item.url}
                          onClick={closeSearch}
                          onMouseEnter={() => setHighlightedIndex(flatIndex)}
                          className={`flex items-center justify-between gap-3 rounded-md px-3 py-2 text-sm ${
                            flatIndex === highlightedIndex
                              ? "bg-accent/20 text-accent"
                              : "text-foreground hover:bg-white/5"
                          }`}
                        >
                          <span className="truncate">{item.name}</span>
                          <span className="shrink-0 text-xs text-muted">
                            {item.parentContext ? `${item.parentContext} · ` : ""}
                            {item.campaignName}
                          </span>
                        </Link>
                      );
                    })}
                  </div>
                ))
              )}
            </div>
          </div>
        </div>
      )}
    </>
  );
}
