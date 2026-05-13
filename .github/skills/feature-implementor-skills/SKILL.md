---
name: feature-implementor-skills
description: 'Use when implementing a feature from a completed spec in this ASP.NET Core Razor Pages project. Orchestrates architect, fullstack-dotnet, tester, and code-reviewer with quality gates and fix loops.'
---

# Feature Implementor Skill

## Purpose
Use this skill after a feature has been documented and approved.
It coordinates subagents in a deterministic sequence so work is planned, implemented, verified, and reviewed before completion.

## Project Context
- Stack: ASP.NET Core Razor Pages (.cshtml + .cshtml.cs)
- Backend: C# on .NET with repository pattern
- Data: MySQL with parameterized queries
- Tests: Laks.Web.Tests

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

### Step 2: Implementation
Agent: fullstack-dotnet

Preconditions:
- Architect contract freeze is complete.
- File ownership map is defined.
- Shared model contract is explicitly versioned or documented.

Goals:
- Implement backend and frontend changes required by the spec.
- Keep SQL access parameterized and aligned with OWASP practices.
- Keep server-rendered-first approach and minimal JavaScript.
- Ensure accessibility and clear error and empty states.
- Add or update tests in Laks.Web.Tests.

Required deliverable:
- Implementation changes plus updated tests and implementation notes.

### Step 3: Sync Gate
Owner: orchestrating skill execution

Goals:
- Validate integration points before verification.
- Confirm contracts still match implementation.
- Resolve any cross-area conflicts.

Required checks:
- Route and handler names match contract.
- Payload and validation behavior match contract.
- Error and empty states are consistent across backend and frontend.

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
1. Route implementation issues to fullstack-dotnet.
2. Route cross-cutting design issues to architect.
3. Re-run sync gate checks.
4. Re-run tester after fixes.
5. Re-run code-reviewer for final sign-off.

Repeat until:
- Acceptance criteria are met.
- Tests pass.
- No critical review findings remain.

## Guardrails
- Do not start implementation without a feature spec.
- Keep changes scoped to approved feature boundaries.
- Avoid unrelated refactors unless required for correctness.
- Keep commits and review units small and traceable.

## Suggested Invocation Prompt
Use this skill when you say:
- Implement feature from spec at [path]
- Run full feature delivery workflow
- Plan, build, verify, and review this feature

## Assumptions To Confirm At Start
- Spec is approved and stable enough to implement.
- Database migration strategy is agreed (if schema changes are needed).
- Expected rollout strategy is known (feature flag, direct release, etc.).
