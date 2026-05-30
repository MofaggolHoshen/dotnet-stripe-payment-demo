using DotnetStripePaymentDemo.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stripe;
using StripeInvoice = Stripe.Invoice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetStripePaymentDemo.Services
{
    /// <summary>
    /// Service for managing invoices via Stripe API
    /// Handles invoice retrieval, tracking, and PDF downloads
    /// </summary>
    public class InvoiceService
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<InvoiceService> _logger;

        public InvoiceService(AppDbContext dbContext, ILogger<InvoiceService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves an invoice by Stripe invoice ID
        /// </summary>
        public async Task<DotnetStripePaymentDemo.Models.Invoice?> GetInvoiceAsync(string stripeInvoiceId)
        {
            try
            {
                _logger.LogInformation($"Retrieving invoice: {stripeInvoiceId}");
                
                var invoice = await _dbContext.Invoices
                    .FirstOrDefaultAsync(i => i.StripeInvoiceId == stripeInvoiceId);

                return invoice;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving invoice: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Lists invoices for a customer
        /// </summary>
        public async Task<List<DotnetStripePaymentDemo.Models.Invoice>> ListCustomerInvoicesAsync(int customerId, string? status = null, int limit = 10)
        {
            try
            {
                _logger.LogInformation($"Listing invoices for customer {customerId}");
                
                var query = _dbContext.Invoices
                    .Where(i => i.CustomerId == customerId)
                    .OrderByDescending(i => i.IssuedAt) as IOrderedQueryable<DotnetStripePaymentDemo.Models.Invoice>;

                if (!string.IsNullOrEmpty(status))
                {
                    query = query!.Where(i => i.Status == status).OrderByDescending(i => i.IssuedAt);
                }

                var invoices = await query.Take(limit).ToListAsync();
                return invoices;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error listing invoices: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Lists invoices for a subscription
        /// </summary>
        public async Task<List<DotnetStripePaymentDemo.Models.Invoice>> ListSubscriptionInvoicesAsync(int subscriptionId, string? status = null, int limit = 10)
        {
            try
            {
                _logger.LogInformation($"Listing invoices for subscription {subscriptionId}");
                
                var query = _dbContext.Invoices
                    .Where(i => i.SubscriptionId == subscriptionId)
                    .OrderByDescending(i => i.IssuedAt) as IOrderedQueryable<DotnetStripePaymentDemo.Models.Invoice>;

                if (!string.IsNullOrEmpty(status))
                {
                    query = query!.Where(i => i.Status == status).OrderByDescending(i => i.IssuedAt);
                }

                var invoices = await query.Take(limit).ToListAsync();
                return invoices;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error listing subscription invoices: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets invoice PDF URL
        /// </summary>
        public async Task<string?> GetInvoicePdfUrlAsync(string stripeInvoiceId)
        {
            try
            {
                _logger.LogInformation($"Retrieving invoice PDF URL: {stripeInvoiceId}");
                
                var invoiceService = new Stripe.InvoiceService();
                StripeInvoice result = await invoiceService.GetAsync(stripeInvoiceId);
                
                if (result != null && result.HostedInvoiceUrl != null)
                {
                    _logger.LogInformation($"Retrieved invoice PDF URL for {stripeInvoiceId}");
                    return result.HostedInvoiceUrl;
                }

                _logger.LogWarning($"No PDF URL available for invoice {stripeInvoiceId}");
                return null;
            }
            catch (StripeException ex)
            {
                _logger.LogError($"Stripe error retrieving invoice PDF URL: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving invoice PDF URL: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Creates or updates an invoice record in the database from Stripe
        /// </summary>
        public async Task<DotnetStripePaymentDemo.Models.Invoice> SyncInvoiceAsync(string stripeInvoiceId)
        {
            try
            {
                _logger.LogInformation($"Syncing invoice: {stripeInvoiceId}");

                var invoiceService = new Stripe.InvoiceService();
                StripeInvoice result = await invoiceService.GetAsync(stripeInvoiceId);
                if (result == null)
                    throw new KeyNotFoundException($"Invoice not found in Stripe: {stripeInvoiceId}");
                
                var invoice = await GetInvoiceAsync(stripeInvoiceId);

                if (invoice == null)
                {
                    // Create new invoice record
                    var customerId = await GetCustomerIdAsync(result.CustomerId);
                    
                    var subscriptionId = !string.IsNullOrEmpty(result.SubscriptionId) 
                        ? await GetSubscriptionIdAsync(result.SubscriptionId)
                        : (int?)null;

                    invoice = new DotnetStripePaymentDemo.Models.Invoice
                    {
                        StripeInvoiceId = result.Id,
                        CustomerId = customerId,
                        SubscriptionId = subscriptionId,
                        Status = result.Status,
                        InvoiceNumber = result.Number,
                        Total = result.Total,
                        AmountDue = result.AmountDue,
                        AmountPaid = result.AmountPaid,
                        Currency = result.Currency,
                        Description = result.Description,
                        DueDate = result.DueDate,
                        IssuedAt = result.Created,
                        PaidAt = result.StatusTransitions?.PaidAt,
                        PaymentMethod = result.Paid ? "paid" : "pending",
                        PdfUrl = result.HostedInvoiceUrl,
                        Metadata = System.Text.Json.JsonSerializer.Serialize(result.Metadata),
                        StripeCreatedAt = result.Created,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _dbContext.Invoices.Add(invoice);
                }
                else
                {
                    // Update existing invoice record
                    invoice.Status = result.Status;
                    invoice.InvoiceNumber = result.Number;
                    invoice.Total = result.Total;
                    invoice.AmountDue = result.AmountDue;
                    invoice.AmountPaid = result.AmountPaid;
                    invoice.Currency = result.Currency;
                    invoice.DueDate = result.DueDate;
                    invoice.PaidAt = result.StatusTransitions?.PaidAt;
                    invoice.PaymentMethod = result.Paid ? "paid" : "pending";
                    invoice.PdfUrl = result.HostedInvoiceUrl;
                    invoice.Metadata = System.Text.Json.JsonSerializer.Serialize(result.Metadata);
                    invoice.UpdatedAt = DateTime.UtcNow;

                    _dbContext.Invoices.Update(invoice);
                }

                await _dbContext.SaveChangesAsync();
                _logger.LogInformation($"Invoice synced: {stripeInvoiceId}");
                return invoice;
            }
            catch (StripeException ex)
            {
                _logger.LogError($"Stripe error syncing invoice: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error syncing invoice: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Marks an invoice as paid
        /// </summary>
        public async Task<DotnetStripePaymentDemo.Models.Invoice> MarkInvoiceAsPaidAsync(string stripeInvoiceId)
        {
            try
            {
                _logger.LogInformation($"Marking invoice as paid: {stripeInvoiceId}");

                var invoice = await GetInvoiceAsync(stripeInvoiceId)
                    ?? throw new KeyNotFoundException($"Invoice not found: {stripeInvoiceId}");

                invoice.Status = "paid";
                invoice.AmountPaid = invoice.Total;
                invoice.PaymentMethod = "paid";
                invoice.PaidAt = DateTime.UtcNow;
                invoice.UpdatedAt = DateTime.UtcNow;

                _dbContext.Invoices.Update(invoice);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Invoice marked as paid: {stripeInvoiceId}");
                return invoice;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error marking invoice as paid: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Marks an invoice as payment failed
        /// </summary>
        public async Task<DotnetStripePaymentDemo.Models.Invoice> MarkInvoiceAsFailedAsync(string stripeInvoiceId, string? reason = null)
        {
            try
            {
                _logger.LogInformation($"Marking invoice as failed: {stripeInvoiceId}");

                var invoice = await GetInvoiceAsync(stripeInvoiceId)
                    ?? throw new KeyNotFoundException($"Invoice not found: {stripeInvoiceId}");

                invoice.Status = "payment_failed";
                invoice.PaymentMethod = reason ?? "failed";
                invoice.UpdatedAt = DateTime.UtcNow;

                _dbContext.Invoices.Update(invoice);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation($"Invoice marked as failed: {stripeInvoiceId}");
                return invoice;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error marking invoice as failed: {ex.Message}");
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

        private async Task<int> GetSubscriptionIdAsync(string stripeSubscriptionId)
        {
            var subscription = await _dbContext.Subscriptions
                .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscriptionId);

            if (subscription == null)
            {
                throw new KeyNotFoundException($"Subscription not found: {stripeSubscriptionId}");
            }

            return subscription.Id;
        }

        private static DateTime? UnixTimeStampToDateTime(long? unixTimeStamp)
        {
            if (unixTimeStamp == null)
                return null;

            var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp.Value).ToUniversalTime();
            return dateTime;
        }
    }
}
