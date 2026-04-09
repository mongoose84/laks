---
name: start-jumbo-goal
description: Use when starting a Jumbo goal. Loads goal context, scope, and architectural constraints, then guides the agent through disciplined execution within defined boundaries.
---

# Start Jumbo Goal

**Prompt:** Start a Jumbo goal by loading its full context — objective, criteria, scope, architecture, components, decisions, invariants, and guidelines — then execute within the defined boundaries.

## Why Structured Implementation Matters

When `jumbo goal start` is run, it assembles the goal's registered relations into structured implementation instructions. The objective defines what to build. The criteria define how success is measured. The scope defines boundaries. The architectural context (components, decisions, invariants, guidelines) defines constraints. Deviating from these instructions produces work that fails QA review.

## Protocol

### 1. Start the Goal

```bash
jumbo goal start --id <goal-id>
```

This loads the full goal context. Read and internalize every section of the output before writing any code.

### 2. Understand the Implementation Instructions

The goal start output contains structured sections. Each section carries binding instructions:

#### Objective
The single purpose of this goal. All work must serve this objective.

#### Success Criteria
The specific, measurable conditions that determine whether the objective is met. Implementation is not complete until every criterion is satisfied.

#### Current Progress (if resuming)
If the goal was previously started, review what has already been accomplished and continue from there. Do not redo completed work.

#### Scope
- **In Scope**: Files and areas you MUST work within.
- **Out of Scope**: Files and areas you MUST NOT touch.

#### Solution Architecture
- **Organization Style**: Namespaces (directory structures) and file names you introduce MUST maintain this style.
- **Design Patterns**: You MUST leverage these patterns where applicable. If the goal requires a pattern not listed, register it with `jumbo architecture update`.
- **Principles**: Artifacts you create MUST directly reflect these principles.

#### Relevant Components
Components that will be modified, created, or depended upon. Consider these while implementing.

#### Relevant Dependencies
External libraries involved in the implementation. Consider version constraints and contracts.

#### Relevant Decisions
Previous architectural decisions that inform or constrain this change. The solution must remain consistent with these decisions.

#### Invariants
Non-negotiable constraints. You MUST adhere to ALL invariants while implementing.

#### Guidelines
Coding standards, testing requirements, and process practices. You SHOULD follow these while implementing.

### 3. Execute Within Scope

- Work only within the defined scope boundaries.
- Follow all architectural constraints, invariants, and guidelines.
- Track progress by documenting completed sub-tasks:

```bash
jumbo goal update-progress --id <goal-id> --task-description "<description>"
```

### 4. Maintain Context as You Implement

Register decisions, components, and corrections as they happen — not as a cleanup step after implementation.

- **Architectural decision made** (e.g., chose pattern X over Y):
  ```bash
  jumbo decision add --title "Chose X over Y" --rationale "Because Z" --context "Background"
  ```

- **New component created** (e.g., introduced a new service or builder):
  ```bash
  jumbo component add --name "ComponentName" --description "What it does"
  jumbo relation add --from-type goal --from-id <goal-id> --to-type component --to-id <component-id> --type involves --description "Created during implementation"
  ```

- **User corrects your approach** (e.g., "don't do X, do Y instead"):
  ```bash
  jumbo invariant add --category "architecture" --description "Never do X because Y"
  jumbo guideline add --category "codingStyle" --description "Prefer X over Y"
  ```

Context registered now is served to future sessions. What you skip is lost.

### 5. Submit for Review

When all success criteria are met and implementation is complete:

```bash
jumbo goal submit --id <goal-id>
```

## Rules

1. **Never deviate from the objective.** All code changes must serve the stated objective.
2. **Never violate scope boundaries.** In-scope files are your workspace. Out-of-scope files are off limits.
3. **Never violate invariants.** Invariants are non-negotiable constraints — no exceptions.
4. **Always track progress.** Use `jumbo goal update-progress` to document completed sub-tasks so future agents can resume if needed.
5. **Always submit when done.** Do not close or codify the goal yourself. Submit it for QA review.
6. **Do not enter plan mode for refined goals.** The goal context IS the plan. Execute immediately.
