# Loan Management System (Take-Home)

This repository contains:

- `backend/`: .NET 6 Web API (EF Core + SQL Server)
- `frontend/`: Angular app
- `compose.yaml`: Docker Compose to run the full stack locally

## Quickstart (Docker)

From the repository root:

```bash
docker compose up --build
```

Services:

- API: `http://localhost:8080`
- Swagger UI: `http://localhost:8080/swagger`
- Health endpoint: `http://localhost:8080/health`
- Frontend: `http://localhost:4200`

Notes:

- The API waits for the DB to be healthy.
- The frontend waits for the API to be healthy.

To stop and remove volumes:

```bash
docker compose down -v
```

## API Authentication (API Key)

Write endpoints are protected by an API key:

- `POST /loans`
- `POST /loans/{id}/payment`

Send this header:

- `X-Api-Key: <your-key>`

Default in Docker Compose:

- `API_KEY=dev-api-key`

In Swagger:

- Open `http://localhost:8080/swagger`
- Click **Authorize**
- Enter the API key value for the `X-Api-Key` header

## Configuration

### Environment variables (Docker Compose)

- `MSSQL_SA_PASSWORD`
  - Default: `Password123!`
- `API_KEY`
  - Default: `dev-api-key`

### Seed data

The API can seed initial loans (safe/idempotent seeding). Configuration keys:

- `SeedData:Enabled`
- `SeedData:Reseed`

Defaults are development-friendly; you can override via environment variables if needed.

## Testing

### Option A: Local (requires .NET 6 SDK/runtime)

From `backend/src`:

```bash
dotnet test
```

### Option B: Docker (.NET 6 SDK container)

From repo root:

```bash
docker run --rm --network take-home-test_default -v "$(pwd):/repo" -w /repo/backend/src mcr.microsoft.com/dotnet/sdk:6.0 dotnet test
```

## Helpful REST client file

If you use the VS Code REST Client extension, you can run requests from:

- `backend/src/Fundo.Applications.WebApi/loans.http`

## CI

A GitHub Actions workflow is included:

- `.github/workflows/backend-ci.yml`

It builds and tests the backend on PRs and pushes to `main`.
