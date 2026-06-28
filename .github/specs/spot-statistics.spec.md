# Feature: Pladser — Per-Spot Statistics Module

## Problem

Holmfoss club members frequently ask "where on the river should I fish today?" — but the existing
`/Statistics` page provides no per-spot view. Trend, angler, type, team, season-progress, hour and
water-level modules are all aggregations across spots, so the data which most directly informs the
in-season decision *"which numbered spot is historically best, and under what conditions?"* is
invisible.

The members already think in terms of the river's numbered/named pegs (`1`, `1a`, `1c`, `2`, `4`,
`5`, `4.5`, `8`, `Foss`, `Hytterne`, `Pynten`, `Klipperne`, `Talerstolen`, `Walle`). These names come
from the free-text `Catch.Location` field and are imported as-is by `Laks.Importer/DataConverter.cs`.
There is currently no surface that exposes:

- which spots produce the most fish over time,
- which spots produce the biggest fish (and who caught them),
- which bait, water level and time of day historically work best at each spot.

These insights are particularly valuable for newer/visiting anglers and for veterans planning the
five fishing days of their group's window.

## Solution

Add a new module — heading **"Pladser"** — to the existing `/Statistics` page, alongside the
existing trend/angler/type/team/season-progress/hour/water-level modules. The module is **all-time
only**: it deliberately ignores the page's year filter and aggregates across every season, because
the value here is the long-running pattern at each spot, not a single-season snapshot.

The module consists of two coordinated views:

1. **Bar chart (Chart.js)** ranking spots by all-time catch count, descending — at-a-glance answer
   to "which spots produce the most fish?"
2. **Sortable table** below the chart with one row per spot, surfacing the count, weight, biggest
   fish (with angler + date) and the "best conditions" trio (top bait, best water-level band, best
   time of day).

The module is data-driven: only spots that actually appear in `Catch.Location` get a row. Blank
locations are excluded. All catch types are included (no salmon-only restriction) because spot
productivity matters for sea trout / other species too.

## User Stories

### Primary (planning the day at Holmfoss)

- As an angler, I want to see a ranked bar chart of which spots have produced the most fish
  all-time, so I can prioritise where to start.
- As an angler, I want to see the biggest fish ever caught at each spot, with who caught it and
  when, so I know which spots have trophy potential.
- As an angler, I want to see the historically best bait, water level and time of day for each
  spot, so I can match my approach to today's conditions.

### Secondary (analysis and storytelling)

- As an angler comparing pegs, I want to sort the table by total kg, average kg or biggest fish, so
  I can answer questions other than "most fish".
- As a club member browsing off-season, I want to see all-time stats per spot, so I can revisit the
  river's history regardless of which year is currently filtered on the rest of the page.

## Placement on `/Statistics`

The module appears as a new `<section>` on `Laks.Web/Pages/Statistics/Index.cshtml`, in this order
relative to existing modules:

```
1. Fangster pr. år (historisk trend)
2. Sæsonens forløb (fangster pr. uge)
3. Fangster fordelt på døgnet  |  Fangster fordelt på vandstand
4. Fangster pr. fisker         |  Fangsttype-fordeling
5. Største laks pr. hold
6. Pladser   ← NEW (full width, all-time, ignores year filter)
```

The year filter (`<select id="year">`) **continues to control the other modules unchanged**. The
"Pladser" section visually signals that it is all-time via a kicker / sub-label (see UI strings
below).

## Module Layout

Full-width section, same `.ed-section` / `.ed-chart-section` rhythm as the existing modules. Within
the section:

```
┌─ Pladser ──────────────────────────────────────── Alle sæsoner ─┐
│  H2: "Pladser"   kicker: "Alle sæsoner"                          │
│  Lead text: short Danish sentence explaining all-time scope.     │
│                                                                  │
│  ┌── Bar chart (Chart.js) ───────────────────────────────────┐  │
│  │  Antal fangster pr. plads (alle sæsoner)                  │  │
│  │  ▇▇▇▇▇▇▇▇  1                                              │  │
│  │  ▇▇▇▇▇▇    Foss                                           │  │
│  │  ▇▇▇▇▇     Pynten                                         │  │
│  │  ▇▇▇▇      …                                              │  │
│  │  (descending, one bar per spot present in data)           │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                  │
│  ┌── Sortable table ────────────────────────────────────────┐   │
│  │ Plads | Fangster | Vægt i alt | Gns. vægt | Største fisk │   │
│  │       | (kg)     | (kg)       | (kg, fisker, dato)       │   │
│  │       | Bedste agn | Bedste vandstand | Bedste tidsrum   │   │
│  └───────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────┘
```

