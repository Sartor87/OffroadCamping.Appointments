---
paths:
  - src/Modules/Identity/**
  - tests/**/Identity*
---

# Authentication Module Rules

This module handles authentication and authorization.

## Patterns
- Use ASP.NET Core Identity for user management
- JWT tokens for API authentication
- Refresh token rotation enabled
- Password rules defined in IdentitySettings

## Testing
- Mock IIdentityService for unit tests
- Use test users defined in IdentityTestData