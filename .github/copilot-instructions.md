# Copilot Instructions

## Project Guidelines
- The user prefers multi-tenant data isolation: all domain tables should include a UserId so each authenticated user can only access their own data, except for PlayerRanking which should not have UserId filtering.