Design language hooks:

- Reuse existing `.ed-section`, `.ed-section-title`, `.ed-kicker`, `.ed-chart-figure`,
  `.ed-chart-wrap`, `.ed-chart-caption`, `.ed-table-wrap`, `.ed-table`, `.ed-td-num` classes — no
  new global styles required.
- Per design language §2, numeric/statistical cells use the monospace stack already applied via
  `.ed-td-num` for column alignment.
- 8 px grid; spacing between chart and table at least 24 px (one `--space-3` increment).

## Data Model

A single new view-model carries one row per spot. Suggested shape (final naming to be confirmed by
`@architect`):

```csharp
// Laks.Web/Models/SpotStats.cs (new)
public class SpotStats
{
    public string Location { get; set; } = string.Empty;   // canonical spot label, as stored
    public int TotalCatches { get; set; }
    public decimal TotalWeightKg { get; set; }
    public decimal AvgWeightKg { get; set; }

    // Biggest fish at this spot
    public decimal BiggestWeightKg { get; set; }
    public string  BiggestAnglerName { get; set; } = string.Empty;
    public DateTime BiggestCatchDate { get; set; }

    // Best conditions (most frequent value, ties broken deterministically — see below)
    public string  TopBait { get; set; } = string.Empty;
    public decimal? BestWaterBandStartM { get; set; }   // null when no catches have water level
    public int?     BestHour { get; set; }              // 0–23, null when no catches have time
}
```

The result list is materialised entirely by the repository (no SQL in the PageModel) and exposed by
a new repository method on `ICatchRepository`:

```csharp
Task<IEnumerable<SpotStats>> GetCatchStatsPerSpotAsync();
```

(No `int? year` parameter — this is intentionally all-time.)

### Aggregation rules (authoritative)

| Metric | Rule |
|--------|------|
| Spot identity | Trimmed, case-sensitive value of `Catch.Location` as stored. No grouping of `4`, `5`, `4.5` etc. Blank/whitespace-only rows are excluded entirely. |
| Catch count | `COUNT(*)` of all catches at the spot, all catch types, all seasons. |
| Total weight (kg) | `SUM(WeightKg)`. |
| Average weight (kg) | `Total weight / Catch count`, rounded to 1 decimal at presentation. |
| Biggest fish | Single catch with max `WeightKg` at that spot. Ties broken by earliest `CatchDate` then lowest `Id`. Captures weight, angler name, date. |
| Top bait | Most frequent non-empty `Bait` value at that spot. Ties broken alphabetically (Danish collation, case-insensitive). Empty string displayed as em dash if no catches have a bait recorded. |
| Best water-level band | 0.25 m band (same logic as `IndexModel.FormatBandLabel` / `GetCatchesByWaterLevelAsync`) with the most catches at that spot. Catches with `WaterLevel == null` are excluded from this count. Ties broken by lowest band. `null` if no catches at the spot have a recorded water level. |
| Best time of day | Hour bucket (0–23) with the most catches at that spot. Catches missing `CatchTime` are excluded. Ties broken by earliest hour. `null` if no catches at the spot have a recorded time. |

### Default sort

The table loads sorted by **`TotalCatches` desc**, with ties broken by **`TotalWeightKg` desc**,
then by `Location` ascending (Danish collation). The bar chart uses the same order so the two views
read consistently.

### Sorting interaction

The table is sortable on every column. Default mechanism: each `<th>` is a `<button>` inside the
header cell (so it is keyboard-operable and announces as a button to AT), with
`aria-sort="ascending|descending|none"` reflecting the active state. Sorting can be implemented
either:

- **Server-side** via a `?spotSort=column&spotDir=desc` query param on `OnGetAsync`, preserving the
  page's existing year filter; or
- **Client-side** via a small vanilla-JS table sorter operating on the rendered DOM.

`@architect` decides; the spec requires only that the interaction works without page reload feeling
janky and that the chart order does *not* re-shuffle when the table is re-sorted (the chart stays
"by count, desc" — it is the canonical ranking).

## Danish UI Strings

All user-facing text in Danish per project rules and the translation map. Strings introduced by
this module:

