using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetStripePaymentDemo.Services
{
    /// <summary>
    /// Base service for Stripe API interactions
    /// Handles customer operations and common Stripe integration patterns
    /// </summary>
    public class StripeService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripeService> _logger;
        private readonly CustomerService _customerService;

        public StripeService(IConfiguration configuration, ILogger<StripeService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            // Initialize Stripe API key
            var secretKey = _configuration["Stripe:SecretKey"];
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("Stripe:SecretKey is not configured");
            }
            
            StripeConfiguration.ApiKey = secretKey;
            _customerService = new CustomerService();
        }

        /// <summary>
        /// Creates a new customer in Stripe
        /// </summary>
        public async Task<Customer> CreateCustomerAsync(string email, string? name = null, Dictionary<string, string>? metadata = null)
        {
            try
            {
                _logger.LogInformation($"Creating Stripe customer for email: {email}");
                
                var options = new CustomerCreateOptions
                {
                    Email = email,
                    Name = name,
                    Metadata = metadata
                };

                var customer = await _customerService.CreateAsync(options);
                _logger.LogInformation($"Successfully created Stripe customer: {customer.Id}");
                
                return customer;
            }
            catch (StripeException ex)
            {
                _logger.LogError($"Stripe error creating customer: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating customer: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Retrieves a customer from Stripe
        /// </summary>
        public async Task<Customer?> GetCustomerAsync(string customerId)
        {
            try
            {
                _logger.LogInformation($"Retrieving Stripe customer: {customerId}");
                
                var customer = await _customerService.GetAsync(customerId);
                
                if (customer == null)
                {
                    _logger.LogWarning($"Customer not found: {customerId}");
                    return null;
                }
                
                return customer;
            }
            catch (StripeException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning($"Stripe customer not found: {customerId}");
                return null;
            }
            catch (StripeException ex)
            {
                _logger.LogError($"Stripe error retrieving customer: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving customer: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Updates a customer in Stripe
        /// </summary>
        public async Task<Customer> UpdateCustomerAsync(string customerId, string? email = null, string? name = null, Dictionary<string, string>? metadata = null)
        {
            try
            {
                _logger.LogInformation($"Updating Stripe customer: {customerId}");
                
                var options = new CustomerUpdateOptions();
                
                if (!string.IsNullOrEmpty(email))
                    options.Email = email;
                    
                if (!string.IsNullOrEmpty(name))
                    options.Name = name;
                    
                if (metadata != null)
                    options.Metadata = metadata;

                var customer = await _customerService.UpdateAsync(customerId, options);
                _logger.LogInformation($"Successfully updated Stripe customer: {customerId}");
                
                return customer;
            }
            catch (StripeException ex)
            {
                _logger.LogError($"Stripe error updating customer: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating customer: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Deletes a customer in Stripe
        /// </summary>
        public async Task<bool> DeleteCustomerAsync(string customerId)
        {
            try
            {
                _logger.LogInformation($"Deleting Stripe customer: {customerId}");
                
                await _customerService.DeleteAsync(customerId);
                _logger.LogInformation($"Successfully deleted Stripe customer: {customerId}");
                
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError($"Stripe error deleting customer: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting customer: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Lists customers from Stripe
        /// </summary>
        public async Task<StripeList<Customer>> ListCustomersAsync(int limit = 10, string? startingAfter = null)
        {
            try
            {
                _logger.LogInformation($"Listing Stripe customers (limit: {limit})");
                
                var options = new CustomerListOptions
                {
                    Limit = limit,
                    StartingAfter = startingAfter
                };

                var customers = await _customerService.ListAsync(options);
                _logger.LogInformation($"Retrieved {customers.Data.Count} customers from Stripe");
                
                return customers;
            }
            catch (StripeException ex)
            {
                _logger.LogError($"Stripe error listing customers: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error listing customers: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets the Stripe publishable key for client-side operations
        /// </summary>
        public string GetPublishableKey()
        {
            var key = _configuration["Stripe:PublishableKey"];
            if (string.IsNullOrEmpty(key))
            {
                throw new InvalidOperationException("Stripe:PublishableKey is not configured");
            }
            return key;
        }

        /// <summary>
        /// Gets the Stripe webhook secret
        /// </summary>
        public string GetWebhookSecret()
        {
            var secret = _configuration["Stripe:WebhookSecret"];
            if (string.IsNullOrEmpty(secret))
            {
                throw new InvalidOperationException("Stripe:WebhookSecret is not configured");
            }
            return secret;
        }
    }
}
