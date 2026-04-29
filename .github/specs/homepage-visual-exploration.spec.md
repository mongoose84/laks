# Feature: Homepage Visual Exploration — 3 Style Alternatives

## Problem

The current homepage, while functional, uses a generic Bootstrap card layout that does not reflect a strong visual identity for the site. Before committing to a redesign, we need to evaluate three distinct visual directions to find a style that feels right for a salmon fishing tracker aimed at a group of ~36 anglers.

This is a **design exploration**, not a production feature. The three alternatives are static, standalone HTML pages with inline or embedded CSS and Chart.js dummy data. They are used purely for visual evaluation by the product owner. No backend connectivity, no Razor Pages, no build pipeline dependency.

## Solution

Produce three self-contained HTML files, each representing a different visual direction for the homepage. Each file must contain:

- A page header with site name, tagline, and a short paragraph of lorem ipsum body text
- A weather conditions strip (dummy data: air temp, water level + trend, water temp, wind)
- A small interactive chart (Chart.js, dummy water level data for 24 hours)
- A mini leaderboard or season stat strip (dummy names and numbers)
- A short recent-catches list or card grid (dummy data)
- A footer

All three must:
- Be **dark-mode** throughout
- Work on **mobile (320px) and desktop (1280px)** with a single HTML file each
- Use **no external fonts that require sign-in** (Google Fonts CDN is fine)
- Use **Chart.js 4 from CDN** for the graph
- Contain **no backend calls**, no `fetch()`, no AJAX — pure static HTML + CSS + JS

### Alternative A — Nordic Rustic Dark

**Mood**: A handwritten fishing journal left on a wooden table by firelight. Warm, organic, textured.

| Design token | Value |
|---|---|
| Background | Deep forest-floor dark `#1a1510` |
| Surface | Warm dark brown `#26201a` |
| Accent | Amber/ember `#c8872a` |
| Text | Warm off-white `#f0e8d8` |
| Chart line | Ember orange-amber |
| Font (headings) | Playfair Display (serif) — Google Fonts CDN |
| Font (body) | Lora (serif) — Google Fonts CDN |
| Border style | Thin, slightly rough — `1px solid rgba(200,135,42,0.3)` |
| Cards | Slightly warm-tinted dark cards with top border accent stripe |
| Decoration | Subtle wood-grain texture via CSS `repeating-linear-gradient` on header |
| Layout | Single-column mobile, 2-column grid desktop |

**Character**: Feels like reading a field journal. Numbers are displayed in large serif type. The header carries a quiet, weathered dignity.

---

### Alternative B — River at Dusk (Dark Moody)

**Mood**: Standing on the riverbank at dusk — deep water, cold air, still surface. Cinematic, dramatic, immersive.

| Design token | Value |
|---|---|
| Background | Near-black deep teal `#080e12` |
| Surface | Dark slate-water `#0d1a22` |
| Accent | Cold river-blue / cyan `#00b4d8` |
| Secondary accent | Pale mint / moonlight `#90e0ef` |
| Text | Cool near-white `#caf0f8` |
| Chart line | Cyan with glow effect (`box-shadow` on canvas wrapper) |
| Font (headings) | Space Grotesk (sans-serif) — Google Fonts CDN |
| Font (body) | Inter (sans-serif) — Google Fonts CDN |
| Border style | Hairline border `1px solid #1a3040` with subtle `rgba` glow on hover |
| Cards | Glassmorphism-inspired: `background: rgba(13,26,34,0.85); backdrop-filter: blur(8px)` |
| Decoration | Hero section has a full-width dark background with a subtle radial gradient imitating a water reflection |
| Layout | Single-column mobile, asymmetric 2-column (conditions large left, chart large right) on desktop |

**Character**: Feels modern, premium, slightly dramatic. Data is the star — large numbers, tight spacing, cold color palette.

---

### Alternative C — Midnight Field Station

**Mood**: A ranger's field station at midnight — scientific instruments, log entries, signal green on black. Somewhere between a weather terminal and a pilot's glass cockpit.

| Design token | Value |
|---|---|
| Background | True near-black `#0b0f0b` |
| Surface | Very dark desaturated green `#101510` |
| Accent | Signal/terminal green `#39ff6e` |
| Muted accent | Olive-grey `#4a5e40` |
| Text | Light grey-green `#d4e8d0` |
| Chart line | Bright terminal green with slight flicker via CSS animation on load |
| Font (headings) | JetBrains Mono (monospace) — Google Fonts CDN |
| Font (body) | JetBrains Mono (monospace) — Google Fonts CDN |
| Border style | Thin dashed `1px dashed #2a3d28` |
| Cards | Flat dark panels with a top-left corner accent mark (CSS pseudo-element) |
| Decoration | Weather labels use monospace ALL-CAPS with a prefix symbol `▸ VANDSTAND` |
| Layout | Single-column mobile, 3-column tight grid on desktop (dense data, minimal whitespace) |

**Character**: Feels like mission control. No decorative elements — pure data readability. Numbers feel urgent. Perfect for a tool used in the field.

---

## File Locations

Each alternative is a fully standalone file in `wwwroot/design-explorations/`:

| File | Style |
|---|---|
| `wwwroot/design-explorations/alt-a-nordic-rustic.html` | Nordic Rustic Dark |
| `wwwroot/design-explorations/alt-b-river-dusk.html` | River at Dusk |
| `wwwroot/design-explorations/alt-c-field-station.html` | Midnight Field Station |

