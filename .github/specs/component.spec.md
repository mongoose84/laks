# Razor UI Unit: [PageOrPartialName]

## Overview
**Purpose**: [What this page/partial solves for the user]

**Framework**: ASP.NET Core Razor Pages
**Language**: .cshtml + C# (.cshtml.cs when applicable)

**Type**: [ ] Razor Page | [ ] Partial View | [ ] Shared Layout Section

**Location**:
- Markup: `src/Laks.Web/Pages/[Path]/[Name].cshtml`
- PageModel (if page): `src/Laks.Web/Pages/[Path]/[Name].cshtml.cs`

## Inputs and Data Contract
**Route/Input Parameters**:
- `[parameter]`: [type, source, validation]

**PageModel Properties**:
- `[property]`: [type, purpose]

**Bound/Posted Fields** (if form):
- `[field]`: [type, validation attributes]

## Data Sources
- Repository/Service calls: `[interfaces/classes]`
- Query assumptions: `[sorting/filtering/paging]`
- Error and empty-state behavior: [description]

## Visual Structure
```
[Layout sketch]
┌─────────────────────────┐
│  Header                 │
├─────────────────────────┤
│  Main content           │
└─────────────────────────┘
```

## Behavior
**Primary User Flows**:
- [Action]: [Expected result]

**Server-Side Handlers**:
- `OnGet[Async]`: [description]
- `OnPost[Action][Async]`: [description]

**Client-Side Scripts** (if any):
- `[script function]`: [description]

## Accessibility
- [ ] Semantic HTML landmarks and headings are correct
- [ ] Form labels and validation messages are linked correctly
- [ ] Keyboard navigation works end-to-end
- [ ] Screen reader output is understandable

## Testing
- [ ] PageModel unit tests cover happy path and failures
- [ ] Validation behavior is tested
- [ ] Rendering/content assertions are verified where practical
- [ ] Edge cases (empty data, invalid input) are covered

## Implementation Checklist
- [ ] Razor markup created/updated
- [ ] PageModel created/updated (if applicable)
- [ ] Repository/service integration completed
- [ ] Validation and error handling implemented
- [ ] Styles and scripts updated only as needed
- [ ] Tests added/updated
- [ ] Documentation updated

## Usage and Navigation
- Route: `/[path]`
- Link entry points: `[where this page/partial is used]`
