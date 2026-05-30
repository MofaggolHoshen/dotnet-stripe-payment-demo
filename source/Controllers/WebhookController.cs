using DotnetStripePaymentDemo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DotnetStripePaymentDemo.Controllers
{
    /// <summary>
    /// API controller for receiving and processing Stripe webhooks
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly WebhookService _webhookService;
        private readonly IConfiguration _configuration;

        public WebhookController(WebhookService webhookService, IConfiguration configuration)
        {
            _webhookService = webhookService;
            _configuration = configuration;
        }

        /// <summary>
        /// Receives and processes Stripe webhook events
        /// </summary>
        [HttpPost("stripe")]
        public async Task<IActionResult> HandleStripeWebhook()
        {
            try
            {
                // Read the request body
                using (var stream = new StreamReader(Request.Body))
                {
                    var json = await stream.ReadToEndAsync();

                    // Get the signature header
                    var signatureHeader = Request.Headers["Stripe-Signature"].ToString();

                    if (string.IsNullOrEmpty(signatureHeader))
                    {
                        return BadRequest(new { error = "Missing Stripe-Signature header" });
                    }

                    // Get webhook secret
                    var webhookSecret = _configuration["Stripe:WebhookSecret"];
                    if (string.IsNullOrEmpty(webhookSecret))
                    {
                        return StatusCode(500, new { error = "Webhook secret not configured" });
                    }

                    // Process the webhook
                    var webhookService = new WebhookService(
                        null, // DbContext will be injected via DI in real implementation
                        null, // SubscriptionService
                        null, // InvoiceService
                        null, // ILogger
                        webhookSecret
                    );

                    bool processed = await _webhookService.ProcessWebhookAsync(json, signatureHeader);

                    if (processed)
                    {
                        return Ok(new { message = "Webhook processed successfully" });
                    }
                    else
                    {
                        return StatusCode(500, new { error = "Failed to process webhook" });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "healthy" });
        }
    }
}
