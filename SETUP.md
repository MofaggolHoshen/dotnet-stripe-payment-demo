# Stripe Payment Demo - Setup Guide

## Prerequisites

Before setting up the project, ensure you have the following installed:

- **.NET 6.0 SDK** or later
  - Download from: https://dotnet.microsoft.com/download/dotnet/6.0
- **SQL Server** (2016 or later) or **LocalDB**
  - SQL Server Express: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
  - Or use LocalDB (included with Visual Studio)
- **Git** (optional, for cloning the repository)

## Step 1: Get Stripe API Keys

1. Create a Stripe account at https://stripe.com
2. Navigate to the Dashboard
3. Go to **Settings** → **API Keys**
4. Copy your:
   - **Publishable Key** (starts with `pk_`)
   - **Secret Key** (starts with `sk_`)

**Important**: Keep your Secret Key private. Never commit it to version control.

## Step 2: Clone/Setup the Repository

```bash
# Navigate to your desired directory
cd C:\Users\{username}\Source\repos

# Clone the repository (if using Git)
git clone https://github.com/yourusername/dotnet-stripe-payment-demo.git

# Navigate into the project directory
cd dotnet-stripe-payment-demo
```

## Step 3: Configure API Keys

### Option A: Using appsettings.json (Development Only)

1. Open `source/appsettings.Development.json`
2. Replace the placeholder values:

```json
{
  "Stripe": {
    "PublishableKey": "pk_test_YOUR_PUBLISHABLE_KEY_HERE",
    "SecretKey": "sk_test_YOUR_SECRET_KEY_HERE",
    "WebhookSecret": "whsec_test_YOUR_WEBHOOK_SECRET_HERE"
  }
}
```

3. Save the file

### Option B: Using Environment Variables (Recommended for Production)

Set environment variables:

```bash
# Windows PowerShell
$env:Stripe__SecretKey = "sk_test_your_key_here"
$env:Stripe__PublishableKey = "pk_test_your_key_here"
$env:Stripe__WebhookSecret = "whsec_test_your_webhook_secret_here"

# Windows CMD
set Stripe__SecretKey=sk_test_your_key_here
set Stripe__PublishableKey=pk_test_your_key_here
set Stripe__WebhookSecret=whsec_test_your_webhook_secret_here

# Linux/Mac Bash
export Stripe__SecretKey="sk_test_your_key_here"
export Stripe__PublishableKey="pk_test_your_key_here"
export Stripe__WebhookSecret="whsec_test_your_webhook_secret_here"
```

## Step 4: Configure Database

### Option A: Using LocalDB (Windows - Easiest)

LocalDB is automatically configured. No additional setup needed.

Connection string in `appsettings.json`:

```json
"ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=StripePaymentDemo;Trusted_Connection=true;"
}
```

### Option B: Using SQL Server

1. Update the connection string in `source/appsettings.json`:

```json
"ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=StripePaymentDemo;User Id=sa;Password=YOUR_PASSWORD;"
}
```

2. Example for local SQL Server:

```json
"ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=StripePaymentDemo;Integrated Security=true;"
}
```

### Option C: Using Azure SQL Database

```json
"ConnectionStrings": {
    "DefaultConnection": "Server=tcp:your-server.database.windows.net,1433;Initial Catalog=StripePaymentDemo;Persist Security Info=False;User ID=admin;Password=YOUR_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
}
```

## Step 5: Install Dependencies

```bash
# Navigate to the project directory
cd C:\path\to\dotnet-stripe-payment-demo

# Restore NuGet packages
dotnet restore
```

## Step 6: Create Database

The database will be created automatically when you run the application for the first time.

Alternatively, you can create it manually:

```bash
# Navigate to source directory
cd source

# Build the project
dotnet build

# Run Entity Framework migrations (if you set them up)
# dotnet ef database update
```

## Step 7: Run the Application

```bash
# Navigate to source directory (if not already there)
cd source

# Run the application
dotnet run
```

**Output:**

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7001
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to exit.
```

## Step 8: Access the API

### Swagger UI (API Documentation)

Navigate to: `https://localhost:7001/swagger`

This provides an interactive interface to test all API endpoints.

### Example API Calls

**Register a Customer:**

```bash
curl -X POST https://localhost:7001/api/customer/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "customer@example.com",
    "name": "John Doe"
  }'
```

**Create a Subscription:**

