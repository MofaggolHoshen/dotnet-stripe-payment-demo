# Stripe Webhook Implementation Guide

## Overview

This guide explains how webhooks work in the Stripe Payment Demo and how to set them up.

## What are Webhooks?

Webhooks are HTTP callbacks that Stripe uses to notify your application about events in real-time. For example:

- A customer is created
- A subscription is updated
- An invoice payment succeeded
- An invoice payment failed

Instead of polling Stripe for updates, webhooks push notifications to your application.

## Webhook Endpoint

**URL:** `POST /api/webhook/stripe`

**Headers Required:**

- `Stripe-Signature` - HMAC-SHA256 signature for verification

## Setting Up Webhooks

### Step 1: Get Your Webhook Signing Secret

1. Go to Stripe Dashboard: https://dashboard.stripe.com
2. Navigate to **Developers** → **Webhooks**
3. Click "Add endpoint"
4. Enter your webhook URL: `https://yourdomain.com/api/webhook/stripe`
5. Select events you want to receive (see Supported Events below)
6. Click "Add endpoint"
7. Copy the **Signing Secret** (starts with `whsec_`)

### Step 2: Configure the Secret in Your Application

Update `source/appsettings.json`:

```json
{
  "Stripe": {
    "WebhookSecret": "whsec_test_your_webhook_secret_here"
  }
}
```

Or set environment variable:

```bash
export Stripe__WebhookSecret="whsec_test_your_webhook_secret_here"
```

### Step 3: Local Testing with ngrok

To test webhooks locally:

1. Install ngrok from https://ngrok.com/download
2. Run your application locally
3. In a new terminal, run:
   ```bash
   ngrok http 7001
   ```
4. Copy the HTTPS URL (e.g., `https://abc123.ngrok.io`)
5. Update your Stripe webhook endpoint to: `https://abc123.ngrok.io/api/webhook/stripe`

### Step 4: Testing with Stripe CLI (Recommended)

Stripe CLI is the easiest way to test webhooks:

1. Install Stripe CLI: https://stripe.com/docs/stripe-cli
2. Login:
   ```bash
   stripe login
   ```
3. Forward events to your local app:
   ```bash
   stripe listen --forward-to localhost:7001/api/webhook/stripe
   ```
4. In another terminal, trigger test events:
   ```bash
   stripe trigger customer.created
   stripe trigger subscription.created
   stripe trigger invoice.created
   ```

## Supported Events

The following Stripe events are currently supported:

### Customer Events

- **`customer.created`** - A new customer is created
- **`customer.updated`** - A customer is updated
- **`customer.deleted`** - A customer is deleted

### Subscription Events

- **`subscription.created`** - A subscription is created
- **`subscription.updated`** - A subscription is updated (status, plan, etc.)
- **`subscription.deleted`** - A subscription is deleted/canceled

### Invoice Events

- **`invoice.created`** - An invoice is created
- **`invoice.payment_succeeded`** - Invoice payment succeeds
- **`invoice.payment_failed`** - Invoice payment fails
- **`invoice.finalized`** - Invoice is finalized (ready to send)

## Webhook Signature Verification

The application automatically verifies webhook signatures using HMAC-SHA256:

1. Stripe sends a `Stripe-Signature` header containing:
   - Timestamp (t=...)
   - Signatures (v1=..., v0=...)
2. The application reconstructs the signature using:
   - Request body
   - Timestamp
   - Webhook signing secret
3. If signatures match, the webhook is valid

This prevents malicious actors from sending fake webhooks to your endpoint.

## Webhook Event Payload

Example webhook event structure:

```json
{
  "id": "evt_1H0nvyH3HkI1E5F9wXDqKqxL",
  "object": "event",
  "api_version": "2020-08-27",
  "created": 1627658400,
  "data": {
    "object": {
      "id": "sub_1H0nvyH3HkI1E5F9wXDqKqxL",
      "object": "subscription",
      "status": "active",
      "customer": "cus_1H0nviH3HkI1E5F9wXDqKqxL"
    }
  },
  "livemode": false,
  "pending_webhooks": 1,
  "request": {
    "id": null,
    "idempotency_key": null
  },
  "type": "subscription.created"
}
```

