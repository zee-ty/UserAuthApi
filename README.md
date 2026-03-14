# User Authentication API (C#)

ASP.NET Core 8 Web API: register, login, logout, and current user profile. Uses PostgreSQL and JWT. Backend only.

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (for local run)
- Docker & Docker Compose (easiest way to run everything)

## Quick start

Clone the repo, then:

```bash
cd UserAuthApi
docker compose up --build
```

- API: http://localhost:5050  
- Swagger: http://localhost:5050/swagger  

Migrations run on startup, so the database is ready. Default config (see `appsettings.json` and `docker-compose.yml`) works for local use. **For production**, set a strong `Jwt__Secret` (at least 32 characters) via environment.

## Run locally (without Docker)

1. Run PostgreSQL port 5432 with database `userauth`, user `userauth`, password `userauth`.
2. From the repo root: `dotnet run`
3. Open http://localhost:5000 (or the URL in `launchSettings.json`) and http://localhost:5000/swagger.

## Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/health` | Health check |
| GET | `/api/health/version` | API version |
| POST | `/api/auth/register` | Register (body: email, password, firstName, lastName) |
| POST | `/api/auth/login` | Login (body: email, password). Returns a token. |
| POST | `/api/auth/logout` | Logout (client discards token) |
| GET | `/api/user` | Current user (firstName, lastName, email). **Requires header:** `Authorization: Bearer <token>` |

## Example

```bash
# Register
curl -X POST http://localhost:5050/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"SecureP@ss123","firstName":"Test","lastName":"User"}'

# Login (copy the "token" from the response)
curl -X POST http://localhost:5050/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"SecureP@ss123"}'

# Get current user (use the token from login)
curl -X GET http://localhost:5050/api/user \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

# Adding the API to Postman

1. In Postman, open **Environments** tab.
2. Create an environment and name it UserAuthApi Local.
3. Add a variable:
   - **Variable:** `baseUrl`
   - **Initial / Current value:** `http://localhost:5050`
4. Save and select this environment in the top-right dropdown.

Then in the collection, set each request URL to `{{baseUrl}}/api/...`

## 3. Using protected endpoints such as GET /api/user

**Manual token**

1. Send **POST** `{{baseUrl}}/api/auth/login` (or **register**) with a JSON body, for example :
   ```json
   {
     "email": "you@example.com",
     "password": "yourpassword"
   }
   ```
2. From the response, copy the `token` value.
3. For **GET** `{{baseUrl}}/api/user` (and any other protected request):
   - Open the request → **Authorization**.
   - Type: **Bearer Token**.
   - Token: paste the copied token.

**Collection auth (Bearer token for whole collection)**

1. Right click the collection → **Edit**.
2. **Authorization** tab → Type: **Bearer Token**.
3. Token: paste the token (or use a variable, for example `{{token}}`).
4. Add an environment variable `token` and set it to the token after login; then use `{{token}}` in the collection auth.

After that, all requests in the collection will send the token.

## 4. Endpoints summary

| Method | Path | Auth | Body (JSON) |
|--------|------|------|-------------|
| GET | `/api/health` | No | — |
| GET | `/api/health/version` | No | — |
| POST | `/api/auth/register` | No | `email`, `password`, `firstName`, `lastName` |
| POST | `/api/auth/login` | No | `email`, `password` |
| POST | `/api/auth/logout` | No | — |
| GET | `/api/user` | **Bearer token** | — |

Once the collection is imported and the base URL and optional token are set, you can run all of these from Postman.
