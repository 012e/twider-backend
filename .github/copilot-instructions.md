# Copilot Instructions for LLMs

## Project Overview
This is a modern, production-grade backend for a Twitter-like social platform, written in C# (.NET 8, ASP.NET Core). It features modular architecture, CQRS, MediatR, Entity Framework Core, and integrates with PostgreSQL, Keycloak (OIDC), MinIO (object storage), Qdrant (vector DB), NATS (messaging), and an ML-powered semantic search service. The project is containerized with Docker Compose for local development.

## Key Directories
- `Backend/` — Main backend API and business logic
  - `Features/` — Modular features (Post, Comment, User, Media, Search, Reaction, Health)
  - `Common/` — Shared helpers, configuration, middlewares, services, and EF Core models
  - `Data/` — Sample data, scripts, and assets
  - `Scripts/` — SQL schema/init scripts
  - `appsettings*.json` — Environment configuration
  - `compose.yaml`, `Dockerfile` — Containerization
- `BackendTest/` — Integration and feature tests
- `Twitter/` — (Optional) Additional app or service

## Main Technologies
- **.NET 8** (ASP.NET Core, MediatR, Entity Framework Core)
- **PostgreSQL** (database)
- **Keycloak** (authentication/authorization)
- **MinIO** (object storage)
- **Qdrant** (vector search)
- **NATS** (messaging)
- **OpenAI** (optional, for ML features)
- **Docker Compose** (local orchestration)

## Core Concepts
- **CQRS**: Command/Query separation via MediatR
- **Modular Endpoints**: Each feature exposes endpoints via `Routes.cs`
- **Cursor-based Pagination**: Used for scalable feeds/search
- **ML Search**: `/search/posts` endpoint uses hybrid semantic/keyword search via external ML service
- **Strong Typing**: DTOs, validation, and error handling throughout
- **Config via `appsettings.json`**: All secrets and endpoints are injected/configured

## How to Use/Extend
- Add new features in `Backend/Features/<FeatureName>`
- Add new endpoints in `Routes.cs` of each feature
- Use MediatR for business logic (Handlers, Commands, Queries)
- Add new DB models in `Common/DbContext/`
- Update `Scripts/init.sql` for schema changes
- Use `appsettings.Development.json` for local secrets/config
- Use Docker Compose (`compose.yaml`) to run all dependencies

## Environment Variables/Secrets
- All sensitive config (DB, Keycloak, MinIO, Qdrant, OpenAI) is set via `appsettings*.json` or Docker Compose env vars
- Never hardcode secrets in code

## Testing
- Tests live in `BackendTest/` and use integration patterns
- Use `IntegrationTestFactory` for test server setup

## LLM/AI Usage Guidance
- **Be concise and accurate**: Use correct types, validate input, and follow CQRS
- **Respect modularity**: Place new logic in the correct feature/module
- **Follow existing patterns**: Use MediatR, DTOs, and error handling as shown
- **Do not leak secrets**: Never output or log sensitive config
- **Document endpoints and models**: Use XML/Swagger comments where possible
- **Support cursor-based pagination**: For all list/search endpoints
- **Use dependency injection**: For all services/clients

## Useful References
- `Backend/Program.cs` — Main entrypoint and DI setup
- `Backend/compose.yaml` — All service dependencies
- `Backend/Scripts/init.sql` — Full DB schema
- `Backend/Common/Configuration/` — All config objects
- `Backend/Common/Helpers/Types/` — Pagination, result, and utility types

## How to implement a feature

- Step 1: Read all related code to what the user intended to create You are a competent programmer, competent programmers are not lazy.
- Step 2: Read the related data layer setup (EF Core used) at Common/DbContext
- Step 2: Implement a command handler (Queries for reads or  Commands for mutations) in the folder Features/<Feature group name>/<Command name>Handler
- Step 3: Implement a minimal Route to expose the created feature inside Features/<Feature group name>/Routes.cs
- Step 4: Take a look at other integration test files to learn the coding guideline and write a exhausive integration test for the feature. The tests must cover all possible cases

---
**This file is for LLMs and Copilot tools. Be exhaustive, concise, and always follow the project’s architecture and best practices.**

