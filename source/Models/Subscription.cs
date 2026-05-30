using System;

namespace DotnetStripePaymentDemo.Models
{
    /// <summary>
    /// Represents a subscription record synced with Stripe
    /// </summary>
    public class Subscription
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Stripe subscription ID (e.g., sub_1234567890)
        /// </summary>
        public string StripeSubscriptionId { get; set; } = null!;
        
        /// <summary>
        /// Reference to the customer
        /// </summary>
        public int CustomerId { get; set; }
        
        /// <summary>
        /// Navigation property to customer
        /// </summary>
        public Customer? Customer { get; set; }
        
        /// <summary>
        /// Stripe price ID for this subscription
        /// </summary>
        public string StripePriceId { get; set; } = null!;
        
        /// <summary>
        /// Stripe product ID
        /// </summary>
        public string StripeProductId { get; set; } = null!;
        
        /// <summary>
        /// Subscription status (active, past_due, canceled, etc.)
        /// </summary>
        public string Status { get; set; } = null!;
        
        /// <summary>
        /// Current billing period start
        /// </summary>
        public DateTime? CurrentPeriodStart { get; set; }
        
        /// <summary>
        /// Current billing period end
        /// </summary>
        public DateTime? CurrentPeriodEnd { get; set; }
        
        /// <summary>
        /// Subscription end date (if canceled)
        /// </summary>
        public DateTime? EndedAt { get; set; }
        
        /// <summary>
        /// Cancellation date (if canceled)
        /// </summary>
        public DateTime? CanceledAt { get; set; }
        
        /// <summary>
        /// Amount charged per billing cycle (in cents)
        /// </summary>
        public long Amount { get; set; }
        
        /// <summary>
        /// Billing cycle interval (day, week, month, year)
        /// </summary>
        public string? BillingInterval { get; set; }
        
        /// <summary>
        /// Number of billing periods
        /// </summary>
        public int? BillingIntervalCount { get; set; }
        
        /// <summary>
        /// Additional subscription metadata
        /// </summary>
        public string? Metadata { get; set; }
        
        /// <summary>
        /// When the subscription was created in Stripe
        /// </summary>
        public DateTime StripeCreatedAt { get; set; }
        
        /// <summary>
        /// When this record was created locally
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// When this record was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}
