# UX Research Findings: Landing Page Dashboard

> Research conducted: March 2026
> Researcher: UX research agent
> Method: Stakeholder interview (product owner / primary angler)

---

## 1. User Profile

### Primary Users: The Anglers

| Attribute | Finding |
|-----------|---------|
| Count | ~36 anglers, split into 3 rotating groups of 12 |
| Demographics | Male, age 40-75, mostly 60+ |
| Wealth | Affluent — willing to invest in the experience |
| Technical ability | Not highly technical; comfortable with basic web browsing, email |
| Communication | Email-based group (no WhatsApp/Messenger — age factor) |
| Relationship | Long-standing social group, 20+ years of shared history |

### Secondary Users: Family Members

- Close family who follow along from home (~15-20 additional)
- Want to see recent catches and how the trip is going
- Passive consumers, not data enterers

### Usage Patterns

| When | What | Device | Context |
|------|------|--------|---------|
| Morning before fishing (June-July) | Water level, weather, where to fish | iPad / phone | Shared screen at the house, group viewing |
| Evening after fishing | Check what others caught, leaderboard | iPad / phone / PC | Relaxed social setting |
| Off-season (Aug-May) | Historical stats, comparisons, nostalgia | PC / tablet at home | Solo browsing |

**Key insight**: The morning check is a **group activity**. One person opens the site on a tablet and others look on. This means the UI must work at arm's length — large numbers, high contrast, no tiny text.

---

## 2. Fishing Context & Group Structure

### Location

- **River**: Numedalslågen
- **Beat/pool**: Holmfoss
- **GPS center**: 59.186959, 9.993806
- **NOT Gaula** — the README and seed data are incorrect and reference "Gaula river in Trøndelag"

### Season Structure

Three groups of 12, each fishing 5 consecutive days. Groups rotate order annually.

