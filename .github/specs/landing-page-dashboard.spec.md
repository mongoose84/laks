# Feature: Landing Page Dashboard

## Problem

The current landing page is a generic hero section with basic stats (total catches, latest season, recent catches table). It doesn't serve the primary user need: **"What are conditions like right now, and where should I fish?"**

The ~36 anglers (aged 40-75) visit the site mainly during their 5-day fishing window at Holmfoss on Numedalslågen in June-July. They check the site from the house before heading to the river. They need to make quick decisions based on:

- **Water level and trend** (rising = better chances)
- **Weather and temperature**
- **Where others have been catching fish**
- **How the group is doing** (friendly competition, leaderboard)

The current v1 site at fisk.krunk.dk addresses some of this (water level card, heatmap on a map), but feels "homemade." The new landing page must feel **polished, data-dense, and professional** — like a stock dashboard or elveguiden.no/laksebors — while being purpose-built for this closed group, not the general public.

## Solution

Replace the current hero + stats cards landing page with a **conditions-first data dashboard** organized into clearly separated card sections. Inspired by the data density of financial dashboards and the domain relevance of elveguiden.no/laksebors, but tailored to a private group of 36 anglers.

### Priority Layout (top to bottom, above-the-fold first)

1. **Conditions strip** — Weather, air temp, water level + trend, water temp, wind
2. **Water level 24h graph** — Chart.js line showing rising/falling trend
3. **Season leaderboard** — Current year top anglers, filterable by group
4. **Catch map** — Heatmap of catch locations at Holmfoss on Numedalslågen
5. **Recent catches feed** — Latest catches with angler, weight, species, location
6. **Season summary strip** — Total catches, avg weight, biggest fish, active days
7. **All-time records** — Biggest fish ever, most prolific angler, best season

### Design Principles

| Principle | Rationale |
|-----------|-----------|
| **Data-dense, not decorative** | Users are affluent men 40-75 who read stock pages. They want numbers, not fluff. No hero banners, no marketing copy. |
| **Glanceable at distance** | Primary metrics (water level, temp) in `display-3` or larger. Readable from 2m on an iPad. |
| **Card-based grid** | Each section is a tight card — professional, scannable, like a Bloomberg terminal or elveguiden.no |
| **Minimal interaction** | No clicks needed for the top 3 sections. Scroll for more. Filter only for leaderboard group toggle. |
| **Responsive** | Single column on phone, 2-column grid on iPad/desktop |
| **Year-round consistency** | Same layout always. Off-season: conditions show last data + "Season starts June X" banner. |

### Behavioral Design Elements

| Principle | Applied As |
|-----------|-----------|
| **Trigger** | Rising water level with green arrow = "conditions are good, go NOW" — creates urgency |
| **Variable reward** | Leaderboard changes after each night's data import, water level shifts hourly — every visit shows new data |
| **Social proof** | "Lars just caught 9.2kg at Holmfoss Øvre" — seeing others succeed motivates action |
| **Competition** | Group leaderboard creates stakes within each 5-day window: "Our group caught 23, last group caught 18" |
| **Investment** | 20 years of personal history — "this is MY fishing record across two decades" |
| **Status** | Biggest fish of the season prominently displayed with angler name and trophy icon |
| **Scarcity** | "Day 3 of 5" — limited fishing days create awareness that each day counts |

## User Stories

### Primary (In-Season Morning Check)

- As an angler, I want to see the current water level and whether it's rising or falling so that I can decide if conditions are favorable
- As an angler, I want to see weather, air temperature, water temperature, and wind so that I can prepare for the day
- As an angler, I want to see where catches have been made on a map so that I can choose which river section to fish
- As an angler, I want to see what was caught yesterday so that I know what bait/technique is working

### Secondary (Competition & Social)

- As an angler, I want to see the leaderboard for my group's 5-day period so that I can track our friendly competition
- As an angler, I want to compare my group's total to the previous group so that I know how we stack up
- As a family member at home, I want to see recent catches so that I can follow the trip remotely

### Tertiary (Nostalgia & Off-Season)

- As an angler, I want to see all-time records and season summaries so that I can browse historical moments
- As an angler, I want to see a countdown to next season so that I stay engaged off-season

