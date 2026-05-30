using System;

namespace DotnetStripePaymentDemo.Models
{
    /// <summary>
    /// Represents an invoice record synced with Stripe
    /// </summary>
    public class Invoice
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Stripe invoice ID (e.g., in_1234567890)
        /// </summary>
        public string StripeInvoiceId { get; set; } = null!;
        
        /// <summary>
        /// Reference to the customer
        /// </summary>
        public int CustomerId { get; set; }
        
        /// <summary>
        /// Navigation property to customer
        /// </summary>
        public Customer? Customer { get; set; }
        
        /// <summary>
        /// Related subscription ID (nullable for one-time invoices)
        /// </summary>
        public int? SubscriptionId { get; set; }
        
        /// <summary>
        /// Invoice status (draft, open, paid, void, uncollectible)
        /// </summary>
        public string Status { get; set; } = null!;
        
        /// <summary>
        /// Invoice number (human-readable)
        /// </summary>
        public string? InvoiceNumber { get; set; }
        
        /// <summary>
        /// Invoice total in cents
        /// </summary>
        public long Total { get; set; }
        
        /// <summary>
        /// Amount due in cents
        /// </summary>
        public long AmountDue { get; set; }
        
        /// <summary>
        /// Amount paid in cents
        /// </summary>
        public long AmountPaid { get; set; }
        
        /// <summary>
        /// Currency code (e.g., usd)
        /// </summary>
        public string? Currency { get; set; }
        
        /// <summary>
        /// Invoice description
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// Due date for the invoice
        /// </summary>
        public DateTime? DueDate { get; set; }
        
        /// <summary>
        /// Invoice issue date
        /// </summary>
        public DateTime? IssuedAt { get; set; }
        
        /// <summary>
        /// Invoice paid date
        /// </summary>
        public DateTime? PaidAt { get; set; }
        
        /// <summary>
        /// Payment method type used
        /// </summary>
        public string? PaymentMethod { get; set; }
        
        /// <summary>
        /// URL to the invoice PDF
        /// </summary>
        public string? PdfUrl { get; set; }
        
        /// <summary>
        /// Invoice line items (JSON serialized)
        /// </summary>
        public string? LineItems { get; set; }
        
        /// <summary>
        /// Additional invoice metadata
        /// </summary>
        public string? Metadata { get; set; }
        
        /// <summary>
        /// When the invoice was created in Stripe
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