**2026 schedule**:
- Group 1: ~June 21-25
- Group 2: June 26-30 (stakeholder's group)
- Group 3: ~July 1-5

Total season: approximately 15 fishing days.

### Competition Structure

- Informal "biggest fish" competition
- Each 12-person group normally has 6 two-man teams
- Team pairings can change each year
- **No team pairings are currently tracked in any system** — this is informal/verbal
- Cross-group comparison is also interesting ("our group caught 23, last group caught 18")

### Data Entry

- One person (stakeholder) logs all catches in a spreadsheet
- Feeds to the MySQL database via script
- Done nightly or after the fishing period
- Anglers do NOT self-report

---

## 3. Current State Analysis (v1 at fisk.krunk.dk)

### What Works Well (Keep)

| Feature | Why It Works |
|---------|-------------|
| Catch heatmap on map | Shows WHERE fish are being caught — directly actionable for "where should I fish?" |
| Water level card + 24h graph | Rising/falling trend is the #1 decision factor |
| Historical stats over 20 years | Deep emotional value — nostalgia, story of the group |
| Personal stats (search by name) | Individual ownership of fishing record |

### What Doesn't Work (Fix)

| Issue | Impact |
|-------|--------|
| "Looks homemade" | Undermines credibility and pride; users feel embarrassed to share |
| No weather/temperature on landing page | Users check weather elsewhere before coming to the site |
| No leaderboard | Competition is tracked informally in heads — missed engagement opportunity |
| No group comparison | No way to see how your 5-day group performed vs. others |
| Generic hero section in v2 | Wastes above-the-fold space with text that adds no value for a closed group |

### v1 Data Sources (Confirmed)

| Data | API / Source |
|------|-------------|
| Water level | NVE Sildre API — station **15.61.0** (Holmfoss i Numedalslågen) |
| Water temperature | NVE Sildre API — station **15.61.0** (confirmed available) |
| Weather | Currently dmi.dk — **switching to yr.no** (MET Norway API) for Norwegian locality |
| Catch locations | GPS coordinates stored per catch (confirmed reliable) |
| Catches | MySQL database, imported from spreadsheet nightly |

---

## 4. User Needs Analysis

### Need Priority Matrix

| Priority | Need | Current State | Desired State |
|----------|------|---------------|---------------|
| **P0** | See water level + trend | On v1 but not on v2 | Above the fold, large numbers, 24h chart |
| **P0** | See weather/temp/wind | Not on site at all | Above the fold in conditions strip |
| **P0** | See water temperature | Not on site | In conditions strip from NVE |
| **P1** | See catch map (where to fish) | On v1 as heatmap | Leaflet.js map on v2 landing page |
| **P1** | See leaderboard | Not available | Group-filterable ranking table |
| **P1** | See recent catches | Basic table in v2 | Card-style feed with relative time |
| **P2** | Compare groups | Not available | Group vs group aggregate comparison |
| **P2** | Season summary at a glance | Partial (stats cards) | Compact strip with totals + biggest fish |
| **P3** | All-time records | On v1 stats page | Dedicated section on landing page |
| **P3** | Personal angler page | On v1 (search by name) | Dedicated page (future phase) |

### Jobs To Be Done

1. **Morning decision**: "Should I fish now, and where?" → needs water level trend + weather + catch map
2. **Evening social**: "What happened today?" → needs recent catches + leaderboard update
3. **Group pride**: "How's our group doing?" → needs group leaderboard + group comparison
4. **Long-term identity**: "What's my fishing story over 20 years?" → needs personal stats + all-time records

---

## 5. Design Direction

### Reference Sites

| Site | What to Borrow | What to Avoid |
|------|---------------|---------------|
| **elveguiden.no/laksebors** | Domain-relevant layout, catch data presentation, river-specific focus | Designed for the public/all rivers — our site is private, single river |
| **Stock/financial dashboards** | Data density, tabular numbers, real-time feel, glanceable metrics | Complexity — keep it to ~7 cards, not 50 widgets |
| **fisk.krunk.dk (v1)** | Water level card, heatmap — these are proven useful | Homemade aesthetic, cluttered layout |

### Visual Design Principles

1. **No hero section** — the 36 users know what the site is. Data IS the hero.
2. **Large numbers** — minimum 36px for primary metrics. Readable from 2 meters.
3. **Card-based layout** — clean separation between sections, Bootstrap 5 cards with shadow
4. **Dark navy headers** — extend the existing `.laks-hero` palette into card headers
5. **Data-dense, not decorative** — every pixel should be information or whitespace that aids scanning
6. **System fonts** — Segoe UI / SF Pro / system-ui. Tabular figures for number alignment.
7. **Restrained color** — navy, white, green (rising), amber (falling). No rainbow.

### Accessibility Considerations

- Water level trend uses **both** color AND arrow shape (not color alone) — some users may be colorblind
- Minimum WCAG AA contrast ratios on all text
- Map markers have text alternatives
- Touch targets minimum 44px for iPad use

---

## 6. Behavioral Design Analysis

### Engagement Hooks

| Behavior Principle | Application | Expected Effect |
|-------------------|-------------|-----------------|
| **Trigger (Fogg)** | Rising water level arrow in green = "go fish now" | Prompts immediate action |
| **Variable Reward (Nir Eyal)** | Leaderboard changes nightly, water level shifts hourly | Every visit shows something new |
| **Social Proof** | Recent catches feed: "Bjørn caught 8.5kg" | Motivates others to get out there |
| **Competition** | Group leaderboard with medal icons | Friendly stakes within each window |
| **Loss Aversion** | "Day 4 of 5" — scarcity of fishing days | Creates urgency to make each day count |
| **Status** | Biggest fish trophy display with angler name | Public recognition drives repeat engagement |
| **Investment (Sunk Cost)** | 20 years of personal catch history | Emotional ownership — "this is MY record" |
| **Endowed Progress** | Season counter showing catches accumulating | Group momentum — "we're on track for a record" |

### Anti-Patterns to Avoid

- **No gamification overkill** — this is a group of 60+ year olds. Badges, points, streaks would feel patronizing.
- **No notifications** — email-based group doesn't want push notifications. The site is pull-based.
- **No social media sharing** — private group, no public sharing needed.

---

## 7. Technical Constraints & Confirmed Decisions

| Decision | Details |
|----------|---------|
| Weather API | **yr.no** (MET Norway API) — free, no key, requires User-Agent header |
| Water level API | **NVE Sildre** — station 15.61.0 (Holmfoss i Numedalslågen) — free, no key |
| Water temperature | **NVE Sildre** — same station 15.61.0, confirmed available |
| Map library | **Leaflet.js** (CDN) + OpenStreetMap tiles |
| Map center | **59.186959, 9.993806** (Holmfoss) |
| Charts | **Chart.js 4** (already in project) |
| CatchType field | Contains **species name** (e.g. "Atlantic Salmon", "Sea Trout") |
| Authentication | **None** — public site, trusted closed group |
| Data freshness | Catches imported nightly by stakeholder from spreadsheet |
| Team pairings | **Not tracked anywhere** — defer to future phase |
| Budget | Hobby project — free APIs only |
| Hosting | IIS, existing infrastructure |

---

## 8. Data Corrections Needed

The following items in the current codebase are factually incorrect and should be corrected in a separate task:

| File | Current | Correct |
|------|---------|---------|
| README.md | "Gaula river in Trøndelag" | "Numedalslågen at Holmfoss" |
| Pages/Index.cshtml | "chase Atlantic salmon on the Gaula river in Trøndelag, Norway" | "fish for Atlantic salmon on Numedalslågen at Holmfoss, Norway" |
| 001_initial_schema.sql seed data | River name "Gaula", location "Støren, Trøndelag" | River name "Numedalslågen", location "Holmfoss" |

---

## 9. Open Research Questions

1. **Exact fishable stretch boundaries**: We have the center point (59.186959, 9.993806) but don't know the upstream/downstream GPS limits for map bounds. Need to determine reasonable zoom level — suggest starting with a ~2km radius around center point and adjusting.
2. **Biggest fish metric**: Is it always by weight (kg), or do anglers also compare by length (cm)? The DB stores both `weight_kg` and `length_cm`.
3. **Group dates for historical years**: Are historical group dates (2005-2025) available anywhere? Or only track from 2026 forward?
4. **Species distribution**: How many distinct species appear in CatchType? This affects the leaderboard display (filter by species?).
5. **yr.no rate limits**: MET Norway recommends max 20 requests/second per product. With server-side caching at 15 min TTL, this will never be an issue, but the User-Agent header must identify the application per their terms of service.

---

## 10. Recommendations Summary

1. **Build the conditions strip first** — it's the #1 unmet need and the reason users open the site
2. **Water level + trend is the killer feature** — make it impossible to miss (largest element above fold)
3. **Add water temperature** to conditions — confirmed available from NVE, high value for anglers
4. **Switch weather to yr.no** — Norwegian government API, better local accuracy than dmi.dk
5. **Leaderboard with group filter** — activates competition within each 5-day window
6. **Catch map with Leaflet.js** — proven valuable in v1, port to v2 with better styling
7. **Remove hero section** — replace with data. The 36 users don't need a sales pitch.
8. **Fix river name** throughout codebase — Numedalslågen, not Gaula
9. **Defer team pairings** — no data exists yet; add when structure is defined
10. **Design for iPad at arm's length** — this is the primary use case

---

# UX Research: Visual Refresh — Dark Theme, Typography & Navigation

> Research conducted: March 2026
> Researcher: UX research agent
> Scope: Topbar/navigation, typography system, dark theme

---

## 11. Problem Statement

The current UI has three specific visual problems that undermine the "polished, professional" goal established in the landing page spec:

### 11.1 The Topbar Feels Generic

**Current state**: Stock Bootstrap `navbar-dark bg-primary` (default blue `#0d6efd`). The brand is an emoji + text ("🎣 LAKS"). Navigation links are plain `text-white` on blue. Collapses at `sm` breakpoint.

**Problems identified**:
- Indistinguishable from any default Bootstrap 5 project — contributes to the "looks homemade" complaint from UX finding §3
- The bright blue `bg-primary` clashes with the dark navy palette (`#0a3d62` → `#2980b9`) used in the conditions strip hero gradient
- No visual weight hierarchy — the brand, nav links, and background all compete at the same intensity
- The `mb-3` margin creates a visible white gap between the topbar and content, breaking visual flow into the dashboard

**User impact**: The topbar is the first thing users see. A stock-looking navbar immediately signals "template site" rather than "purpose-built tool for our fishing group."

### 11.2 Typography Is Undefined

**Current state**: No explicit `font-family` declaration anywhere in the CSS. The only font-related styles are:
- `html { font-size: 14px }` (16px above 768px) — just Bootstrap base size overrides
- `.laks-primary-value` uses `font-weight: 700` and `clamp()` sizing
- `font-variant-numeric: tabular-nums` on leaderboard and primary values
- Everything else relies on Bootstrap 5 defaults (native font stack: `-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, ...`)

**Problems identified**:
- No intentional typographic hierarchy — headings, body, data values, and labels all use the same font family
- The Bootstrap native stack is adequate for body text but lacks the "data dashboard" character needed for a Bloomberg-style interface
- Numbers in the dashboard (water level, temperatures, weights) need a font with excellent tabular figures — `font-variant-numeric: tabular-nums` only works if the active font supports OpenType `tnum`
- Card headers use Bootstrap `.h5` defaults which are too similar in weight to body text, reducing visual structure
- Prior research (§5.6) recommended "System fonts — Segoe UI / SF Pro / system-ui" but no implementation followed

**User impact**: The typography doesn't communicate "data tool" — it reads as "another blog." Numbers don't align cleanly in leaderboards. There's no clear visual rhythm between section headers, data values, and labels.

### 11.3 The Light Theme Mismatches the Content & Context

**Current state**: 100% light theme. White page background, white/near-white card backgrounds (`#f8fafc`, `#f6f8fb`, `#fbfdff`), light borders (`#d8e0ea`, `#d0dae4`). The only dark element is the navbar and the `.laks-hero` gradient (used only on conditions cards' initials circles).

**Problems identified**:
- A data-dense dashboard on a white background creates visual fatigue, especially on iPads in dimly lit morning/evening settings (the two primary usage times per §1 Usage Patterns)
- The existing color palette (§5.7) specified "dark navy headers" and "restrained color" — but the implementation went bright white, the opposite direction
- Financial dashboards and trading terminals (the stated reference aesthetic) universally use dark backgrounds — Bloomberg, Reuters, TradingView — because they reduce eye strain during long sessions and make colored data (green/amber trend indicators) pop more
- The Leaflet map and Chart.js charts will need dark-mode variants anyway; starting dark avoids a future retrofit
- The 40-75 age demographic benefits from reduced glare on screens — dark backgrounds with high-contrast text are more comfortable for aging eyes than white backgrounds
- Evening iPad viewing context: users check the site in the shared house after fishing. White screens in dim lighting are harsh.

**User impact**: The bright white UI feels clinical and generic rather than atmospheric. A dark theme would better match the premium, purposeful feel that the stakeholder wants — like stepping into a well-appointed fishing lodge rather than a hospital waiting room.

---

## 12. Competitive & Reference Analysis

### 12.1 Dark Dashboard References

| Reference | What works | Applicable lesson |
|-----------|-----------|-------------------|
| **Bloomberg Terminal** | Dark charcoal background (`#1e1e1e`–`#2d2d2d`), bright data, color only for status | Use dark surface with bright numbers for maximum glanceability |
| **TradingView (dark mode)** | Near-black canvas (`#131722`), card surfaces slightly lifted (`#1e222d`), green/red data | Two-level dark surface system: page bg vs. card bg |
| **Strava Dashboard** | Dark navy (`#1a1a2e`) with orange accent, activity feed on cards | Dark but warm — not cold/gray. Accent color for key actions |
| **yr.no (dark mode)** | The actual weather service our users would recognize. Charcoal bg, white text, blue accent | Users may already be familiar with this aesthetic from yr.no itself |
| **elveguiden.no** | Dark header band, earthy greens, clean data tables | Domain-appropriate — fishing context, Scandinavian design |

### 12.2 Typography References for Data Dashboards

| Font / Stack | Use case | Pros | Cons |
|-------------|----------|------|------|
| **Inter** (Google Fonts) | Headlines + body | Designed for screens, excellent tabular figures, free, variable weight. Very clean. | Needs a CDN/Google Fonts load |
| **JetBrains Mono** | Data values only | Perfect monospace tabular numbers | Too "developer" for non-technical users |
| **System UI stack** (current) | Body text fallback | Zero load time, familiar | No guarantee of tabular fig support, no character |
| **DM Sans** (Google Fonts) | Headlines | Geometric, modern, premium feel | Less suited for dense data tables |
| **IBM Plex Sans** | Full stack | Excellent tabular figures, designed for dashboards, free | Slightly corporate |

**Recommendation**: **Inter** for the full type stack. Reasons:
1. Purpose-built for UI/screen, not adapted from print
2. True tabular figures via `font-variant-numeric: tabular-nums` — columns of numbers align perfectly
3. Variable font = one file, all weights (400, 500, 600, 700)
4. Extensive language support (Norwegian characters: æ, ø, å)
5. Used by Linear, Vercel, Raycast — the "premium data tool" aesthetic
6. Free via Google Fonts or self-hosted
7. x-height designed for readability at small sizes AND at arm's length

### 12.3 Topbar / Navigation Patterns

| Pattern | Example | Applicable? |
|---------|---------|-------------|
| **Slim dark bar, integrated into page** | Linear, Vercel | Yes — topbar blends with dark bg, no visual break |
| **Transparent navbar over hero content** | Many landing pages | No — we have no hero; data starts immediately |
| **Sidebar navigation** | Bloomberg, Grafana | No — only 3 nav items, sidebar is overkill |
| **Tab bar / segment control** | Mobile apps, Strava | Maybe for mobile, but 3 items don't warrant it |
| **Minimal header with logo left, nav right** | Most SaaS dashboards | Yes — clean, professional, doesn't steal attention from data |

**Recommendation**: Slim integrated dark header that blends into the dark page background. No gap between header and content. Logo on left, 3 nav links on right. Same dark surface color as the page — the header is distinguished by a subtle bottom border or slight lightness shift, not a contrasting color block.

---

## 13. Proposed Design System

### 13.1 Color Palette — Dark Theme

```
┌─────────────────────────────────────────────────────┐
│ Surface Hierarchy (dark to light)                   │
├─────────────────┬───────────────────────────────────┤
│ --bg-base       │ #0f1923   (page background)       │
│ --bg-surface    │ #162029   (card backgrounds)       │
│ --bg-raised     │ #1c2a35   (hover states, popups)   │
│ --border-subtle │ #243442   (card borders, dividers) │
│ --border-strong │ #2f4558   (active borders)         │
├─────────────────┼───────────────────────────────────┤
│ Text Hierarchy                                      │
├─────────────────┼───────────────────────────────────┤
│ --text-primary  │ #e8edf2   (headings, data values) │
│ --text-secondary│ #8899a8   (labels, helper text)   │
│ --text-muted    │ #5a6e80   (timestamps, captions)  │
├─────────────────┼───────────────────────────────────┤
│ Semantic / Status Colors                            │
├─────────────────┼───────────────────────────────────┤
│ --status-rising │ #34d399   (water rising, positive) │
│ --status-stable │ #60a5fa   (stable, neutral info)   │
│ --status-falling│ #fbbf24   (water falling, caution) │
│ --accent        │ #3b82f6   (links, active nav)      │
│ --accent-hover  │ #60a5fa   (link hover)             │
├─────────────────┼───────────────────────────────────┤
│ Card Variants (status backgrounds, muted)           │
├─────────────────┼───────────────────────────────────┤
│ --bg-rising     │ #0d2e1f   (rising card tint)       │
│ --bg-stable     │ #0f1f36   (stable card tint)       │
│ --bg-falling    │ #2a1f0a   (falling card tint)      │
└─────────────────┴───────────────────────────────────┘
```

**Design rationale**:
- `--bg-base` (#0f1923) is a dark navy, not pure black — warmer, more atmospheric, matches the "fishing lodge" vs "hospital" goal
- Two-level surface system (base → surface) creates depth without drop shadows, following the TradingView pattern
- Status colors are bright enough to pop on dark backgrounds, passing WCAG AA against `--bg-surface`
- Green/amber for rising/falling preserved from existing design, just brightened for dark backgrounds
- Blue accent (`--accent`) matches the existing `bg-primary` brand identity but is used sparingly (links, active nav) rather than as a background flood

### 13.2 Typography System

```
Font: Inter (variable, Google Fonts)
Fallback: system-ui, -apple-system, sans-serif

Hierarchy:
┌──────────────────┬────────┬────────┬───────────────────────────┐
│ Role             │ Size   │ Weight │ Usage                     │
├──────────────────┼────────┼────────┼───────────────────────────┤
│ Primary value    │ clamp  │ 700    │ Water level, temps        │
│                  │(2.25-  │        │ (unchanged from current)  │
│                  │ 3.25rem│        │                           │
│ Section heading  │ 1.1rem │ 600    │ Card headers              │
│ Table heading    │ 0.8rem │ 600    │ Column labels, UPPERCASE  │
│ Body             │ 0.9rem │ 400    │ Catch descriptions, notes │
│ Data value       │ 1.25rem│ 600    │ Leaderboard numbers       │
│ Label / caption  │ 0.75rem│ 500    │ "Air Temp", "Water Level" │
│ Tiny / timestamp │ 0.7rem │ 400    │ "2 hours ago", footnotes  │
└──────────────────┴────────┴────────┴───────────────────────────┘

Numeric styling:
- All numeric displays: font-variant-numeric: tabular-nums
- Inter supports this natively — numbers align in columns automatically
```

**Why Inter over system fonts**: The primary complaint is "looks homemade." System fonts are invisible — they're what every default site uses. Inter is a deliberate design choice that signals "we thought about this." Its tabular figures are best-in-class for dashboard number alignment, and its x-height ensures readability for older users at arm's length.

### 13.3 Navigation / Topbar

**Current**: `<nav class="navbar navbar-expand-sm navbar-dark bg-primary border-bottom box-shadow mb-3">`
- Bright blue background
- 3px bottom margin creates white gap
- Standard Bootstrap hamburger at `sm`

**Proposed**:

```
┌──────────────────────────────────────────────────────────────┐
│  🎣 LAKS                              Home  Catches  Stats  │
│──────────────────────────────────────────────────────────────│
│  [conditions strip starts immediately, no gap]               │
```

- **Background**: `--bg-surface` (#162029) — same family as page background, no contrasting color block
- **Bottom border**: 1px `--border-subtle` (#243442) — just enough to separate, not a decorative band
- **No margin-bottom** — content flows directly beneath the nav, reinforcing the "integrated dashboard" feel
- **Brand**: "🎣 LAKS" in Inter 700, `--text-primary`, 1.1rem — understated but clear
- **Nav links**: Inter 500, `--text-secondary` (#8899a8) default. Active page link in `--accent` (#3b82f6) with a 2px bottom indicator line. Hover: `--text-primary`
- **No hamburger collapse up to md** — only 3 links, they fit on any screen wider than 375px. Below that: small horizontal scroll or stacked gracefully.
- **Sticky top** — stays visible while scrolling the data-dense dashboard. Slim height (~48px) to preserve vertical data space.
- **Remove `box-shadow`** — shadows on a dark background look muddy. The border is sufficient.

### 13.4 Component Restyling Summary

| Component | Current | Proposed |
|-----------|---------|----------|
| **Page background** | White (`#fff`) | `--bg-base` (#0f1923) |
| **Cards** | White bg, `#d8e0ea` border, `shadow-sm` | `--bg-surface` bg, `--border-subtle` border, no shadow |
| **Card headers** | Bootstrap default (light gray bg) | `--bg-raised` bg, section heading in `--text-primary` |
| **Table headers** | `.table-primary` (blue tint) | `--bg-raised`, uppercase small text in `--text-secondary` |
| **Table rows** | `.table-striped` alternating white/gray | Even: `--bg-surface`, odd: `--bg-base` (subtle alternation) |
| **Table hover** | `.table-hover` light highlight | `--bg-raised` on hover |
| **Condition cards (rising)** | `#edf8f0` bg + green left border | `--bg-rising` bg + green left border (brighter on dark) |
| **Condition cards (falling)** | `#fff7ea` bg + amber left border | `--bg-falling` bg + amber left border |
| **Condition cards (stable)** | `#ecf3fb` bg + blue left border | `--bg-stable` bg + blue left border |
| **Summary cells** | `#f6f8fb` bg, `#d8e0ea` border | `--bg-surface`, `--border-subtle` |
| **Record cards** | `#fbfdff` bg | `--bg-surface` |
| **Recent catch items** | `#f8fafc` bg | `--bg-surface` |
| **Initials circle** | Navy gradient | Keep — already works on dark bg |
| **Footer** | Light border-top, `text-muted` | `--border-subtle` border, `--text-muted` text |
| **Links** | `#0077cc` | `--accent` (#3b82f6), hover `--accent-hover` |
| **Buttons (primary)** | `#1b6ec2` bg | `--accent` bg, white text |
| **Leaderboard toggle btns** | `btn-primary` / `btn-outline-primary` | Active: `--accent` bg. Inactive: `--bg-raised` bg, `--text-secondary` text |
| **Form selects** | Bootstrap default (white bg) | `--bg-surface` bg, `--text-primary` text, `--border-subtle` border |
| **Alerts** | `.alert-info` (light blue) | `--bg-stable` bg, `--status-stable` text accent |
| **Map border** | `#d0dae4` | `--border-subtle` |
| **Charts** | Light default Chart.js theme | Dark axes, gridlines in `--border-subtle`, data in status colors |

### 13.5 Chart.js & Leaflet Dark Mode Notes

**Chart.js** — needs theming:
- Grid lines: `--border-subtle` (#243442)
- Axis text: `--text-secondary` (#8899a8)
- Tooltip background: `--bg-raised` (#1c2a35)
- Data line: `--status-stable` or `--accent`
- Fill area: semi-transparent status color

**Leaflet map** — needs dark tiles:
- Switch from default OpenStreetMap to **CartoDB Dark Matter** tiles (`https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png`) — free, no API key, designed for dark UIs
- Marker colors: bright status colors on dark map visually pop

---

## 14. Accessibility Audit for Dark Theme

| Concern | Mitigation |
|---------|-----------|
| **Contrast ratios** | `--text-primary` (#e8edf2) on `--bg-surface` (#162029) = **~12:1** — exceeds WCAG AAA. `--text-secondary` (#8899a8) on `--bg-surface` = **~4.8:1** — passes WCAG AA. |
| **Status color on dark bg** | `--status-rising` (#34d399) on `--bg-rising` (#0d2e1f) = **~7.5:1** — passes AA. Amber on `--bg-falling` similar. All status indicators also use shape (arrows ↑→↓) per existing design. |
| **Link visibility** | `--accent` (#3b82f6) on `--bg-surface` = **~5.2:1** — passes AA. Underline on hover for additional affordance. |
| **Form inputs** | Light text on dark inputs. Border becomes `--border-strong` on focus + blue outline ring. |
| **Aging eyes (60+ users)** | Dark backgrounds reduce glare. Inter font has tall x-height for legibility. Primary values remain 36px+ per existing spec. |
| **iPad at arm's length** | High contrast dark theme is MORE readable at distance than low-contrast light theme. Bright numbers on dark bg = higher perceived contrast. |
| **No dark-mode toggle** | Ship dark-only. The user base is fixed (36 people) and the stakeholder explicitly wants dark. No need for a preference toggle — that adds complexity for no audience benefit. |

---

## 15. Risk & Assumptions

| Item | Type | Mitigation |
|------|------|-----------|
| Bootstrap 5 assumes light theme by default | Risk | Override via CSS custom properties on `:root`. Bootstrap 5.3+ has `data-bs-theme="dark"` — check if project uses 5.3+. If not, manual overrides needed. |
| Google Fonts load adds latency | Risk | Use `font-display: swap` + preconnect hint. Inter variable woff2 is ~100KB — one-time load, cached. Alternatively, self-host from `/wwwroot/fonts/`. |
| Chart.js default colors assume light bg | Risk | Override via `Chart.defaults.color` and `Chart.defaults.borderColor` globals. |
| Leaflet tile change (CartoDB Dark Matter) | Assumption | Free, no API key, CDN-hosted. Verify availability and check attribution requirements. |
| Users (60+) prefer dark mode | Assumption | Stakeholder said the site should be dark. Anti-glare benefits are well-documented for this age group. No toggle shipped initially — can add later if feedback indicates need. |
| Inter font supports Norwegian chars | Verified | Inter includes full Latin Extended set (æ, ø, å confirmed). |

---

## 16. Open Questions

1. **Bootstrap version**: Does the project use Bootstrap 5.3+? If so, `data-bs-theme="dark"` gives us dark form controls, dropdowns, and modals for free. If 5.2 or earlier, we need manual overrides for every Bootstrap component.
2. **Self-host Inter vs Google Fonts CDN**: Self-hosting from `/wwwroot/fonts/` avoids a third-party dependency and is better for GDPR (no Google tracking). Recommended approach given the small user base.
3. **CartoDB attribution**: Dark Matter tiles require attribution. Current Leaflet map likely already has OSM attribution — verify it's swappable.
4. **Existing CSS specificity**: The current hardcoded hex colors in `dashboard.css` and `_Layout.cshtml.css` will all need replacement with CSS custom properties. This is a one-time refactor but touches every color declaration.

---

## 17. Implementation Scope

This visual refresh is **CSS/frontend only** — no backend changes, no model changes, no database changes.

### Files to Change

| File | Change |
|------|--------|
| `Pages/Shared/_Layout.cshtml` | New `<link>` for Inter font. Add `data-bs-theme="dark"` to `<html>`. Restyle `<nav>` element. Remove `mb-3` from nav. Update footer classes. |
| `wwwroot/css/site.css` | Add `:root` CSS custom properties for entire dark palette. Set `body` background/color. Restyle links, buttons, forms. Override Bootstrap component colors. |
| `wwwroot/css/dashboard.css` | Replace all hardcoded light-theme hex colors with `var(--*)` custom properties. Update condition card backgrounds, summary cells, record cards, recent catch items. |
| `Pages/Shared/_Layout.cshtml.css` | Replace navbar, link, button, footer colors with custom properties. |
| `Pages/Shared/_Leaderboard.cshtml` | Update button classes from `btn-primary`/`btn-outline-primary` to themed variants. |
| `Pages/Catches/Index.cshtml` | Update `table-primary`, `alert-info`, `btn-outline-primary` classes. |
| `Pages/Statistics/Index.cshtml` | Update card/badge classes. Add Chart.js global dark defaults in `<script>`. |
| `wwwroot/js/water-level-chart.js` | Apply dark grid/axis colors. |
| `wwwroot/js/catch-map.js` | Switch tile URL to CartoDB Dark Matter. |

### Files NOT Changed
- No `.cs` files
- No `.cshtml.cs` PageModel files
- No repository/service/model files
- No database migrations
- No test files (visual changes only)

---

## 18. Recommendations Summary

1. **Go dark** — adopt `--bg-base` (#0f1923) dark navy theme. It matches the "premium data dashboard" goal, reduces glare for 60+ users on iPads, and makes colored data (trends, status) more visible.
2. **Adopt Inter font** — self-hosted variable font. One file, excellent tabular figures, designed for data UIs, supports Norwegian characters. Replace the implicit Bootstrap system font stack.
3. **Flatten the topbar** — remove the bright blue `bg-primary` block. Make it a slim, integrated dark bar that blends with the page. Add sticky positioning. Remove the margin-bottom gap. Use accent color only for the active nav link indicator.
4. **Implement via CSS custom properties** — define the full palette in `:root`, then reference everywhere. This makes future theming trivial and keeps values consistent.
5. **Use Bootstrap 5.3 `data-bs-theme="dark"`** if available — gives free dark-mode styling for all Bootstrap components (forms, dropdowns, modals, tables).
6. **Switch map tiles to CartoDB Dark Matter** — free, no API key, purpose-built for dark UIs.
7. **No light/dark toggle** — ship dark-only. The audience is 36 known users and the stakeholder wants dark. Simplicity over configurability.