## Page Layout (Top to Bottom)

### Section 1: Conditions Strip (MUST be above fold on iPad)

A horizontal row of 5 compact, tight cards. No padding waste — pure data.

| Card | Content | Source |
|------|---------|--------|
| Air Temp | °C + icon (sun/cloud/rain) | yr.no (MET Norway API) |
| Water Level | Current meters + trend arrow ↑↗→↘↓ + color coding | NVE Sildre API, station 15.61.0 |
| Water Temp | Current °C | NVE Sildre API, station 15.61.0 |
| Wind | Speed (m/s) + direction | yr.no (MET Norway API) |
| Season Day | "Day 3 of 5 · Group 2" or off-season: "Season starts June 21" | Manual config / DB |

**Typography**: Primary number in `display-3` (bold). Label in `small text-muted`. Arrow/icon beside the number.

**Color coding for water level trend** (accessible — uses both color AND shape):

- Rising: Green background tint + ↑ arrow
- Stable: Blue/neutral + → arrow
- Falling: Amber/orange + ↓ arrow
- Never red/green alone (colorblind accessibility)

### Section 2: Water Level 24h Chart

- Chart.js line chart, last 24 hours of readings from NVE station 15.61.0
- Background fill: green tint below the line when trend is rising, amber when falling
- Large current-value label above the chart
- Time axis labels every 6 hours
- "Last updated: HH:MM" in card footer
- Card header: "Vannstand – Holmfoss (15.61.0)" with link to NVE source

### Section 3: Season Leaderboard

**Group filter tabs** at top of card: `My Group (26-30 Jun)` | `All Groups 2026` | `Last Year`

**Individual ranking table:**

| # | Angler | Fish | Total kg | Best kg |
|---|--------|------|----------|---------|
| 🥇 | Bjørn Hansen | 5 | 38.2 | 12.1 |
| 🥈 | Lars Johansen | 4 | 29.8 | 9.5 |
| ... | ... | ... | ... | ... |

- Top 3 get medal indicators
- "Biggest fish" entry has a subtle highlight/trophy
- Group comparison footer: "Group 2: 23 fish / 156 kg — Group 1 caught 18 fish / 112 kg"
- Updated nightly (user imports data and it refreshes)

### Section 4: Catch Map

- Leaflet.js map centered on Holmfoss (59.186959, 9.993806)
- Circle markers for catches, sized by weight, colored by recency (darker = more recent)
- Click marker → popup with: angler, weight, species, date, bait
- Default view: current season. Toggle: "This season" / "All time"
- OpenStreetMap tiles (free)

### Section 5: Recent Catches Feed

Card-based list, not a table — more visual, less spreadsheet-like:

Each entry:
```
[Initials circle]  Erik Andersen · 8.5 kg Atlantic Salmon
                   Holmfoss Øvre · 2 hours ago
```

- Last 10 catches, newest first
- Weight in bold
- Species name (from CatchType field)
- Relative timestamp ("2 hours ago", "yesterday")
- "View all catches →" link to /Catches

### Section 6: Season Summary Strip

Compact horizontal row of key numbers (same style as conditions strip):

| Total Fish | Total kg | Avg kg | Biggest Fish | Active Anglers | Season Day |
|------------|----------|--------|--------------|----------------|------------|
| 47 | 312.4 | 6.6 | 12.1 kg (Bjørn) 🏆 | 24 / 36 | Day 3 of 5 |

- "Biggest Fish" shows weight + angler name — the **status reward**
- "Active Anglers" = anglers with at least 1 catch this season
- Off-season: "Season Day" becomes "Next season: June 2027"

### Section 7: All-Time Records (bottom of page)

Three cards in a row:

| Biggest Fish Ever | Most Prolific Angler | Best Season |
|-------------------|---------------------|-------------|
| 14.3 kg | Erik Andersen | 2019 |
| Ole Kristiansen, 2018 | 127 fish over 20 years | 63 fish, 412 kg |

- Evergreen content, great for off-season browsing
- Links to filtered views in /Catches and /Statistics

## Technical Changes

### Backend

**Stack**: C# with .NET 9, Dapper, MySQL

**Components**:

