using DotnetStripePaymentDemo.Data;
using DotnetStripePaymentDemo.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace DotnetStripePaymentDemo.Controllers
{
    /// <summary>
    /// API controller for invoice operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly InvoiceService _invoiceService;

        public InvoiceController(AppDbContext dbContext, InvoiceService invoiceService)
        {
            _dbContext = dbContext;
            _invoiceService = invoiceService;
        }

        /// <summary>
        /// Gets invoice details
        /// </summary>
        [HttpGet("{invoiceId}")]
        public async Task<IActionResult> GetInvoice(int invoiceId)
        {
            try
            {
                var invoice = await _dbContext.Invoices.FindAsync(invoiceId);
                if (invoice == null)
                {
                    return NotFound(new { error = "Invoice not found" });
                }

                return Ok(invoice);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Lists invoices for a customer
        /// </summary>
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> ListCustomerInvoices(int customerId, string? status = null, int limit = 10)
        {
            try
            {
                var invoices = await _invoiceService.ListCustomerInvoicesAsync(customerId, status, limit);

                return Ok(new
                {
                    data = invoices,
                    count = invoices.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Lists invoices for a subscription
        /// </summary>
        [HttpGet("subscription/{subscriptionId}")]
        public async Task<IActionResult> ListSubscriptionInvoices(int subscriptionId, string? status = null, int limit = 10)
        {
            try
            {
                var invoices = await _invoiceService.ListSubscriptionInvoicesAsync(subscriptionId, status, limit);

                return Ok(new
                {
                    data = invoices,
                    count = invoices.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets invoice PDF URL
        /// </summary>
        [HttpGet("{invoiceId}/pdf")]
        public async Task<IActionResult> GetInvoicePdf(int invoiceId)
        {
            try
            {
                var invoice = await _dbContext.Invoices.FindAsync(invoiceId);
                if (invoice == null)
                {
                    return NotFound(new { error = "Invoice not found" });
                }

                var pdfUrl = await _invoiceService.GetInvoicePdfUrlAsync(invoice.StripeInvoiceId);
                if (string.IsNullOrEmpty(pdfUrl))
                {
                    return NotFound(new { error = "Invoice PDF not available" });
                }

                return Ok(new { pdfUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Syncs an invoice from Stripe
        /// </summary>
        [HttpPost("{invoiceId}/sync")]
        public async Task<IActionResult> SyncInvoice(int invoiceId)
        {
            try
            {
                var invoice = await _dbContext.Invoices.FindAsync(invoiceId);
                if (invoice == null)
                {
                    return NotFound(new { error = "Invoice not found" });
                }

                var syncedInvoice = await _invoiceService.SyncInvoiceAsync(invoice.StripeInvoiceId);

                return Ok(new
                {
                    message = "Invoice synced successfully",
                    invoice = syncedInvoice
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Marks an invoice as paid
        /// </summary>
        [HttpPost("{invoiceId}/mark-paid")]
        public async Task<IActionResult> MarkInvoiceAsPaid(int invoiceId)
        {
            try
            {
                var invoice = await _dbContext.Invoices.FindAsync(invoiceId);
                if (invoice == null)
                {
                    return NotFound(new { error = "Invoice not found" });
                }

                var paidInvoice = await _invoiceService.MarkInvoiceAsPaidAsync(invoice.StripeInvoiceId);

                return Ok(new
                {
                    message = "Invoice marked as paid",
                    status = paidInvoice.Status
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Marks an invoice as payment failed
        /// </summary>
        [HttpPost("{invoiceId}/mark-failed")]
        public async Task<IActionResult> MarkInvoiceAsFailed(int invoiceId, [FromBody] MarkInvoiceAsFailedRequest? request = null)
        {
            try
            {
                var invoice = await _dbContext.Invoices.FindAsync(invoiceId);
                if (invoice == null)
                {
                    return NotFound(new { error = "Invoice not found" });
                }

                var failedInvoice = await _invoiceService.MarkInvoiceAsFailedAsync(
                    invoice.StripeInvoiceId,
                    request?.Reason
                );

                return Ok(new
                {
                    message = "Invoice marked as failed",
                    status = failedInvoice.Status
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class MarkInvoiceAsFailedRequest
    {
        public string? Reason { get; set; }
    }
}