| Location | Danish |
|----------|--------|
| Section heading (H2) | `Pladser` |
| Kicker / sub-label | `Alle sæsoner` |
| Lead paragraph | `Statistik pr. plads opgøres på tværs af alle sæsoner — uafhængigt af årsfilteret ovenfor.` |
| Chart card heading | `Antal fangster pr. plads` |
| Chart `aria-label` | `Søjlediagram over antal fangster pr. plads, alle sæsoner, sorteret faldende` |
| Chart caption | `Alle registrerede fangster pr. plads, på tværs af alle sæsoner.` |
| Chart empty state | `Ingen fangster med registreret plads.` |
| Chart x-axis title | `Plads` |
| Chart y-axis title | `Fangster` |
| Chart tooltip suffix | `fangster` |
| Chart legend / dataset label | `Fangster` |
| Table `aria-label` | `Statistik pr. plads, alle sæsoner` |
| Table caption (visually hidden ok) | `Statistik pr. plads — sortérbar tabel` |
| Table column: spot name | `Plads` |
| Table column: catches | `Fangster` |
| Table column: total weight | `Vægt i alt (kg)` |
| Table column: average weight | `Gns. vægt (kg)` |
| Table column: biggest weight | `Største (kg)` |
| Table column: biggest angler | `Fanget af` |
| Table column: biggest date | `Dato` |
| Table column: top bait | `Bedste agn` |
| Table column: best water band | `Bedste vandstand` |
| Table column: best time bucket | `Bedste tidsrum` |
| Empty cell placeholder | `—` (em dash) |
| Sort button `aria-label` (asc) | `Sortér efter {kolonne} stigende` |
| Sort button `aria-label` (desc) | `Sortér efter {kolonne} faldende` |
| Empty state (no spots at all) | `Ingen fangster registreret med plads endnu.` |

Notes on formatting:

- Numeric formatting uses `da-DK` culture (already established in `Statistics/Index.cshtml`), e.g.
  comma decimal separator: `4,2 kg`.
- Water band cell renders via the existing `FormatBandLabel` shape, e.g. `1,25–1,50 m`.
- Time bucket cell renders as `HH–HH`, e.g. `08–09`.
- Date cell renders as `d. MMM yyyy` in `da-DK`, e.g. `27. jun. 2023`.

## Accessibility (WCAG 2.1 AA)

- Section is a proper `<section>` with `aria-labelledby` pointing at the H2.
- Bar chart `<canvas>` has `role="img"` and a descriptive `aria-label`. A visually hidden
  `<table>` or `<dl>` mirror of the chart data is provided for screen-reader users (consistent
  with how other chart modules should be — note this raises the bar slightly for the rest of the
  page; flagged as an open question).
- The data table uses real `<table>`, `<thead>`, `<tbody>`, `<th scope="col">` semantics.
- Sortable headers expose `aria-sort`. Sort controls are real `<button>` elements; clicking or
  pressing `Enter` / `Space` triggers sorting. The active sort is reflected on page load (e.g.
  default: `aria-sort="descending"` on `Fangster`).
- Empty / `null` cells use the em dash with `aria-label="ingen data"` so SR users hear "ingen
  data" instead of nothing.
- All interactive elements meet 3:1 non-text contrast and 4.5:1 text contrast against the card
  background. Chart palette reuses the existing `PALETTE` constant in `Statistics/Index.cshtml`
  for visual consistency.
- Keyboard: every sort control is tab-reachable, has a visible focus outline (design language §4
  "tydelig fokusstil").

## Edge Cases

