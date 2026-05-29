# Phase 1: Project Setup & Configuration

## Status

⏳ **IN PROGRESS**

## Overview

Initialize the .NET ASP.NET Core project structure, configure dependencies, set up Stripe API integration, and prepare the database connection. This foundational phase establishes the project architecture and ensures all necessary tools and configurations are in place.

## Duration Estimate

**2-3 hours**

## Objectives

### 1.1 Create ASP.NET Core Project

- [ ] Initialize new .NET 6+ ASP.NET Core Web API project
- [ ] Use template: `dotnet new webapi`
- [ ] Project name: `StripePaymentDemo`

### 1.2 Install NuGet Dependencies

- [ ] **Stripe.net** (v43.0.0+) - Official Stripe SDK
- [ ] **Microsoft.EntityFrameworkCore** (v6.0+) - ORM
- [ ] **Microsoft.EntityFrameworkCore.SqlServer** - SQL Server provider
- [ ] **Microsoft.Extensions.Configuration** - Configuration management
- [ ] **Microsoft.Extensions.Logging** - Logging framework
- [ ] **Microsoft.AspNetCore.Mvc** - MVC framework
- [ ] **Serilog** (optional) - Advanced logging

### 1.3 Configure Project Structure

- [ ] Verify/create folder hierarchy:
  - `/source` → C# project files
  - `/source/Models` → Data models
  - `/source/Services` → Business logic
  - `/source/Controllers` → API endpoints
  - `/source/Data` → Database context
  - `/docs` → Documentation
  - `/tests` → Unit and integration tests

### 1.4 Set Up Stripe Configuration

- [ ] Add `appsettings.json` with configuration sections
- [ ] Create `appsettings.Development.json` for local development
- [ ] Configure Stripe API keys (Publishable & Secret)
- [ ] Add webhook signing secret

### 1.5 Configure Database Connection

- [ ] Set up SQL Server connection string
- [ ] Configure Entity Framework Core
- [ ] Set up dependency injection for DbContext

### 1.6 Initialize Git Repository

- [ ] Create `.gitignore` (exclude secrets, binaries, cache)
- [ ] First commit with project structure

## Step-by-Step Implementation

### Step 1: Create Project

```bash
# Create new ASP.NET Core Web API project
dotnet new webapi -n StripePaymentDemo -o .

# Verify project structure
dotnet --version
```

### Step 2: Add NuGet Packages

```bash
# Install Stripe SDK
dotnet add package Stripe.net --version 43.0.0

# Install Entity Framework Core
dotnet add package Microsoft.EntityFrameworkCore --version 6.0.0
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 6.0.0

# Install configuration and logging (if not included)
dotnet add package Microsoft.Extensions.Configuration.Json
dotnet add package Serilog.AspNetCore
```

### Step 3: Create Configuration Files

#### appsettings.json Template

See: `appsettings.json.template`

#### appsettings.Development.json

Create locally (never commit with real keys):

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  },
  "Stripe": {
    "PublishableKey": "pk_test_xxxxx",
    "SecretKey": "sk_test_xxxxx",
    "WebhookSigningSecret": "whsec_xxxxx"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=StripeDemoDb;Integrated Security=true;"
  }
}
```

### Step 4: Update Program.cs

See: `Program.cs.example`

### Step 5: Create .gitignore

See: `.gitignore.template`

### Step 6: Verify Installation

```bash
# Build project
dotnet build

# Run project (should start successfully)
dotnet run

# Test health endpoint
curl http://localhost:5000/health
```

## Configuration Details

### Stripe API Keys

1. Go to https://dashboard.stripe.com
2. Navigate to: Developers → API keys
3. Copy:
   - **Publishable Key** (starts with `pk_test_` or `pk_live_`)
   - **Secret Key** (starts with `sk_test_` or `sk_live_`)
4. For webhooks:
   - Go to: Developers → Webhooks
   - Copy **Signing Secret** (starts with `whsec_`)

### Database Connection

**SQL Server:**

```
Server=localhost;Database=StripeDemoDb;Integrated Security=true;
```

**SQL Server (with credentials):**

```
Server=localhost;Database=StripeDemoDb;User Id=sa;Password=YourPassword;
```

**Local SQL Express:**

```
Server=(localdb)\mssqllocaldb;Database=StripeDemoDb;Integrated Security=true;
```

## Deliverables

- ✅ `.csproj` file with all NuGet dependencies
- ✅ `appsettings.json` with configuration sections
- ✅ `Program.cs` with Stripe and EF Core setup
- ✅ Folder structure created (Models, Services, Controllers, Data)
- ✅ `.gitignore` configured
- ✅ Initial Git commit
- ✅ Health endpoint working (`/health`)

## Testing

### Verify Project Builds

```bash
dotnet build
```

Expected: ✅ Build succeeded

### Verify Project Runs

```bash
dotnet run
```

Expected: ✅ Application started on http://localhost:5000

### Test Health Endpoint

```bash
curl http://localhost:5000/health
```

Expected: ✅ Returns 200 OK

### Verify NuGet Packages

```bash
dotnet list package
```

Expected: ✅ All required packages listed

## Success Criteria

- [x] .NET 6+ ASP.NET Core project created
- [x] All NuGet dependencies installed successfully
- [x] Folder structure matches specification
- [x] `appsettings.json` configured with Stripe section
- [x] Database connection string configured
- [x] Project builds without errors
- [x] Application runs and responds to health endpoint
- [x] `.gitignore` excludes secrets and binaries
- [x] Git repository initialized with first commit
- [x] No hardcoded secrets in version control

## Dependencies

**Required for Phase 1:**

- None (foundational phase)

**Blocks the following phases:**

- Phase 2: Core Models & Database
- Phase 3: Stripe Service Layer

## Files Created/Modified

| File                              | Purpose                           |
| --------------------------------- | --------------------------------- |
| `StripePaymentDemo.csproj`        | Project file with dependencies    |
| `Program.cs`                      | App startup and DI configuration  |
| `appsettings.json`                | Configuration template            |
| `appsettings.Development.json`    | Local development config (secret) |
| `.gitignore`                      | Version control exclusions        |
| `Controllers/HealthController.cs` | Simple health check endpoint      |
| `Startup.cs`                      | Service configuration             |

## Common Issues

### Issue: Stripe Package Not Found

**Solution:** Ensure NuGet source is configured

```bash
dotnet nuget add source https://api.nuget.org/v3/index.json --name nuget.org
```

### Issue: SQL Server Connection Fails

**Solution:** Verify SQL Server is running

```bash
# Check if SQL Server is running (Windows)
Get-Service "MSSQLSERVER"
```

### Issue: Port 5000 Already in Use

**Solution:** Change port in `launchSettings.json`

```json
"urls": "https://localhost:5001;http://localhost:5001"
```

## Next Steps

Once Phase 1 is complete:

1. Review and verify all configurations
2. Ensure project builds and runs successfully
3. Proceed to **Phase 2: Core Models & Database**

---

**Created:** 2026-05-30  
**Last Updated:** 2026-05-30  
**Estimated Completion:** 2-3 hours
