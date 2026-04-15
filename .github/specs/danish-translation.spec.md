# Feature: Translate All User-Facing Text to Danish

## Problem
The site currently displays all UI text in English, but the target audience is Danish-speaking fishing enthusiasts (the original site at fisk.krunk.dk is in Danish). Users expect a Danish-language experience. There is no need for multi-language support — the entire site should simply be in Danish.

## Solution
Replace all hardcoded English user-facing text strings with Danish equivalents across all Razor pages, partial views, JavaScript files, and the `SeasonDay` model. This is a straight find-and-replace exercise — no i18n framework is needed. Also update the `<html lang>` attribute to `da`.

## User Stories
- As a returning user from fisk.krunk.dk, I want the new site to be in Danish so that the experience feels natural and familiar.
- As a Danish angler, I want all labels, headings, buttons, navigation, and messages in Danish so I can understand everything without translation.

---

## Inventory of Text to Translate

### 1. Layout (`Pages/Shared/_Layout.cshtml`)

| Location | Current English | Danish |
|----------|----------------|--------|
| `<html lang>` | `en` | `da` |
| Page title suffix | `LAKS 🎣` | `LAKS 🎣` *(keep — brand name)* |
| Nav link | `Home` | `Forside` |
| Nav link | `Catches` | `Fangster` |
| Nav link | `Statistics` | `Statistik` |
| Nav toggle aria-label | `Toggle navigation` | `Skift navigation` |
| Footer | `LAKS – Norwegian Fishing Trip Records` | `LAKS – Laksefiskeri i Holmfoss` |
| Footer | `Every fish tells a story` | `Hver fisk fortæller en historie` |

### 2. Home / Dashboard (`Pages/Index.cshtml`)

| Location | Current English | Danish |
|----------|----------------|--------|
| ViewData["Title"] | `Home` | `Forside` |

### 3. Conditions Strip (`Pages/Shared/_ConditionsStrip.cshtml`)

| Location | Current English | Danish |
|----------|----------------|--------|
| Trend text | `Rising` | `Stigende` |
| Trend text | `Falling` | `Faldende` |
| Trend text | `Stable` | `Stabil` |
| Section aria-label | `Current conditions` | `Aktuelle forhold` |
| Label | `Air Temp` | `Lufttemperatur` |
| Label | `Water Level` | `Vandstand` |
| Label | `Water Temp` | `Vandtemperatur` |
| Label | `Wind m/s` | `Vind m/s` |
| Label | `Season Day` | `Sæsondag` |

### 4. Leaderboard (`Pages/Shared/_Leaderboard.cshtml`)

| Location | Current English | Danish |
|----------|----------------|--------|
| Heading | `Season Leaderboard` | `Sæson-rangliste` |
| Button aria-label | `Leaderboard scope` | `Rangliste-omfang` |
| Button | `My Group` | `Min gruppe` |
| Button | `All Groups {year}` | `Alle grupper {year}` |
| Button | `Last Year` | `Sidste år` |
| Empty state | `No leaderboard data available.` | `Ingen rangliste-data tilgængelig.` |
| Caption | `Leaderboard table for selected period` | `Rangliste for valgt periode` |
| Column header | `Angler` | `Fisker` |
| Column header | `Fish` | `Fisk` |
| Column header | `Total kg` | `Total kg` *(keep)* |
| Column header | `Best kg` | `Bedste kg` |
| Footer text | `Group {n} ({label}): {n} fish / {n} kg` | `Gruppe {n} ({label}): {n} fisk / {n} kg` |

### 5. Recent Catches (`Pages/Shared/_RecentCatches.cshtml`)

| Location | Current English | Danish |
|----------|----------------|--------|
| Heading | `Recent Catches` | `Seneste fangster` |
| Link | `View all catches` | `Se alle fangster` |
| Empty state | `No catches yet this season.` | `Ingen fangster i denne sæson endnu.` |
| Relative time | `{n} min ago` | `{n} min siden` |
| Relative time | `{n} hours ago` | `{n} timer siden` |
| Relative time | `Yesterday` | `I går` |
| Relative time | `{n} days ago` | `{n} dage siden` |

