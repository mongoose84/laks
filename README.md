# 🎣 LAKS – Norwegian Fishing Trip Records

A production-ready web application for tracking yearly salmon fishing results on the Numedalslågen river in Holmfoss, Norway.

## Recommended Stack (5 lines)

- **Backend:** ASP.NET Core 9 – Razor Pages (server-rendered, minimal JavaScript)
- **Data access:** Dapper + parameterized queries against an existing MySQL database
- **Charts:** Chart.js 4 (CDN) – trend line, bar comparison, donut/pie distribution
- **Logging:** Serilog – rolling file logs suitable for IIS hosting
- **Deployment:** IIS in-process hosting via Web Deploy, auto-deployed on push to `main`

---

## Project Layout

```
laks/
├── .github/workflows/
│   └── ci-cd.yml               # Build → Test → Publish → Deploy
├── src/
│   └── Laks.Web/
│       ├── Data/
│       │   ├── DbConnectionFactory.cs
│       │   ├── Migrations/
│       │   │   └── 001_initial_schema.sql   # Run once on the DB
│       │   └── Repositories/
│       │       ├── ICatchRepository.cs
│       │       ├── CatchRepository.cs
│       │       ├── ITripRepository.cs
│       │       ├── TripRepository.cs
│       │       ├── IAnglerRepository.cs
│       │       └── AnglerRepository.cs
│       ├── Models/
│       │   ├── Catch.cs
│       │   ├── Angler.cs
│       │   ├── Trip.cs
│       │   ├── Species.cs
│       │   └── ChartModels.cs
│       ├── Pages/
│       │   ├── Index.cshtml(.cs)            # Home – latest trip + recent catches
│       │   ├── Catches/
│       │   │   └── Index.cshtml(.cs)        # Full catch log with trip filter
│       │   ├── Statistics/
│       │   │   └── Index.cshtml(.cs)        # 3 Chart.js charts
│       │   └── Shared/_Layout.cshtml
│       ├── wwwroot/css/site.css
│       ├── Program.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       ├── appsettings.Production.json
│       └── web.config                       # IIS in-process hosting
└── tests/
    └── Laks.Web.Tests/
        └── Unit/
            ├── ModelTests.cs
            └── StatisticsPageModelTests.cs
```

---

## Bootstrap Guide

### Prerequisites

| Tool | Version |
|------|---------|
| .NET SDK | 9.x |
| MySQL Server | 8.x |
| IIS | 10+ with ASP.NET Core Hosting Bundle 9 |

### 1 – Clone & restore

```bash
git clone https://github.com/mongoose84/laks.git
cd laks
dotnet restore Laks.slnx
```

### 2 – Create the database

Run the migration script once:

```bash
mysql -u root -p laks < src/Laks.Web/Data/Migrations/001_initial_schema.sql
```

### 3 – Configure the connection string (Development)

Edit `src/Laks.Web/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "Laks": "Server=localhost;Database=laks_dev;Uid=your_user;Pwd=your_password;"
  }
}
```

> **Never commit production credentials.** Set them via IIS environment variables or
> the `ASPNETCORE_ConnectionStrings__Laks` environment variable on the server.

### 4 – Run locally

```bash
dotnet run --project .\src\Laks.Web\Laks.Web.csproj
# Open the HTTPS URL shown in the terminal
```

### 5 – Run tests

```bash
dotnet test Laks.slnx --configuration Release
```

---

## Chart Strategy

| Chart | Type | Data source endpoint | Purpose |
|-------|------|---------------------|---------|
| Catches per year | Line (dual-axis) | `GET /api/stats/catches-per-year` | Long-term trend |
| Catches per angler | Grouped bar | `GET /api/stats/catches-per-angler?year=` | Angler comparison |
| Species distribution | Doughnut | `GET /api/stats/catches-by-species?year=` | Species mix |

All three charts are rendered by **Chart.js 4** on the Statistics page using data
serialised to JSON by the Razor PageModel.

---

## GitHub Actions + IIS Deployment

### Required GitHub Secrets

| Secret | Example value | Purpose |
|--------|--------------|---------|
| `IIS_SERVER_URL` | `https://myserver.com:8172/msdeploy.axd` | Web Deploy endpoint |
| `IIS_SITE_NAME` | `laks` | IIS site name |
| `IIS_USER` | `deploy_user` | Windows user with deploy rights |
| `IIS_PASSWORD` | `*****` | Password for above user |
| `DB_CONNECTION_STRING` | `Server=prod-db;Database=laks;...` | MySQL connection string |

### Workflow steps

1. **Push to `main`** triggers the pipeline.
2. `ubuntu-latest` runner – restore, build, test.
3. `windows-latest` runner – publish, inject production `appsettings.Production.json`, deploy via `msdeploy.exe`.
4. Smoke test hits `/health` endpoint; if it returns 200 the deploy is considered successful.
5. The published folder is uploaded as a GitHub artifact (kept 14 days) for rollback.

### Rollback

Download the previous artifact from GitHub Actions → re-run the deploy step with that folder,
or use Web Deploy's built-in "take a backup" option before sync.

---

## IIS Prerequisites Checklist

- [ ] Windows Server with IIS 10+
- [ ] .NET 9 Hosting Bundle installed (`dotnet-hosting-win.exe`)
- [ ] Web Deploy 3.x installed
- [ ] `AspNetCoreModuleV2` present in IIS modules
- [ ] Application pool set to **No Managed Code** (in-process model)
- [ ] MySQL 8.x accessible from the web server
- [ ] Firewall allows inbound port 80/443
- [ ] TLS certificate bound to HTTPS binding

---

## Production Readiness Checklist

- [ ] Connection string set via IIS environment variable (not in source)
- [ ] `ASPNETCORE_ENVIRONMENT=Production` environment variable set
- [ ] Logs folder writable by IIS app pool identity (`logs/`)
- [ ] `stdoutLogEnabled` left as `false` in `web.config` (Serilog handles logging)
- [ ] Health endpoint accessible: `https://yoursite.com/health`
- [ ] Static file caching enabled (configured in `web.config` + `Program.cs`)
- [ ] Response compression enabled (configured in `Program.cs`)
- [ ] Security headers applied via `web.config`
- [ ] MySQL user has minimum required permissions: `SELECT` only (read-only for v1)

---

## Performance Hardening (June/July Peak)

- Response compression (gzip) is enabled for all content types.
- Static assets (JS, CSS) are served with a 7-day `Cache-Control` header.
- Chart.js is loaded from CDN with SRI integrity hash.
- Dapper uses parameterized queries and returns `IEnumerable<T>` (streams rows).
- IIS output caching can be enabled for `/api/stats/*` endpoints if needed.
- Connection pooling is handled by `MySqlConnection` automatically.

---

## 12-Week Delivery Plan

| Week | Milestone |
|------|-----------|
| 1–2 | Scaffold project, set up CI/CD pipeline, connect to existing database |
| 3–4 | Home page + Catches log with trip filter |
| 5–6 | Statistics page with 3 Chart.js charts |
| 7–8 | Style polish, mobile responsiveness, favicon/branding |
| 9–10 | IIS deploy, smoke test, production connection verified |
| 11 | Performance testing, health check, logging review |
| 12 | Buffer: bug fixes, content review, go-live |