- [ ] Models/Data: `Models/WeatherData.cs`, `Models/WaterLevel.cs`, `Models/WaterLevelReading.cs`, `Models/LeaderboardEntry.cs`, `Models/GroupSummary.cs`, `Models/AllTimeRecord.cs`, `Models/SeasonConfig.cs`
- [ ] Services: `Services/IWeatherService.cs` + `Services/WeatherService.cs` — wraps yr.no MET Norway API for Holmfoss area (59.186959, 9.993806)
- [ ] Services: `Services/IWaterLevelService.cs` + `Services/WaterLevelService.cs` — wraps NVE Sildre API for station 15.61.0 (water level + water temperature)
- [ ] Caching: `IMemoryCache` — weather: 15 min TTL, water level: 5 min TTL
- [ ] Repository additions to `ICatchRepository`:
  - `GetLeaderboardAsync(int year, int? groupNumber = null)` — top anglers by catch count
  - `GetGroupSummaryAsync(int year, int groupNumber)` — aggregates for a fishing group period
  - `GetAllTimeRecordsAsync()` — biggest fish, most prolific, best season
  - `GetCatchLocationsAsync(int? year = null)` — lat/long + metadata for map markers
- [ ] Repository additions to `ISeasonRepository`:
  - `GetSeasonConfigAsync(int year)` — group dates, rotation order
- [ ] Database Changes: `002_add_season_config.sql` — `season_config` table storing year, group_number, start_date, end_date, and which anglers are in which group
- [ ] PageModel: Rewrite `Pages/Index.cshtml.cs` to load all dashboard sections in parallel

### Frontend

**Stack**: ASP.NET Core Razor Pages, Bootstrap 5, Chart.js 4, Leaflet.js

**Components**:

- [ ] Rewrite `Pages/Index.cshtml` as dashboard layout
- [ ] Partials: `_ConditionsStrip.cshtml`, `_WaterLevelChart.cshtml`, `_Leaderboard.cshtml`, `_CatchMap.cshtml`, `_RecentCatches.cshtml`, `_SeasonSummary.cshtml`, `_AllTimeRecords.cshtml`
- [ ] CSS: `wwwroot/css/dashboard.css` — data-dense card grid, large number typography, condition color system, responsive breakpoints
- [ ] JS: `wwwroot/js/water-level-chart.js` (Chart.js line chart)
- [ ] JS: `wwwroot/js/catch-map.js` (Leaflet.js map init + marker rendering)
- [ ] CDN additions in layout: Leaflet.js CSS + JS

## Testing

- [ ] Unit tests: `WeatherService` response parsing with mocked HTTP
- [ ] Unit tests: `WaterLevelService` parsing + trend calculation (rising/falling/stable from 24h readings)
- [ ] Unit tests: Leaderboard ranking (ties, empty season, single angler, group filtering)
- [ ] Unit tests: `IndexModel.OnGetAsync` with mocked repos and services
- [ ] Integration: Page loads without errors when external APIs timeout (graceful degradation)
- [ ] Edge cases: No catches yet in season, season not started, only one group has fished, zero water readings
- [ ] Responsive: 375px (phone), 768px (iPad portrait), 1024px (iPad landscape), 1440px (desktop)
- [ ] Accessibility: Color contrast WCAG AA, trend arrows have text alternatives, map markers have aria labels

## Acceptance Criteria

- [ ] Water level (current value + trend arrow + color) and weather (temp, water temp, wind) visible **without scrolling** on iPad portrait
- [ ] Water level 24h chart shows last 24 hours from NVE station 15.61.0 with rising/falling visual indicator
- [ ] Water temperature from NVE station 15.61.0 displayed in the conditions strip
- [ ] Weather data sourced from yr.no (MET Norway API) for coordinates 59.186959, 9.993806
- [ ] Conditions strip numbers use minimum 36px font size for primary values
- [ ] Leaderboard shows top anglers for current season, filterable by group period
- [ ] Group comparison line shows aggregate (fish count + kg) for current group vs. previous group
- [ ] Catch map displays current season catches on Leaflet.js map centered on Holmfoss (59.186959, 9.993806) with clickable markers
- [ ] Recent catches shows last 10 entries with angler, weight, species (from CatchType), location, relative time
- [ ] Season summary shows totals + biggest fish with angler name highlighted
- [ ] All-time records section shows biggest fish ever, most prolific angler, best season
- [ ] Page degrades gracefully if weather/water APIs are unavailable (shows "Unavailable" + last known time)
- [ ] Page loads under 2 seconds on 4G (API data cached server-side)
- [ ] Fully responsive layout: single column mobile, multi-column tablet/desktop
- [ ] Existing /Catches and /Statistics pages remain unchanged
- [ ] All tests pass