### 6. Season Summary (`Pages/Shared/_SeasonSummary.cshtml`)

| Location | Current English | Danish |
|----------|----------------|--------|
| Section aria-label | `Season summary` | `Sæsonoversigt` |
| Empty state | `Season summary unavailable.` | `Sæsonoversigt ikke tilgængelig.` |
| Label | `Total Fish` | `Fisk i alt` |
| Label | `Total kg` | `Total kg` *(keep)* |
| Label | `Avg kg` | `Gns. kg` |
| Label | `Biggest ({angler})` | `Største ({angler})` |
| Label | `Active Anglers` | `Aktive fiskere` |
| Label | `Season Day` | `Sæsondag` |

### 7. All-Time Records (`Pages/Shared/_AllTimeRecords.cshtml`)

| Location | Current English | Danish |
|----------|----------------|--------|
| Heading | `All-Time Records` | `Alle tiders rekorder` |
| Empty state | `No all-time records available.` | `Ingen rekorder tilgængelige.` |
| Label | `Biggest Fish Ever` | `Største fisk nogensinde` |
| Label | `Most Prolific Angler` | `Mest produktive fisker` |
| Label | `Best Season` | `Bedste sæson` |
| Suffix | `fish` | `fisk` |

### 8. Catch Map (`Pages/Shared/_CatchMap.cshtml`)

| Location | Current English | Danish |
|----------|----------------|--------|
| Heading | `Catch Map` | `Fangstkort` |
| Button | `This season` | `Denne sæson` |
| Button | `All time` | `Alle tider` |
| aria-label (map) | `Catch locations at Holmfoss on Numedalslagen` | `Fangststeder ved Holmfoss på Numedalslågen` |
| Empty state | `No catch locations to display.` | `Ingen fangststeder at vise.` |

### 9. Water Level Chart (`Pages/Shared/_WaterLevelChart.cshtml`)

| Location | Current English | Danish |
|----------|----------------|--------|
| Header label | `Vannstand - Holmfoss (15.61.0)` | *(already Norwegian — keep or use)* `Vandstand – Holmfoss (15.61.0)` |
| Label | `Current level` | `Aktuel vandstand` |
| Label | `Unavailable` | `Ikke tilgængelig` |
| Label | `Last updated:` | `Sidst opdateret:` |
| Alert | `No water level readings in the last 24 hours.` | `Ingen vandstandsmålinger de seneste 24 timer.` |
| aria-label (chart) | `Water level chart for the last 24 hours, Holmfoss station 15.61.0` | `Vandstandsgraf for de seneste 24 timer, Holmfoss station 15.61.0` |
| Hidden text | `Water level chart data for the last 24 hours from NVE station 15.61.0.` | `Vandstandsdata for de seneste 24 timer fra NVE station 15.61.0.` |

### 10. Catches Page (`Pages/Catches/Index.cshtml`)

| Location | Current English | Danish |
|----------|----------------|--------|
| ViewData["Title"] | `Catches` | `Fangster` |
| Heading | `🐟 Catch Log` | `🐟 Fangstlog` |
| Link | `Statistics »` | `Statistik »` |
| Label | `Filter by season year` | `Filtrer efter sæsonår` |
| Option | `— All seasons —` | `— Alle sæsoner —` |
| Suffix | `catches` *(in season option)* | `fangster` |
| Info alert | `Season {year}` | `Sæson {year}` |
| Info alert | `catches` | `fangster` |
| Info alert | `participants` | `deltagere` |
| Caption | `{n} catch(es) shown` | `{n} fangst(er) vist` |
| Column header | `Date` | `Dato` |
| Column header | `Time` | `Tid` |
| Column header | `Year` | `År` |
| Column header | `Angler` | `Fisker` |
| Column header | `Type` | `Type` *(keep)* |
| Column header | `Weight (kg)` | `Vægt (kg)` |
| Column header | `Location` | `Sted` |
| Column header | `Weather` | `Vejr` |
| Column header | `Water level` | `Vandstand` |
| Column header | `Bait` | `Agn` |
| Column header | `Notes` | `Noter` |
| Empty state | `No catches found for the selected filter.` | `Ingen fangster fundet for det valgte filter.` |

