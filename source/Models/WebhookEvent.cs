using System;

namespace DotnetStripePaymentDemo.Models
{
    /// <summary>
    /// Represents a webhook event received from Stripe
    /// </summary>
    public class WebhookEvent
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Stripe event ID (e.g., evt_1234567890)
        /// </summary>
        public string StripeEventId { get; set; } = null!;
        
        /// <summary>
        /// Event type (e.g., customer.created, subscription.updated)
        /// </summary>
        public string EventType { get; set; } = null!;
        
        /// <summary>
        /// API version used by Stripe
        /// </summary>
        public string? ApiVersion { get; set; }
        
        /// <summary>
        /// The event data (JSON serialized)
        /// </summary>
        public string EventData { get; set; } = null!;
        
        /// <summary>
        /// Processing status (pending, processed, failed, skipped)
        /// </summary>
        public string Status { get; set; } = null!;
        
        /// <summary>
        /// Number of processing attempts
        /// </summary>
        public int ProcessingAttempts { get; set; }
        
        /// <summary>
        /// Error message if processing failed
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// When the event was processed
        /// </summary>
        public DateTime? ProcessedAt { get; set; }
        
        /// <summary>
        /// Related customer ID (if applicable)
        /// </summary>
        public int? CustomerId { get; set; }
        
        /// <summary>
        /// Related subscription ID (if applicable)
        /// </summary>
        public int? SubscriptionId { get; set; }
        
        /// <summary>
        /// Related invoice ID (if applicable)
        /// </summary>
        public int? InvoiceId { get; set; }
        
        /// <summary>
        /// When the event was received
        /// </summary>
        public DateTime ReceivedAt { get; set; }
        
        /// <summary>
        /// When this record was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// When this record was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}