| Case | Behaviour |
|------|-----------|
| `Catch.Location` is null, empty or whitespace | Catch is excluded from this module entirely (does not contribute to any spot). |
| Spot has only catches with `WaterLevel = null` | `BestWaterBandStartM` is `null`; cell renders em dash. |
| Spot has only catches with no `CatchTime` (or `00:00:00` interpreted as missing — see open Q) | `BestHour` is `null`; cell renders em dash. |
| Spot has only catches with no `Bait` | `TopBait` is empty; cell renders em dash. |
| Ties on top bait | Pick alphabetically first using `da-DK` collation (case-insensitive). |
| Ties on best water band | Pick the **lowest** band start. |
| Ties on best hour | Pick the **earliest** hour. |
| Ties on biggest fish (same weight, different anglers) | Pick the **earliest** `CatchDate`, then the lowest `Catch.Id`. |
| Ties on row sort (Catches equal) | Break by `TotalWeightKg` desc, then `Location` asc. |
| Database has zero catches with a non-blank location | Whole module renders the "Ingen fangster registreret med plads endnu" empty state in place of both chart and table. |
| Only one spot has data | Chart still renders (single bar), table renders single row. No special case. |
| Spot label casing variants (e.g. `Foss` vs `foss`) | Treated as distinct rows. The importer normalises labels at write time; this module trusts the stored value. Flagged as open question. |
| Page year filter is changed | This module does **not** re-query. Its state is independent of `Model.SelectedYear`. |
| Repository call fails | Module shows the empty state; failure is logged via `_logger.LogError`, consistent with existing `OnGetAsync` catch block. The rest of the page continues to render. |

## Technical Changes

> Identified, not implemented. Implementation is delegated to `@fullstack-dotnet` per orchestrator
> rules. All data access must go through repositories. All SQL must be parameterised (OWASP).

### Backend

**Stack**: C# / ASP.NET Core, Razor Pages, Dapper-style repository over MySQL.

- [ ] New model: `Laks.Web/Models/SpotStats.cs` with the shape above. Live alongside existing
      `ChartModels.cs` types, or added to that file.
- [ ] New method on `Laks.Web/Data/Repositories/ICatchRepository.cs`:
      `Task<IEnumerable<SpotStats>> GetCatchStatsPerSpotAsync();`
- [ ] Implementation in `Laks.Web/Data/Repositories/CatchRepository.cs`. Must use parameterised
      queries; aggregation may be done either fully in SQL (single round-trip preferred) or via
      a server-side group-by over a flat `SELECT` of relevant columns
      (`Location`, `WeightKg`, `Bait`, `WaterLevel`, `CatchTime`, `AnglerName`, `CatchDate`, `Id`).
- [ ] `Statistics/Index.cshtml.cs` (`OnGetAsync`):
      - call `GetCatchStatsPerSpotAsync()` in the existing `Task.WhenAll` block,
      - expose new properties: `SpotStatsRows` (for the table) and `SpotChartLabelsJson` /
        `SpotChartCountsJson` (for the bar chart),
      - **do not** pass `year` to the new method.
- [ ] No database migrations required — all data already exists in the `catches` table.

### Frontend

**Stack**: ASP.NET Core Razor Pages (.cshtml). Minimal vanilla JS, only for the Chart.js bar
chart and (optionally) the table sorter.

- [ ] `Laks.Web/Pages/Statistics/Index.cshtml`:
      add a new `<section class="ed-section" aria-labelledby="spots-title">` containing
      the chart `<figure>` and the data table, placed after the team module.
- [ ] Bar chart wired in the existing inline `<script>` block, reusing `Chart.defaults` and
      `PALETTE` already declared there. Dataset label `Fangster`, single colour from PALETTE.
      `indexAxis: 'y'` is acceptable if a horizontal bar is preferred for long spot labels —
      to be decided at implementation time.
- [ ] Optional small client-side table sorter in `wwwroot/js/` if the architect chooses
      client-side sorting. Otherwise add `?spotSort=…&spotDir=…` query handling in `OnGetAsync`
      and render headers as `<form>`-submit buttons (works without JS).
- [ ] No new CSS files; reuse existing editorial classes. Add only what cannot be expressed via
      existing utility classes.

## Acceptance Criteria

- [ ] A new section with heading `Pladser` appears on `/Statistics` after the team module.
- [ ] The section heading has a visible "Alle sæsoner" kicker.
- [ ] Changing the page's year filter has **no effect** on the chart or table in this section.
- [ ] The bar chart renders one bar per distinct, non-blank `Catch.Location` value in the data,
      sorted by all-time catch count, descending.
- [ ] The chart includes all catch types (no `CatchType = 'Salmon'` filter).
- [ ] The table includes columns: `Plads`, `Fangster`, `Vægt i alt (kg)`, `Gns. vægt (kg)`,
      `Største (kg)`, `Fanget af`, `Dato`, `Bedste agn`, `Bedste vandstand`, `Bedste tidsrum` —
      with the Danish headings as listed.
- [ ] Default table sort is catch count descending; ties broken by total weight descending.
- [ ] Every column header is operable as a sort control (click and keyboard) and reflects state
      via `aria-sort`.