These files are **not linked from the app** and are never deployed to production. They are for local evaluation only.

## User Stories

- As the product owner, I want to open each HTML file in a browser and compare the three visual styles side by side so that I can select a direction for the real homepage redesign
- As the product owner, I want to resize the browser / open on my phone so that I can verify that my preferred style also looks good on mobile
- As the frontend engineer implementing the chosen style, I want a working HTML prototype so that I have a concrete visual reference with exact colors, fonts, and layout to implement in Razor Pages

## Content Requirements (all dummy/lorem)

All three alternatives must include the following dummy content:

### Header area
- Site name: **LAKS**
- Tagline: **Laksefiskeri i Holmfoss**
- Sub-heading: **Sæson 2026**
- Body paragraph: 2 sentences of lorem ipsum in Danish flavour (can be real lorem ipsum)

### Conditions strip (5 items)
| Label | Dummy value |
|---|---|
| Lufttemperatur | 12,4° |
| Vandstand | 1,83 m ↑ |
| Vandtemperatur | 9,1° |
| Vind | 3 m/s NV |
| Nedbør | 0,2 mm |

### Chart
- Type: Line chart (Chart.js)
- Data: 24 hourly dummy water level readings (values between 1.5–2.1 m, with a general rising trend)
- X-axis: hours `00:00` through `23:00`
- Y-axis: meters, range 1.4–2.2
- Label: `Vandstand (m)`

### Leaderboard / season strip
Dummy leaderboard with 5 entries:

| Plads | Navn | Fisk | Vægt |
|---|---|---|---|
| 1 | Lars H. | 4 | 34,2 kg |
| 2 | Morten K. | 3 | 28,7 kg |
| 3 | Søren B. | 3 | 25,1 kg |
| 4 | Jan M. | 2 | 19,4 kg |
| 5 | Niels P. | 1 | 11,2 kg |

### Recent catches (3 items)
| Dato | Navn | Vægt | Sted |
|---|---|---|---|
| 28. apr | Lars H. | 9,4 kg | Holmfoss Øvre |
| 27. apr | Morten K. | 8,7 kg | Mellempolen |
| 27. apr | Søren B. | 7,2 kg | Nedre Hul |

### Footer
- Text: `© 2026 LAKS – Laksefiskeri i Holmfoss`

## Technical Changes

### Backend
None. These are static files only.

### Frontend

**Components** (all new, standalone — no impact on existing Razor Pages):
- [ ] `wwwroot/design-explorations/alt-a-nordic-rustic.html` — Alternative A, fully self-contained (inline `<style>` + `<script>`)
- [ ] `wwwroot/design-explorations/alt-b-river-dusk.html` — Alternative B, fully self-contained
- [ ] `wwwroot/design-explorations/alt-c-field-station.html` — Alternative C, fully self-contained

**Dependencies per file** (CDN only, no npm):
- Chart.js 4.4.x — `https://cdn.jsdelivr.net/npm/chart.js@4.4.3/dist/chart.umd.min.js`
- Google Fonts — one or two families per alternative (see design tokens above)

**Implementation notes**:
- All CSS must be inside a `<style>` block in `<head>` — no external `.css` files
- All JavaScript (Chart.js initialization with dummy data) inside `<script>` at end of `<body>`
- Use CSS custom properties (`--var`) for all color tokens so they are easy to inspect and copy
- Use CSS Grid or Flexbox for layout — no Bootstrap dependency
- Mobile-first: base styles target 320px, `@media (min-width: 768px)` for tablet, `@media (min-width: 1100px)` for desktop
- Chart canvas must have `max-height: 220px` on mobile, `max-height: 280px` on desktop
- No JavaScript beyond the Chart.js initialization block

## Testing

- [ ] Open each file directly in a browser (no server needed — `file://` should work)
- [ ] Resize to 375px width (iPhone) and verify no horizontal scroll
- [ ] Resize to 1280px width and verify 2- or 3-column layout activates
- [ ] Verify chart renders correctly with dummy data
- [ ] Verify no console errors

## Acceptance Criteria

- [ ] Three `.html` files exist at `wwwroot/design-explorations/`
- [ ] Each file is completely self-contained (no external `.css` or `.js` files from the project)
- [ ] Each file renders without errors in a modern browser opened via `file://`
- [ ] Each file is visually distinct — a reviewer can immediately tell the three apart
- [ ] All three are dark-mode (no light backgrounds)
- [ ] All three are responsive (no horizontal scroll at 375px)
- [ ] Chart.js renders a line chart with 24 dummy data points in each file
- [ ] The dummy leaderboard, conditions strip, and recent-catches list are present in each file
- [ ] No backend calls, no `fetch()`, no API dependencies

## Dependencies

- [ ] Chart.js 4.4.3 (CDN) — already used in production, no version conflict
- [ ] Google Fonts CDN — existing `<link rel="preconnect">` pattern from `_Layout.cshtml` can be reused

## Notes

- These files should **not** be committed to the `main` branch if they are only for local evaluation. Consider a short-lived feature branch or keeping them in `.gitignore` after the decision is made.
- Once a style direction is chosen, the actual implementation will be tracked in a separate spec (likely extending `landing-page-dashboard.spec.md`).
- The `wwwroot/design-explorations/` folder should be added to `.gitignore` or removed before production deployment to avoid serving evaluation artefacts.
- Open questions: Should the chosen style also apply to the Catches and Statistics pages, or only the homepage? Decide before implementation begins.
