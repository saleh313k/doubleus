# Handoff: implementing Phase 1 Task 3 with Claude Code (local)

Why: the planning environment cannot reach NuGet or the owner's SQL Server, so it cannot compile or test EF Core code. Claude Code runs in the local clone with real `dotnet` and the local database. The repository docs are the shared memory.

## 1. Get a proper git clone (owner, once)
Your current folder came from zip files and has no history. Make a real clone from the delivered bundle:
```powershell
cd D:\Projects
Rename-Item DoubleUs DoubleUs-old           # keep as a backup (copy any .env from it later)
git clone <path-to>\doubleus-task3-plan.bundle DoubleUs   # use the latest bundle you received
cd DoubleUs
git remote set-url origin https://github.com/saleh313k/doubleus.git
git branch docs/phase-0-planning origin/docs/phase-0-planning
git branch                                   # feature/backend-foundation should be checked out
git log --oneline -5
```
Optional, to back the work up on GitHub (does not touch main; needs your GitHub login):
```powershell
git push -u origin docs/phase-0-planning feature/backend-foundation
```
Do not merge anything into `main` without telling Claude and reviewing it yourself.

## 2. Test connection strings (owner, each PowerShell session, or set in user env vars)
Use your real passwords; never commit them.
```powershell
$env:DOUBLEUS_TEST_OWNER_CONNECTION = "Server=localhost\SQLEXPRESS;Database=DoubleUs;User Id=doubleus_owner;Password=<owner-password>;Encrypt=True;TrustServerCertificate=True"
$env:DOUBLEUS_TEST_APP_CONNECTION   = "Server=localhost\SQLEXPRESS;Database=DoubleUs;User Id=doubleus_app;Password=<app-password>;Encrypt=True;TrustServerCertificate=True"
```
Check once: `sqlcmd -S localhost\SQLEXPRESS -U doubleus_app -P "<app-password>" -d DoubleUs -C -Q "CREATE TABLE dbo.must_fail (id int)"` must fail with a permission error.

## 3. Start Claude Code in the repo
```powershell
cd D:\Projects\DoubleUs
claude
```
Paste this kickoff prompt:

> Read CLAUDE.md and follow its session-start routine (IMPLEMENTATION_STATUS.md, SESSION_STATE.md, git status, recent commits). Then implement Phase 1 Task 3 exactly as specified in docs/security/RLS_ROLLOUT_PLAN.md, which the owner has approved (D-017). Also respect D-009, D-016 and D-018 in docs/development/DECISIONS.md. Look up current NuGet versions yourself. Run `dotnet build DoubleUs.sln` and `dotnet test DoubleUs.sln` for real, including the SQL Server tests via the DOUBLEUS_TEST_* environment variables, and report actual results. Work in small commits on feature/backend-foundation. If the connection-pooling tests fail, stop and ask the owner which fallback to use before choosing. Do not start Task 4, do not add business tables, do not merge into main. When Task 3's Definition of Done is met, update SESSION_STATE.md and IMPLEMENTATION_STATUS.md, commit, and report exactly what was done, what was verified, and what is still open.

## 4. Come back here to review
Bring the Claude Code report (and any failing output) back to this chat for review and the Task 4 plan. Meta compliance items (Oman eligibility, Graph API v27.0 messaging review) stay open and are not part of Task 3.