- [ ] `Bedste vandstand` cells display 0.25 m bands using the existing `FormatBandLabel` style
      (e.g. `1,25–1,50 m`).
- [ ] `Bedste tidsrum` cells display the winning hour as `HH–HH` (e.g. `08–09`).
- [ ] `Bedste agn` cells show the most frequent bait at that spot, em dash when none.
- [ ] When a spot has no catches with water level / time / bait, the corresponding cell shows `—`
      with `aria-label="ingen data"`.
- [ ] When the dataset contains zero catches with a non-blank location, the section shows the
      empty state `Ingen fangster registreret med plads endnu.` in place of chart and table.
- [ ] Numbers are formatted in `da-DK`: weights as `0,0` kg, dates as `27. jun. 2023`.
- [ ] Data is loaded via a new `ICatchRepository.GetCatchStatsPerSpotAsync()` method. No SQL in
      the PageModel. All queries are parameterised.
- [ ] No regression: all existing modules on `/Statistics` continue to honour the year filter.
- [ ] WCAG 2.1 AA: keyboard navigation works through the sort buttons; focus styles visible;
      chart has descriptive `aria-label`; table uses correct semantic markup.
- [ ] Unit tests cover: aggregation correctness, deterministic tie-breaking for top bait / best
      band / best hour, default row sort, exclusion of blank locations, behaviour with empty
      data, and `OnGetAsync` exposing the data to the view.

## Dependencies

- [ ] No new third-party libraries. Reuses Chart.js (already loaded by the Statistics page).
- [ ] No new database migrations.
- [ ] Depends on existing `Catch.Location`, `Catch.WeightKg`, `Catch.Bait`, `Catch.WaterLevel`,
      `Catch.CatchTime`, `Catch.AnglerName`, `Catch.CatchDate` fields populated by the importer.

## Risks & Assumptions

| Item | Type | Note |
|------|------|------|
| Spot labels are stored consistently (no `Foss` vs `foss` vs `FOSS` drift) | Assumption | Importer normalises today, but historic data has not been audited. Worst case: a small number of duplicate-looking rows in the table. Flagged as open question. |
| `CatchTime` of `00:00:00` means "midnight" vs "unknown" | Assumption | Existing hour chart treats them as hour 0. Same treatment here for consistency. Confirm with data owner. |
| The list of distinct spots stays manageable for a single bar chart (currently ~14 known) | Validated | Spec assumes no horizontal scroll needed at the supported viewport widths. |
| All-time scope is the right default (vs. last N years) | Validated by user | Settled in elicitation. |
| Single chart + table is sufficient (no per-spot drilldown page in this scope) | Validated by user | Drilldown is a separate future feature. |
| Ignoring the page year filter is unambiguous to users | Risk | Mitigated by the "Alle sæsoner" kicker and lead text. |

## Open Questions

1. **Casing / whitespace normalisation of `Location`**: do we trust the importer's normalisation, or
   should the repository upper-case / trim defensively when grouping? Recommended: trim + group
   case-insensitive, display the most-common original casing. Confirm with data owner.
2. **Sort implementation**: server-side query-param sort (no-JS friendly) vs. small client-side
   sorter. Defer to `@architect`.
3. **Chart orientation**: vertical bars (current convention on the page) vs. horizontal bars
   (better for long spot labels like `Talerstolen`). Defer to `@architect` / visual review.
4. **Accessible chart mirror**: should we ship a visually hidden data table mirror for the bar
   chart? If yes, the same pattern arguably should be retrofitted to the existing charts on the
   page. Out of scope here, but flagged.
5. **Per-spot drilldown**: clicking a row could later route to `/Statistics/Plads/{name}` with a
   richer view. Not in scope for this module — capture only as a future enhancement.

## Notes

- The `Bedste vandstand` band logic must reuse `IndexModel.FormatBandLabel` (or the equivalent
  formatting rule) to stay visually consistent with the existing water-level module.
- Per `Laks.Web/Pages/CLAUDE.md`, monospace font already applies via `.ed-td-num` to the numeric
  columns; the band and time-bucket columns are also numeric-ish — apply `.ed-td-num` to them as
  well so columns align.
- This module is intentionally read-only and has no form posts.
- This module deliberately does **not** add any new external dependencies (no map, no extra chart
  library, no extra CSS file) — it is additive to the existing `/Statistics` page only.