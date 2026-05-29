# Webhook Setup & Configuration

## Overview

Guide for configuring and testing Stripe webhooks in this project.

## Stripe Webhook Events

The following events are handled by this project:

- `customer.created` - New customer created
- `subscription.created` - New subscription created
- `subscription.updated` - Subscription updated
- `subscription.deleted` - Subscription cancelled
- `invoice.created` - New invoice created
- `invoice.payment_succeeded` - Invoice payment successful
- `invoice.payment_failed` - Invoice payment failed

## Setting Up Webhooks

1. Go to Stripe Dashboard → Developers → Webhooks
2. Add endpoint: `https://yourdomain.com/api/webhooks/stripe`
3. Select events listed above
4. Copy signing secret to `appsettings.json`

## Webhook Security

All webhooks are validated using Stripe's signature verification to ensure authenticity.

---

**More details coming in Phase 6**
