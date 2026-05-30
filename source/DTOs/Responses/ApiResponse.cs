using System;
using System.Collections.Generic;

namespace DotnetStripePaymentDemo.DTOs.Responses
{
    /// <summary>
    /// Standard API response wrapper
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public Dictionary<string, string> Errors { get; set; }

        public ApiResponse(T data, string message = "Success")
        {
            Success = true;
            Data = data;
            Message = message;
            Errors = new Dictionary<string, string>();
        }

        public ApiResponse(string message = "An error occurred")
        {
            Success = false;
            Message = message;
            Errors = new Dictionary<string, string>();
        }

        public static ApiResponse<T> SuccessResponse(T data, string message = "Operation successful")
        {
            return new ApiResponse<T>(data, message);
        }

        public static ApiResponse<T> ErrorResponse(string message = "An error occurred")
        {
            return new ApiResponse<T>(message);
        }

        public static ApiResponse<T> ValidationErrorResponse(Dictionary<string, string> errors, string message = "Validation failed")
        {
            return new ApiResponse<T>(message) { Errors = errors };
        }
    }

    /// <summary>
    /// Paginated response wrapper
    /// </summary>
    public class PaginatedResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<T> Data { get; set; }
        public PaginationInfo Pagination { get; set; }
        public Dictionary<string, string> Errors { get; set; }

        public PaginatedResponse(List<T> data, int page, int pageSize, int total, string message = "Success")
        {
            Success = true;
            Data = data;
            Message = message;
            Pagination = new PaginationInfo { Page = page, PageSize = pageSize, Total = total };
            Errors = new Dictionary<string, string>();
        }

        public static PaginatedResponse<T> SuccessResponse(List<T> data, int page, int pageSize, int total, string message = "Success")
        {
            return new PaginatedResponse<T>(data, page, pageSize, total, message);
        }
    }

    public class PaginationInfo
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);
    }

    /// <summary>
    /// Customer response DTO
    /// </summary>
    public class CustomerResponse
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string StripeCustomerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Subscription response DTO
    /// </summary>
    public class SubscriptionResponse
    {
        public int Id { get; set; }
        public string StripeSubscriptionId { get; set; }
        public int CustomerId { get; set; }
        public string Plan { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTime? CurrentPeriodStart { get; set; }
        public DateTime? CurrentPeriodEnd { get; set; }
        public DateTime? CanceledAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Invoice response DTO
    /// </summary>
    public class InvoiceResponse
    {
        public int Id { get; set; }
        public string StripeInvoiceId { get; set; }
        public int CustomerId { get; set; }
        public int? SubscriptionId { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountPaid { get; set; }
        public string Status { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? DueDate { get; set; }
        public string PdfUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
