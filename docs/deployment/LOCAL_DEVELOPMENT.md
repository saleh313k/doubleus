# Local Development Setup

Database: **SQL Server 2025** (or Azure SQL Database). SQL Server 2025 is required because the `vector` data type used for AI knowledge retrieval does not exist in earlier versions (ADR D-016). Docker is optional.

## Route A (recommended on Windows): native SQL Server 2025 (Developer or Express)
1. Install **SQL Server 2025 Developer** (free, dev/test only) or **Express** (free, size/resource limits). Choose the Database Engine and **Mixed Mode** authentication (the `doubleus_*` logins are SQL logins). Express installs as the **named instance `SQLEXPRESS`** by default, so connect with `-S localhost\SQLEXPRESS`; the default instance uses `-S localhost`.
2. Install `sqlcmd` (or SSMS / Azure Data Studio) to run scripts.
3. Create the database and logins (use two different passwords):
```powershell
sqlcmd -S localhost\SQLEXPRESS -E -C -i infrastructure\scripts\local-sqlserver-setup.sql -v OwnerPassword="<owner-password>" AppPassword="<app-password>"
```
The last query it runs should show `doubleus_app` and `IsDbOwner = 0`.
4. **Enable SQL logins if the instance is Windows-authentication-only** (check: `SELECT SERVERPROPERTY('IsIntegratedSecurityOnly')` returns 1). The script still creates the logins in that mode, but they cannot connect until you switch:
```powershell
sqlcmd -S localhost\SQLEXPRESS -E -C -Q "EXEC xp_instance_regwrite N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\MSSQLServer', N'LoginMode', REG_DWORD, 2;"
Restart-Service 'MSSQL$SQLEXPRESS'
```
5. **Fixed TCP port 1433** (named instances use a dynamic port by default, which the API health check cannot find): SQL Server Configuration Manager, SQL Server Network Configuration, Protocols for SQLEXPRESS, enable **TCP/IP**; in its properties, IP Addresses tab, **IPAll**: clear "TCP Dynamic Ports", set "TCP Port" to `1433`. Then `Restart-Service 'MSSQL$SQLEXPRESS'` and check `Test-NetConnection localhost -Port 1433`.
6. Check the application login (over TCP) and that it is least-privilege:
```powershell
sqlcmd -S tcp:localhost,1433 -U doubleus_app -P "<app-password>" -d DoubleUs -C -Q "SELECT USER_NAME(), IS_MEMBER('db_owner')"
sqlcmd -S tcp:localhost,1433 -U doubleus_app -P "<app-password>" -d DoubleUs -C -Q "CREATE TABLE dbo.must_fail (id int)"
```
The first returns `doubleus_app` and `0`; the second must fail with a permission-denied error (the runtime login cannot create objects).
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
