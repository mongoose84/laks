---
applyTo: "**/*.cs"
description: "c# backend development guidelines with context engineering"
---
# c# Backend Development Guidelines

Inherits from [global instructions](../copilot-instructions.md).

## Context Loading
Review [project dependencies](../../) and
[application structure](../../) before starting.

## Deterministic Requirements
- Follow c# best practices and idioms
- Follow .NET patterns and conventions
- All user-facing strings in C# models and services must be in Danish
- Implement structured logging with appropriate levels
- Use proper HTTP status codes and error responses
- RESTful API design
- Follow standard formatting
- Follow OWASP top 10

## Structured Output
Generate code with:
- [ ] Comprehensive error handling
- [ ] Unit tests with appropriate framework
- [ ] Package/module documentation
- [ ] Integration tests for API endpoints
- [ ] Graceful shutdown handling
