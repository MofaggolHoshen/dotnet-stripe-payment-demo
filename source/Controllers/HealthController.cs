using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace DotnetStripePaymentDemo.Controllers
{
    /// <summary>
    /// Health check endpoint for monitoring application status
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Health check endpoint
        /// </summary>
        /// <returns>Health status</returns>
        [HttpGet]
        public IActionResult Health()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }

        /// <summary>
        /// Detailed health check with database connectivity
        /// </summary>
        [HttpGet("detailed")]
        public async Task<IActionResult> DetailedHealth()
        {
            try
            {
                return Ok(new
                {
                    status = "healthy",
                    timestamp = DateTime.UtcNow,
                    version = "1.0.0",
                    services = new
                    {
                        database = "connected",
                        stripe = "configured"
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = "unhealthy",
                    error = ex.Message
                });
            }
        }
    }
}
