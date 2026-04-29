# Feature: Editorial Magazine Homepage (Alt-E)

## Problem

The current Laks.Web homepage (see [src/Laks.Web/Pages/Index.cshtml](src/Laks.Web/Pages/Index.cshtml)) renders the dashboard partials inside a generic Bootstrap card grid on a flat dark theme. After exploring 11 visual directions in [.github/specs/homepage-visual-exploration.spec.md](.github/specs/homepage-visual-exploration.spec.md), the team has selected **Alt-E "Editorial Magazine"** ([src/Laks.Web/wwwroot/design-explorations/alt-e-editorial-magazine.html](src/Laks.Web/wwwroot/design-explorations/alt-e-editorial-magazine.html)) as the winning direction.

The homepage must be rebuilt to express that visual identity while preserving all existing data, behaviour, and acceptance criteria from the dashboard spec ([.github/specs/landing-page-dashboard.spec.md](.github/specs/landing-page-dashboard.spec.md)).

## Solution

Re-skin the production homepage to match Alt-E:

- Warm dark base (`#14110e`) with rust/amber accent (`#d97757`) and cream text (`#f5ede0`).
- **Fraunces** serif for masthead, headlines, leaderboard names, condition values, and pull quotes.
- **Inter** sans-serif for kickers, labels, ticker text, body copy.
- Editorial structural devices:
  - Centered **masthead** with oversized `LAKS` wordmark and italic tagline replacing the current navbar/hero.
  - **Top bar** with season number, "Holmfoss", and date in small-caps tracking.
  - **Lede paragraph** with drop-cap introducing today's conditions narrative.
  - **Double-rule** (`4px double`) borders separating masthead, hero, and footer.
  - **Single-rule** dividers on section titles, leaderboard rows, and catch articles.
  - Conditions presented as a 5-column bordered table-like grid (no shadows, no rounded corners).
  - Two-column feature grid (chart left ~1.7fr, leaderboard right ~1fr) collapsing to single column < 900px.
  - Recent catches rendered as **byline + headline + weight** "articles" (Fraunces italic for emphasized clauses).

The production page must keep all existing functionality from the current dashboard:

- Real conditions (weather, water level + trend, water temp, wind, precipitation).
- Live 24h Chart.js water level series.
- Group-aware leaderboard with scope toggle (`my-group`, `all-groups`, `last-year`) and group selector.
- Catch map (Leaflet heatmap of catch locations).
- Recent catches list.
- Season summary and all-time records.
- Off-season banner / countdown to next season.

The exploration HTML uses dummy data and omits the map, season summary, all-time records, group filter, and leaderboard scope toggle — these must be re-introduced in the editorial style.

## User Stories

- As an angler, I want the homepage to feel like a daily fishing journal, so that checking it in the morning feels like a ritual rather than a tool.
- As an angler, I want every existing piece of data (conditions, chart, leaderboard, map, recent catches, season summary, records) to remain visible, so that I do not lose information when the visual changes.
- As an angler on iPad in the river house, I want the masthead and conditions strip above the fold, so that I can decide whether to fish without scrolling.
- As an angler on phone, I want the layout to collapse to a single readable column, so that the editorial style does not break on small screens.
- As a returning user, I want the rest of the site (Fangster, Statistik) to remain consistent with the new identity, so that the site feels coherent.

## Technical Changes

### Backend
**Stack**: c# with .NET

**Components**:
- [ ] Models/Data: No model changes required.
- [ ] Business Logic: No service changes required.
- [ ] API Endpoints: None.
- [ ] Database Changes: None.
- [ ] [src/Laks.Web/Pages/Index.cshtml.cs](src/Laks.Web/Pages/Index.cshtml.cs) — keep `IndexModel` as-is. Add only:
  - A computed `IssueDateLabel` (e.g. `"29. april 2026"`) for the top bar, formatted in Danish.
  - A computed `LedeText` string (or a small view model) describing today's conditions in 1–2 sentences (e.g. `"Vandet stiger, og morgenens forhold lover godt."`). May be derived from trend + weather; fallback static text when data missing.
  - A computed `LastUpdatedLabel` for the hero kicker (e.g. `"Sidste opdatering · 06:42"`) sourced from the most recent water level / weather timestamp.

### Frontend
**Stack**: ASP.NET Core Razor Pages (.cshtml + PageModel)

**Components**:
- [ ] Layout — [src/Laks.Web/Pages/Shared/_Layout.cshtml](src/Laks.Web/Pages/Shared/_Layout.cshtml):
  - Add Google Fonts preconnect + Fraunces + Inter stylesheet link (replace current Inter-only link).
  - Decide scope (see Open Questions): if site-wide, swap navbar styling; if homepage-only, keep navbar but allow `Index.cshtml` to render its own masthead and hide the navbar via a layout flag (`ViewData["HideChrome"] = true`).
