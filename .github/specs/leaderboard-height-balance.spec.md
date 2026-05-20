# Feature: Sæsontavlen højdebalance mod vandstandsgraf

## Problem
På forsiden ligger `Vandstand seneste 24 timer` og `Sæsontavlen` side om side i `ed-feature-grid` (1.7fr / 1fr, `align-items: start`). Vandstandsgrafen har en fast højde (~290 px canvas + meta + figcaption ≈ 400–450 px). Sæsontavlen er derimod en liste, hvor højden vokser lineært med antallet af anglere.

Følger vi den nuværende rækkehøjde (~50–55 px pr. række inkl. padding og border) bliver konsekvenserne:

| Scope | Typisk antal rækker | Estimeret højde |
|-------|---------------------|-----------------|
| Mit hold (12 anglere) | op til 12 | ~600–660 px |
| Alle hold (~36 anglere) | op til 36 | ~1.800 px |
| Sidste år | op til 36 | ~1.800 px |

Resultatet i dag:
- Når listen er kortere end grafen, opstår tom plads under tavlen (mindre alvorligt).
- Når listen er længere end grafen, opstår en stor tom plads til **højre under grafen**, fordi grid-rækken bliver så høj som den højeste celle. Dette er det scenarie brugeren rapporterer.
- På morgenens fælles tablet-visning skubbes `Catch map`, `Recent catches` og `Season summary` langt ned, så det vigtigste indhold (vejr + de seneste fangster) ryger under fold.

Målgruppen er overvejende 60+ år, der ofte ser siden sammen omkring én iPad. Læsbarhed og prioritering af “hvad sker der lige nu” vægter højere end at se hele ranglisten på forsiden.

## Solution
Fastsæt **Sæsontavlen som en kompakt, højde-bevidst komponent** der ikke længere må diktere højden på grid-rækken. Vi reducerer informationstætheden i toppen, holder en konstant maks-højde tæt på grafens, og flytter den fulde rangliste til en dedikeret side.

### Anbefalet retning (kombination af A + C + E nedenfor)
1. **Vis Top 5 som standard** med fremhævet #1 ("førertrøje"). Dette dækker det reelle behov i morgenscenariet: "hvem fører?".
2. **Cap højden på listen** til ca. grafens højde (`max-height` baseret på chart-højden, intern scroll med diskret indikator hvis flere). Det fjerner den tomme højre-side helt.
3. **"Se hele ranglisten" link** under listen, som åbner en fuld side (eller modal) med alle anglere, sortering og filtre. Der hører også de mere detaljerede stats (bedste fangst, antal fisk) til.
4. **Komprimer hold-skifteren** når der er flere scopes aktive, og flyt `ed-lb-groups`-summary op som tre små "chips" i stedet for én tekstlinje, så højden bliver forudsigelig.
5. På smalle viewports (<900 px) bevarer vi nuværende stack-layout, hvor højdeproblemet ikke findes.

Denne retning løser overflow-problemet uden at fjerne information — den flytter blot dybden ét klik væk.

### Alternative løsninger (vurderet)
Dokumenteret så teamet kan vælge bevidst.

| # | Idé | Fordele | Ulemper |
|---|-----|---------|---------|
| A | **Top N + "Vis alle" link/side** (anbefalet) | Forudsigelig højde, hurtigt overblik, plads til detaljer på fuld side | Kræver ny side/route til fuld rangliste |
| B | **Top N + inline ekspansion ("Vis flere")** | Ingen ny side, alt på forsiden | Genskaber problemet når brugeren ekspanderer; layout-shift |
| C | **Intern scroll i tavlen, capped til grafens højde** | Visuelt balanceret grid; hele listen tilgængelig uden navigation | Scroll-i-scroll på mobil/tablet er ofte forvirrende for målgruppen 60+ |
| D | **Podium top 3 + komprimeret liste under** | Stærk visuel hierarki; godt til "konkurrence"-følelsen | Stadig variabel højde hvis listen er lang; mere designarbejde |
| E | **Komprimer rækkehøjde** (mindre `ed-lb-rank`-typografi, fjern best-fish detail fra forsiden) | Hurtig gevinst, ingen nye sider | Alene løser det ikke for 36 anglere; svækker editorial-look |
| F | **Pagination (5 ad gangen, frem/tilbage)** | Konstant højde | Klikkrævende for målgruppen, dårligt til "glance"-brug |
| G | **Flyt tavlen under grafen i fuld bredde** | Eliminerer grid-konflikten | Mister side-om-side narrativet og skubber `Catch map` længere ned |
| H | **Equalize heights med `align-items: stretch` + tavle = højeste** | Ingen overflow visuelt | Grafen får falsk tom plads; flytter problemet |

## User Stories
- Som angler der tjekker forsiden om morgenen på fællestabletten, vil jeg på 2 sekunder kunne se hvem der fører mit hold, så vi har samtalestof inden vi går ud.
- Som angler der vil dykke ned i ranglisten, vil jeg kunne åbne en dedikeret side med hele listen, sortering og detaljer, så jeg kan sammenligne mig med alle deltagere.
- Som familiemedlem der følger med hjemmefra, vil jeg se en visuelt balanceret forside uden store tomme felter, så det føles som et færdigt produkt.
- Som bruger på en lille skærm (telefon) vil jeg fortsat se vandstandsgrafen og tavlen i en naturlig stak uden scroll-i-scroll.
- Som bruger der skifter mellem `Mit hold`, `Alle hold` og `Sidste år` vil jeg ikke opleve at sidens layout hopper voldsomt mellem scopes.

