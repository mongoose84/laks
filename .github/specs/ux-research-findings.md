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
