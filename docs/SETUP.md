# Project Setup & Installation

## Prerequisites

- .NET 6.0 or higher
- SQL Server (or modify connection string for your DB)
- Stripe account (https://stripe.com)
- Git

## Getting Started

### 1. Clone Repository

```bash
git clone https://github.com/yourusername/dotnet-stripe-payment-demo.git
cd dotnet-stripe-payment-demo
```

### 2. Configure Stripe Keys

Create `appsettings.Development.json`:

```json
{
  "Stripe": {
    "PublishableKey": "pk_test_...",
    "SecretKey": "sk_test_...",
    "WebhookSecret": "whsec_..."
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=StripeDemoDb;Integrated Security=true;"
  }
}
```

### 3. Install Dependencies

```bash
dotnet restore
```

### 4. Create Database

```bash
dotnet ef database update
```

### 5. Run Project

```bash
dotnet run
```

### 6. Test Endpoints

```bash
curl http://localhost:5000/api/health
```

---

**More details coming in Phase 1**
