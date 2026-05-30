using System;

namespace DotnetStripePaymentDemo.Services.Exceptions
{
    /// <summary>
    /// Base exception for Stripe-related errors
    /// </summary>
    public class StripeException : Exception
    {
        public string ErrorCode { get; set; }
        public string StripeErrorType { get; set; }

        public StripeException(string message) : base(message) { }

        public StripeException(string message, Exception innerException)
            : base(message, innerException) { }

        public StripeException(string message, string errorCode, string stripeErrorType)
            : base(message)
        {
            ErrorCode = errorCode;
            StripeErrorType = stripeErrorType;
        }
    }

    /// <summary>
    /// Exception for Stripe API errors
    /// </summary>
    public class StripeApiException : StripeException
    {
        public StripeApiException(string message) : base(message) { }
    }

    /// <summary>
    /// Exception for validation errors
    /// </summary>
    public class StripeValidationException : StripeException
    {
        public StripeValidationException(string message) : base(message) { }
    }

    /// <summary>
    /// Exception for authentication errors
    /// </summary>
    public class StripeAuthenticationException : StripeException
    {
        public StripeAuthenticationException(string message) : base(message) { }
    }
}
