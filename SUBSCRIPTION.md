# Stripe Subscription Management Guide

## Overview

This guide explains how subscriptions work in the Stripe Payment Demo and how to manage them.

## What is a Subscription?

A subscription is a recurring billing arrangement where a customer is charged repeatedly at a set interval (daily, weekly, monthly, or yearly).

**Example Subscription Flow:**

1. Customer creates account and registers payment method
2. Customer chooses a plan (Premium - $99/month)
3. First charge happens immediately
4. Monthly charges continue until subscription is canceled

## Subscription Lifecycle

```
┌─────────────────────────────────────────────────────────────┐
│                    SUBSCRIPTION LIFECYCLE                     │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Created → Active → [Updates] → Canceled → Ended           │
│     ↑        ↑          ↑           ↑         ↑             │
│     │        │          │           │         │             │
│  stripe     payment   upgrade/    customer  final            │
│  creates    charged   downgrade    or       cleanup          │
│  sub                              failed                    │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

## Subscription Statuses

| Status       | Description                                 | Charges    |
| ------------ | ------------------------------------------- | ---------- |
| `active`     | Subscription is active and in good standing | ✅ Yes     |
| `past_due`   | Payment failed but has not been removed     | ⚠️ Pending |
| `canceled`   | Subscription has been canceled              | ❌ No      |
| `unpaid`     | Subscription payment is overdue             | ❌ No      |
| `incomplete` | Initial payment has not been completed      | ❌ Pending |

## Creating a Subscription

### Prerequisites

Before creating a subscription, you need:

1. **A Stripe Customer**

   ```csharp
   // Create customer
   var customer = await _stripeService.CreateCustomerAsync(
       email: "customer@example.com",
       name: "John Doe"
   );
   ```

2. **A Stripe Price**
   - Prices are configured in your Stripe Dashboard
   - Format: `price_xxxxxxxxxxxxx`
   - Can be different amounts or billing intervals
   - Example: `price_1H0nvyH3HkI1E5F9...`

3. **A Payment Method**
   - Customer should have a payment method on file
   - Can be added during subscription creation

### Creating a Subscription

**API Endpoint:** `POST /api/subscription/create`

**Example Request:**

```bash
curl -X POST https://localhost:7001/api/subscription/create \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": 1,
    "stripePriceId": "price_1H0nvyH3HkI1E5F9wXDqKqxL",
    "metadata": {
      "plan_name": "premium",
      "user_tier": "professional"
    }
  }'
```

**Response:**

```json
{
  "message": "Subscription created successfully",
  "subscriptionId": 1,
  "stripeSubscriptionId": "sub_1H0nvyH3HkI1E5F9wXDqKqxL",
  "status": "active"
}
```

### Creating with Payment Method

If you need to add a payment method during subscription creation:

1. Create payment method in Stripe
2. Attach to customer
3. Create subscription with `default_payment_method`

## Subscription Tiers

### Example Pricing Structure

| Tier       | Monthly | Annual  | Price ID              |
| ---------- | ------- | ------- | --------------------- |
| Free       | $0      | $0      | N/A                   |
| Basic      | $9.99   | $99.90  | `price_basic_monthly` |
| Pro        | $49.99  | $499.90 | `price_pro_monthly`   |
| Enterprise | Custom  | Custom  | Contact sales         |

### Creating Tiered Subscriptions

```csharp
// User upgrades from Basic to Pro
var currentSubscription = await _subscriptionService.GetSubscriptionAsync(
    stripeSubscriptionId: "sub_xxxxx"
);

var updatedSubscription = await _subscriptionService.UpdateSubscriptionAsync(
    stripeSubscriptionId: currentSubscription.StripeSubscriptionId,
    newStripePriceId: "price_pro_monthly"
);

// Stripe automatically handles proration:
// If the user is mid-cycle, they'll be credited/charged the difference
```

## Updating a Subscription

You can update a subscription to:

- Change the billing plan/price
- Modify billing interval
- Update payment method
- Add/remove metadata

### Change Plan (Upgrade/Downgrade)

**API Endpoint:** `PUT /api/subscription/{subscriptionId}`

**Example: Upgrade from Basic to Pro**

```bash
curl -X PUT https://localhost:7001/api/subscription/1 \
  -H "Content-Type: application/json" \
  -d '{
    "newStripePriceId": "price_pro_monthly",
    "metadata": {
      "upgrade_reason": "user_requested",
      "upgraded_at": "2026-05-30"
    }
  }'
```

**Response:**

```json
{
  "message": "Subscription updated successfully",
  "subscription": {
    "id": 1,
    "status": "active",
    "stripePriceId": "price_pro_monthly"
  }
}
```

### Proration

When changing plans mid-cycle, Stripe automatically handles proration:

**Example Scenario:**

- Monthly subscription on Day 15 of 30-day cycle
- Current plan: $99/month
- New plan: $199/month
- Pro-rata charge: ~$33 (16 days remaining)

Stripe calculates:

- Credit for unused days on old plan
- Charge for new plan
- Applies difference to next invoice

## Canceling a Subscription

### Immediate Cancellation

Stops billing immediately and prevents future charges.

**API Endpoint:** `POST /api/subscription/{subscriptionId}/cancel`

```bash
curl -X POST https://localhost:7001/api/subscription/1/cancel \
  -H "Content-Type: application/json" \
  -d '{
    "immediately": true
  }'
