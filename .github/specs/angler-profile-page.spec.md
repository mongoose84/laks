# Feature: Angler Profile Page (Fiskerside)

> Status: Draft — research-led spec, not yet implementation-frozen
> Related research: `.github/specs/ux-research-findings.md` §4 P3 "Personal angler page"
> Related design: `.github/specs/editorial-magazine-homepage.spec.md` (active visual direction, `ed-*` class system)

## Problem

The Holmfoss group has ~36 anglers with a shared history that stretches back roughly 20 years.
Today, an angler's name is everywhere on the site — on the leaderboard, in the recent-catches
feed, in the all-time records, in the catch log table — but a name is a **dead end**. Clicking
or tapping it does nothing.

The UX research (`ux-research-findings.md` §4, Job-to-be-Done #4) identifies "long-term identity"
as a primary user need:

> **Long-term identity**: "What's my fishing story over 20 years?" → needs personal stats +
> all-time records

The v1 site (fisk.krunk.dk) already supports a personal page via name search — and the same
research lists it as a confirmed "what works" item (§3, "Personal stats — individual ownership
of fishing record"). v2 lost this capability when it was first built; the priority matrix lists
the dedicated personal angler page as **P3, "future phase"**, but it is now being picked up.

Concrete friction points the profile page resolves:

1. **No follow-through on names.** Anglers see "Bjørn topped the leaderboard with 38,2 kg" and
   have no way to drill into how he caught those fish — was it one big day, or spread out?
   Which bait? Where? They have to ask him in person.
2. **No nostalgia surface.** Off-season (Aug–May), the group is the audience for the site, but
   there is nothing personal to come back to outside the dashboards. The stakeholder explicitly
   wants 20 years of history to feel owned.
3. **Family members lose context.** Secondary users (§1, "Family Members") want to follow a
   specific angler — their partner, father, grandfather — not aggregate data. There is no
   per-person view to share with them.
4. **The leaderboard tells only half the story.** Total weight and best fish are visible, but
   not season-by-season trajectory, favourite bait, or time-of-day pattern — the kind of
   bragging-rights detail that drives the friendly competition the research identifies as a
   primary engagement hook.

## Solution

Introduce a dedicated **angler profile page** at `/Anglers/{id:int}` and make every angler-name
rendering across the site a link to that page.

### Core principles

| Principle | Source | Application |
|---|---|---|
| Data IS the hero | research §5, dashboard spec | No marketing hero — name + identity strip, then numbers |
| Glanceable at distance | research §1 (iPad, arm's length, ages 40-75) | Career headlines render at the same scale as conditions on the homepage |
| Editorial magazine identity | `editorial-magazine-homepage.spec.md` | Reuse `ed-*` primitives (`ed-section`, `ed-conditions-grid`, `ed-table`, `ed-catch-article`, `ed-pill`) |
| Selective, not exhaustive | research §1 ("not highly technical") | Pick statistics that tell a story; do not surface everything technically possible |
| Server-rendered first | project CLAUDE.md | One PageModel load. No client-side fetches. |

### Information architecture (top to bottom)

The page is organised as **identity → headline numbers → trajectory → recent activity →
preferences → context**, mirroring how the stakeholder described browsing his own record:
"How am I doing overall? How did I get here? What's working for me?"

```
┌──────────────────────────────────────────────────────────────┐
│  Top bar (existing _TopBar)                                  │
├──────────────────────────────────────────────────────────────┤
│  ⮌ Tilbage           (back link to referrer or /Statistics)  │
│                                                              │
│  Bjørn Hansen                                                │
│  Fisker · Norge                            ▸ kicker line     │
│  ──────────────────────────────────────                       │
│                                                              │
│  ┌──── KARRIERE (ALL-TIME) ─────────────────────────────┐    │
│  │ Fangster       Total kg     Største      Sæsoner    │    │
│  │   127            842,4       12,1 kg       18       │    │
│  │   stk             kg          2018         år       │    │
│  └──────────────────────────────────────────────────────┘    │
│                                                              │
│  SÆSON 2026 (only when there is current-season activity)     │
│  ┌──────────────────────────────────────────────────────┐    │
│  │ Fangster   kg     Største   Rang        Hold        │    │
│  │   5       38,2     12,1      #1 af 24    2          │    │
│  └──────────────────────────────────────────────────────┘    │
│                                                              │
│  SÆSON FOR SÆSON                                             │
│  (table: year · fish · total kg · biggest · rank)            │
│                                                              │
│  SENESTE FANGSTER (10 newest)                                │
│  (editorial article rows — same template as homepage)        │
│                                                              │
│  HVAD VIRKER                                                 │
│  ┌──── Foretrukken agn ──┬──── Tid på dagen ────┐            │
│  │  Flue · 42 %          │  Morgen · 53 %       │            │
│  │  (top 3 list)         │  (sparkline 0–23)    │            │
│  └───────────────────────┴──────────────────────┘            │
│                                                              │
│  Footer (existing)                                           │
└──────────────────────────────────────────────────────────────┘
```

### Statistic selection — what to show and why

Each module is justified against the user profiles and JTBD from the research. **Selectivity
is intentional**: we are not adding a "personal statistics" subapp, we are surfacing the few
numbers that match how the group actually talks about itself.

| Module | Fields | JTBD served | Rationale |
|---|---|---|---|
| **Identity strip** | Name (Fraunces headline), `Country` (kicker), career-span ("2005–2026 · 22 sæsoner") | Identity / "this is MY record" | The 20-year story starts with framing. Country only shown if present (data is `string?`). |
| **Karriere strip** (`_AnglerCareerStrip`) | `Fangster i alt` (count), `Total kg`, `Største fangst` (kg + year), `Sæsoner aktive` (distinct year count) | "What's my fishing story over 20 years?" (§4 JTBD 4) | These are the four numbers the stakeholder bragged about in interviews ("127 fish over 20 years"). All four are derivable from `GetByAnglerAsync`. |
| **Sæson 2026 strip** (`_AnglerCurrentSeasonStrip`) | Fangster i sæsonen, kg, Største, **Rang** (in current-year leaderboard), **Hold** (group number for current year if angler is registered) | "How am I doing right now?" (in-season Morning Check, §1) | Only renders when current season has catches OR the angler has a current-year group config. Off-season: hide entirely. Rank reuses existing `GetLeaderboardAsync(year)` ordering. |
| **Sæson for sæson** (`_AnglerSeasonHistory`) | Table: År · Fangster · kg · Største · Rang (in that year) | "What's my best year? When did I peak?" (§4 JTBD 4, §6 "Investment / sunk cost") | Year-by-year is the unit anglers compare themselves on. Rank-per-year is the bragging detail. Sorted newest first. |
| **Seneste fangster** (`_AnglerRecentCatches`) | 10 most recent catches for this angler — same editorial article template as homepage | "How am I doing right now?" + family member curiosity | Reuses `_RecentCatches` styling for visual coherence. |
| **Foretrukken agn** (`_AnglerBaitPreference`) | Top 3 baits by catch count, with % share of career catches | "What's working for me?" / friendly competition (§6) | Bait selection is the most discussed tactical detail in the group; the existing `Bait` field on `Catch` is reliably populated based on existing usage in `_RecentCatches`. |
| **Tid på dagen** (`_AnglerTimeOfDay`) | Bucket the angler's catches into 4 day-parts: Morgen (04–10), Dag (10–16), Aften (16–22), Nat (22–04). Highlight the biggest bucket. | Tactical curiosity — same family as §6 statistics insight modules ("time of day") | Mirrors the existing `GetCatchesByHourAsync` time-of-day chart on the statistics page, but for one angler. A four-bucket textual presentation avoids needing Chart.js on the profile page. |

### Statistics explicitly **deferred or rejected** (and why)

To stop scope creep, the spec is explicit about what we are **not** showing on the first pass:

- **Favourite location.** `Catch.Location` is free text. Without normalisation it would produce
  noisy lists ("Holmfoss Øvre" vs "Holmfoss øvre" vs "Øvre Holmfoss"). Defer until location
  taxonomy is decided. Surface in catch list instead.
- **Personal map.** A Leaflet map per angler is technically possible but the catch map on the
  homepage already serves the "where to fish" job. A per-angler map serves vanity more than a
  user need. Defer.
- **Comparison with another angler.** Side-by-side compare is a feature, not a page. Defer.
- **Weather affinity / water-level affinity.** The existing statistics page water-level-band
  chart serves the group-level insight. Per-angler is too granular and probably noisy at
  ~5 catches per season per person.
- **Badges / achievements.** Research §6 explicitly warns against gamification overkill for
  a 60+ audience.
- **Length (cm) records.** The research §9 open question 2 still flags length vs weight as
  unresolved. Until resolved, weight is the single metric.
- **Team / partner stats.** Research §2 confirms team pairings are not tracked. Defer.

## User Stories

### Primary

- As an angler, I want to click my own name on the leaderboard and see my full career numbers,
  so that I get a sense of ownership over my 20 years of fishing history.
- As an angler, I want to click another angler's name on the leaderboard and see how they got
  to the top, so that I can talk about specifics next time I see them.
- As an angler in-season, I want to see my current rank and group on my own profile, so that
  I know exactly where I stand without scanning the full leaderboard.

### Secondary

- As an angler in the off-season, I want to browse my own season-by-season history, so that
  I can relive the good years (research §6 "Investment / sunk cost").
- As an angler curious about tactics, I want to see which bait and time of day have worked
  best for a specific angler, so that I can learn from what's working.
- As a family member, I want to open a link to my partner's profile and see their season,
  so that I can follow along from home without needing to interpret a leaderboard.

### Tertiary / edge

- As a visitor who lands on a profile for an angler with no catches yet, I want to see an
  empty-state that explains the situation rather than a broken page.
- As a visitor who follows a stale link to a deleted/unknown angler id, I want a Danish 404.

## Click targets — every name becomes a link

The user request was: "We have a lot of names, and all names should be clickable." The
following inventory was verified by grepping for `AnglerName` and `AnglerId` across
`Laks.Web/`.

| File | Current rendering | After this spec | Has `AnglerId` available? |
|---|---|---|---|
| `Pages/Shared/_Leaderboard.cshtml` (homepage top-5) | `<span class="ed-lb-name">@item.AnglerName</span>` | `<a class="ed-lb-name" href="/Anglers/@item.AnglerId">@item.AnglerName</a>` | **Yes** (`LeaderboardEntry.AnglerId`) |
| `Pages/Statistics/Leaderboard.cshtml` (full leaderboard table) | `<td>@item.AnglerName</td>` | `<td><a href="/Anglers/@item.AnglerId">@item.AnglerName</a></td>` | **Yes** |
| `Pages/Shared/_RecentCatches.cshtml` (homepage feed) | `@c.AnglerName landede en ...` inside `.ed-catch-headline` | Wrap `c.AnglerName` in `<a href="/Anglers/@c.AnglerId">` | **Yes** (`Catch.AnglerId`) |
| `Pages/Catches/Index.cshtml` (catch log table) | `<td>@c.AnglerName</td>` | `<td><a href="/Anglers/@c.AnglerId">@c.AnglerName</a></td>` | **Yes** |
| `Pages/Shared/_AllTimeRecords.cshtml` | Renders `Records.BiggestFishAngler` and `Records.MostProlificAngler` as plain text | Link to the angler profile if id can be resolved — **but see data-contract note below** | **No** — `AllTimeRecords` only carries `BiggestFishAngler` (string) and `MostProlificAngler` (string) |
| `Pages/Statistics/Index.cshtml` — team table "Fanget af" column (`@team.AnglerName`) | Plain text | Link if id can be resolved | **No** — `BiggestSalmonPerTeam.AnglerName` is a string, no id |
| `wwwroot/js/catch-map.js` — popup with angler name (verify) | Verify what the popup actually shows | If the popup shows an angler name, link it via `CatchLocation.AnglerName` | **No** — `CatchLocation` carries `AnglerName` but no `AnglerId` |
| `Pages/Statistics/Index.cshtml` — "Fangster pr. fisker" bar chart labels (Chart.js) | Names rendered inside a `<canvas>` (no DOM) | **Out of scope** — Chart.js canvas labels are not HTML links. Optionally render an accessible name list below the chart with links in a follow-up. | n/a |

### Data-contract consideration (flagged risk)

Three of the seven name-rendering surfaces only carry the angler's **name string**, not an id:

- `AllTimeRecords` (`BiggestFishAngler`, `MostProlificAngler`)
- `BiggestSalmonPerTeam` (`AnglerName`)
- `CatchLocation` (`AnglerName`)

For the spec to deliver "every name is clickable everywhere", the architect must choose one of
the following at contract-freeze time:

1. **Extend the models** to include `AnglerId` (and `MostProlificAnglerId`, `BiggestFishAnglerId`,
   etc.) alongside the names. This is the cleanest approach and matches how
   `LeaderboardEntry`/`Catch` already work. Requires repository SQL changes (already JOIN to
   `Person`, so the id is one column away) but no migration.
2. **Server-side name lookup** in the Razor page — resolve name → id via
   `IAnglerRepository.GetAllAsync()` in the page model and emit links by name match. Fragile
   if two anglers ever share a name. Cheap to ship.
3. **Defer linking** for these three surfaces in v1 of this feature, ship the other four (the
   high-value ones), and open a follow-up for the remaining three once the data contract is
   updated.

**Recommendation:** Option 1 — extend the models. The fix is small (one additional SELECT
column in three SQL queries) and removes the entire class of "what if two anglers share a
name?" bugs from this spec.

### Link styling

All angler-name links use a single shared treatment so the click target is recognisable
across pages without being visually noisy:

- Inherits the page text colour (cream `--text-primary`) — does **not** use accent rust
  except on hover/focus.
- Underline on hover and focus only (not at rest) — keeps the editorial typography clean.
  This is permitted by WCAG since names are inside table rows / styled containers that
  already telegraph interactivity (table hover, leaderboard row hover).
- Focus state: a clearly visible 2px `--accent` outline (the existing focus ring used by
  `ed-pill`), so keyboard users can see where they are.
- `aria-label="Se @AnglerName's profil"` is **not** added — the link text already names the
  angler, and adding a second name would be noisy for screen reader users. Default link text
  is sufficient.

## Page layout — Danish strings

All user-facing text mirrors the conventions in `.github/specs/danish-translation.spec.md`.

| Element | Danish |
|---|---|
| `ViewData["Title"]` | `Fiskerprofil` |
| Back link | `← Tilbage` |
| Identity kicker | `Fisker` (or `Fisker · {Country}` if country is present) |
| Career-span line | `Aktiv {firstYear}–{lastYear} · {n} sæsoner` (e.g. `Aktiv 2005–2026 · 18 sæsoner`) |
| Career section header | `Karriere` |
| Career labels | `Fangster`, `Total kg`, `Største fangst`, `Sæsoner aktive` |
| "Største" sub-label | `kg · sæson {year}` (e.g. `12,1 kg · sæson 2018`) |
| Current-season section header | `Sæson @currentYear` |
| Current-season labels | `Fangster`, `Total kg`, `Største`, `Rang`, `Hold` |
| Rang format | `#@rank af @total` (e.g. `#1 af 24`) — `total` is the count of anglers with at least one catch in the season |
| Hold format | `@groupNumber` if registered in `season_config` for the current year, otherwise hide the cell |
| Off-season fallback (no current season activity) | Skip the section entirely; do not render an empty card |
| Season history header | `Sæson for sæson` |
| Season history columns | `År`, `Fangster`, `Total kg`, `Største`, `Rang` |
| Recent catches header | `Seneste fangster` with `Se alle i fangstloggen →` link to `/Catches/Index?year=&anglerId=…` (future enhancement: see Open Questions) |
| "What works" section header | `Hvad virker` |
| Bait sub-header | `Foretrukken agn` |
| Bait list item | `@bait · @pct %` (e.g. `Flue · 42 %`) |
| Bait empty | `Ingen agn registreret.` |
| Time-of-day sub-header | `Tid på dagen` |
| Time-of-day buckets | `Morgen` (04:00–09:59), `Dag` (10:00–15:59), `Aften` (16:00–21:59), `Nat` (22:00–03:59) |
| Time-of-day winner | `@bucketName · @pct %` |
| Empty state — angler has no catches | `Ingen fangster registreret endnu.` |
| 404 page heading | `Side ikke fundet` (generic — see note below) |
| 404 page body | `Siden du ledte efter findes ikke. Gå tilbage til <a asp-page="/Index">forsiden</a>.` (generic — see note below) |

> **Review decision (site-wide 404):** The `NotFound` Razor Page is registered via
> `UseStatusCodePagesWithReExecute("/NotFound")` in `Program.cs`, which means it handles ALL
> 404s on the site, not only unknown angler ids. The copy was therefore made generic during
> code review so it is appropriate for any missing page. The angler-specific wording originally
> specified here (`Fisker ikke fundet`) has been replaced with the generic copy above.

### Number formatting

All numeric formatting reuses the existing `da-DK` culture pattern already used across the
site (comma decimal). Examples: `38,2 kg`, `12,1 kg`. Counts are integer without decimal.

## Technical Changes

### Backend

**Stack**: C# with .NET, Razor Pages, MySQL via Dapper repositories.

**Components**:

- [ ] Route: `/Anglers/{id:int}` — a Razor Page (see Open Questions for path bikeshed).
- [ ] PageModel: `Pages/Anglers/Index.cshtml.cs` (or `Profile.cshtml.cs` — see Open Questions).
  Loads: `IAnglerRepository.GetByIdAsync(id)` (404 if null), `ICatchRepository.GetByAnglerAsync(id)`
  (career data), `ISeasonRepository.GetSeasonConfigAsync(currentYear)` (current group), and
  `ICatchRepository.GetLeaderboardAsync(currentYear)` for the current-year rank lookup.
  All loads run in parallel as the existing `IndexModel` does.
- [ ] **No new repository methods are required for the minimal happy path** — career stats,
  season history, bait preference, and time-of-day are all derivable in-memory from
  `GetByAnglerAsync(id)`. With ~36 anglers and ~5–10 catches per angler per season over
  ~20 years, the worst-case payload is in the low hundreds of rows — safe to compute in
  the PageModel without optimisation.
- [ ] **Optional new repository method** (recommended once volume grows or if perf becomes
  a concern): `ICatchRepository.GetAnglerProfileAsync(int anglerId)` returning a
  pre-aggregated `AnglerProfileStats` projection. Defer to architect.
- [ ] New view model: `Models/AnglerProfile.cs` (or a record nested in the PageModel) holding
  the computed career numbers, the per-season list, the bait top-3, and the time-of-day
  bucket counts. Keeps the `.cshtml` thin.
- [ ] Data-contract update (see "Data-contract consideration" above): add `AnglerId` (and
  related ids) to `AllTimeRecords`, `BiggestSalmonPerTeam`, and `CatchLocation`, plus the
  matching SELECT additions in their repository queries.
- [ ] No database migration is required for the minimal scope — existing schema covers all
  computed values.

### Frontend

**Stack**: ASP.NET Core Razor Pages (.cshtml + PageModel), `ed-*` editorial CSS, no JS.

**Components**:

- [ ] New Razor Page: `Pages/Anglers/Index.cshtml` rendering the layout sketched above.
- [ ] New partials (each scoped, each reusing `ed-section` / `ed-conditions-grid`):
  - `Pages/Shared/_AnglerIdentity.cshtml` — name + kicker + active-years line
  - `Pages/Shared/_AnglerCareerStrip.cshtml`
  - `Pages/Shared/_AnglerCurrentSeasonStrip.cshtml`
  - `Pages/Shared/_AnglerSeasonHistory.cshtml`
  - `Pages/Shared/_AnglerRecentCatches.cshtml` — same template structure as `_RecentCatches`
  - `Pages/Shared/_AnglerBaitPreference.cshtml`
  - `Pages/Shared/_AnglerTimeOfDay.cshtml`
- [ ] Updated partials and pages — add anchor wrappers around angler names:
  - `Pages/Shared/_Leaderboard.cshtml`
  - `Pages/Statistics/Leaderboard.cshtml`
  - `Pages/Shared/_RecentCatches.cshtml`
  - `Pages/Catches/Index.cshtml`
  - `Pages/Shared/_AllTimeRecords.cshtml` (after data-contract update)
  - `Pages/Statistics/Index.cshtml` — team table "Fanget af" column (after data-contract update)
  - `wwwroot/js/catch-map.js` — popup HTML (after data-contract update to `CatchLocation`)
- [ ] CSS: extend `wwwroot/css/editorial.css` with a small ruleset for `.ed-angler-link`
  (or `.ed-link` if generalised) — underline-on-hover, focus ring. **No** new colours or
  tokens; reuses the existing `--text-primary` / `--accent` palette.
- [ ] Client-side script integration: **none required**. All computation happens server-side.

## Testing

- [ ] Unit tests for the PageModel `OnGetAsync`:
  - Happy path: angler exists, has catches across multiple seasons → all numbers correct.
  - Angler exists, zero catches → renders identity + empty-state, no NREs.
  - Angler exists, current-season activity → current-season strip renders with rank.
  - Angler exists, no current-season activity → current-season strip is hidden.
  - Unknown id → returns 404 (`NotFound()`).
- [ ] Unit tests for the aggregation helpers (`BuildCareerStats`, `BuildSeasonHistory`,
  `BuildBaitPreference`, `BuildTimeOfDayBuckets`) — pure functions over `IEnumerable<Catch>`,
  easy to test without DB.
- [ ] Time-of-day bucket boundaries — verify a catch at 03:59 lands in `Nat`, 04:00 in
  `Morgen`, etc.
- [ ] Bait preference — verify case-insensitive grouping (e.g. `Flue` and `flue` count as
  one bait) and that the top 3 are sorted by count descending, ties broken by name.
- [ ] Rank lookup edge cases: angler ties with another on weight (existing repository order
  defines tie-break — verify same ordering is used here).
- [ ] Link rendering tests on the partials — at minimum a snapshot/string-contains assertion
  that the anchor is present with the correct `href` for each updated partial.
- [ ] Manual visual regression at 360, 480, 768, 1024, 1280 px against the editorial palette.
- [ ] Keyboard navigation: tab from leaderboard row to profile, back link returns focus
  near the originating row (browser-default behaviour is acceptable; explicit focus
  management is out of scope).

## Acceptance Criteria

- [ ] Navigating to `/Anglers/{id}` for a known angler renders the page with identity strip,
  career strip, season history table, recent catches, bait preference, and time-of-day.
- [ ] All user-facing text is in Danish and matches the strings table above.
- [ ] Current-season strip renders if and only if the angler has at least one catch in the
  current year **or** is registered in a `season_config` row for the current year.
- [ ] Rank in the current-season strip equals the angler's position from
  `GetLeaderboardAsync(currentYear)`.
- [ ] `Største fangst` shows the heaviest weight ever recorded for the angler, with the
  season year next to it.
- [ ] `Sæson for sæson` table lists one row per distinct `SeasonYear` the angler has caught
  in, sorted newest first, with rank from that year's leaderboard.
- [ ] Bait preference lists the top 3 baits by catch count for the angler, with percentage
  of career catches.
- [ ] Time-of-day section displays the four day-part buckets with counts and percentages and
  highlights the largest.
- [ ] `_Leaderboard` (homepage top-5) and `Statistics/Leaderboard` (full table) render
  angler names as links to `/Anglers/{id}`.
- [ ] `_RecentCatches` (homepage feed) and `Catches/Index` (catch log table) render angler
  names as links to `/Anglers/{id}`.
- [ ] `_AllTimeRecords`, the Statistics team table "Fanget af" column, and the catch-map
  popup link to `/Anglers/{id}` **after** the data-contract update is applied (see
  Dependencies).
- [ ] Hitting `/Anglers/{id}` for an unknown id returns HTTP 404 with the Danish 404 page.
- [ ] Hitting `/Anglers/{id}` for a known angler with zero catches renders the identity
  strip and the empty state `Ingen fangster registreret endnu.`, no NREs.
- [ ] Link styling: angler-name links are visually consistent across all surfaces (same
  hover and focus treatment).
- [ ] WCAG 2.1 AA: link contrast ratio ≥ 4.5:1 against its container background; focus
  outline is visible on keyboard navigation; the `<h1>` is the angler's name; section
  headers are `<h2>`; the season-history table has a proper `<caption>` and `<th scope>`.
- [ ] All existing tests continue to pass; new tests added per the Testing section pass.

## Dependencies

- [ ] Data-contract update (architect decision): add `AnglerId` (and counterparts) to
  `AllTimeRecords`, `BiggestSalmonPerTeam`, and `CatchLocation`. Without this, three of the
  seven name-rendering surfaces cannot be linked in v1 — and the user request was "all names
  should be clickable", so this is on the critical path for full acceptance.
- [ ] No new NuGet packages.
- [ ] No new CDN dependencies.
- [ ] No new external API dependencies.
- [ ] No database migration.

## Risks & Assumptions

| Item | Type | Notes |
|---|---|---|
| Rank computation uses existing `GetLeaderboardAsync` ordering | Assumption | Inherits tie-break behaviour (weight → fish count → name) from the existing repository. Verify with tester. |
| `Catch.Bait` field is populated reliably enough to compute preferences | Assumption | `_RecentCatches` already branches on `string.IsNullOrWhiteSpace(c.Bait)`, suggesting it is occasionally empty. Empty baits are excluded from the preference count, with an empty-state fallback. |
| Three name-rendering surfaces lack ids | Risk | Mitigated by Option 1 above (extend models). If architect picks Option 3 (defer), update Acceptance Criteria accordingly. |
| Active-year span uses distinct `SeasonYear` values | Assumption | `2005, 2007, 2009` renders as `Aktiv 2005–2009 · 3 sæsoner` (count is distinct years, not span). Documented above so it is unambiguous. |
| Page load time | Assumption | A single `GetByAnglerAsync` returns at most a few hundred rows for the most prolific angler. No paging or caching needed in v1. |
| Privacy | Confirmed | Site is unauthenticated, trusted closed group, ~36 known users — no PII concern beyond name/country already in the database (per existing Privacy page). |
| Linking from Chart.js canvas labels | Out of scope | Chart bars on `/Statistics/Index` "Fangster pr. fisker" cannot be linked because Chart.js renders them inside `<canvas>`. Acceptable v1 trade-off; a follow-up could render a names list under the chart. |

## Open Questions

1. **Route shape**: `/Anglers/{id:int}` (collection convention, matches `/Catches` and
   `/Statistics`) or `/Angler/{id:int}` (singular)? Prefer `/Anglers/{id:int}` for symmetry.
2. **Slug vs id**: Should the URL include a slug for shareability (`/Anglers/12-bjorn-hansen`)?
   The site is private, so shareability is low value. Recommend id-only for v1.
3. **Default route on `/Anglers` (no id)**: Should `/Anglers` list all anglers as a directory,
   or redirect to `/Statistics/Leaderboard`? Stakeholder did not request a directory; recommend
   a 404 or a redirect to the leaderboard for v1.
4. **Length (cm) on the profile**: Same as research §9 Q2 — the data is in the database but
   not currently surfaced. Decision pending. The spec defers it.
5. **"Se alle i fangstloggen →" deep link**: Would be cleaner if `/Catches/Index` accepted an
   `anglerId` query parameter to pre-filter the catch log. Out of scope for this spec; flag
   as a follow-up.
6. **Recent catches count**: 10 was chosen by analogy with the homepage. Stakeholder may
   prefer a different value (e.g. "this season only"). Verify with a 5-minute confirmation.
7. **Off-season identity headline**: When there is no current-season activity, the page
   reads as a record-keeping archive. Is that the right tone for ages 40–75 to engage with
   off-season, or should we add a "season starts in N days" line tied to `SeasonDay`?
   Recommend deferring until the on-season MVP ships and we can observe usage patterns.
8. **Chart vs text for time-of-day**: A four-bucket text presentation is simpler and avoids
   adding Chart.js to the profile page. If the stakeholder finds it too coarse, a per-hour
   sparkline matching the statistics page is a future enhancement.

## Notes

- The page is **read-only**. There is no edit, no comment, no follow. Per CLAUDE.md and the
  research §6 anti-patterns, the site is pull-based with no social features.
- The page must work on iPad portrait (768×1024) as the primary device, per research §1.
- Country is the only `Angler` field beyond name, and it is nullable. Render it as a kicker
  only when present; do not render an "Ukendt" placeholder.
- The `Pages/CLAUDE.md` design-language note still references the older blue palette
  (`#1A4D7A`); however, the active homepage redesign (`editorial-magazine-homepage.spec.md`)
  is the visual source of truth for any new page added today, and the `ed-*` class system is
  what every existing page now uses. This spec follows the editorial direction for visual
  coherence with the rest of the site.
