---
applyTo: "**/{test,tests}/**"
description: "Testing guidelines with context engineering"
---
# Testing Guidelines

Inherits from [global instructions](../copilot-instructions.md).

## Context Loading
Review [project conventions](../../README.md) and
[existing tests](../../) before writing tests.

## Deterministic Requirements
- Follow the AAA pattern: Arrange, Act, Assert
- Write descriptive test names that explain the scenario
- Mock external dependencies — keep tests isolated
- Ensure tests are deterministic and repeatable
- Cover both happy paths and error conditions
- Unit and Integration tests
- Follow standard formatting

## Structured Output
Generate tests with:
- [ ] Setup and teardown for shared state
- [ ] Edge case and error condition coverage
- [ ] Mock implementations for external dependencies
- [ ] Clear test documentation
