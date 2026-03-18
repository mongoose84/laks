# API Endpoint: [Endpoint Name]

## Overview
**Purpose**: [What this endpoint does]

**Endpoint**: `[METHOD] /api/v1/[resource]`

**Stack**: c# with .NET

## Authentication
- **Required**: [Yes/No]
- **Method**: [JWT/API Key/OAuth2/None]
- **Permissions**: [Required roles]

## Request

### URL Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | integer | Yes | [Description] |

### Request Body
```json
{
  "field1": "string",
  "field2": 123
}
```

### Validation
| Field | Type | Required | Rules | Description |
|-------|------|----------|-------|-------------|
| `field1` | string | Yes | Max 255 | [Description] |

## Response

### Success (200 OK)
```json
{
  "success": true,
  "data": {
    "id": 123,
    "field1": "value"
  }
}
```

### Error Responses
- **400 Bad Request**: Invalid input
- **401 Unauthorized**: Missing/invalid auth
- **404 Not Found**: Resource not found
- **500 Internal Error**: Server error

## Implementation

**Files**:
- Controller: `[file path]`
- Service: `[file path]`
- Model: `[file path]`
- Tests: `[file path]`

**Logic Flow**:
1. Validate input
2. Check authentication/authorization
3. [Business logic steps]
4. Return response

## Testing
- [ ] Valid request returns 200
- [ ] Invalid input returns 400
- [ ] Unauthorized returns 401
- [ ] Edge cases handled

## Security
**Project Requirements**: Follow OWASP top 10

- [ ] Input validation
- [ ] SQL injection prevention
- [ ] Authentication checks
- [ ] Rate limiting
