# Subscription Management Flow

## Overview

Complete subscription lifecycle management with Stripe.

## Subscription States

- **TRIALING** - Customer is in trial period
- **ACTIVE** - Subscription is active and billing
- **PAST_DUE** - Payment failed, customer has grace period
- **CANCELED** - Subscription has been cancelled
- **UNPAID** - Subscription is unpaid

## Subscription Lifecycle

```
CREATE SUBSCRIPTION
    ↓
    ├→ TRIALING (if trial period)
    └→ ACTIVE (starts immediately)
        ↓
    [ACTIVE] ← UPDATE (change plan)
        ↓
    [PAYMENT PROCESSING]
        ├→ SUCCESS → [ACTIVE]
        └→ FAILURE → [PAST_DUE]
        ↓
    CANCEL SUBSCRIPTION
        ↓
    [CANCELED]
```

## Operations

### Create Subscription

- Requires: Customer ID, Price ID
- Optional: Trial period, metadata
- Creates recurring billing

### Update Subscription

- Change plan/price
- Update payment method
- Modify metadata

### Cancel Subscription

- Immediate cancellation
- End of billing period option
- Proration handling

---

**More details coming in Phase 4**
