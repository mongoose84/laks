---
name: feature-implementor-skills
description: "Use when implementing a feature from a completed spec in this ASP.NET Core Razor Pages project. Orchestrates architect, backend-engineer, frontend-engineer, tester, and code-reviewer with quality gates and fix loops."
---

# Feature Implementor Skill

## Purpose
Use this skill after a feature has been documented and approved.
It coordinates subagents in a deterministic sequence so work is planned, implemented, verified, and reviewed before completion.

## Project Context
- Stack: ASP.NET Core Razor Pages (.cshtml + .cshtml.cs)
- Backend: C# on .NET with repository pattern
- Data: MySQL with parameterized queries
- Tests: tests/Laks.Web.Tests

## Inputs Required
- Feature spec path under .github/specs
- Definition of done (acceptance criteria)
- Scope constraints (in scope / out of scope)

## Output
- Implemented feature aligned with spec
- Updated tests covering changed behavior
- Review findings addressed or explicitly tracked
- Final implementation summary with changed files

## Recommended Orchestration Model
You do not need a separate orchestrator agent to start.
This skill itself should orchestrate the workflow and call specialized subagents.

Create a dedicated orchestrator agent only if one or more of these are true:
- You want strict tool restrictions per phase.
- You want reusable delivery governance across many repositories.
- You need compliance gates beyond normal review and testing.

## Agent Sequence

### Step 1: Architecture Breakdown
Agent: architect

Goals:
- Break feature into backend, frontend, data, and test work items.
- Identify risks, dependencies, migration needs, and non-functional constraints.
- Produce a phased implementation plan with acceptance checks.

Required deliverable:
- Task breakdown with ordered milestones and risk notes.

Contract freeze output (required before implementation):
- Backend-frontend interface contract (routes, handler names, request/response shape).
- Validation and error-state contract.
- File ownership map (which agent edits which files).

### Step 2: Parallel Implementation Phase
Agents: backend-engineer and frontend-engineer (in parallel)

Parallel preconditions:
- Architect contract freeze is complete.
- File ownership map avoids overlapping edits.
- Any shared model contract is explicitly versioned or documented.

#### Step 2A: Backend Implementation
Agent: backend-engineer

Goals:
- Implement domain, repository, service/API, and validation changes.
- Keep SQL access parameterized and aligned with OWASP practices.
- Add or update backend tests.

Required deliverable:
- Backend code changes plus test updates and implementation notes.

#### Step 2B: Frontend Implementation
Agent: frontend-engineer

Goals:
- Implement Razor page and PageModel changes required by spec.
- Keep server-rendered-first approach and minimal JavaScript.
- Ensure accessibility and clear error/empty states.

Required deliverable:
- Frontend code changes plus relevant tests and UX notes.

### Step 3: Sync Gate (Post-Parallel)
Owner: orchestrating skill execution

Goals:
- Reconcile integration points after parallel work.
- Confirm contracts still match implementation.
- Resolve any cross-agent conflicts before verification.

Required checks:
- Route and handler names match contract.
- Payload and validation behavior match contract.
- Error and empty states are consistent across backend/frontend.

### Step 4: Verification
Agent: tester

Goals:
- Verify acceptance criteria against implemented behavior.
- Validate regression risk in touched areas.
- Confirm tests are deterministic and meaningful.

Required deliverable:
- Verification report with pass/fail criteria and defects.

### Step 5: Code Review
Agent: code-reviewer

Goals:
- Review for correctness, maintainability, security, and test adequacy.
- Prioritize issues by severity and identify required fixes.

Required deliverable:
- Findings list ordered by severity, with file references.

## Fix Loop Policy
If verification or review finds issues:
1. Route backend issues to backend-engineer.
2. Route Razor UI or PageModel issues to frontend-engineer.
3. Route cross-cutting design issues to architect.
4. If backend and frontend are both impacted, run fixes in parallel with a mini contract freeze.
5. Re-run sync gate checks.
6. Re-run tester after fixes.
7. Re-run code-reviewer for final sign-off.

Repeat until:
- Acceptance criteria are met.
- Tests pass.
- No critical review findings remain.

## Guardrails
- Do not start implementation without a feature spec.
- Keep changes scoped to approved feature boundaries.
- Avoid unrelated refactors unless required for correctness.
- Keep commits/review units small and traceable.

## Suggested Invocation Prompt
Use this skill when you say:
- Implement feature from spec at [path]
- Run full feature delivery workflow
- Plan, build, verify, and review this feature

## Assumptions To Confirm At Start
- Spec is approved and stable enough to implement.
- Database migration strategy is agreed (if schema changes are needed).
- Expected rollout strategy is known (feature flag, direct release, etc.).
