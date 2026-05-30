using System.ComponentModel.DataAnnotations;

namespace DotnetStripePaymentDemo.DTOs.Requests
{
    /// <summary>
    /// Create customer request
    /// </summary>
    public class CreateCustomerRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Phone(ErrorMessage = "Invalid phone format")]
        public string Phone { get; set; }

        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string Address { get; set; }
    }

    /// <summary>
    /// Update customer request
    /// </summary>
    public class UpdateCustomerRequest
    {
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Phone(ErrorMessage = "Invalid phone format")]
        public string Phone { get; set; }

        [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
        public string Address { get; set; }
    }

    /// <summary>
    /// Create subscription request
    /// </summary>
    public class CreateSubscriptionRequest
    {
        [Required(ErrorMessage = "Customer ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid customer ID")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Plan is required")]
        [StringLength(100, ErrorMessage = "Plan cannot exceed 100 characters")]
        public string Plan { get; set; }

        [StringLength(50)]
        public string BillingCycle { get; set; } = "monthly";
    }

    /// <summary>
    /// Update subscription request
    /// </summary>
    public class UpdateSubscriptionRequest
    {
        [StringLength(100)]
        public string Plan { get; set; }

        [StringLength(500)]
        public string Metadata { get; set; }
    }

    /// <summary>
    /// Cancel subscription request
    /// </summary>
    public class CancelSubscriptionRequest
    {
        [Required(ErrorMessage = "Subscription ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid subscription ID")]
        public int SubscriptionId { get; set; }

        public bool Immediate { get; set; } = false;
    }

    /// <summary>
    /// Webhook event request (minimal validation)
    /// </summary>
    public class WebhookRequest
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public string Data { get; set; }
    }
}
