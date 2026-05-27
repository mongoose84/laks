Run the full feature delivery workflow for the spec at: $ARGUMENTS

---

## Phase 1 — Architecture Breakdown
Invoke the @architect agent:

Read the feature spec at the path above. Produce:
1. Task breakdown: backend, frontend, data, and test work items with ordered milestones
2. Risk and dependency notes (including any migration needs)
3. Contract freeze:
   - Backend-frontend interface contract (routes, handler names, request/response shape)
   - Validation and error-state contract
   - File ownership map (which agent edits which files)

Do not proceed to Phase 2 until the contract freeze is complete and reviewed.

---

## Phase 2 — Implementation
Invoke the @fullstack-dotnet agent:

Using the architect's contract freeze and file ownership map, implement:
1. Backend changes (models, repositories, services, migrations if needed)
2. Frontend changes (Razor Pages .cshtml, PageModels, partials)
3. Tests in `Laks.Web.Tests/Unit/` covering the new behaviour

Confirm `dotnet build` passes before completing this phase.

---

## Phase 3 — Sync Gate
Before proceeding to verification, confirm all of:
- [ ] Route and handler names match the architect's interface contract
- [ ] Payload and validation behaviour match the validation contract
- [ ] Error and empty states are consistent across backend and frontend
- [ ] All user-facing text is in Danish
- [ ] `dotnet build` passes with no errors

---

## Phase 4 — Verification
Invoke the @tester agent:

Verify the implementation against the acceptance criteria in the spec. Run `dotnet test`. Produce:
- Pass/fail result per acceptance criterion
- Defect list with file and line references
- Confirmation that all tests are deterministic (no `DateTime.Now`, no random values)

---

## Phase 5 — Code Review
Invoke the @code-reviewer agent:

Review all changes for correctness, OWASP security (especially parameterized SQL), Danish language compliance, and test adequacy. Produce:
- Findings ordered by severity with file references
- Critical issues that must be resolved before merge

---

## Fix Loop
If Phase 4 or 5 finds issues:
1. Implementation bugs → route to @fullstack-dotnet
2. Design or contract issues → route to @architect, update contract freeze
3. Re-run sync gate checks
4. Re-run @tester after fixes
5. Re-run @code-reviewer for final sign-off

Repeat until:
- All acceptance criteria pass
- `dotnet test` passes with no failures
- No critical review findings remain
