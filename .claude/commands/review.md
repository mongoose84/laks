Run a structured code review of the current changes.

1. Run `git diff HEAD` and `git diff --cached` to see all pending changes
2. Run `git status` to identify any untracked new files relevant to the change
3. Read each changed file in full for context
4. Apply the review checklist below

---

## Review Checklist

### Security (OWASP Top 10) — check first
- All SQL must use parameterized queries — flag any string interpolation in queries immediately
- No hardcoded secrets or credentials
- Input validation present on all user-supplied data
- Authentication/authorization checks in place where required
- No XSS vectors (`@Html.Raw` only where justified with a comment)

### Danish Language Compliance
- All user-facing text in Danish: labels, headings, buttons, links, navigation
- All `aria-label`, `title`, tooltip attributes in Danish
- All error messages and empty states in Danish

### Architecture & Patterns
- Pages and PageModels contain no direct SQL — only repository calls
- New models follow patterns in `Laks.Web/Models/`
- New repositories follow patterns in `Laks.Web/Data/Repositories/`
- Razor markup and PageModel logic are properly separated

### Code Quality
- Clear, descriptive names (no cryptic abbreviations)
- No unnecessary complexity; DRY where it matters
- No dead code or commented-out blocks

### Tests
- New behaviour has corresponding tests in `Laks.Web.Tests/Unit/`
- Tests are deterministic (no `DateTime.Now`, no random values)
- Edge cases and error conditions covered

### Performance
- No N+1 query patterns
- Expensive aggregation paths reviewed for efficiency
- Disposables and DB connections properly cleaned up

---

## Output Format

### Summary
[Overall assessment — what changed and quality signal]

### Critical Issues
[Must fix — security vulnerabilities, data integrity problems, broken behaviour]

### Danish Language Issues
[Any user-facing text not in Danish]

### Suggestions
[Improvements to quality, clarity, test coverage — not blocking]

### Positive Observations
[Good patterns worth noting]
