# Phase 2: Core Models & Database

## Status

⏳ **PENDING** (Depends on Phase 1)

## Overview

Design and implement core data models (Customer, Subscription, Invoice, WebhookEvent) and configure Entity Framework Core with SQL Server. This phase establishes the database schema and ORM mapping that will support all subsequent operations.

## Duration Estimate

**3-4 hours**

## Objectives

### 2.1 Create Data Models

- [ ] Create **Customer.cs** model
- [ ] Create **Subscription.cs** model
- [ ] Create **Invoice.cs** model
- [ ] Create **WebhookEvent.cs** model
- [ ] Define relationships between models
- [ ] Add validation annotations

### 2.2 Create DbContext

- [ ] Create **AppDbContext.cs**
- [ ] Configure DbSets for all models
- [ ] Define relationships in OnModelCreating
- [ ] Configure table names and constraints

### 2.3 Configure Entity Relationships

- [ ] Customer → Subscriptions (one-to-many)
- [ ] Subscription → Invoices (one-to-many)
- [ ] Customer → Invoices (one-to-many)
- [ ] Set up foreign keys and cascading deletes

### 2.4 Create Database Migrations

- [ ] Initialize migration: `dotnet ef migrations add InitialCreate`
- [ ] Apply migration: `dotnet ef database update`
- [ ] Verify database creation

### 2.5 Add Seed Data (Optional)

- [ ] Create seeding logic in DbContext
- [ ] Add test customers and subscriptions
- [ ] Useful for development/testing

## Step-by-Step Implementation

### Step 1: Create Models

#### Customer.cs

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StripePaymentDemo.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string StripeCustomerId { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(500)]
        public string Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}
```

#### Subscription.cs

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StripePaymentDemo.Models
{
    public class Subscription
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string StripeSubscriptionId { get; set; }

        [Required]
        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        [Required]
        [StringLength(100)]
        public string Plan { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } // active, trialing, past_due, canceled, unpaid

        public decimal Amount { get; set; }

        [StringLength(10)]
        public string Currency { get; set; } = "USD";

        [StringLength(50)]
        public string BillingCycle { get; set; } // monthly, yearly, etc.

        public DateTime? CurrentPeriodStart { get; set; }
        public DateTime? CurrentPeriodEnd { get; set; }
        public DateTime? TrialStart { get; set; }
        public DateTime? TrialEnd { get; set; }
        public DateTime? CancelledAt { get; set; }

        [StringLength(500)]
        public string Metadata { get; set; } // JSON metadata from Stripe

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Customer Customer { get; set; }
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}
```

#### Invoice.cs

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StripePaymentDemo.Models
{
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string StripeInvoiceId { get; set; }

        [Required]
        [ForeignKey("Customer")]
        public int CustomerId { get; set; }

        [ForeignKey("Subscription")]
        public int? SubscriptionId { get; set; }

        public decimal Amount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AmountDue { get; set; }

        [StringLength(10)]
        public string Currency { get; set; } = "USD";

        [Required]
        [StringLength(50)]
        public string Status { get; set; } // draft, open, paid, void, uncollectible

        public DateTime? DueDate { get; set; }
        public DateTime? PaidAt { get; set; }

        [StringLength(255)]
        public string ReceiptNumber { get; set; }

        [StringLength(255)]
        public string PdfUrl { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(500)]
        public string Metadata { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Customer Customer { get; set; }
        public Subscription Subscription { get; set; }
    }
}
```

#### WebhookEvent.cs

```csharp
using System;
using System.ComponentModel.DataAnnotations;

namespace StripePaymentDemo.Models
{
    public class WebhookEvent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string StripeEventId { get; set; }

        [Required]
        [StringLength(100)]
        public string EventType { get; set; } // subscription.created, invoice.paid, etc.

        [Required]
        public string EventData { get; set; } // Full JSON payload

        [StringLength(50)]
        public string Status { get; set; } = "pending"; // pending, processed, failed

        [StringLength(500)]
        public string ErrorMessage { get; set; }

        public int? RetryCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
    }
}
```

### Step 2: Create AppDbContext

#### Data/AppDbContext.cs

See: `AppDbContext.cs.example`

### Step 3: Create Initial Migration

```bash
# Install EF Core CLI tools (if not already installed)
dotnet tool install --global dotnet-ef

# Create migration
dotnet ef migrations add InitialCreate --output-dir Data/Migrations

# Apply migration to database
dotnet ef database update
```

### Step 4: Verify Database

**SQL Server Management Studio:**

1. Open SSMS
2. Connect to your server
3. Check `StripeDemoDb` database
4. Verify tables: Customers, Subscriptions, Invoices, WebhookEvents

**OR via PowerShell:**

```powershell
sqlcmd -S localhost -E -Q "USE StripeDemoDb; SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES;"
```

## Data Model Diagrams

### Entity Relationships

```
Customer (1)
    ├── Subscriptions (*)
    │   └── Invoices (*)
    └── Invoices (*)

