# Local Development Setup

Database: **SQL Server 2025** (or Azure SQL Database). SQL Server 2025 is required because the `vector` data type used for AI knowledge retrieval does not exist in earlier versions (ADR D-016). Docker is optional.

## Route A (recommended on Windows): native SQL Server 2025 Developer edition
1. Install **SQL Server 2025 Developer** (free for development and testing only, never for production) from Microsoft. Choose the Database Engine, **Mixed Mode** authentication (the `doubleus_*` logins are SQL logins), and the **default instance** (it listens on TCP 1433; named instances such as SQLEXPRESS use dynamic ports and would need a fixed port set in SQL Server Configuration Manager).
2. Install `sqlcmd` (or SSMS / Azure Data Studio) to run scripts.
3. Create the database and logins (replace both passwords):
```powershell
sqlcmd -S localhost -E -C -i infrastructure\scripts\local-sqlserver-setup.sql -v OwnerPassword="<owner-password>" AppPassword="<app-password>"
```
The last query it runs should show `doubleus_app` and `IsDbOwner = 0`.
4. Check the application login works:
```powershell
sqlcmd -S localhost -U doubleus_app -P "<app-password>" -d DoubleUs -C -Q "SELECT USER_NAME(), IS_MEMBER('db_owner')"
```
Redis is **not needed in Phase 1** (it is optional in the health check).

## Route B (optional): Docker
```powershell
Copy-Item .env.example .env      # set ACCEPT_EULA=Y after reading the license, and replace every change-me-* value
docker compose up -d
```
Then run the same setup script against `localhost,1433` using `-U sa -P "<MSSQL_SA_PASSWORD>"` instead of `-E`. `docker-compose.yml` is unverified. Ports bind to `127.0.0.1` only.

## Logins
| Login | Purpose |
|---|---|
| `sa` / Windows admin | Used only to run the setup script. The app never uses it. |
| `doubleus_owner` | `db_owner` of `DoubleUs`; migration step only (DDL, security policies). |
| `doubleus_app` | Runtime login: DML only on `dbo`; must not be able to alter policies or objects. Required so Row-Level Security cannot be bypassed by the application (docs/security/TENANT_ISOLATION.md). |

## Health endpoints (API)
| Endpoint | Meaning | Failure |
|---|---|---|
| `GET /health/live` | Process is up; checks nothing external | n/a |
| `GET /health/ready` | Database TCP port reachable (required); Redis reachable (optional unless `Dependencies:Redis:Required=true`) | 503 `Unhealthy`; an unreachable optional Redis reports `Degraded` with HTTP 200 |

Responses contain only the status word. **Limitation:** the readiness probe is TCP-connectivity only (no login, no query); Phase 1 task 3 replaces the database probe with a real EF Core check.
Locations are configuration (`Dependencies:Database:Host/Port`, `Dependencies:Redis:Host/Port/Required`), overridable by env vars such as `Dependencies__Database__Port`.

Run the API: `dotnet run --project src/DoubleUs.API`, then open `/health/ready` on the port shown in the console.
