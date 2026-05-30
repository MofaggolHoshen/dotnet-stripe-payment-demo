using DotnetStripePaymentDemo.Data;
using DotnetStripePaymentDemo.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stripe;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DotnetStripePaymentDemo.Services
{
    /// <summary>
    /// Service for handling Stripe webhook events
    /// Validates webhook signatures and processes various event types
    /// </summary>
    public class WebhookService
    {
        private readonly AppDbContext _dbContext;
        private readonly SubscriptionService _subscriptionService;
        private readonly InvoiceService _invoiceService;
        private readonly ILogger<WebhookService> _logger;
        private readonly string _webhookSecret;

        public WebhookService(
            AppDbContext dbContext,
            SubscriptionService subscriptionService,
            InvoiceService invoiceService,
            ILogger<WebhookService> logger,
            string webhookSecret)
        {
            _dbContext = dbContext;
            _subscriptionService = subscriptionService;
            _invoiceService = invoiceService;
            _logger = logger;
            _webhookSecret = webhookSecret;
        }

        /// <summary>
        /// Validates and processes a webhook event from Stripe
        /// </summary>
        public async Task<bool> ProcessWebhookAsync(string json, string signatureHeader)
        {
            try
            {
                // Verify the webhook signature
                Event stripeEvent;
                try
                {
                    stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, _webhookSecret);
                    _logger.LogInformation($"Webhook verified: {stripeEvent.Type}");
                }
                catch (StripeException ex)
                {
                    _logger.LogError($"Webhook signature verification failed: {ex.Message}");
                    return false;
                }

                // Save the webhook event to database
                var webhookEvent = new WebhookEvent
                {
                    StripeEventId = stripeEvent.Id,
                    EventType = stripeEvent.Type,
                    ApiVersion = stripeEvent.ApiVersion,
                    EventData = json,
                    Status = "pending",
                    ReceivedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.WebhookEvents.Add(webhookEvent);
                await _dbContext.SaveChangesAsync();

                // Process the event
                bool processed = await HandleWebhookEventAsync(stripeEvent);

                // Update event status
                webhookEvent.Status = processed ? "processed" : "failed";
                webhookEvent.ProcessedAt = DateTime.UtcNow;
                webhookEvent.UpdatedAt = DateTime.UtcNow;
                _dbContext.WebhookEvents.Update(webhookEvent);
                await _dbContext.SaveChangesAsync();

                return processed;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing webhook: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Handles specific webhook event types
        /// </summary>
        private async Task<bool> HandleWebhookEventAsync(Event stripeEvent)
        {
            try
            {
                return stripeEvent.Type switch
                {
                    "customer.created" => await HandleCustomerCreatedAsync(stripeEvent),
                    "customer.updated" => await HandleCustomerUpdatedAsync(stripeEvent),
                    "customer.deleted" => await HandleCustomerDeletedAsync(stripeEvent),
                    
                    "subscription.created" => await HandleSubscriptionCreatedAsync(stripeEvent),
                    "subscription.updated" => await HandleSubscriptionUpdatedAsync(stripeEvent),
                    "subscription.deleted" => await HandleSubscriptionDeletedAsync(stripeEvent),
                    
                    "invoice.created" => await HandleInvoiceCreatedAsync(stripeEvent),
                    "invoice.payment_succeeded" => await HandleInvoicePaymentSucceededAsync(stripeEvent),
                    "invoice.payment_failed" => await HandleInvoicePaymentFailedAsync(stripeEvent),
                    "invoice.finalized" => await HandleInvoiceFinalizedAsync(stripeEvent),
                    
                    _ => await HandleUnknownEventAsync(stripeEvent)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling webhook event {stripeEvent.Type}: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> HandleCustomerCreatedAsync(Event stripeEvent)
        {
            _logger.LogInformation("Processing customer.created webhook");
            // Customer creation is typically handled synchronously, but we can log it here
            return await Task.FromResult(true);
        }

        private async Task<bool> HandleCustomerUpdatedAsync(Event stripeEvent)
        {
            _logger.LogInformation("Processing customer.updated webhook");
            return await Task.FromResult(true);
        }

        private async Task<bool> HandleCustomerDeletedAsync(Event stripeEvent)
        {
            _logger.LogInformation("Processing customer.deleted webhook");
            return await Task.FromResult(true);
        }

        private async Task<bool> HandleSubscriptionCreatedAsync(Event stripeEvent)
        {
            _logger.LogInformation("Processing subscription.created webhook");
            try
            {
                var subscription = stripeEvent.Data.Object as Stripe.Subscription;
                if (subscription == null)
                    return false;

                // Check if subscription already exists in database
                var existingSubscription = await _subscriptionService.GetSubscriptionAsync(subscription.Id);
                if (existingSubscription != null)
                {
                    return true; // Already exists
                }

                // Sync from Stripe (this will create the subscription record)
                // Implementation would depend on your sync strategy
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling subscription.created: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> HandleSubscriptionUpdatedAsync(Event stripeEvent)
        {
            _logger.LogInformation("Processing subscription.updated webhook");
            try
            {
                var subscription = stripeEvent.Data.Object as Stripe.Subscription;
                if (subscription == null)
                    return false;

                await _subscriptionService.SyncSubscriptionAsync(subscription.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling subscription.updated: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> HandleSubscriptionDeletedAsync(Event stripeEvent)
        {
            _logger.LogInformation("Processing subscription.deleted webhook");
            try
            {
                var subscription = stripeEvent.Data.Object as Stripe.Subscription;
                if (subscription == null)
                    return false;

                await _subscriptionService.SyncSubscriptionAsync(subscription.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling subscription.deleted: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> HandleInvoiceCreatedAsync(Event stripeEvent)
        {
            _logger.LogInformation("Processing invoice.created webhook");
            try
            {
                var invoice = stripeEvent.Data.Object as Stripe.Invoice;
                if (invoice == null)
                    return false;

                await _invoiceService.SyncInvoiceAsync(invoice.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling invoice.created: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> HandleInvoicePaymentSucceededAsync(Event stripeEvent)
        {
            _logger.LogInformation("Processing invoice.payment_succeeded webhook");
            try
            {
                var invoice = stripeEvent.Data.Object as Stripe.Invoice;
                if (invoice == null)
                    return false;

                await _invoiceService.MarkInvoiceAsPaidAsync(invoice.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling invoice.payment_succeeded: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> HandleInvoicePaymentFailedAsync(Event stripeEvent)
        {
            _logger.LogInformation("Processing invoice.payment_failed webhook");
            try
            {
                var invoice = stripeEvent.Data.Object as Stripe.Invoice;
                if (invoice == null)
                    return false;

                await _invoiceService.MarkInvoiceAsFailedAsync(invoice.Id, "payment_failed");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling invoice.payment_failed: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> HandleInvoiceFinalizedAsync(Event stripeEvent)
        {
            _logger.LogInformation("Processing invoice.finalized webhook");
            try
            {
                var invoice = stripeEvent.Data.Object as Stripe.Invoice;
                if (invoice == null)
                    return false;

                await _invoiceService.SyncInvoiceAsync(invoice.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling invoice.finalized: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> HandleUnknownEventAsync(Event stripeEvent)
        {
            _logger.LogInformation($"Received unhandled webhook event type: {stripeEvent.Type}");
            return await Task.FromResult(true); // Return true to acknowledge receipt
        }
    }
}
