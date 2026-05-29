# Phase 5: Invoice Management

## Status

⏳ **PENDING** (Depends on Phase 3)

## Overview

Implement invoice management features using Stripe's Invoice API. Create services for retrieving, listing, and managing invoices with proper tracking in the database.

## Duration Estimate

**3-4 hours**

## Objectives

### 5.1 Create InvoiceService

- [ ] Implement GetInvoiceAsync
- [ ] Implement ListInvoicesAsync
- [ ] Implement GetInvoicePdfAsync
- [ ] Implement MarkInvoiceAsPaidAsync
- [ ] Implement RetryInvoicePaymentAsync
- [ ] Implement invoice status synchronization

### 5.2 Implement Webhook Integration

- [ ] Listen to `invoice.created` events
- [ ] Listen to `invoice.payment_succeeded` events
- [ ] Listen to `invoice.payment_failed` events
- [ ] Update local invoice records

### 5.3 Invoice Status Tracking

- [ ] Track invoice states (draft, open, paid, void, uncollectible)
- [ ] Store payment dates
- [ ] Track amounts (total, paid, due)
- [ ] Store PDF URL for download

### 5.4 Database Persistence

- [ ] Save invoices to database
- [ ] Update existing invoices
- [ ] Link to customers and subscriptions
- [ ] Handle cascading updates

### 5.5 Unit & Integration Tests

- [ ] Unit tests for invoice operations
- [ ] Integration tests with database
- [ ] Mock Stripe API responses
- [ ] Test error scenarios

## Step-by-Step Implementation

### Step 1: Create IInvoiceService Interface

```csharp
public interface IInvoiceService
{
    Task<Stripe.Invoice> GetInvoiceAsync(string invoiceId);
    Task<List<Stripe.Invoice>> ListInvoicesAsync(string customerId = null, int limit = 10);
    Task<Stream> GetInvoicePdfAsync(string invoiceId);
    Task<Stripe.Invoice> MarkInvoiceAsPaidAsync(string invoiceId);
    Task<Stripe.Invoice> RetryInvoicePaymentAsync(string invoiceId);
    Task SyncInvoiceAsync(Stripe.Invoice stripeInvoice);
}
```

### Step 2: Invoice Retrieval Implementation

```csharp
public async Task<Stripe.Invoice> GetInvoiceAsync(string invoiceId)
{
    return await _stripeService.ExecuteWithRetryAsync(async () =>
    {
        _logger.LogInformation($"Retrieving invoice: {invoiceId}");
        var invoice = await _invoiceService.GetAsync(invoiceId);
        await SyncInvoiceAsync(invoice);
        return invoice;
    }, "GetInvoice");
}
```

### Step 3: Invoice Listing Implementation

```csharp
public async Task<List<Stripe.Invoice>> ListInvoicesAsync(
    string customerId = null,
    int limit = 10)
{
    return await _stripeService.ExecuteWithRetryAsync(async () =>
    {
        _logger.LogInformation($"Listing invoices for customer: {customerId}");

        var options = new InvoiceListOptions { Limit = limit };
        if (!string.IsNullOrEmpty(customerId))
            options.Customer = customerId;

        var invoices = await _invoiceService.ListAsync(options);

        foreach (var invoice in invoices.Data)
        {
            await SyncInvoiceAsync(invoice);
        }

        return invoices.Data;
    }, "ListInvoices");
}
```

### Step 4: Invoice Database Sync

```csharp
public async Task SyncInvoiceAsync(Stripe.Invoice stripeInvoice)
{
    var invoice = await _context.Invoices
        .FirstOrDefaultAsync(i => i.StripeInvoiceId == stripeInvoice.Id);

    if (invoice == null)
    {
        invoice = new Invoice();
        _context.Invoices.Add(invoice);
    }

    invoice.StripeInvoiceId = stripeInvoice.Id;
    invoice.CustomerId = GetCustomerId(stripeInvoice.CustomerId);
    invoice.Status = stripeInvoice.Status;
    invoice.Amount = (decimal)(stripeInvoice.Total ?? 0) / 100;
    invoice.AmountPaid = (decimal)(stripeInvoice.Paid ?? 0) / 100;
    invoice.AmountDue = (decimal)(stripeInvoice.AmountDue ?? 0) / 100;
    invoice.DueDate = stripeInvoice.DueDate;
    invoice.PaidAt = stripeInvoice.PaidAt;
    invoice.PdfUrl = stripeInvoice.PdfUrl;
    invoice.ReceiptNumber = stripeInvoice.ReceiptNumber;
    invoice.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();
}
```

### Step 5: PDF Retrieval Implementation

```csharp
public async Task<Stream> GetInvoicePdfAsync(string invoiceId)
{
    var invoice = await GetInvoiceAsync(invoiceId);

    if (string.IsNullOrEmpty(invoice.PdfUrl))
        throw new InvalidOperationException("PDF not available for this invoice");

    using (var client = new HttpClient())
    {
        var pdf = await client.GetStreamAsync(invoice.PdfUrl);
        return pdf;
    }
}
```

## Invoice States

```
CREATE INVOICE
    │
    ├─ Draft (unpublished)
    │
    └─ Open (awaiting payment)
         │
         ├─ Payment Received → PAID
         │
         ├─ Payment Failed → PAST_DUE
         │
         └─ Void → VOID
```

## Deliverables

- ✅ IInvoiceService interface
- ✅ InvoiceService implementation
- ✅ Get, List, PDF download operations
- ✅ Database persistence layer
- ✅ Status synchronization
- ✅ Webhook event handlers
- ✅ Unit tests (50+ tests)
- ✅ Integration tests
- ✅ Documentation

## Testing

```bash
dotnet test --filter "InvoiceServiceTests"
```

## Success Criteria

- [x] All invoice operations implemented
- [x] Database persistence working
- [x] Status tracking accurate
- [x] PDF download working
- [x] Webhook events processed
- [x] Unit tests passing (>80% coverage)
- [x] Error handling comprehensive
- [x] Documentation complete

---

**Created:** 2026-05-30  
**Estimated Completion:** 3-4 hours
