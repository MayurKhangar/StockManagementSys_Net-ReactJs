# SmartStock — Store Management System

A full-stack store management system built with **ASP.NET Core 8 Web API** (Clean N-Tier architecture) and **React (JavaScript, Vite + Tailwind CSS)**.

## Architecture

```
SmartStock/
├── SmartStock.sln
├── SmartStock.Api/              → ASP.NET Core 8 Web API (entry project)
│   ├── Controllers/
│   ├── ClientApp/               → React app (Vite, JavaScript, Tailwind CSS)
│   ├── Program.cs
│   └── SmartStock.Api.csproj
├── SmartStock.Application/      → Services, Interfaces, DTOs, ResultModel<T>
├── SmartStock.Domain/           → Entities, Enums
├── SmartStock.Infrastructure/   → EF Core DbContext, Repositories, Migrations
└── SmartStock.Shared/           → Common constants/helpers
```

Request flow: **Controller → Service (interface + impl) → Repository (Unit of Work) → EF Core DbContext**.
Every service method returns a `ResultModel<T>` (`Success`, `Message`, `Data`).

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) and npm
- SQL Server LocalDB (installed with Visual Studio) or any local SQL Server instance
- **Visual Studio 2022 version 17.8 or later** if opening via the IDE (earlier 17.x releases don't support .NET 8 projects). Install the **ASP.NET and web development** workload — it brings IIS Express and SQL Server LocalDB. The Node.js workload is optional (nice for npm IntelliSense) but not required; SpaProxy just shells out to `npm`.

## Setup

### 1. Configure the connection string

Edit `SmartStock.Api/appsettings.json` and update `ConnectionStrings:DefaultConnection` if you're not using the default LocalDB instance:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SmartStockDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

Also replace `JwtSettings:Secret` with your own long random string (32+ characters) before deploying anywhere beyond local development.

### 2. Apply the EF Core migration

```bash
dotnet ef database update --project SmartStock.Infrastructure --startup-project SmartStock.Api
```

This creates the `SmartStockDb` database and schema. On every app startup, `Program.cs` also calls `Database.MigrateAsync()` and seeds baseline data automatically, so this step is a one-time convenience / CI step — you can skip it and just run the app.

### 3. Install frontend dependencies (one-time)

```bash
cd SmartStock.Api/ClientApp
npm install
cd ../..
```

### 4. Run the app

From the repository root:

```bash
dotnet run --project SmartStock.Api
```

`Microsoft.AspNetCore.SpaProxy` automatically launches the React dev server (`npm start`, Vite on port 3000) alongside the API and proxies non-API requests to it. Just open the URL printed in the console (e.g. `http://localhost:5131`) — both frontend and backend are running from this single command.

### Running from Visual Studio 2022 instead

1. Open `SmartStock.sln`.
2. Make sure **SmartStock.Api** is the startup project (right-click it → *Set as Startup Project*).
3. Pick the **http** or **https** launch profile from the debug target dropdown (either works — avoid the **IIS Express** profile, it isn't needed and adds friction with SpaProxy) and press **F5**.
4. VS2022 builds all five projects, starts Kestrel, and — via the `Microsoft.AspNetCore.SpaProxy` hosting startup — launches `npm start` in `ClientApp` for you, then opens your browser once both are ready.

Running the EF Core migration from the IDE instead of the CLI: open **Tools → NuGet Package Manager → Package Manager Console**, set the *Default project* dropdown to `SmartStock.Infrastructure`, and run:

```powershell
Update-Database -StartupProject SmartStock.Api
```

Verified: this solution builds cleanly both via `dotnet build` and via Visual Studio 2022's own `devenv.exe /Rebuild` (17.14, all 5 projects succeed).

## Seed data

On first run the database is seeded with:

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@smartstock.com | Admin@123 |
| Customer | customer@smartstock.com | Customer@123 |

Plus 3 categories, 2 suppliers, and 7 sample products (including one intentionally below its low-stock threshold).

## Modules

1. **Auth** — Register/Login/Refresh token (JWT), role-based authorization (Admin, StoreManager, Customer), BCrypt password hashing.
2. **Product Management** (Admin/StoreManager) — CRUD Products, Categories, Suppliers.
3. **Stock Management** (Admin/StoreManager) — Stock In, Stock Adjustment, full StockTransaction ledger (In/Out/Adjustment), low-stock alerts.
4. **Purchase Flow** (Customer) — Browse products, cart (persisted client-side), place order with concurrency-safe stock deduction (`UPDLOCK`/`ROWLOCK` atomic SQL update — prevents overselling under concurrent orders).
5. **Billing/Invoice** — Auto-generated on order placement, sequential invoice numbers (`INV-2026-0001`), tax/discount calculation, PDF export (QuestPDF).
6. **Dashboard/Reports** (Admin/StoreManager) — Sales summary, stock valuation, top products, sales trend — charted with Recharts.

## Tech stack

- **Backend**: ASP.NET Core 8, EF Core 8 (SQL Server), JWT Bearer auth, BCrypt.Net, QuestPDF, Swagger/Swashbuckle
- **Frontend**: React 19 (JavaScript), Vite, React Router v6+, Axios (JWT interceptor + auto refresh), Context API (auth + cart state), Tailwind CSS v4, Recharts

## API documentation

With the app running in Development, Swagger UI is available at `/swagger`.
