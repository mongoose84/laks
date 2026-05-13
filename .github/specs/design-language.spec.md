# Holmfoss Laks Design Language Specification

## Formål
Dette dokument beskriver det visuelle og funktionelle designgrundlag for Holmfoss lakseside. Brug dette dokument som reference, når der udvikles nye UI-komponenter eller opdateres eksisterende.

---

## 1. Farvepalette
- **Primærfarve:** Blå (#1A4D7A) – bruges til hovedknapper, links, og vigtige elementer.
- **Sekundærfarve:** Lys blå (#E6F0FA) – baggrunde, paneler, og hover-effekter.
- **Accentfarve:** Orange (#FF9900) – fremhævning, advarsler, og CTA.
- **Neutral:** Hvid (#FFFFFF), Grå (#F5F5F5, #CCCCCC, #333333) – baggrunde, tekst, borders.

## 2. Typografi
- **Primær font:** 'Segoe UI', Arial, sans-serif.
- **Overskrifter:** Fed, store bogstaver, tydelig afstand.
- **Brødtekst:** Normal vægt, let læselig størrelse (min. 16px).
- **Tal og data:** Brug monospace font til tabeller og statistik.

## 3. Ikoner og billeder
- Brug enkle, flade ikoner uden skygger.
- Ikoner skal være entydige og forståelige uden tekst.
- Billeder skal være relevante, i høj opløsning og optimeret til web.

## 4. Knapper
- **Primær knap:** Blå baggrund, hvid tekst, afrundede hjørner (4px radius).
- **Sekundær knap:** Hvid baggrund, blå tekst, blå border.
- **Hover:** Lys blå baggrund, mørkere tekst.
- **Disabled:** Grå baggrund, grå tekst.
- Knapper skal have tydelig fokusstil (outline).

## 5. Formularer
- Felter med tydelig label på dansk.
- Placeholder-tekst skal være vejledende, ikke erstatte label.
- Fejlmeddelelser vises under feltet i rød (#D32F2F).
- Brug store klikbare områder og tydelige feltafgrænsninger.

## 6. Layout og spacing
- Brug et 8px grid-system til margin og padding.
- Maksimal bredde på indhold: 1200px.
- Luft mellem sektioner: min. 24px.
- Brug cards/paneler til at gruppere relateret indhold.

## 7. Navigation
- Topnavigation med logo til venstre, menupunkter til højre.
- Aktivt menupunkt markeres med blå underline.
- Navigation skal være responsiv og mobilvenlig.

## 8. Tilgængelighed
- Alle interaktive elementer skal kunne bruges med tastatur.
- Farvekontrast skal overholde WCAG AA.
- Brug aria-labels på knapper og links, hvor det er nødvendigt.
- Tekst skal kunne forstørres uden at layoutet bryder.

## 9. Sprog og tone
- Alt brugerrettet tekst skal være på dansk.
- Skriv kort, venligt og informativt.
- Brug altid danske labels, tooltips og fejlbeskeder.

## 10. Animationer og feedback
- Brug kun diskrete animationer (fade, slide) til overgange.
- Loading-indikatorer skal være tydelige men ikke forstyrrende.
- Bekræftelser og fejl vises som toast eller inline besked.

---

## Instruktioner til udviklere
- Reference denne fil ved UI-udvikling.
- Ved tvivl: vælg altid den løsning, der bedst matcher ovenstående principper.
- Opdater denne fil, hvis designprincipper ændres.
- Overhold altid dansk sprog og tilgængelighed.

---

## Eksempler
- **Primær knap:**
  - Baggrund: #1A4D7A
  - Tekst: Hvid
  - Hover: #E6F0FA
- **Fejlmeddelelse:**
  - Tekst: "Dette felt er påkrævet."
  - Farve: #D32F2F

---

> Sidst opdateret: 13. maj 2026
