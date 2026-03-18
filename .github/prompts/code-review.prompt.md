---
agent: code-reviewer
model: gpt-4
tools: ['file-search', 'semantic-search', 'changes', 'problems']
description: 'Structured code review workflow with validation gates'
---
# Code Review Workflow

## Context Loading
1. Review [global instructions](../../.github/copilot-instructions.md)
2. Review [backend instructions](../../.github/instructions/backend.instructions.md)
2. Review [frontend instructions](../../.github/instructions/frontend.instructions.md)
3. Check changed files and context
4. Analyze existing issues and warnings
5. Verify adherence to: Follow standard formatting
6. Check security requirements: Follow OWASP top 10
7. Verify Razor Pages conventions (.cshtml + .cshtml.cs) and repository pattern consistency

## Review Checklist
### Code Quality
- [ ] Code follows project style guidelines
- [ ] Handlers, repositories, and models have clear, single responsibilities
- [ ] Variable and function names are descriptive
- [ ] No unnecessary complexity or over-engineering
- [ ] Code is DRY (Don't Repeat Yourself)
- [ ] Razor markup and PageModel logic are properly separated

### Security
- [ ] No hard-coded credentials or secrets
- [ ] Input validation is present
- [ ] No SQL injection vulnerabilities
- [ ] Authentication/authorization checks in place
- [ ] Sensitive data is properly handled
- [ ] Database access uses parameterized queries

### Testing
- [ ] Unit tests cover new/modified code
- [ ] Edge cases are tested
- [ ] Tests are meaningful and not just for coverage
- [ ] Integration tests updated if needed
- [ ] PageModel tests cover validation and handler behavior

### Documentation
- [ ] Public APIs and page behavior are documented when needed
- [ ] Complex logic has explanatory comments
- [ ] README updated if needed
- [ ] CHANGELOG updated for user-facing changes

### Performance
- [ ] No obvious performance bottlenecks
- [ ] Database queries are optimized
- [ ] No N+1 query problems
- [ ] Resource cleanup (connections, files) is handled
- [ ] Expensive query or chart aggregation paths are reviewed for efficient execution

## Deterministic Requirements
- Search codebase for similar patterns
- Locate related test files
- Check for consistent patterns across the project

## Structured Output
Provide review feedback in the following format:

### Summary
[High-level assessment of the changes]

### Critical Issues
[Issues that must be fixed before merging]

### Suggestions
[Recommended improvements]

### Positive Observations
[Good patterns or improvements worth noting]

## Human Validation Gate
🚨 **STOP**: Review feedback before posting.
Confirm: Feedback is constructive, specific, and actionable.
