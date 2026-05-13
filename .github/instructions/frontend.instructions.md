---
applyTo: "**/*.{cshtml,cshtml.cs,js,css,html,scss,sass,less}"
description: ".cshtml development guidelines with context engineering"
---
# .cshtml Development Guidelines

Inherits from [global instructions](../copilot-instructions.md).

## Context Loading
Review [project conventions](../../README.md) and
[component patterns](../../) before starting.

## Deterministic Requirements
- Follow .cshtml best practices and conventions
- All user-facing text must be in Danish (labels, headings, buttons, aria-labels, empty states, tooltips)
- Ensure accessibility (WCAG guidelines, semantic HTML)
- Apply responsive design principles
- Follow standard formatting
- Follow OWASP top 10

## Structured Output
Generate code with:
- [ ] Clear page and PageModel documentation where needed
- [ ] Unit tests in the appropriate tests directory
- [ ] Accessibility attributes (aria-labels, roles)
- [ ] Loading and error states for async operations