```

**Response:**

```json
{
  "message": "Subscription canceled successfully",
  "status": "canceled",
  "canceledAt": "2026-05-30T10:30:00Z"
}
```

### Cancel at Period End

Allows current billing period to complete, then cancels.

```bash
curl -X POST https://localhost:7001/api/subscription/1/cancel \
  -H "Content-Type: application/json" \
  -d '{
    "immediately": false
  }'
```

**Example:**

- Current period: May 1 - May 31
- Cancel request: May 15
- Service available: Until May 31
- Cancellation effective: June 1

### Proration on Cancellation

When canceling before the billing period ends:

- If `immediately: true` → No refund (default)
- If `immediately: false` → Completes current billing cycle

## Reactivating Subscriptions

A canceled subscription cannot be reactivated directly. Instead:

1. **Cancel the old subscription** (if not already canceled)
2. **Create a new subscription** with the same or different plan

```csharp
// Get canceled subscription
var canceledSubscription = await _subscriptionService.GetSubscriptionAsync(
    stripeSubscriptionId: "sub_xxxxx"
);

// Create new subscription for same customer
var newSubscription = await _subscriptionService.CreateSubscriptionAsync(
    stripeCustomerId: canceledSubscription.Customer.StripeCustomerId,
    stripePriceId: "price_xxxxx"
);
```

## Handling Payment Failures

When a subscription payment fails:

1. **Webhook Notification**

   ```
   invoice.payment_failed
   ```

2. **Status Changes**
   - Subscription status: `past_due`
   - Invoice status: `open`

3. **Retry Attempts**
   - Stripe automatically retries failed payments
   - Retries continue for up to 3 days

4. **Service Access**
   - Customers can still access the service during `past_due` period
   - After 3 days of failed payment, subscription may be canceled

### Handling Past Due Subscriptions

```csharp
// Get subscriptions in past_due status
var pastDueSubscriptions = await _subscriptionService.ListCustomerSubscriptionsAsync(
    customerId: 1,
    status: "past_due"
);

// Send payment reminder to customer
foreach (var sub in pastDueSubscriptions)
{
    // Send email notification
    // Offer to update payment method
}
```

## Free Trials

Subscriptions can include trial periods:

1. **Create subscription with trial**

   ```json
   {
     "customerId": 1,
     "stripePriceId": "price_monthly",
     "trialDays": 14
   }
   ```

2. **Timeline**
   - Day 0-14: Trial period, no charge
   - Day 15: Trial ends, first charge occurs
   - Day 15+: Regular billing begins

3. **Customer-Initiated Cancellation**
   - If canceled during trial: No charge
   - If canceled after trial starts: Charged for first month

## Subscription Metadata

Store custom information with subscriptions:

```csharp
// Create with metadata
var subscription = await _subscriptionService.CreateSubscriptionAsync(
    stripeCustomerId: "cus_xxxxx",
    stripePriceId: "price_xxxxx",
    metadata: new Dictionary<string, string>
    {
        { "plan_type", "premium" },
        { "user_segment", "enterprise" },
        { "sales_rep", "john@company.com" },
        { "contract_id", "CON-12345" }
    }
);

// Query by metadata (after implementation)
var enterpriseSubscriptions = await _dbContext.Subscriptions
    .Where(s => s.Metadata.Contains("\"plan_type\":\"enterprise\""))
    .ToListAsync();
```

## Listing Subscriptions

### Get Customer Subscriptions

**API Endpoint:** `GET /api/subscription/customer/{customerId}`

```bash
curl -X GET "https://localhost:7001/api/subscription/customer/1?status=active" \
  -H "Content-Type: application/json"
```

**Response:**

```json
{
  "data": [
    {
      "id": 1,
      "stripeSubscriptionId": "sub_xxxxx",
      "status": "active",
      "currentPeriodStart": "2026-05-01T00:00:00Z",
      "currentPeriodEnd": "2026-06-01T00:00:00Z",
      "amount": 9999,
      "billingInterval": "month"
    }
  ],
  "count": 1
}
```

### Filter by Status

```bash
# Get only active subscriptions
curl -X GET "https://localhost:7001/api/subscription/customer/1?status=active"

# Get canceled subscriptions
curl -X GET "https://localhost:7001/api/subscription/customer/1?status=canceled"

# Get past_due subscriptions
curl -X GET "https://localhost:7001/api/subscription/customer/1?status=past_due"
```

## Syncing Subscription Status

To synchronize subscription status between Stripe and your database:

**API Endpoint:** `POST /api/subscription/{subscriptionId}/sync`

```bash
curl -X POST https://localhost:7001/api/subscription/1/sync \
  -H "Content-Type: application/json"
