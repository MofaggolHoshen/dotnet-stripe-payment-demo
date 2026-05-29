# Phase 2 Checklist

## Models Creation

- [ ] Customer.cs created with all properties
- [ ] Subscription.cs created with all properties
- [ ] Invoice.cs created with all properties
- [ ] WebhookEvent.cs created with all properties
- [ ] All models placed in `/source/Models/` folder
- [ ] Using statements correctly configured

## Model Validation

- [ ] [Required] annotations on required fields
- [ ] [EmailAddress] on Email fields
- [ ] [StringLength] on string fields
- [ ] [Key] on primary keys
- [ ] [ForeignKey] on foreign key properties
- [ ] Decimal fields typed as decimal(10,2)

## Navigation Properties

- [ ] Customer has Subscriptions collection
- [ ] Customer has Invoices collection
- [ ] Subscription has Customer reference
- [ ] Subscription has Invoices collection
- [ ] Invoice has Customer reference
- [ ] Invoice has Subscription reference
- [ ] All initialized to new collections

## DbContext Configuration

- [ ] AppDbContext.cs created in `/source/Data/` folder
- [ ] DbSets defined for all 4 models
- [ ] OnModelCreating configured relationships
- [ ] Unique indexes on Stripe IDs
- [ ] Indexes on frequently queried columns
- [ ] Cascade delete behaviors configured correctly

## Migrations

- [ ] `dotnet ef migrations add InitialCreate` executed
- [ ] Migration file created in `Data/Migrations/`
- [ ] Migration Up method creates all tables
- [ ] Migration Down method drops all tables

## Database Setup

- [ ] SQL Server database `StripeDemoDb` exists
- [ ] `dotnet ef database update` executed successfully
- [ ] 4 tables created: Customers, Subscriptions, Invoices, WebhookEvents
- [ ] All columns present in tables
- [ ] Indexes created
- [ ] Foreign keys configured

## Verification

- [ ] All models compile without errors
- [ ] DbContext compiles without errors
- [ ] IntelliSense works for all entities
- [ ] `dotnet build` succeeds
- [ ] Database tables visible in SSMS or sqlcmd

## Code Quality

- [ ] No hardcoded values in models
- [ ] Proper naming conventions followed
- [ ] PascalCase for properties
- [ ] camelCase for parameters
- [ ] Meaningful property names

## Documentation

- [ ] README.md written with full instructions
- [ ] AppDbContext.cs.example provided
- [ ] Entity diagrams documented
- [ ] Database schema documented

## Sign Off

- [ ] All checklist items complete
- [ ] Database verified and working
- [ ] Models tested and working
- [ ] Ready to proceed to Phase 3
- [ ] Changes committed to Git

**Completed By:** ******\_\_\_******  
**Date:** ******\_\_\_******  
**Notes:**