```bash
curl -X POST https://localhost:7001/api/subscription/create \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": 1,
    "stripePriceId": "price_YOUR_PRICE_ID"
  }'
```

## Step 9: Set Up Webhook Forwarding (Optional)

To receive Stripe webhook events locally, you'll need to forward them from Stripe to your local machine.

### Using ngrok (Recommended)

1. Download ngrok from https://ngrok.com/download
2. Extract and run:
   ```bash
   ngrok http 7001
   ```
3. Copy the public URL (e.g., `https://abc123.ngrok.io`)
4. In Stripe Dashboard → Developers → Webhooks:
   - Add endpoint: `https://abc123.ngrok.io/api/webhook/stripe`
   - Select events: `customer.*`, `subscription.*`, `invoice.*`
   - Copy the **Signing Secret**
5. Update your `appsettings.json`:
   ```json
   "Stripe": {
     "WebhookSecret": "your_webhook_secret_here"
   }
   ```

### Testing Webhooks

Use Stripe CLI for easier testing:

```bash
# Install Stripe CLI from https://stripe.com/docs/stripe-cli

# Login to Stripe
stripe login

# Forward events to your local application
stripe listen --forward-to localhost:7001/api/webhook/stripe

# Trigger test events
stripe trigger customer.created
stripe trigger subscription.created
stripe trigger invoice.created
```

## Running Tests

```bash
# Run all tests
dotnet test

# Run tests with verbose output
dotnet test --verbosity detailed

# Run specific test class
dotnet test --filter "FullyQualifiedName~StripeServiceTests"

# Generate coverage report
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
```

## Troubleshooting

### Database Connection Issues

**Error:** `Cannot open database "StripePaymentDemo" requested by the login. The login failed.`

**Solution:**

- Verify SQL Server is running
- Check connection string in `appsettings.json`
- Ensure LocalDB is installed (for Windows)

### Stripe API Key Issues

**Error:** `Stripe.StripeException: Invalid API Key provided`

**Solution:**

- Verify API keys in `appsettings.json` or environment variables
- Use test keys from Stripe Dashboard (start with `test_`)
- Check for trailing/leading whitespace in keys

### Port Already in Use

**Error:** `The SSL port xxx is already in use`

**Solution:**

- Change the port in `launchSettings.json`
- Or kill the process using the port:

  ```bash
  # Windows
  netstat -ano | findstr :7001
  taskkill /PID <PID> /F

  # Linux/Mac
  lsof -i :7001
  kill -9 <PID>
  ```

### HTTPS Certificate Issues

**Error:** `The certificate is self-signed and was not found in the Certifi library`

**Solution:**

```bash
# Install HTTPS development certificate
dotnet dev-certs https --trust
```

## Development Workflow

### Using Visual Studio

1. Open `dotnet-stripe-payment-demo.csproj` in Visual Studio
2. Set `source` as the Startup Project
3. Press `F5` to run
4. Visual Studio will open the Swagger UI automatically

### Using Visual Studio Code

1. Install the C# extension
2. Open the project folder
3. Press `F5` to start debugging
4. Application will run at `https://localhost:7001`

## Project Structure

```
dotnet-stripe-payment-demo/
├── source/                  # Main application code
│   ├── Models/             # Entity models
│   ├── Services/           # Business logic
│   ├── Controllers/        # API endpoints
│   ├── Data/              # Database context
│   ├── Program.cs         # Entry point
│   └── appsettings*.json  # Configuration
├── tests/                  # Unit and integration tests
├── docs/                   # Documentation
└── IMPLEMENTATION_SUMMARY.md
```

## Next Steps

1. Explore the API using Swagger UI
2. Read the API documentation in `docs/API.md`
3. Understand the webhook system in `docs/WEBHOOK.md`
4. Review subscription management in `docs/SUBSCRIPTION.md`
5. Run the test suite to verify installation
6. Create test customers and subscriptions in Stripe
7. Test the webhook integration

## Additional Resources

- [Stripe API Documentation](https://stripe.com/docs/api)
- [Stripe.NET SDK Documentation](https://github.com/stripe/stripe-dotnet)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)

## Support

For issues or questions:

1. Check the troubleshooting section above
2. Review Stripe documentation at https://stripe.com/docs
3. Check Entity Framework Core issues: https://github.com/dotnet/efcore
4. Post in ASP.NET Core forums: https://github.com/dotnet/aspnetcore

---

**Last Updated**: 2026-05-30
**Status**: Ready for setup and testing