```

**When to Sync:**

- After Stripe webhook processing
- When displaying subscription details
- When billing cycle changes
- When checking payment status

## Webhook Events

Subscriptions trigger several webhook events:

### `subscription.created`

Triggered when subscription is first created.

**Example Payload:**

```json
{
  "type": "subscription.created",
  "data": {
    "object": {
      "id": "sub_xxxxx",
      "customer": "cus_xxxxx",
      "status": "active",
      "current_period_start": 1627658400,
      "current_period_end": 1630337200
    }
  }
}
```

### `subscription.updated`

Triggered when subscription is modified (plan change, metadata update, etc.).

### `subscription.deleted`

Triggered when subscription is canceled.

### `invoice.payment_succeeded`

Triggered when automatic payment succeeds.

### `invoice.payment_failed`

Triggered when automatic payment fails.

## Best Practices

### 1. Always Validate Plans

```csharp
// Verify price exists and is active before creating subscription
var price = await stripeService.GetPriceAsync(stripePriceId);
if (price == null || price.Deleted)
{
    throw new ArgumentException("Invalid price ID");
}
```

### 2. Handle Exceptions

```csharp
try
{
    await _subscriptionService.CreateSubscriptionAsync(...);
}
catch (StripeException ex) when (ex.HttpStatusCode == 402)
{
    // Payment method declined
    // Update UI to request new payment method
}
catch (StripeException ex)
{
    // Log and show generic error
    _logger.LogError($"Stripe error: {ex.Message}");
}
```

### 3. Use Idempotency

```csharp
// Prevent duplicate subscriptions if request is retried
var idempotencyKey = $"subscription-{customerId}-{stripePriceId}";
// Store and check this key in database
```

### 4. Sync Regularly

```csharp
// Periodically sync subscriptions from Stripe
using (var timer = new Timer(async _ =>
{
    await _subscriptionService.SyncAllSubscriptionsAsync();
}, null, TimeSpan.Zero, TimeSpan.FromHours(1)))
{
    // Runs every hour
}
```

### 5. Monitor Failed Payments

```sql
-- Get subscriptions with recent failed payments
SELECT s.*, COUNT(i.Id) as FailedInvoiceCount
FROM Subscriptions s
LEFT JOIN Invoices i ON s.Id = i.SubscriptionId
WHERE s.Status = 'past_due'
  AND i.Status = 'payment_failed'
  AND i.UpdatedAt > DATEADD(day, -7, GETDATE())
GROUP BY s.Id;
```

## Advanced Scenarios

### Concurrent Subscriptions

A customer can have multiple subscriptions:

```csharp
// Customer has Basic plan and add-on
var subscriptions = await _subscriptionService.ListCustomerSubscriptionsAsync(
    customerId: 1
);

// subscriptions.Count could be 2 or more
```

### Moving Between Plans

Upgrade path: Free → Basic → Pro → Enterprise

```csharp
// Get current subscription
var currentSub = subscriptions.First(s => s.Status == "active");

// Downgrade after free trial
var downgradeAmount = Math.Max(0,
    currentSub.Amount - newPrice
);

// Stripe handles proration automatically
```

### Usage-Based Billing

For usage-based pricing, use metered billing:

1. Create metered price in Stripe
2. Report usage via API
3. Stripe calculates charge based on usage

```csharp
// Report usage (Phase 10 enhancement)
await stripeService.ReportUsageAsync(
    subscriptionItemId: "si_xxxxx",
    quantity: 100,
    action: "increment"
);
```

## Testing Subscriptions

### Test Card Numbers

Stripe provides test card numbers for different scenarios:

| Card      | Number                | Status             |
| --------- | --------------------- | ------------------ |
| Success   | `4242 4242 4242 4242` | Charges succeed    |
| Decline   | `4000 0000 0000 0002` | Charges declined   |
| 3D Secure | `4000 0025 0000 3155` | Requires 3D Secure |
| Expired   | `4000 0000 0000 0069` | Card expired       |

### Test Subscription Lifecycle

```bash
# Using Stripe CLI
stripe trigger subscription.created
stripe trigger subscription.updated
stripe trigger invoice.payment_succeeded
stripe trigger subscription.deleted
```

## Troubleshooting

### Subscription Not Charging

**Check:**

1. Payment method is valid and not expired
2. Subscription status is `active` (not `past_due` or `canceled`)
3. Trial period has ended
4. Billing cycle date hasn't passed

### Webhook Not Processing

**Check:**

1. Webhook signing secret is correct
2. Application is running
3. Webhook endpoint is publicly accessible
4. Firewall allows incoming requests

### Proration Not Applied

**Check:**

1. Proration is enabled in Stripe settings
2. Plan change happens mid-cycle (not on billing date)
3. New price is different from current price

## Additional Resources

- [Stripe Subscriptions API](https://stripe.com/docs/api/subscriptions)
- [Stripe Billing Guide](https://stripe.com/docs/billing)
- [Subscription Lifecycle](https://stripe.com/docs/billing/lifecycle)
- [Proration Settings](https://stripe.com/docs/billing/subscriptions/billing-cycle)

---

**Last Updated**: 2026-05-30
**Status**: Production Ready (Phase 4 Complete)