## Dependencies

- [ ] **NVE Sildre API** (free, no key) — station 15.61.0 (Holmfoss i Numedalslågen) for water level + water temperature
- [ ] **yr.no MET Norway API** (free, no key, requires User-Agent identification) — weather forecast for 59.186959, 9.993806
- [ ] **Leaflet.js** (open source, CDN) — interactive map
- [ ] **OpenStreetMap** tiles (free) — base map layer
- [ ] **Chart.js 4** (already in project) — water level graph
- [ ] Database migration for `season_config` table

## Risks & Assumptions

| Item | Type | Status |
|------|------|--------|
| NVE station 15.61.0 is the correct gauge | Confirmed | Holmfoss i Numedalslågen |
| River is Numedalslågen at Holmfoss, not Gaula | Confirmed | README/seed data references Gaula incorrectly — needs separate cleanup |
| Map center coordinates: 59.186959, 9.993806 | Confirmed | User provided |
| Weather API: yr.no (MET Norway) | Confirmed | Replacing dmi.dk |
| CatchType field contains species name | Confirmed | e.g. "Atlantic Salmon", "Sea Trout" |
| Water temperature available from NVE station 15.61.0 | Confirmed | Include in conditions strip |
| 3 groups of 12, 5 days each, rotating annually | Confirmed | 2026: Group 2 = June 26-30 |
| No team pairings exist yet | Confirmed | Defer to future phase |
| GPS coordinates reliably captured for catches | Confirmed | User says locations are reliable |
| Nightly data import by user is acceptable | Confirmed | User manually imports from spreadsheet |
| No authentication required | Confirmed | Public site, trusted group |

## Open Questions

1. **Group assignment storage**: How to represent which anglers belong to which group each year? Suggest a simple `season_groups` table: `(year, group_number, angler_id)` populated manually.
2. **Biggest fish highlight**: Should this be biggest single fish (by weight), or do users also care about longest fish (by cm)?
3. **yr.no User-Agent**: MET Norway API requires a unique User-Agent header identifying the application — need to set this in `WeatherService`.

## Implementation Phases

### Phase 1 — Core Dashboard (Minimum Lovable Product)

- Conditions strip (weather from yr.no + water level + water temp from NVE + wind + season day)
- Water level 24h chart from NVE station 15.61.0
- Season leaderboard (individual, group-filterable)
- Recent catches feed
- Season summary strip
- `season_config` table + manual setup for 2026

### Phase 2 — Map & Records

- Catch map with Leaflet.js centered on 59.186959, 9.993806
- All-time records section
- Group-vs-group comparison in leaderboard footer

### Phase 3 — Future Enhancements

- Team pairings (2-man teams) once structure is defined
- Personal angler page ("My stats across 20 years")
- Yearly photo album
- Off-season countdown with historical "on this day" throwback
- Email digest: weekly summary during season

## Notes

- **Remove the hero section entirely.** No marketing text needed — every user already knows what this site is. The data IS the landing page.
- The README and DB seed data reference "Gaula river in Trøndelag" — this is incorrect and should be updated to "Numedalslågen at Holmfoss" in a separate cleanup task.
- All external API calls must be server-side with caching. Never expose API endpoints or make client-side calls to third-party services.
- Typography: system sans-serif stack (Segoe UI / SF Pro / system-ui). Numbers use tabular figures for column alignment in leaderboards.
- The design should feel closer to a Bloomberg terminal or elveguiden.no in data density than to a typical "fishing blog" — tight spacing, lots of numbers, minimal decorative elements.
- Color palette: dark navy headers (like current `.laks-hero`), white card backgrounds, green/amber for water level status. Restrained — not colorful for its own sake.
