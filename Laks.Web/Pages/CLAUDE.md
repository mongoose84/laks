# Frontend Context — Razor Pages

Inherits from project root `CLAUDE.md`.

## Design System
All UI work in this directory must follow the design language spec:
→ `.github/specs/design-language.spec.md`

Key rules at a glance:
- Primary colour: `#1A4D7A` (blue) — buttons, links, key elements
- Secondary: `#E6F0FA` (light blue) — backgrounds, hover states
- Accent: `#FF9900` (orange) — highlights, CTAs, warnings
- 8px grid — all margin/padding in multiples of 8
- Max content width: 1200px
- Error text colour: `#D32F2F`
- Primary font: `'Segoe UI', Arial, sans-serif`
- Monospace for tables and statistics data

## Danish Translation Reference
The authoritative mapping of every English string to its Danish equivalent is in:
→ `.github/specs/danish-translation.spec.md`

This covers all Razor pages, partials, JS chart labels, and C# model strings — look here before
writing any user-facing text to ensure consistency with the agreed translations.

## Component Specs
When implementing a new Razor Page or partial, use the component spec template:
→ `.github/specs/component.spec.md`

## Active Homepage Feature
The current homepage is being rebuilt to the editorial magazine design. See:
→ `.github/specs/editorial-magazine-homepage.spec.md` — active implementation spec
→ `.github/specs/landing-page-dashboard.spec.md` — full dashboard data requirements

## Requirements
- All user-facing text in Danish (labels, aria-labels, errors, empty states)
- WCAG 2.1 AA: semantic HTML, correct aria attributes, keyboard navigable
- Server-rendered-first — no JavaScript unless unavoidable
