using DotnetStripePaymentDemo.Data;
using DotnetStripePaymentDemo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotnetStripePaymentDemo.Controllers
{
    /// <summary>
    /// API controller for subscription operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly SubscriptionService _subscriptionService;

        public SubscriptionController(AppDbContext dbContext, SubscriptionService subscriptionService)
        {
            _dbContext = dbContext;
            _subscriptionService = subscriptionService;
        }

        /// <summary>
        /// Creates a new subscription for a customer
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateSubscription([FromBody] CreateSubscriptionRequest request)
        {
            try
            {
                if (request.CustomerId <= 0 || string.IsNullOrEmpty(request.StripePriceId))
                {
                    return BadRequest(new { error = "Customer ID and Price ID are required" });
                }

                var customer = await _dbContext.Customers.FindAsync(request.CustomerId);
                if (customer == null)
                {
                    return NotFound(new { error = "Customer not found" });
                }

                var subscription = await _subscriptionService.CreateSubscriptionAsync(
                    customer.StripeCustomerId,
                    request.StripePriceId,
                    request.Metadata
                );

                return Ok(new
                {
                    message = "Subscription created successfully",
                    subscriptionId = subscription.Id,
                    stripeSubscriptionId = subscription.StripeSubscriptionId,
                    status = subscription.Status
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets subscription details
        /// </summary>
        [HttpGet("{subscriptionId}")]
        public async Task<IActionResult> GetSubscription(int subscriptionId)
        {
            try
            {
                var subscription = await _dbContext.Subscriptions.FindAsync(subscriptionId);
                if (subscription == null)
                {
                    return NotFound(new { error = "Subscription not found" });
                }

                return Ok(subscription);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Lists subscriptions for a customer
        /// </summary>
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> ListCustomerSubscriptions(int customerId, string? status = null)
        {
            try
            {
                var subscriptions = await _subscriptionService.ListCustomerSubscriptionsAsync(customerId, status);

                return Ok(new
                {
                    data = subscriptions,
                    count = subscriptions.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Updates a subscription (e.g., change plan)
        /// </summary>
        [HttpPut("{subscriptionId}")]
        public async Task<IActionResult> UpdateSubscription(int subscriptionId, [FromBody] UpdateSubscriptionRequest request)
        {
            try
            {
                var subscription = await _dbContext.Subscriptions.FindAsync(subscriptionId);
                if (subscription == null)
                {
                    return NotFound(new { error = "Subscription not found" });
                }

                var updatedSubscription = await _subscriptionService.UpdateSubscriptionAsync(
                    subscription.StripeSubscriptionId,
                    request.NewStripePriceId,
                    request.Metadata
                );

                return Ok(new
                {
                    message = "Subscription updated successfully",
                    subscription = updatedSubscription
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Cancels a subscription
        /// </summary>
        [HttpPost("{subscriptionId}/cancel")]
        public async Task<IActionResult> CancelSubscription(int subscriptionId, [FromBody] CancelSubscriptionRequest? request = null)
        {
            try
            {
                var subscription = await _dbContext.Subscriptions.FindAsync(subscriptionId);
                if (subscription == null)
                {
                    return NotFound(new { error = "Subscription not found" });
                }

                bool immediately = request?.Immediately ?? false;
                var canceledSubscription = await _subscriptionService.CancelSubscriptionAsync(
                    subscription.StripeSubscriptionId,
                    immediately
                );

                return Ok(new
                {
                    message = "Subscription canceled successfully",
                    status = canceledSubscription.Status,
                    canceledAt = canceledSubscription.CanceledAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Syncs subscription status from Stripe
        /// </summary>
        [HttpPost("{subscriptionId}/sync")]
        public async Task<IActionResult> SyncSubscription(int subscriptionId)
        {
            try
            {
                var subscription = await _dbContext.Subscriptions.FindAsync(subscriptionId);
                if (subscription == null)
                {
                    return NotFound(new { error = "Subscription not found" });
                }

                var syncedSubscription = await _subscriptionService.SyncSubscriptionAsync(
                    subscription.StripeSubscriptionId
                );

                return Ok(new
                {
                    message = "Subscription synced successfully",
                    subscription = syncedSubscription
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class CreateSubscriptionRequest
    {
        public int CustomerId { get; set; }
        public string StripePriceId { get; set; } = null!;
        public Dictionary<string, string>? Metadata { get; set; }
    }

    public class UpdateSubscriptionRequest
    {
        public string? NewStripePriceId { get; set; }
        public Dictionary<string, string>? Metadata { get; set; }
    }

    public class CancelSubscriptionRequest
    {
        public bool Immediately { get; set; }
    }
}
