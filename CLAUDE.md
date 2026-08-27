# Project Instructions

This repository contains a tabletop RPG campaign-management application.

The full product specification is located at:

@docs/PRODUCT_SPEC.md

## Development Rules

- Follow the product specification unless I explicitly override it.
- Work one milestone at a time.
- Do not implement future features prematurely.
- Explain significant architectural decisions before implementing them.
- Prefer simple, maintainable solutions over unnecessary abstraction.
- Backend: C# / ASP.NET Core / Entity Framework Core / PostgreSQL.
- Frontend: Next.js / React / TypeScript.
- Backend business logic should not live in controllers.
- Use DTOs rather than returning Entity Framework entities directly.
- Enforce campaign ownership server-side.
- Keep the frontend componentized and reusable.
- Write tests for important business logic.
- Do not implement battle maps, quest graphs, encounters, AI features, or player permissions until their milestone is reached.