- [ ] [src/Laks.Web/Pages/Index.cshtml](src/Laks.Web/Pages/Index.cshtml) — restructure to:
  1. `_TopBar` (season, location, date)
  2. `_Masthead` (LAKS wordmark + tagline)
  3. `_Hero` (kicker, headline, drop-cap lede)
  4. `_ConditionsStrip` (existing, restyled)
  5. `_FeatureGrid` wrapper containing `_WaterLevelChart` + `_Leaderboard`
  6. `_CatchMap` (existing, restyled frame)
  7. `_RecentCatches` (re-templated as editorial articles)
  8. `_SeasonSummary` (existing, restyled)
  9. `_AllTimeRecords` (existing, restyled)
- [ ] New partials in [src/Laks.Web/Pages/Shared/](src/Laks.Web/Pages/Shared/):
  - `_TopBar.cshtml`
  - `_Masthead.cshtml`
  - `_Hero.cshtml`
- [ ] Updated partials (markup tweaks for editorial structure, no logic change):
  - [_ConditionsStrip.cshtml](src/Laks.Web/Pages/Shared/_ConditionsStrip.cshtml) — drop Bootstrap `card`, render as 5-column bordered grid; rising trend uses `--accent` (rust) for the value.
  - [_WaterLevelChart.cshtml](src/Laks.Web/Pages/Shared/_WaterLevelChart.cshtml) — wrap in `<figure class="chart-figure">` with `<figcaption>` italic caption.
  - [_Leaderboard.cshtml](src/Laks.Web/Pages/Shared/_Leaderboard.cshtml) — render as serif-numeral rows with bottom rules; keep group selector + scope toggle but restyle as small-caps Inter buttons above the rule.
  - [_RecentCatches.cshtml](src/Laks.Web/Pages/Shared/_RecentCatches.cshtml) — `byline + headline + weight` article rows; emphasize a phrase per item with `<em>`.
  - [_CatchMap.cshtml](src/Laks.Web/Pages/Shared/_CatchMap.cshtml) — keep Leaflet, frame map with 1px rule border, remove rounded corners and shadow.
  - [_SeasonSummary.cshtml](src/Laks.Web/Pages/Shared/_SeasonSummary.cshtml) and [_AllTimeRecords.cshtml](src/Laks.Web/Pages/Shared/_AllTimeRecords.cshtml) — match the bordered grid + Fraunces value pattern from conditions.
- [ ] Client-side script integration:
  - [src/Laks.Web/wwwroot/js/water-level-chart.js](src/Laks.Web/wwwroot/js/water-level-chart.js) — update Chart.js colors:
    - `borderColor: '#d97757'`
    - `backgroundColor: 'rgba(217,119,87,0.12)'`
    - tooltip `backgroundColor: '#1c1814'`, `borderColor: '#3a3128'`, `titleColor: '#d97757'`, `bodyColor: '#f5ede0'`
    - axis ticks `#7a6f60`, grid `rgba(217,119,87,0.04)`, border `#3a3128`
  - [src/Laks.Web/wwwroot/js/catch-map.js](src/Laks.Web/wwwroot/js/catch-map.js) — update marker / heatmap palette to rust/amber.
- [ ] CSS:
  - Replace [src/Laks.Web/wwwroot/css/dashboard.css](src/Laks.Web/wwwroot/css/dashboard.css) (or add a new `editorial.css` loaded only on `/Index`) with the token set and rules from alt-e:
    - Color tokens (`--bg-base`, `--bg-surface`, `--bg-raised`, `--accent`, `--accent-soft`, `--accent-deep`, `--rule`, `--text-primary`, `--text-secondary`, `--text-muted`).
    - Typography stacks (Fraunces / Inter).
    - Editorial primitives (`.masthead`, `.masthead-title`, `.hero`, `.hero-lede::first-letter`, `.section-title`, `.conditions-grid`, `.condition-card`, `.feature-grid`, `.chart-figure`, `.lb-row`, `.lb-rank`, `.catch-article`, `.catch-headline`, `.catch-weight`).
    - Responsive breakpoints at 480, 768, 900, 1100 px matching alt-e.
  - Decide whether [site.css](src/Laks.Web/wwwroot/css/site.css) keeps the current dark Bootstrap theme or is migrated to the editorial token set (see Open Questions).