## Technical Changes

### Backend
**Stack**: c# with .NET

**Components**:
- [ ] PageModel/ViewModel updates: `Laks.Web/Pages/Index.cshtml.cs`
  - Eksponer `LeaderboardPreview` (top N, fx 5) til forsiden, og fortsæt med at udstille fuldt sæt til den dedikerede side.
  - Tilføj `LeaderboardTotalCount` så forsiden kan vise "Se alle 36 →".
- [ ] Ny side / route: `Laks.Web/Pages/Statistics/Leaderboard.cshtml(.cs)` (eller eksisterende statistik-side udvidet) som viser fuld rangliste, scope-skift og sortering.
- [ ] Ingen DB- eller modelændringer nødvendige; data findes allerede via `LeaderboardEntry` og repositories.

### Frontend
**Stack**: ASP.NET Core Razor Pages (.cshtml + PageModel)

**Components**:
- [ ] Razor Pages/Partials: `Laks.Web/Pages/Shared/_Leaderboard.cshtml`
  - Render kun `LeaderboardPreview` (top 5) på forsiden.
  - Tilføj footer-link `Se hele ranglisten →` til ny side; vis kun hvis `LeaderboardTotalCount > preview-count`.
  - Komprimer `ed-lb-groups` til tre chips i én række med fast linjehøjde.
  - Fjern "bedste fangst" fra preview-rækken; behold den på fuld side.
- [ ] CSS: `Laks.Web/wwwroot/css/editorial.css`
  - `.ed-feature-grid { align-items: stretch; }` kombineret med `.ed-section--leaderboard { display:flex; flex-direction:column; max-height: <chart-højde>; }`.
  - Reducér `.ed-lb-row` padding fra `0.75rem` → `0.55rem`; reducér `.ed-lb-rank` fra `1.7rem` → `1.35rem`.
  - Indfør `.ed-lb-list` wrapper med `overflow: hidden` (preview viser præcis Top 5; ingen intern scroll).
  - Sørg for at media query `<900px` bevarer nuværende stack uden max-height.
- [ ] Ny side: `Laks.Web/Pages/Statistics/Leaderboard.cshtml`
  - Fuld liste, samme scope-pills, evt. tabel-layout med kolonner: Rang, Angler, Fisk, Vægt total, Bedste, Hold.

## Testing
- [ ] Unit tests: `IndexModel` returnerer korrekt antal preview-entries pr. scope (Mit hold ≤ 5, Alle hold ≤ 5, Sidste år ≤ 5) og korrekt `LeaderboardTotalCount`.
- [ ] Unit tests: ny `LeaderboardPageModel` returnerer fuld liste i forventet sortering for hvert scope.
- [ ] Visuel/manuel test: forsiden har ingen tom plads under grafen når tavlen vises med 0, 1, 5, 12 og 36 entries.
- [ ] Responsiv test: <900px stacker stadig naturligt uden afkortning.
- [ ] Tilgængelighed: "Se hele ranglisten"-link har korrekt `aria-label`, scope-pills bevarer eksisterende `role="group"`-mønster.

## Acceptance Criteria
- [ ] På viewport ≥ 900 px overstiger `.ed-section` for tavlen aldrig højden af `.ed-chart-figure` med mere end 24 px, uanset antal anglere.
- [ ] Forsiden viser højst 5 rækker pr. scope.
- [ ] Når der findes flere end 5 anglere i scopet, vises et tydeligt link til den fulde rangliste-side.
- [ ] Den fulde rangliste-side viser alle anglere for det valgte scope og understøtter samme scope-skift som forsiden.
- [ ] Hold-summary (`ed-lb-groups`) optager altid samme antal linjer (1) på desktop.
- [ ] Ingen tekst eller funktionalitet på engelsk; alle nye strenge er på dansk.
- [ ] Eksisterende tests består; nye tests dækker preview-trunkering og fuld-side rendering.

## Dependencies
- [ ] Ingen nye eksterne biblioteker.
- [ ] Afhænger af eksisterende `LeaderboardEntry`-model og repository.

## Notes / Open Questions
- **Hvor mange entries i preview?** 5 anbefales (passer til chart-højde med komprimeret rækkehøjde). 3 ("podium") er et alternativ hvis vi går D-vejen.
- **Skal "Sidste år" overhovedet være et forsidescope?** Det er overvejende et nostalgi-scope og kan måske kun leve på fuld rangliste-side, hvilket yderligere stabiliserer forsidens højde.
- **Førertrøje-fremhævning**: Skal #1 vises som hero-kort over Top 5-listen (à la "Dagens leder")? Det styrker konkurrencefølelsen uden at koste højde.
- **Ny side vs. modal**: Modal undgår navigation, men målgruppen 60+ er bedre tjent med en rigtig side med URL der kan deles via mail (deres primære kanal).
- **Komprimering alene (option E)** er en mulig hurtig fix, men løser ikke 36-rækker-scenariet — bør kun bruges som mellemstation.