## Event Processing Flow

```
Stripe Server
    ↓
POST /api/webhook/stripe (with Stripe-Signature header)
    ↓
WebhookController.HandleStripeWebhook()
    ↓
Verify Signature → Validate Event
    ↓
WebhookService.ProcessWebhookAsync()
    ↓
Route to Handler (customer.created, subscription.updated, etc.)
    ↓
Service Layer (SubscriptionService, InvoiceService, etc.)
    ↓
Update Database + Stripe
    ↓
Log Event + Return 200 OK
```

## Event Handlers

### Customer Events

**`customer.created`**

- Logs the event
- Customer data synced from webhook data

**`customer.updated`**

- Updates customer record in database
- Refreshes cached customer data

**`customer.deleted`**

- Removes customer from database
- Cascades delete to subscriptions and invoices

### Subscription Events

**`subscription.created`**

- Creates subscription record in database
- Stores Stripe subscription ID and status
- Syncs plan and pricing information

**`subscription.updated`**

- Updates subscription status
- Updates billing period dates
- Handles plan changes
- Updates pricing information

**`subscription.deleted`**

- Marks subscription as canceled
- Updates cancellation date
- Sends notification

### Invoice Events

**`invoice.created`**

- Creates invoice record in database
- Stores invoice number and details
- Tracks due date and amount

**`invoice.payment_succeeded`**

- Marks invoice as paid
- Updates payment date
- Updates customer payment method (if applicable)

**`invoice.payment_failed`**

- Marks invoice as failed
- Logs failure reason
- Could trigger retry logic (Phase 10)

**`invoice.finalized`**

- Updates invoice status
- Syncs final amounts
- Ready for payment collection

## Handling Failures

### Failed Event Processing

If an event fails to process:

1. Event is logged with status `failed`
2. Error message is stored
3. Processing attempts are tracked
4. Future retries can be configured

Example:

```sql
SELECT * FROM WebhookEvents
WHERE Status = 'failed'
ORDER BY UpdatedAt DESC;
```

### Replay Failed Events

To retry failed webhook events:

```csharp
// In WebhookController
var failedEvent = _dbContext.WebhookEvents
    .Where(e => e.Status == "failed")
    .First();

await _webhookService.ProcessWebhookAsync(failedEvent.EventData, signature);
```

### Dead Letter Queue

Events that consistently fail should be investigated:

1. Check application logs
2. Verify Stripe API key
3. Check database connectivity
4. Review event data format
5. Manually process if necessary

## Best Practices

### 1. Idempotency

Webhooks may be delivered multiple times. Your handlers should be idempotent:

```csharp
// ❌ Bad: Multiple processing causes issues
var invoice = await _invoiceService.MarkInvoiceAsPaidAsync(invoiceId);

// ✅ Good: Check current state before updating
if (existingInvoice.Status != "paid")
{
    await _invoiceService.MarkInvoiceAsPaidAsync(invoiceId);
}
```

### 2. Quick Response

Always respond with `200 OK` immediately:

```csharp
// ✅ Good: Respond immediately
return Ok(new { message = "Webhook received" });

// Process asynchronously
_ = Task.Run(() => ProcessWebhookAsync(data));
```

### 3. Idempotency Keys

For critical operations, use Stripe's idempotency feature:

```csharp
var options = new SubscriptionCreateOptions
{
    Customer = customerId,
    Items = new List<SubscriptionItemOptions> { ... }
};

// Add idempotency key to prevent duplicate processing
var requestOptions = new RequestOptions();
requestOptions.IdempotencyKey = webhookEvent.Id;

var subscription = await SubscriptionService.CreateAsync(options, requestOptions);
```

### 4. Logging

Always log webhook events for debugging:

```csharp
_logger.LogInformation($"Processing webhook: {eventType} for {eventId}");
_logger.LogError($"Webhook processing failed: {errorMessage}");
```

