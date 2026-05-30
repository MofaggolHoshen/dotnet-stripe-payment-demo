using DotnetStripePaymentDemo.Data;
using DotnetStripePaymentDemo.Models;
using DotnetStripePaymentDemo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotnetStripePaymentDemo.Controllers
{
    /// <summary>
    /// API controller for customer operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly StripeService _stripeService;

        public CustomerController(AppDbContext dbContext, StripeService stripeService)
        {
            _dbContext = dbContext;
            _stripeService = stripeService;
        }

        /// <summary>
        /// Creates a new customer in Stripe and stores it locally
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> RegisterCustomer([FromBody] CreateCustomerRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email))
                {
                    return BadRequest(new { error = "Email is required" });
                }

                // Create customer in Stripe
                var stripeCustomer = await _stripeService.CreateCustomerAsync(request.Email, request.Name);

                // Store locally
                var customer = new Customer
                {
                    StripeCustomerId = stripeCustomer.Id,
                    Email = request.Email,
                    Name = request.Name,
                    StripeCreatedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.Customers.Add(customer);
                await _dbContext.SaveChangesAsync();

                return Ok(new
                {
                    message = "Customer registered successfully",
                    customerId = customer.Id,
                    stripeCustomerId = customer.StripeCustomerId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Gets customer details
        /// </summary>
        [HttpGet("{customerId}")]
        public async Task<IActionResult> GetCustomer(int customerId)
        {
            try
            {
                var customer = await _dbContext.Customers.FindAsync(customerId);
                if (customer == null)
                {
                    return NotFound(new { error = "Customer not found" });
                }

                return Ok(customer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Updates customer information
        /// </summary>
        [HttpPut("{customerId}")]
        public async Task<IActionResult> UpdateCustomer(int customerId, [FromBody] UpdateCustomerRequest request)
        {
            try
            {
                var customer = await _dbContext.Customers.FindAsync(customerId);
                if (customer == null)
                {
                    return NotFound(new { error = "Customer not found" });
                }

                // Update in Stripe
                await _stripeService.UpdateCustomerAsync(
                    customer.StripeCustomerId,
                    request.Email,
                    request.Name
                );

                // Update locally
                if (!string.IsNullOrEmpty(request.Email))
                    customer.Email = request.Email;
                if (!string.IsNullOrEmpty(request.Name))
                    customer.Name = request.Name;

                customer.UpdatedAt = DateTime.UtcNow;
                _dbContext.Customers.Update(customer);
                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "Customer updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Lists all customers
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ListCustomers(int limit = 10, int page = 1)
        {
            try
            {
                var skip = (page - 1) * limit;
                var customers = await _dbContext.Customers
                    .Skip(skip)
                    .Take(limit)
                    .ToListAsync();

                var total = await _dbContext.Customers.CountAsync();

                return Ok(new
                {
                    data = customers,
                    pagination = new
                    {
                        page,
                        limit,
                        total,
                        pages = (int)System.Math.Ceiling((double)total / limit)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a customer
        /// </summary>
        [HttpDelete("{customerId}")]
        public async Task<IActionResult> DeleteCustomer(int customerId)
        {
            try
            {
                var customer = await _dbContext.Customers.FindAsync(customerId);
                if (customer == null)
                {
                    return NotFound(new { error = "Customer not found" });
                }

                // Delete from Stripe
                await _stripeService.DeleteCustomerAsync(customer.StripeCustomerId);

                // Delete locally
                _dbContext.Customers.Remove(customer);
                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "Customer deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class CreateCustomerRequest
    {
        public string Email { get; set; } = null!;
        public string? Name { get; set; }
    }

    public class UpdateCustomerRequest
    {
        public string? Email { get; set; }
        public string? Name { get; set; }
    }
}