### 11. Statistics Page (`Pages/Statistics/Index.cshtml`)

| Location | Current English | Danish |
|----------|----------------|--------|
| ViewData["Title"] | `Statistics` | `Statistik` |
| Heading | `📊 Fishing Statistics` | `📊 Fiskestatistik` |
| Label | `Filter angler & type charts by year` | `Filtrer fisker- og typediagrammer efter år` |
| Option | `— All years —` | `— Alle år —` |
| Card header | `Total Catches per Year (all-time trend)` | `Fangster pr. år (historisk trend)` |
| Card header | `Catches per Angler` | `Fangster pr. fisker` |
| Card header | `Catch Type Distribution` | `Fangsttype-fordeling` |
| Card header | `🏆 Biggest Salmons by Team` | `🏆 Største laks pr. hold` |
| Table header | `Team` | `Hold` |
| Table header | `Biggest (kg)` | `Største (kg)` |
| Table header | `Caught by` | `Fanget af` |
| Table header | `Salmon` | `Laks` |
| Table header | `Avg (kg)` | `Gns. (kg)` |
| Empty state | `No team salmon data available.` | `Ingen holddata for laks tilgængelig.` |

### 12. Statistics Page – JavaScript Chart Labels (inline `<script>`)

| Location | Current English | Danish |
|----------|----------------|--------|
| Dataset label | `Number of catches` | `Antal fangster` |
| Dataset label | `Total weight (kg)` | `Samlet vægt (kg)` |
| Tooltip suffix | `catches` | `fangster` |
| Axis title | `Catches` | `Fangster` |
| Dataset label | `Catches` *(bar chart)* | `Fangster` |
| Dataset label | `Total weight (kg)` *(bar chart)* | `Samlet vægt (kg)` |
| Dataset label | `Biggest salmon (kg)` | `Største laks (kg)` |
| Dataset label | `Avg salmon weight (kg)` | `Gns. laksevægt (kg)` |
| Axis title | `Biggest (kg)` | `Største (kg)` |
| Axis title | `Avg (kg)` | `Gns. (kg)` |

### 13. Catch Map – JavaScript (`wwwroot/js/catch-map.js`)

| Location | Current English | Danish |
|----------|----------------|--------|
| Popup label | `Bait:` | `Agn:` |

### 14. Privacy Page (`Pages/Privacy.cshtml`)

| Location | Current English | Danish |
|----------|----------------|--------|
| ViewData["Title"] | `Privacy Policy` | `Privatlivspolitik` |
| Body text (2 paragraphs) | *(English text)* | Full Danish rewrite — see below |

**Danish privacy text:**
> LAKS er en personlig, ikke-kommerciel hjemmeside, der registrerer fiskeresultater fra en årlig tur til Norge. Der opbevares ingen persondata ud over for- og efternavn på deltagende fiskere. Der sættes ingen cookies, og der indsamles ingen analysedata.
>
> Hvis du har spørgsmål om data, der er gemt på denne side, kontakt venligst sidens administrator.

### 15. Error Page (`Pages/Error.cshtml`)

| Location | Current English | Danish |
|----------|----------------|--------|
| ViewData["Title"] | `Error` | `Fejl` |
| Heading | `Error.` | `Fejl.` |
| Subheading | `An error occurred while processing your request.` | `Der opstod en fejl under behandling af din forespørgsel.` |
| Label | `Request ID:` | `Forespørgsels-ID:` |
| Heading | `Development Mode` | `Udviklertilstand` |
| Body text | *(development instructions)* | Can remain in English (developer-only, never shown in production) |

### 16. C# Model – `SeasonDay.DisplayText` (`Models/SeasonDay.cs`)