### 5. Error Handling

Catch and log specific errors:

```csharp
try
{
    await ProcessEventAsync(stripeEvent);
}
catch (StripeException ex)
{
    _logger.LogError($"Stripe API error: {ex.Message}");
}
catch (DbUpdateException ex)
{
    _logger.LogError($"Database error: {ex.Message}");
}
catch (Exception ex)
{
    _logger.LogError($"Unexpected error: {ex.Message}");
}
```

## Testing Webhooks

### Manual Testing

1. Trigger event via Stripe CLI:

   ```bash
   stripe trigger subscription.updated
   ```

2. Check webhook logs:

   ```bash
   stripe logs
   ```

3. Verify in database:
   ```sql
   SELECT * FROM WebhookEvents
   WHERE StripeEventId = 'evt_xxxxx';
   ```

### Automated Testing

Create unit tests for webhook handlers:

```csharp
[Fact]
public async Task HandleSubscriptionCreatedAsync_WithValidEvent_ShouldCreateSubscription()
{
    // Arrange
    var webhookData = /* ... */;
    var stripeEvent = new Event
    {
        Type = "subscription.created",
        Data = webhookData
    };

    // Act
    var result = await _webhookService.ProcessWebhookAsync(json, signature);

    // Assert
    Assert.True(result);
    var createdSubscription = _dbContext.Subscriptions.FirstOrDefault();
    Assert.NotNull(createdSubscription);
}
```

## Webhook Retries

Stripe automatically retries failed webhooks for 3 days:

- 1st attempt: Immediately
- 2nd attempt: 5 minutes later
- 3rd attempt: 30 minutes later
- 4th attempt: 2 hours later
- 5th attempt: 5 hours later
- 6th attempt: 10 hours later
- 7th attempt: 24 hours later

You can manually retry failed webhooks in the Stripe Dashboard.

## Monitoring Webhooks

### Health Checks

Monitor webhook delivery success:

```sql
-- Check webhook delivery rate
SELECT
    EventType,
    COUNT(*) as Total,
    SUM(CASE WHEN Status = 'processed' THEN 1 ELSE 0 END) as Processed,
    SUM(CASE WHEN Status = 'failed' THEN 1 ELSE 0 END) as Failed
FROM WebhookEvents
WHERE ReceivedAt > DATEADD(day, -7, GETDATE())
GROUP BY EventType;
```

### Alerts

Set up alerts for:

- Failed webhook processing
- Webhook processing delays
- Duplicate webhook events
- Missing expected webhooks

## Troubleshooting

### Webhooks Not Being Received

**Check:**

1. Endpoint is publicly accessible
2. HTTPS is enabled
3. Firewall allows incoming requests
4. DNS is resolving correctly
5. Webhook endpoint is registered in Stripe Dashboard

### Signature Verification Fails

**Check:**

1. Webhook signing secret is correct
2. Signing secret hasn't been rotated
3. Request body hasn't been modified
4. No middleware is altering the request

### Events Are Not Being Processed

**Check:**

1. Application logs for errors
2. Database connectivity
3. Event payload format
4. Event data contains required fields
5. Service layer can access external APIs

### Performance Issues

**Optimize:**

1. Move processing to background job queue
2. Implement caching for frequently accessed data
3. Use database indexes on webhook fields
4. Implement rate limiting for high-volume events

## Security

### Signature Verification

✅ Always verify webhook signatures using the signing secret

### HTTPS

✅ Always use HTTPS for webhook endpoints

### Rate Limiting

⏳ To be implemented in Phase 9

### Authentication

⏳ To be implemented in Phase 9

## Additional Resources

- [Stripe Webhooks Documentation](https://stripe.com/docs/webhooks)
- [Stripe Event Types](https://stripe.com/docs/api/events/types)
- [Stripe CLI Documentation](https://stripe.com/docs/stripe-cli)
- [Webhook Security](https://stripe.com/docs/webhooks/signatures)

---

**Last Updated**: 2026-05-30
**Status**: Production Ready (Phase 6 Complete)
