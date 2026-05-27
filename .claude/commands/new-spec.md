Create a new feature spec for: $ARGUMENTS

1. Read the spec template at `.github/specs/feature-template.spec.md`
2. Read `CLAUDE.md` for project context (tech stack, constraints, file layout)
3. Scan `Laks.Web/Pages/` and `Laks.Web/Models/` for relevant existing patterns
4. Check `.github/specs/` for any related existing specs

Then create `.github/specs/$ARGUMENTS.spec.md` using the template structure.

Fill in what can be inferred from the project (tech stack details, file paths, constraint reminders).
Leave placeholders clearly marked with `[TODO: ...]` for sections that require decisions or research.

The spec must include:
- **Problem** — user need this solves (be specific to the fishing club context)
- **Solution** — high-level approach
- **User Stories** — primary and secondary flows
- **Technical Changes** — Backend and Frontend sections with actual file paths from this project
- **Testing** — what needs to be tested
- **Acceptance Criteria** — measurable, verifiable items
- **Dependencies** — internal or external
- **Notes** — risks, assumptions, open questions

Spec language: English (specs are internal developer documentation).
After creating the file, print its path and a one-line summary of the feature.