| Location | Current English | Danish |
|----------|----------------|--------|
| Format | `Day {n} of {n} · Group {n}` | `Dag {n} af {n} · Gruppe {n}` |
| Format | `Fishing starts today` | `Fiskeriet starter i dag` |
| Format | `Fishing starts tomorrow` | `Fiskeriet starter i morgen` |
| Format | `Fishing starts in {n} days` | `Fiskeriet starter om {n} dage` |
| Fallback | `Season not configured` | `Sæson ikke konfigureret` |

---

## Technical Changes

### Backend
**Stack**: C# with .NET

**Components**:
- [ ] Models: `src/Laks.Web/Models/SeasonDay.cs` — translate `DisplayText` strings

### Frontend
**Stack**: ASP.NET Core Razor Pages (.cshtml + PageModel)

**Components**:
- [ ] Layout: `src/Laks.Web/Pages/Shared/_Layout.cshtml` — nav links, footer, `<html lang="da">`
- [ ] Dashboard partials:
  - `src/Laks.Web/Pages/Shared/_ConditionsStrip.cshtml`
  - `src/Laks.Web/Pages/Shared/_Leaderboard.cshtml`
  - `src/Laks.Web/Pages/Shared/_RecentCatches.cshtml`
  - `src/Laks.Web/Pages/Shared/_SeasonSummary.cshtml`
  - `src/Laks.Web/Pages/Shared/_AllTimeRecords.cshtml`
  - `src/Laks.Web/Pages/Shared/_CatchMap.cshtml`
  - `src/Laks.Web/Pages/Shared/_WaterLevelChart.cshtml`
- [ ] Razor pages:
  - `src/Laks.Web/Pages/Index.cshtml`
  - `src/Laks.Web/Pages/Catches/Index.cshtml`
  - `src/Laks.Web/Pages/Statistics/Index.cshtml`
  - `src/Laks.Web/Pages/Privacy.cshtml`
  - `src/Laks.Web/Pages/Error.cshtml`
- [ ] JavaScript: `src/Laks.Web/wwwroot/js/catch-map.js` — popup label "Bait:"

### Not in Scope
- Database column names / stored values (e.g. catch types, location names, angler names)
- API response field names
- Developer-facing log messages or comments
- NuGet package or CI/CD configuration

## Testing
- [ ] Visual walkthrough of every page confirming all visible text is in Danish
- [ ] Verify `<html lang="da">` renders correctly
- [ ] Verify Chart.js tooltip and axis labels display Danish strings
- [ ] Verify `SeasonDay.DisplayText` returns Danish for all three code paths (active day, buffer day, off-season)
- [ ] Verify date formatting still uses `nb-NO` or switch to `da-DK` in JS `toLocaleDateString` / `toLocaleTimeString` calls
- [ ] Screen reader check for Danish aria-labels
- [ ] Existing unit tests in `tests/Laks.Web.Tests/` still pass (update any string assertions that reference English text)

## Acceptance Criteria
- [ ] Zero English user-facing text remains on any page (nav, headings, labels, buttons, tooltips, empty states, footer)
- [ ] `<html lang="da">` is set in the layout
- [ ] `SeasonDay.DisplayText` produces Danish strings
- [ ] Chart labels in Statistics page are in Danish
- [ ] Catch map popup shows "Agn:" instead of "Bait:"
- [ ] Privacy page content is fully in Danish
- [ ] All existing tests pass (with updated assertions where needed)
- [ ] No changes to database schema, stored data, or API contracts

## Dependencies
- [ ] None — purely a text replacement exercise

## Notes
- The water level chart header already uses Norwegian ("Vannstand") — update to Danish spelling ("Vandstand") for consistency.
- Date/time locale in JS files currently uses `nb-NO` (Norwegian Bokmål). Consider switching to `da-DK` for Danish formatting, though the formats are nearly identical.
- The Error page "Development Mode" section can remain in English since it is only visible to developers and never shown in production.
- Total count: approximately **90+ individual text strings** across 16 files.