WebhookEvent (standalone)
```

### Customer Model

```
Customer
├── Id (PK)
├── Name
├── Email (unique)
├── StripeCustomerId (unique)
├── Phone
├── Address
├── CreatedAt
├── UpdatedAt
└── Subscriptions (navigation)
```

### Subscription Model

```
Subscription
├── Id (PK)
├── StripeSubscriptionId (unique)
├── CustomerId (FK)
├── Plan
├── Status (active, trialing, etc)
├── Amount
├── Currency
├── BillingCycle
├── CurrentPeriodStart
├── CurrentPeriodEnd
├── TrialStart
├── TrialEnd
├── CancelledAt
├── Metadata (JSON)
├── CreatedAt
├── UpdatedAt
└── Invoices (navigation)
```

### Invoice Model

```
Invoice
├── Id (PK)
├── StripeInvoiceId (unique)
├── CustomerId (FK)
├── SubscriptionId (FK)
├── Amount
├── AmountPaid
├── AmountDue
├── Currency
├── Status (draft, open, paid, etc)
├── DueDate
├── PaidAt
├── ReceiptNumber
├── PdfUrl
├── Description
├── Metadata (JSON)
├── CreatedAt
└── UpdatedAt
```

## Deliverables

- ✅ 4 data model classes with properties and validation
- ✅ AppDbContext with all DbSets and relationships
- ✅ Initial database migration
- ✅ Database created and verified
- ✅ Table schema matching specification
- ✅ Foreign key relationships configured
- ✅ Index/constraint definitions

## Testing

### Verify DbContext

```csharp
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlServer("Server=localhost;Database=StripeDemoDb;Integrated Security=true;")
    .Options;

using (var context = new AppDbContext(options))
{
    context.Database.EnsureCreated();
    var customer = new Customer { Name = "Test", Email = "test@example.com", StripeCustomerId = "cus_123" };
    context.Customers.Add(customer);
    context.SaveChanges();
    Assert.Equal(1, context.Customers.Count());
}
```

### Verify Database Tables

```bash
dotnet ef dbcontext info
```

Expected output: Shows all DbSets and models

## Success Criteria

- [x] All 4 models created with proper properties
- [x] AppDbContext configured with DbSets
- [x] Entity relationships defined (Foreign Keys)
- [x] Validation annotations applied
- [x] Migration created: `InitialCreate`
- [x] Database created successfully
- [x] All tables present in database
- [x] Foreign key constraints working
- [x] Models compile without errors
- [x] IntelliSense shows properties correctly

## Dependencies

**Requires:**

- Phase 1 (Project Setup)

**Blocks:**

- Phase 3 (Stripe Service Layer)
- Phase 4 (Subscription Management)
- Phase 5 (Invoice Management)
- Phase 6 (Webhook Implementation)

## Database Schema

### Customers Table

| Column           | Type          | Notes            |
| ---------------- | ------------- | ---------------- |
| Id               | INT PK        | Auto-increment   |
| Name             | NVARCHAR(100) | Required         |
| Email            | NVARCHAR(255) | Required, Unique |
| StripeCustomerId | NVARCHAR(255) | Required, Unique |
| Phone            | NVARCHAR(20)  | Optional         |
| Address          | NVARCHAR(500) | Optional         |
| CreatedAt        | DATETIME      | Default: UtcNow  |
| UpdatedAt        | DATETIME      | Nullable         |

### Subscriptions Table

| Column               | Type          | Notes                    |
| -------------------- | ------------- | ------------------------ |
| Id                   | INT PK        | Auto-increment           |
| StripeSubscriptionId | NVARCHAR(255) | Required, Unique         |
| CustomerId           | INT FK        | References Customers(Id) |
| Plan                 | NVARCHAR(100) | Required                 |
| Status               | NVARCHAR(50)  | Required                 |
| Amount               | DECIMAL(10,2) | Required                 |
| Currency             | NVARCHAR(10)  | Default: USD             |
| BillingCycle         | NVARCHAR(50)  | Optional                 |
| CurrentPeriodStart   | DATETIME      | Nullable                 |
| CurrentPeriodEnd     | DATETIME      | Nullable                 |

## Migration Commands

```bash
# List migrations
dotnet ef migrations list

# Create new migration
dotnet ef migrations add <MigrationName>

# Revert last migration
dotnet ef migrations remove

# Update database to latest
dotnet ef database update

# Update database to specific migration
dotnet ef database update <MigrationName>

# Drop database
dotnet ef database drop
```

## Common Issues

### Issue: Migration Fails - "Migrations folder not found"

**Solution:** EF creates folder automatically, but ensure project file references:

```xml
<ItemGroup>
    <Folder Include="Data\Migrations\" />
</ItemGroup>
```

### Issue: Database Already Exists

**Solution:** Either drop and recreate or run migrations:

```bash
dotnet ef database drop -f
dotnet ef database update
```

### Issue: Foreign Key Constraint Errors

**Solution:** Ensure OnDelete behavior is configured:

```csharp
.WithMany()
.HasForeignKey(x => x.CustomerId)
.OnDelete(DeleteBehavior.Cascade);
```

## Next Steps

Once Phase 2 is complete:

1. Verify database structure
2. Test model creation and relationships
3. Commit migration and models
4. Proceed to **Phase 3: Stripe Service Layer**

---

**Created:** 2026-05-30  
**Last Updated:** 2026-05-30  
**Estimated Completion:** 3-4 hours
