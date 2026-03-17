# CheckInSystem

Industrial-style full-stack sample project for:

- Admin Management System
- Check-in Website

## Tech Stack

- Backend: .NET 8, ASP.NET Core Web API, EF Core Code First, SQL Server
- Frontend: React + Vite + axios + React Router
- Auth: JWT
- Architecture: layered architecture (`Api / Application / Domain / Infrastructure`)

## Solution Structure

```text
CheckInSystem.sln
src/
  backend/
    CheckInSystem.Api/
      Controllers/
      Extensions/
      Middleware/
      Program.cs
    CheckInSystem.Application/
      Common/
      DTOs/
      Interfaces/
    CheckInSystem.Domain/
      Entities/
      Enums/
    CheckInSystem.Infrastructure/
      Data/
        Migrations/
      Repositories/
      Services/
  frontend/
    checkin-client/
      src/
        api/
        components/
        context/
        hooks/
        pages/
        router/
        utils/
```

## Core Features

- User CRUD, enable/disable, role assignment
- Role CRUD and permission assignment
- RBAC permission-based authorization on backend APIs
- JWT admin login
- Per-user single active check-in token
- Token regeneration revokes old token automatically
- Check-in endpoint validates:
  - JWT signature
  - expiration
  - revoked/superseded status
  - active linked user
- Check-in accepts token from request body or `Authorization: Bearer`
- Check-in audit records with filters
- Unified API response structure
- Global exception handling middleware
- Swagger support

## Database Tables

- `Users`
- `Roles`
- `Permissions`
- `RolePermissions`
- `UserRoles`
- `UserTokens`
- `CheckInRecords`

## Default Seed Data

Application startup seeds:

- 1 admin user
- 2 roles: `Administrator`, `Attendee`
- 7 permissions

Default admin account:

- Username: `admin`
- Password: `Admin123!`

## Backend Run

Requirements:

- SQL Server local instance available on `localhost`
- .NET SDK 8 installed

Commands:

```powershell
dotnet restore CheckInSystem.sln
dotnet build CheckInSystem.sln
dotnet ef database update --project src\backend\CheckInSystem.Infrastructure\CheckInSystem.Infrastructure.csproj --startup-project src\backend\CheckInSystem.Api\CheckInSystem.Api.csproj
dotnet run --project src\backend\CheckInSystem.Api\CheckInSystem.Api.csproj
```

API default URL:

- `http://localhost:5246`
- Swagger: `http://localhost:5246/swagger`

Connection string is in:

- `src/backend/CheckInSystem.Api/appsettings.json`

## Frontend Run

Requirements:

- Node.js 20+

Commands:

```powershell
cd src\frontend\checkin-client
npm install
npm run dev
```

Frontend default URL:

- `http://localhost:5173`

Optional API base URL override:

```powershell
$env:VITE_API_BASE_URL="http://localhost:5246/api"
npm run dev
```

## Unified API Response

```json
{
  "success": true,
  "message": "Success",
  "code": 200,
  "data": {}
}
```

## Global Exception Handling

`ExceptionHandlingMiddleware` handles:

- `ValidationException`
- `UnauthorizedAccessException`
- `KeyNotFoundException`
- `BusinessException`
- `Exception`

## Main Endpoints

- `POST /api/auth/login`
- `GET /api/users`
- `POST /api/users`
- `GET /api/roles`
- `POST /api/roles`
- `POST /api/tokens/users/{userId}/generate`
- `GET /api/tokens/users/{userId}/current`
- `GET /api/tokens/users/{userId}`
- `POST /api/checkin`
- `GET /api/checkinrecords`

## Verified Locally

- `dotnet build CheckInSystem.sln`
- `dotnet ef migrations add InitialCreate`
- `dotnet ef database update`
- `npm run build`

Database verification after startup:

- `Users = 1`
- `Roles = 2`
- `Permissions = 7`

Frontend currently includes:

- login
- user create/edit/delete, enable/disable, role assignment
- role create/edit/delete
- token generation and history
- check-in page with latest token reuse
- check-in record filtering by user, status, time range

## Automated Tests

Commands:

```powershell
dotnet test CheckInSystem.sln
```

Current test coverage includes:

- `TokenService.GenerateAsync`
- `CheckInService.CheckInAsync` failure path
- `CheckInService.CheckInAsync` success path

## Docker Run

Requirements:

- Docker Desktop

Commands:

```powershell
docker compose up --build
```

Docker endpoints:

- Frontend: `http://localhost:5173`
- Backend Swagger: `http://localhost:5246/swagger`
- SQL Server: `localhost,1433`

Docker notes:

- Frontend container uses `nginx` and proxies `/api` to the backend container.
- Backend startup includes retry logic for initial database migration/seed.
- SQL Server default SA password in compose is `YourStrong!Passw0rd`.
