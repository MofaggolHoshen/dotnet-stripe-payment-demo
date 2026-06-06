# Stripe Payment Demo - .NET Implementation

A comprehensive .NET 6.0 ASP.NET Core Web API demonstration showcasing Stripe payment integration, including subscription management, webhook handling, invoice management, and customer operations.

## Features

- **Customer Management**: Create, retrieve, and manage Stripe customers
- **Subscription Management**: Full subscription lifecycle management (create, update, cancel, retrieve)
- **Invoice Management**: Generate, retrieve, and manage invoices with Stripe
- **Webhook Handling**: Secure webhook processing for Stripe events
- **Payment Processing**: End-to-end payment processing with Stripe integration
- **Database Persistence**: Entity Framework Core with SQL Server integration
- **API Documentation**: Swagger/OpenAPI documentation
- **Error Handling**: Comprehensive middleware-based error handling
- **CORS Support**: Cross-Origin Resource Sharing configured for flexibility
- **Rate Limiting**: Request rate limiting capabilities
- **Validation**: FluentValidation for input validation
- **Unit Tests**: XUnit test framework with Moq for mocking

## Tech Stack

- **.NET Framework**: .NET 6.0
- **Web Framework**: ASP.NET Core
- **Database**: Entity Framework Core 6.0.29 with SQL Server
- **Payment Processing**: Stripe.NET SDK (v45.12.0)
- **API Documentation**: Swagger/Swashbuckle (v6.5.0)
- **Validation**: FluentValidation (v11.9.2)
- **Testing**: XUnit (v2.7.0), Moq (v4.20.70)
- **Logging**: Microsoft.Extensions.Logging
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection

## Project Structure

```
source/
├── Controllers/           # API endpoints
│   ├── CustomerController.cs
│   ├── SubscriptionController.cs
│   ├── InvoiceController.cs
│   ├── WebhookController.cs
│   └── HealthController.cs
├── Models/               # Domain models
├── DTOs/                 # Data Transfer Objects
├── Services/             # Business logic
│   ├── StripeService.cs
│   ├── SubscriptionService.cs
│   ├── InvoiceService.cs
│   └── WebhookService.cs
├── Data/                 # Database context and migrations
├── Middleware/           # Custom middleware (error handling)
├── appsettings.json      # Configuration
└── Program.cs            # Application entry point

tests/
└── StripeServiceTests.cs # Unit tests
```

## Prerequisites

- .NET 6.0 SDK or later
- SQL Server (LocalDB or full instance)
- Stripe API Key and Webhook Secret

## Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/MofaggolHoshen/dotnet-stripe-payment-demo.git
   cd dotnet-stripe-payment-demo
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Configure the database connection**
   Update `appsettings.json` with your SQL Server connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=your-server;Database=StripePaymentDemo;Trusted_Connection=true;"
     }
   }
   ```

4. **Set Stripe API Key**
   Add your Stripe API key to `appsettings.json` or user secrets:
   ```json
   {
     "Stripe": {
       "ApiKey": "sk_test_your_key_here",
       "WebhookSecret": "whsec_your_webhook_secret_here"
     }
   }
   ```

5. **Create the database**
   ```bash
   dotnet ef database update
   ```

## Running the Application

### Development
```bash
dotnet run --environment Development
```

The application will start on `https://localhost:5001` by default.

### Access Swagger UI
Navigate to `https://localhost:5001/swagger/ui` to view the interactive API documentation.

## API Endpoints

### Health Check
- `GET /api/health` - Health check endpoint

### Customers
- `POST /api/customers` - Create a new customer
- `GET /api/customers/{id}` - Get customer details
- `GET /api/customers` - List all customers
- `PUT /api/customers/{id}` - Update customer

### Subscriptions
- `POST /api/subscriptions` - Create a subscription
- `GET /api/subscriptions/{id}` - Get subscription details
- `PUT /api/subscriptions/{id}` - Update subscription
- `DELETE /api/subscriptions/{id}` - Cancel subscription
- `GET /api/subscriptions` - List subscriptions

### Invoices
- `POST /api/invoices` - Create an invoice
- `GET /api/invoices/{id}` - Get invoice details
- `GET /api/invoices` - List invoices
- `POST /api/invoices/{id}/send` - Send invoice

### Webhooks
- `POST /api/webhooks` - Handle Stripe webhook events

## Configuration

Key settings in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "Stripe": {
    "ApiKey": "your_stripe_api_key",
    "WebhookSecret": "your_webhook_secret"
  },
  "ConnectionStrings": {
    "DefaultConnection": "your_connection_string"
  }
}
```

## Testing

Run unit tests using:

```bash
dotnet test
```

Tests are located in the `tests/` directory and use XUnit and Moq for testing Stripe service operations.

## Error Handling

The application includes comprehensive error handling through middleware that catches exceptions and returns standardized error responses with appropriate HTTP status codes.

## CORS Configuration

CORS is configured to allow requests from any origin. For production, update this policy in `Program.cs` to restrict to specific origins.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Support

For issues or questions, please open an issue on the repository.

---

**Note**: This is a demonstration project for educational purposes. Ensure you use proper security practices when handling payment information in production environments.