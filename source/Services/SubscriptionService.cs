using DotnetStripePaymentDemo.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stripe;
using StripeSubscription = Stripe.Subscription;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetStripePaymentDemo.Services
{
    /// <summary>
    /// Service for managing subscriptions via Stripe API
    /// Handles subscription lifecycle: creation, updates, cancellation
    /// </summary>
    public class SubscriptionService
    {
        private readonly AppDbContext _dbContext;
        private readonly StripeService _stripeService;
        private readonly ILogger<SubscriptionService> _logger;

        public SubscriptionService(AppDbContext dbContext, StripeService stripeService, ILogger<SubscriptionService> logger)
        {
            _dbContext = dbContext;
            _stripeService = stripeService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new subscription for a customer
        /// </summary>
        public async Task<DotnetStripePaymentDemo.Models.Subscription> CreateSubscriptionAsync(string stripeCustomerId, string stripePriceId, Dictionary<string, string>? metadata = null)
        {
            try
            {
                _logger.LogInformation($"Creating subscription for customer {stripeCustomerId} with price {stripePriceId}");

                // Create subscription in Stripe
                var options = new SubscriptionCreateOptions
                {
                    Customer = stripeCustomerId,
                    Items = new List<SubscriptionItemOptions>
                    {
                        new SubscriptionItemOptions { Price = stripePriceId }
                    },
                    Metadata = metadata,
                    PaymentBehavior = "default_incomplete"
                };

                var subscriptionService = new Stripe.SubscriptionService();
                var stripeSubscription = await subscriptionService.CreateAsync(options);

                // Get price and product information
                var priceService = new PriceService();
                var price = await priceService.GetAsync(stripePriceId);

                // Save to database
                var subscription = new DotnetStripePaymentDemo.Models.Subscription
                {
                    StripeSubscriptionId = stripeSubscription.Id,
                    CustomerId = await GetCustomerIdAsync(stripeCustomerId),
                    StripePriceId = stripePriceId,
                    StripeProductId = price.ProductId,
                    Status = stripeSubscription.Status,
                    CurrentPeriodStart = stripeSubscription.CurrentPeriodStart,
                    CurrentPeriodEnd = stripeSubscription.CurrentPeriodEnd,
                    Amount = price.UnitAmountDecimal.HasValue ? (long)price.UnitAmountDecimal.Value : 0,
                    BillingInterval = price.Recurring?.Interval,
                    BillingIntervalCount = (int?)(price.Recurring?.IntervalCount ?? 0),
                    Metadata = System.Text.Json.JsonSerializer.Serialize(metadata),
                    StripeCreatedAt = stripeSubscription.Created,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.Subscriptions.Add(subscription);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Subscription created: {stripeSubscription.Id}");
                return subscription;
            }
            catch (StripeException ex)
            {
                _logger.LogError($"Stripe error creating subscription: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating subscription: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a subscription by Stripe subscription ID
        /// </summary>
        public async Task<DotnetStripePaymentDemo.Models.Subscription?> GetSubscriptionAsync(string stripeSubscriptionId)
        {
            try
            {
                _logger.LogInformation($"Retrieving subscription: {stripeSubscriptionId}");
                
                var subscription = await _dbContext.Subscriptions
                    .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscriptionId);

                return subscription;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving subscription: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Lists all subscriptions for a customer
        /// </summary>
        public async Task<List<DotnetStripePaymentDemo.Models.Subscription>> ListCustomerSubscriptionsAsync(int customerId, string? status = null)
        {
            try
            {
                _logger.LogInformation($"Listing subscriptions for customer {customerId}");
                
                var query = _dbContext.Subscriptions
                    .Where(s => s.CustomerId == customerId);

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(s => s.Status == status);
                }

                var subscriptions = await query.ToListAsync();
                return subscriptions;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error listing subscriptions: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Updates a subscription (e.g., change plan)
        /// </summary>
        public async Task<DotnetStripePaymentDemo.Models.Subscription> UpdateSubscriptionAsync(string stripeSubscriptionId, string? newStripePriceId = null, Dictionary<string, string>? metadata = null)
        {
            try
            {
                _logger.LogInformation($"Updating subscription: {stripeSubscriptionId}");

                // Get current subscription from database
                var subscription = await GetSubscriptionAsync(stripeSubscriptionId)
                    ?? throw new KeyNotFoundException($"Subscription not found: {stripeSubscriptionId}");

                // Build update options
                var updateOptions = new SubscriptionUpdateOptions();

                if (!string.IsNullOrEmpty(newStripePriceId) && newStripePriceId != subscription.StripePriceId)
                {
                    // Get the subscription items to update the price
                    var stripeSubscriptionService = new Stripe.SubscriptionService();
                    var stripeSubscription = await stripeSubscriptionService.GetAsync(stripeSubscriptionId);
                    var itemId = stripeSubscription.Items.Data.FirstOrDefault()?.Id;

                    if (!string.IsNullOrEmpty(itemId))
                    {
                        updateOptions.Items = new List<SubscriptionItemOptions>
                        {
                            new SubscriptionItemOptions
                            {
                                Id = itemId,
                                Price = newStripePriceId
                            }
                        };
                    }

                    subscription.StripePriceId = newStripePriceId;
                }

                if (metadata != null)
                {
                    updateOptions.Metadata = metadata;
                }

                // Update in Stripe
                var stripeSubscriptionService2 = new Stripe.SubscriptionService();
                var updatedStripeSubscription = await stripeSubscriptionService2.UpdateAsync(stripeSubscriptionId, updateOptions);

                // Update in database
                subscription.Status = updatedStripeSubscription.Status;
                subscription.Metadata = System.Text.Json.JsonSerializer.Serialize(metadata);
                subscription.UpdatedAt = DateTime.UtcNow;

                _dbContext.Subscriptions.Update(subscription);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Subscription updated: {stripeSubscriptionId}");
                return subscription;
            }
            catch (StripeException ex)
            {
                _logger.LogError($"Stripe error updating subscription: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating subscription: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Cancels a subscription
        /// </summary>
        public async Task<DotnetStripePaymentDemo.Models.Subscription> CancelSubscriptionAsync(string stripeSubscriptionId, bool immediately = false)
        {
            try
            {
                _logger.LogInformation($"Canceling subscription: {stripeSubscriptionId} (immediately: {immediately})");

                var subscription = await GetSubscriptionAsync(stripeSubscriptionId)
                    ?? throw new KeyNotFoundException($"Subscription not found: {stripeSubscriptionId}");

                var options = new SubscriptionCancelOptions
                {
                    Prorate = !immediately
                };

                var stripeSubscriptionService = new Stripe.SubscriptionService();
                var canceledStripeSubscription = await stripeSubscriptionService.CancelAsync(stripeSubscriptionId, options);

                subscription.Status = canceledStripeSubscription.Status;
                subscription.CanceledAt = DateTime.UtcNow;
                subscription.EndedAt = canceledStripeSubscription.EndedAt;
                subscription.UpdatedAt = DateTime.UtcNow;

                _dbContext.Subscriptions.Update(subscription);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Subscription canceled: {stripeSubscriptionId}");
                return subscription;
            }
            catch (StripeException ex)
            {
                _logger.LogError($"Stripe error canceling subscription: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error canceling subscription: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Sync subscription status from Stripe
        /// </summary>
        public async Task<DotnetStripePaymentDemo.Models.Subscription> SyncSubscriptionAsync(string stripeSubscriptionId)
        {
            try
            {
                _logger.LogInformation($"Syncing subscription: {stripeSubscriptionId}");

                var subscription = await GetSubscriptionAsync(stripeSubscriptionId)
                    ?? throw new KeyNotFoundException($"Subscription not found: {stripeSubscriptionId}");

                var stripeSubscriptionService = new Stripe.SubscriptionService();
                var stripeSubscription = await stripeSubscriptionService.GetAsync(stripeSubscriptionId);

                subscription.Status = stripeSubscription.Status;
                subscription.CurrentPeriodStart = stripeSubscription.CurrentPeriodStart;
                subscription.CurrentPeriodEnd = stripeSubscription.CurrentPeriodEnd;
                subscription.EndedAt = stripeSubscription.EndedAt;
                subscription.CanceledAt = stripeSubscription.CanceledAt;
                subscription.UpdatedAt = DateTime.UtcNow;

                _dbContext.Subscriptions.Update(subscription);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Subscription synced: {stripeSubscriptionId}");
                return subscription;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error syncing subscription: {ex.Message}");
                throw;
            }
        }

        private async Task<int> GetCustomerIdAsync(string stripeCustomerId)
        {
            var customer = await _dbContext.Customers
                .FirstOrDefaultAsync(c => c.StripeCustomerId == stripeCustomerId);

            if (customer == null)
            {
                throw new KeyNotFoundException($"Customer not found: {stripeCustomerId}");
            }

            return customer.Id;
        }

        private static DateTime UnixTimeStampToDateTime(long? unixTimeStamp)
        {
            if (unixTimeStamp == null)
                return default;

            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp.Value).ToUniversalTime();
            return dateTime;
        }
    }
}
