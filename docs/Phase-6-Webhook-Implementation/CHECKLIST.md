# Phase 6 Checklist

## WebhookService Implementation

- [ ] IWebhookService interface created
- [ ] WebhookService class implemented
- [ ] VerifyWebhookSignature implemented
- [ ] ProcessEventAsync implemented
- [ ] RouteEventAsync implemented
- [ ] Timestamp verification (5-minute window)

## Signature Verification

- [ ] HMAC-SHA256 signature computation
- [ ] Signature comparison (secure comparison)
- [ ] Timestamp extraction from signature
- [ ] Timestamp validation
- [ ] Replay attack prevention

## Event Handlers

- [ ] IWebhookHandler interface created
- [ ] CustomerCreatedHandler implemented
- [ ] SubscriptionCreatedHandler implemented
- [ ] SubscriptionUpdatedHandler implemented
- [ ] SubscriptionDeletedHandler implemented
- [ ] InvoiceCreatedHandler implemented
- [ ] InvoicePaymentSucceededHandler implemented
- [ ] InvoicePaymentFailedHandler implemented

## WebhookController

- [ ] POST /api/webhook/stripe endpoint created
- [ ] Request body reading implemented
- [ ] Signature header extraction
- [ ] Signature verification called
- [ ] Event processing called
- [ ] 200 OK response on success
- [ ] 401 Unauthorized on invalid signature

## Event Persistence

- [ ] WebhookEvent model used correctly
- [ ] Event ID stored (Stripe event ID)
- [ ] Event type stored
- [ ] Event data stored (JSON)
- [ ] Status tracked (pending/processed/failed)
- [ ] Timestamps recorded
- [ ] Retry count tracked

## Event Processing

- [ ] Events routed to correct handler
- [ ] Handler executed asynchronously
- [ ] Exceptions caught and logged
- [ ] Status updated after processing
- [ ] ProcessedAt timestamp set
- [ ] Error messages logged

## Retry Logic

- [ ] Failed events marked for retry
- [ ] Retry count incremented
- [ ] Retry mechanism implemented
- [ ] Max retry limit enforced
- [ ] Dead letter queue for failed events

## Error Handling

- [ ] Invalid signatures rejected (401)
- [ ] Malformed JSON handled gracefully
- [ ] Handler exceptions logged
- [ ] Database errors handled
- [ ] Missing handlers handled

## Unit Tests

- [ ] VerifyWebhookSignature tests
  - [ ] Valid signature test
  - [ ] Invalid signature test
  - [ ] Expired timestamp test
  - [ ] Malformed signature test
- [ ] Event handler tests
  - [ ] subscription.created test
  - [ ] subscription.updated test
  - [ ] invoice.payment_succeeded test
  - [ ] Other event tests
- [ ] Error scenario tests
  - [ ] Invalid JSON test
  - [ ] Missing fields test
  - [ ] Exception in handler test

## Integration Tests

- [ ] End-to-end webhook flow
- [ ] Database persistence verified
- [ ] Event status transitions correct
- [ ] Webhook signature validation with real Stripe format

## Logging

- [ ] Info log on webhook received
- [ ] Debug log on signature verification
- [ ] Warning log on invalid signature
- [ ] Error log on handler exception
- [ ] All logs contain event ID and type

## Configuration

- [ ] WebhookSigningSecret in appsettings
- [ ] Secret never logged
- [ ] Configuration validated on startup

## Verification

- [ ] `dotnet build` succeeds
- [ ] All tests pass
- [ ] Stripe CLI can forward to endpoint
- [ ] Events processed correctly
- [ ] Database updated from webhooks
- [ ] Replay attacks prevented

## Documentation

- [ ] README.md with full instructions
- [ ] Event handler examples provided
- [ ] Webhook setup guide created
- [ ] Signature verification explained
- [ ] Testing instructions provided
- [ ] Deployment considerations noted

## Sign Off

- [ ] All items complete
- [ ] Webhook service tested and working
- [ ] Security verified
- [ ] Ready for Phase 7
- [ ] Changes committed to Git

**Completed By:** ******\_\_\_******  
**Date:** ******\_\_\_******
