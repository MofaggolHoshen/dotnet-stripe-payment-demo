using System;
using System.Collections.Generic;

namespace DotnetStripePaymentDemo.Models
{
    /// <summary>
    /// Represents a customer record synced with Stripe
    /// </summary>
    public class Customer
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Stripe customer ID (e.g., cus_1234567890)
        /// </summary>
        public string StripeCustomerId { get; set; } = null!;
        
        /// <summary>
        /// Customer's email address
        /// </summary>
        public string Email { get; set; } = null!;
        
        /// <summary>
        /// Customer's full name
        /// </summary>
        public string? Name { get; set; }
        
        /// <summary>
        /// Additional customer metadata from Stripe
        /// </summary>
        public string? Metadata { get; set; }
        
        /// <summary>
        /// Default payment method ID in Stripe
        /// </summary>
        public string? DefaultPaymentMethodId { get; set; }
        
        /// <summary>
        /// Customer's billing address
        /// </summary>
        public string? BillingAddress { get; set; }
        
        /// <summary>
        /// When the customer was created in Stripe
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
        
        // Navigation properties
        public ICollection<Subscription>? Subscriptions { get; set; }
        public ICollection<Invoice>? Invoices { get; set; }
    }
}
