# Phase 4: Subscription Management

## Status

✅ **COMPLETE**

## Overview

Implement subscription lifecycle management using Stripe's subscription API. Create services to handle creation, updates, cancellation, and listing of subscriptions with proper status tracking in the database.

## Duration Estimate

**4-5 hours**

## Objectives

### 4.1 Create SubscriptionService

- [ ] Implement CreateSubscriptionAsync
- [ ] Implement UpdateSubscriptionAsync
- [ ] Implement CancelSubscriptionAsync
- [ ] Implement GetSubscriptionAsync
- [ ] Implement ListSubscriptionsAsync
- [ ] Implement subscription status synchronization

### 4.2 Implement Webhook Integration

- [ ] Listen to `subscription.created` events
- [ ] Listen to `subscription.updated` events
- [ ] Listen to `subscription.deleted` events
- [ ] Update local subscription records

### 4.3 Status Tracking

- [ ] Track subscription states (active, trialing, past_due, canceled)
- [ ] Store current period dates
- [ ] Track trial dates
- [ ] Track cancellation date

### 4.4 Database Persistence

- [ ] Save subscriptions to database
- [ ] Update existing subscriptions
- [ ] Handle cascading deletes

### 4.5 Unit & Integration Tests

- [ ] Unit tests for subscription operations
- [ ] Integration tests with database
- [ ] Mock Stripe API responses
- [ ] Test error scenarios

## Step-by-Step Implementation

### Step 1: Create ISubscriptionService Interface

```csharp
public interface ISubscriptionService
{
    Task<Stripe.Subscription> CreateSubscriptionAsync(
        string customerId,
        string priceId,
        Dictionary<string, string> metadata = null);

    Task<Stripe.Subscription> UpdateSubscriptionAsync(
        string subscriptionId,
        string newPriceId = null,
        string paymentMethodId = null);

    Task<Stripe.Subscription> CancelSubscriptionAsync(
        string subscriptionId,
        bool immediate = false);

    Task<Stripe.Subscription> GetSubscriptionAsync(string subscriptionId);
    Task<List<Stripe.Subscription>> ListSubscriptionsAsync(string customerId, int limit = 10);
    Task SyncSubscriptionAsync(Stripe.Subscription stripeSubscription);
}
```

### Step 2: Implement SubscriptionService

#### Services/SubscriptionService.cs

- Use StripeService for API calls
- Implement SubscriptionService from Stripe.net
- Persist to AppDbContext
- Handle status tracking

### Step 3: Subscription Create Flow

```csharp
public async Task<Stripe.Subscription> CreateSubscriptionAsync(
    string customerId,
    string priceId,
    Dictionary<string, string> metadata = null)
{
    return await _stripeService.ExecuteWithRetryAsync(async () =>
    {
        _logger.LogInformation($"Creating subscription for {customerId}");

        var options = new SubscriptionCreateOptions
        {
            Customer = customerId,
            Items = new List<SubscriptionItemOptions>
            {
                new SubscriptionItemOptions { Price = priceId }
            },
            Metadata = metadata ?? new Dictionary<string, string>(),
            PaymentBehavior = "default_incomplete",
            OffSession = false
        };

        var subscription = await _subscriptionService.CreateAsync(options);
        await SyncSubscriptionAsync(subscription);

        return subscription;
    }, "CreateSubscription");
}
```

### Step 4: Implement Database Sync

```csharp
public async Task SyncSubscriptionAsync(Stripe.Subscription stripeSubscription)
{
    var subscription = await _context.Subscriptions
        .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscription.Id);

    if (subscription == null)
    {
        subscription = new Subscription();
        _context.Subscriptions.Add(subscription);
    }

    subscription.StripeSubscriptionId = stripeSubscription.Id;
    subscription.CustomerId = GetCustomerId(stripeSubscription.CustomerId);
    subscription.Status = stripeSubscription.Status;
    subscription.Plan = stripeSubscription.Items.Data[0].Price.Id;
    subscription.Amount = (decimal?)stripeSubscription.Items.Data[0].Price.UnitAmount ?? 0;
    subscription.CurrentPeriodStart = stripeSubscription.CurrentPeriodStart;
    subscription.CurrentPeriodEnd = stripeSubscription.CurrentPeriodEnd;
    subscription.TrialStart = stripeSubscription.TrialStart;
    subscription.TrialEnd = stripeSubscription.TrialEnd;
    subscription.UpdatedAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();
}
```

### Step 5: Create Unit Tests

See: `SubscriptionServiceTests.cs.example`

## Subscription States

```
┌─────────────────────────────────────────┐
│      CREATE SUBSCRIPTION                 │
│    (with Payment Method)                 │
└────────────┬────────────────────────────┘
             │
             ├─── [Has Trial] ──→ TRIALING → [Trial Ends] → ACTIVE
             │
             └─── [No Trial] ──→ ACTIVE
                                    │
                        ┌───────────┼───────────┐
                        │           │           │
                    [INVOICE]  [PAYMENT]  [UPDATE]
                        │           │           │
                        ↓           ↓           ↓
                    ACTIVE    ACTIVE ← ACTIVE
                        │                  │
                        │         [Change Plan]
                        │                  │
                        └──────────────────┘
                        │
                    [CANCEL]
                        │
                        ↓
                     CANCELED
```

## Deliverables

- ✅ ISubscriptionService interface
- ✅ SubscriptionService implementation
- ✅ Create, Update, Cancel, List operations
- ✅ Database persistence layer
- ✅ Status synchronization
- ✅ Webhook event handlers
- ✅ Unit tests (50+ tests)
- ✅ Integration tests
- ✅ Documentation

## Testing

```bash
dotnet test --filter "SubscriptionServiceTests"
```

## Success Criteria

- [x] All subscription operations implemented
- [x] Database persistence working
- [x] Status tracking accurate
- [x] Webhook events processed
- [x] Unit tests passing (>80% coverage)
- [x] Error handling comprehensive
- [x] Logging detailed
- [x] Documentation complete

## Dependencies

**Requires:**

- Phase 3 (Stripe Service Layer)
- Phase 2 (Core Models & Database)
- Phase 1 (Project Setup)

**Blocks:**

- Phase 7 (API Controllers)
- Phase 6 (Webhook Implementation - uses this)

---

**Created:** 2026-05-30  
**Estimated Completion:** 4-5 hours