## Testing
- [ ] Existing unit tests in [tests/Laks.Web.Tests/Unit/DashboardPageModelTests.cs](tests/Laks.Web.Tests/Unit/DashboardPageModelTests.cs) continue to pass; extend to cover new computed properties (`IssueDateLabel`, `LedeText`, `LastUpdatedLabel`) including off-season / null-data fallbacks.
- [ ] Manual visual regression: compare rendered `/Index` to [alt-e-editorial-magazine.html](src/Laks.Web/wwwroot/design-explorations/alt-e-editorial-magazine.html) at 360, 480, 768, 1024, 1280 px widths.
- [ ] Off-season scenario (no recent water level / weather) renders without empty cards and shows the off-season banner in the editorial style.
- [ ] Accessibility: contrast ratio for `--text-primary` on `--bg-base` and `--accent` on `--bg-base` meets WCAG AA; `aria-label`s preserved on each `<section>`; drop-cap is purely presentational (does not break screen reader reading order).
- [ ] Chart.js tooltip uses Danish number format (comma decimal) — already handled by chart script; verify after color swap.

## Acceptance Criteria
- [ ] `/Index` visually matches Alt-E within reasonable tolerance (typography, palette, spacing rhythm, double-rule borders, drop-cap lede).
- [ ] All data sections present in the current homepage are present in the new homepage: conditions, water-level chart, leaderboard (with group + scope controls), catch map, recent catches, season summary, all-time records.
- [ ] Conditions strip and masthead are visible above the fold on iPad portrait (768×1024).
- [ ] Layout collapses to a single column at < 600 px without horizontal scroll.
- [ ] All user-facing text is in Danish.
- [ ] Off-season state shows a styled banner with countdown to next season opening.
- [ ] Existing dashboard unit tests pass; new tests for added PageModel properties pass.
- [ ] No console errors; Chart.js and Leaflet load with valid SRI hashes.
- [ ] Documentation updated in this spec when scope decisions are resolved.

## Dependencies
- [ ] Google Fonts: `Fraunces` (opsz 9..144, weights 400/600/900) and `Inter` (400/500/600) — already used (Inter); add Fraunces.
- [ ] Existing Chart.js 4.4.3 and Leaflet 1.9.4 CDN bundles — no version change.
- [ ] No new NuGet packages.

## Notes

### Scope decision required (see Open Questions)
The exploration was a single-page mock. The team must decide whether the editorial identity applies:

- **Option A — Homepage only**: Keep current `_Layout.cshtml` chrome (navbar, footer) for `/Catches` and `/Statistics`. `/Index` opts out of the navbar and renders its own masthead. Lowest risk, fastest delivery.
- **Option B — Site-wide**: Migrate `_Layout.cshtml`, `site.css`, and the Catches/Statistics pages to the editorial palette + typography. Higher effort but coherent identity. Likely a follow-up spec.

**Recommendation**: ship Option A first (this spec), then plan a follow-up spec for site-wide migration once the homepage is validated in the river house.

### Things in alt-e that need to be added back
The exploration HTML drops several existing dashboard elements for clarity. These must be re-introduced in editorial form:

- Group selector (which 5-day group's leaderboard to show).
- Leaderboard scope toggle (`my-group` / `all-groups` / `last-year`).
- Catch map (Leaflet).
- Season summary section.
- All-time records section.
- Off-season banner / countdown.

### Things in the current homepage that should be removed or de-emphasized
- Bootstrap `card`, `shadow-sm`, rounded corners — replaced with rules and bordered grids.
- The navbar emoji `🎣 LAKS` — replaced by serif masthead (if Option A: hide on `/Index`).
- The footer emoji `🐟 Hver fisk fortæller en historie` — re-styled as small-caps editorial colophon.

### Open Questions
1. **Scope**: Option A (homepage only) or Option B (site-wide)?
2. **Lede text**: hand-curated daily by an editor, or auto-generated from data (trend + weather + last catch)?
3. **Top bar date**: today's date, or the date of the latest data point?
4. **Catch map placement**: keep below the leaderboard as in current layout, or move to a dedicated full-width section between feature grid and recent catches?
5. **Issue number**: should the top bar say `Sæson 2026` (year) or `Nr. 142` (e.g. day of season)? Alt-e mock uses the year.

### Risks
- Fraunces at large sizes (`clamp(4rem, 14vw, 8.5rem)`) is heavy; verify CLS impact and consider `font-display: swap` + a serif fallback metric override.
- Drop-cap (`::first-letter`) rendering varies across browsers; test on Safari/iOS where most river-house iPads run.
- Removing the navbar on `/Index` (Option A) may confuse users about how to reach Fangster/Statistik. Mitigation: place a small Inter small-caps nav row in the top bar or directly under the masthead.
